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

namespace VEDrivers.Economy.Wallets
{
    public class NeblioAccount : CommonAccount
    {
        public NeblioAccount()
        {
            Tokens = new ConcurrentDictionary<string, IToken>();
            Transactions = new ConcurrentDictionary<string, ITransaction>();
            NumberOfTransaction = 0;
            Type = AccountTypes.Neblio;

            client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
        }

        private QTWalletRPCClient rpcClient;
        private HttpClient httpClient = new HttpClient();
        private IClient client;
        private ICryptocurrency NeblioCrypto { get; set; } = new NeblioCryptocurrency();

        public override event EventHandler<IAccount> DetailsLoaded;
        public override event EventHandler<NewTransactionDTO> TxDetailsLoaded;
        public override event EventHandler<NewTransactionDTO> ConfirmedTransaction;

        private bool firstInit = true;
        private bool firstLoading = false;
        public void AddToken(string address, IToken token)
        {
            Tokens.Add(address, token);
        }

        private async Task ReloadTransactions(ICollection<string> transactions)
        {
            if (firstLoading && firstInit)
                return;

            if (transactions == null)
                return;

            // todo cancelation token
            await Task.Run(async () =>
            {
                if (transactions.Count > NumberOfTransaction)
                {
                    firstLoading = true;

                    var txs = transactions.ToArray();

                    for (int k = (int)(NumberOfTransaction); k < transactions.Count; k++)
                    {
                        if (!Transactions.TryGetValue(txs[k], out var tx))
                        {
                            tx = TransactionFactory.GetTranaction(TransactionTypes.Neblio, txs[k], Address, WalletName, false);
                            if (tx != null)
                            {
                                if (txs[k] == LastProcessedTxId)
                                    firstInit = false;

                                if (!firstInit)
                                {
                                    tx.DetailsLoaded += Tx_DetailsLoaded;
                                    tx.ConfirmedTransaction += Tx_ConfirmedTransaction;
                                }

                                // todo cancelation token
                                tx.GetInfo();

                                Transactions.TryAdd(txs[k], tx);
                                NumberOfTransaction++;
                                
                            }
                        }
                    }
                }

                //NumberOfTransaction = transactions.Count;
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
            if (Transactions.TryGetValue(tx.TxId, out var t))
            {
                t.DetailsLoaded -= Tx_DetailsLoaded;
                LastProcessedTxId = t.TxId;
                TxDetailsLoaded?.Invoke(this, dto);
            }
        }

        public async Task LoadTokensFromTokenAccounts(List<NeblioQTWalletTokenAccountDetails> tokacc)
        {
            Tokens.Clear();
            foreach (var t in tokacc)
            {
                var tdetails = await NeblioTransactionHelpers.TokenMetadataAsync(client, TokenTypes.NTP1, t.TokenId, string.Empty);
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
                                            var tdetails = await NeblioTransactionHelpers.TokenMetadataAsync(client, TokenTypes.NTP1, tokid, string.Empty);
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
            // todo cancelation token
            _ = Task.Run(async () =>
            {
                GetAddressResponse addrinfo = null;

                while (true)
                {
                    try
                    {
                        addrinfo = await AddressInfoAsync(client, Address);
                    }
                    catch(Exception ex)
                    {
                        // todo
                    }

                    if (addrinfo != null)
                    {
                        TotalBalance = addrinfo.Balance;
                        TotalUnconfirmedBalance = addrinfo.UnconfirmedBalance;

                        try
                        {
                            await ReloadTransactions(addrinfo?.Transactions);
                        }
                        catch (Exception ex)
                        {
                            // todo
                        }

                        DetailsLoaded?.Invoke(null, this);
                    }

                    await Task.Delay(interval);
                }
            });

            return await Task.FromResult("END");
        }

        //todo move to transaction helpers and preprocess data to get just IAccount
        private async Task<GetAddressResponse> AddressInfoAsync(IClient client, string addr)
        {
            var address = await client.GetAddressAsync(addr);
            /*
            Console.WriteLine($"AddrStr                     = {address.AddrStr                 }   ");
            Console.WriteLine($"Balance                     = {address.Balance                 }   ");
            Console.WriteLine($"BalanceSat                  = {address.BalanceSat              }   ");
            Console.WriteLine($"TotalReceived               = {address.TotalReceived           }   ");
            Console.WriteLine($"TotalReceivedSat            = {address.TotalReceivedSat        }   ");
            Console.WriteLine($"TotalSent                   = {address.TotalSent               }   ");
            Console.WriteLine($"TotalSentSat                = {address.TotalSentSat            }   ");
            Console.WriteLine($"UnconfirmedBalance          = {address.UnconfirmedBalance      }   ");
            Console.WriteLine($"UnconfirmedBalanceSat       = {address.UnconfirmedBalanceSat   }   ");
            Console.WriteLine($"UnconfirmedTxAppearances    = {address.UnconfirmedTxAppearances}   ");
            Console.WriteLine($"TxAppearances               = {address.TxAppearances           }   ");
            */

            /*
            foreach (var item in address.AdditionalProperties)
            {
                Console.WriteLine($"Property: Key = {item.Key}, Value = {item.Value}");
            }
            Console.WriteLine();
            */
            return address;
        }

    }
}
