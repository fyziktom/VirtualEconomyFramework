using Neblio.RestApi;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Linq;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Economy.Coins;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Transactions;
using VEDrivers.Nodes;
using VEDrivers.Economy.DTO;
using HtmlAgilityPack;
using log4net;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;

namespace VEDrivers.Economy.Wallets
{
    public class NeblioAccount : CommonAccount
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public NeblioAccount()
        {
            Tokens = new ConcurrentDictionary<string, IToken>();
            Transactions = new ConcurrentDictionary<string, ITransaction>();
            NumberOfTransaction = 0;
            Type = AccountTypes.Neblio;
            //client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };

            lastTxSaveDto = new LastTxSaveDto();
        }

        //private QTWalletRPCClient rpcClient;
        //private HttpClient httpClient = new HttpClient();
        //private IClient client;
        //private ICryptocurrency NeblioCrypto { get; set; } = new NeblioCryptocurrency();

        public override event EventHandler<IAccount> DetailsLoaded;
        public override event EventHandler<NewTransactionDTO> TxDetailsLoaded;
        public override event EventHandler<NewTransactionDTO> ConfirmedTransaction;

        private LastTxSaveDto lastTxSaveDto;

        public double NumberOfLoadedTransaction 
        { 
            get
            {
                return Transactions.Count;
            }
        }

        private double initNumberOfTx = 0;

        private ConcurrentQueue<string> transactionsToLoad = new ConcurrentQueue<string>();
        private ConcurrentDictionary<string, string> transactionsInLoading = new ConcurrentDictionary<string, string>();

        public override bool IsLocked()
        {
            if (AccountKey != null)
            {
                if (AccountKey.IsEncrypted)
                {
                    if (AccountKey.IsPassLoaded)
                        return false;
                    else
                        return true;
                }
                else
                {
                    if (AccountKey.IsLoaded)
                        return false;
                    else
                        return true;
                }
            }
            else
            {
                return true;
            }
        }

        public void AddToken(string address, IToken token)
        {
            Tokens.Add(address, token);
        }

        private async Task LoadOneTx(string txId, bool initLoad = false)
        {
            var tx = TransactionFactory.GetTransaction(TransactionTypes.Neblio, txId, Address, WalletName, false);
            if (tx != null)
            {
                tx.DetailsLoaded += Tx_DetailsLoaded;
                tx.ConfirmedTransaction += Tx_ConfirmedTransaction;

                tx.InvokeLoadFinish = !initLoad; // for init load (after start, old tx which has been processed in last run] dont call all events
                // todo cancelation token
                tx.GetInfo();

                transactionsInLoading.TryAdd(txId, txId);
                Transactions.TryAdd(txId, tx);
            }
        }

        private async Task AutoTxReload()
        {
            
            _ = Task.Run(async () =>
            {
                try
                {
                    var firstInit = false;
                    var cpus = Environment.ProcessorCount;
                    var optimalLoad = cpus * 10; // this must be tested! just guess. Maybe it should be as exponential function

                    while (true) // todo cancelation token
                    {
                        while(transactionsInLoading.Count < optimalLoad) // run just as much loading as possible due to cpu count
                        {
                            // if all init tx is already loade, turn off firstInit flag
                            if (NumberOfLoadedTransaction >= initNumberOfTx)
                                firstInit = false;
                            else
                                firstInit = true;

                            if (transactionsToLoad.TryDequeue(out var txId))
                            {
                                if (!string.IsNullOrEmpty(txId))
                                    await LoadOneTx(txId, firstInit);
                            }
                            else
                            {
                                break;
                            }
                        }

                        await Task.Delay(100);
                    }
                }
                catch(Exception ex)
                {
                    log.Error("Neblio Auto Tx reload end with exception - ", ex);
                    Console.WriteLine($"Neblio Auto Tx reload end with exception - {ex}");
                }
            });
        }

        private async Task ReloadTransactions(ICollection<string> transactions)
        {
            if (transactions == null)
                return;

            await Task.Run(async () =>
            {
                if ((NumberOfLoadedTransaction + transactionsToLoad.Count) < NumberOfTransaction)
                {
                    foreach (var t in transactions)
                    {
                        if (!Transactions.ContainsKey(t))
                        {
                            if (!transactionsToLoad.Contains(t))
                                transactionsToLoad.Enqueue(t);
                        }
                    }
                }
            });
        }

        private void Tx_ConfirmedTransaction(object sender, NewTransactionDTO dto)
        {
            var tx = sender as NeblioTransaction;
            if (Transactions.TryGetValue(tx.TxId, out var t))
            {
                t.ConfirmedTransaction -= Tx_ConfirmedTransaction;
                LastConfirmedTxId = t.TxId;
                ConfirmedTransaction?.Invoke(this, dto);
            }
        }

        private void Tx_DetailsLoaded(object sender, NewTransactionDTO dto)
        {
            var tx = sender as NeblioTransaction;

            transactionsInLoading.TryRemove(tx.TxId, out var txid);

            if (tx.InvokeLoadFinish)
            { 
                if (Transactions.TryGetValue(tx.TxId, out var t))
                {
                    t.DetailsLoaded -= Tx_DetailsLoaded;
                    LastProcessedTxId = t.TxId;
                    TxDetailsLoaded?.Invoke(this, dto);
                }
            }
        }

        public async Task LoadTokensFromTokenAccounts(List<NeblioQTWalletTokenAccountDetails> tokacc)
        {
            Tokens.Clear();
            foreach (var t in tokacc)
            {
                var tdetails = await NeblioTransactionHelpers.TokenMetadataAsync(TokenTypes.NTP1, t.TokenId, string.Empty);
                tdetails.ActualBalance = t.Balance;
                Tokens.Add(t.TokenId, tdetails);
            }
        }

        /// <summary>
        /// Load actual address token state from neblio explorer
        /// </summary>
        /// <returns></returns>
        public async Task LoadTokensStates()
        {
            try
            {
                var tokens = new Dictionary<string, IToken>();

                string url = "https://explorer.nebl.io/address/" + Address; //loading actual state of tokens from explorer
                var webGet = new HtmlWeb();
                var document = webGet.Load(url);

                HtmlNodeCollection tl = document.DocumentNode.SelectNodes("//a");
                if (tl != null)
                {
                    foreach (HtmlNode node in tl)
                    {
                        //Console.WriteLine(node.InnerText.Trim());
                        var href = node.Attributes["href"].Value;

                        if (!string.IsNullOrEmpty(href))
                        {
                            if (href.Contains("/token/"))
                            {
                                var tabletr = node.ParentNode.ParentNode.ParentNode;
                                if (tabletr != null)
                                {
                                    var symbol = node.InnerText;
                                    var tokid = href.Remove(0, 7);
                                    var balanceLine = tabletr.LastChild;

                                    if (balanceLine != null)
                                    {
                                        var bal = Convert.ToDouble(balanceLine.InnerText);

                                        try
                                        {
                                            var tdetails = await NeblioTransactionHelpers.TokenMetadataAsync(TokenTypes.NTP1, tokid, string.Empty);
                                            tdetails.ActualBalance = bal;
                                            tokens.Add(tokid, tdetails);
                                        }
                                        catch(Exception ex)
                                        {
                                            // todo
                                        }
                                        
                                    }
                                }
                            }
                        }
                    }
                }

                Tokens.Clear();
                foreach (var t in tokens)
                {
                    Tokens.Add(t.Key, t.Value);
                }

                //cleanup
                webGet = null;
                document = null;
                tl = null;
                tokens = null;
            }
            catch(Exception ex)
            {
                // todo
            }
        }

        public override async Task<string> StartRefreshingData(int interval = 1000)
        {

            AutoTxReload(); // start autoTxreload task, not awaited. It runs at background. It should be placed in main loop with recovery

            // todo cancelation token
            _ = Task.Run(async () =>
            {
                GetAddressResponse addrinfo = null;

                while (true)
                {
                    try
                    {
                        addrinfo = await NeblioTransactionHelpers.AddressInfoAsync(Address);
                    }
                    catch(Exception ex)
                    {
                        // todo
                    }

                    if (addrinfo != null)
                    {
                        TotalBalance = addrinfo.Balance;
                        TotalUnconfirmedBalance = addrinfo.UnconfirmedBalance;

                        if (addrinfo.Transactions != null)
                        {
                            SpendableTxId = addrinfo.Transactions.LastOrDefault();
                            // this will run just in first turn after init of account
                            // if there is some stored LastProcessedTxId it will load all tx until this one without invoke event
                            // if there is no last tx stored it will count until end and set all as already handled in some previous run of the app
                            if (NumberOfTransaction == 0)
                            {
                                var txs = addrinfo.Transactions.ToArray();

                                for (int t = 0; t < txs.Length; t++)
                                {
                                    if (txs[t] == LastProcessedTxId)
                                    {
                                        initNumberOfTx = t;
                                    }
                                }

                                if (initNumberOfTx == 0)
                                    initNumberOfTx = addrinfo.Transactions.Count;
                            }

                            NumberOfTransaction = addrinfo.Transactions.Count;

                            try
                            {
                                await ReloadTransactions(addrinfo.Transactions);
                            }
                            catch (Exception ex)
                            {
                                // todo
                            }
                        }

                        DetailsLoaded?.Invoke(null, this);

                        // check if some new tx was processed or confirmed and save them for recovery as last processed
                        if (lastTxSaveDto.LastConfirmedTxId != LastConfirmedTxId || lastTxSaveDto.LastProcessedTxId != LastProcessedTxId)
                        {
                            try
                            {
                                lastTxSaveDto.LastConfirmedTxId = LastConfirmedTxId;
                                lastTxSaveDto.LastProcessedTxId = LastProcessedTxId;

                                var output = JsonConvert.SerializeObject(lastTxSaveDto);
                                FileHelpers.WriteTextToFile(Path.Join(EconomyMainContext.CurrentLocation, $"Accounts/{Address}.txt"), output);
                            }
                            catch (Exception ex)
                            {
                                log.Error("Cannot write file with last processed confirmed Tx!", ex);
                            }
                        }
                    }

                    await Task.Delay(interval);
                }
            });

            return await Task.FromResult("END");
        }
    }
}
