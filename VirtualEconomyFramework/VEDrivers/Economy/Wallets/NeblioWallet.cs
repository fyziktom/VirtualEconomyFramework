using HtmlAgilityPack;
using log4net;
using Microsoft.Extensions.Configuration;
using Neblio.RestApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Economy.DTO;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Transactions;

namespace VEDrivers.Economy.Wallets
{
    public class NeblioWallet : CommonWallet
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public NeblioWallet(Guid id, Guid owner, string name, bool useRPC = false, string urlbase = "127.0.0.1", int port = 6326)
        {
            Owner = owner;
            Name = name;
            ConnectionUrlBaseAddress = urlbase;
            ConnectionPort = port;
            Id = id;

            if (useRPC)
            {
                rpcClient = new QTWalletRPCClient(ConnectionUrlBaseAddress, ConnectionPort);
                rpcClient.InitClients();
            }

            Type = WalletTypes.Neblio;

            Accounts = new ConcurrentDictionary<string, IAccount>();

            client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
        }

        public override event EventHandler<NewTransactionDTO> NewTransaction;
        public override event EventHandler<NewTransactionDTO> NewConfirmedTransactionDetailsReceived;

        private QTWalletRPCClient rpcClient;
        private HttpClient httpClient = new HttpClient();
        private IClient client;
        private ICryptocurrency NeblioCrypto { get; set; } = new NeblioCryptocurrency();

        public override async Task<IWallet> GetDetails()
        {
            try
            {
                var r = await rpcClient.RPCLocalCommandAsync("getwalletinfo", null);

                var jsonread = JsonConvert.DeserializeObject<NeblioQTWalletInfo>(r);

                TotalBalance = jsonread.result.balance;
                TotalUnconfirmedBalance = jsonread.result.unconfirmed_balance;

                await ListAccounts();

                Console.WriteLine("Result:\n\n" + r);
            }
            catch (Exception ex)
            {
                log.Error("Exeption in input data format");
            }

            return this;
        }

        private List<NeblioQTWalletTokenAccountDetails> parseTokenAccounts(JToken array)
        {
            var list = new List<NeblioQTWalletTokenAccountDetails>();

            foreach(var obj in array)
            {
                var ntad = new NeblioQTWalletTokenAccountDetails();

                var i = 0;
                foreach (var o in obj)
                {
                    switch (i)
                    {
                        case 0:
                            ntad.Symbol = o.ToObject<string>();
                            break;
                        case 1:
                            ntad.Balance = o.ToObject<double>();
                            break;
                        case 2:
                            ntad.TokenId = o.ToObject<string>();
                            break;
                    }

                    i++;
                }

                list.Add(ntad);
            }
            
            return list;
        }

        private async Task LoadAccountObject(string address, string name, double balance, List<NeblioQTWalletTokenAccountDetails> tokacc, bool withTx = false)
        {
            if (!string.IsNullOrEmpty(address))
            {
                if (!string.IsNullOrEmpty(name) || (balance != 0))
                {
                    if (Accounts.TryGetValue(address, out var a))
                    {
                        if (a != null)
                        {
                            //await a.GetDetails();

                            TotalBalance += a.TotalBalance;

                            if (tokacc != null)
                            {
                                await (a as NeblioAccount).LoadTokensFromTokenAccounts(tokacc);
                            }
                            else
                            {
                                ////////////////
                                await (a as NeblioAccount).LoadTokensStates();
                                a.LoadingData = false;
                            }

                        }
                    }
                    ////////////////////////////////////////////////////
                }
            }
        }

        /// <summary>
        /// This function will load info from QT wallet via RPC commands
        /// Then it parse for addresses with tokens and without tokens and load the details about the addressed
        /// </summary>
        /// <param name="withTx"></param>
        /// <returns></returns>
        private async Task ListAccountWithRPC(bool withTx = false)
        {
            var res = "ERROR";
            var accounts = new List<NeblioQTWalletAccountDetails>();
            var addrInfos = new List<GetAddressResponse>();

            TotalBalance = 0;

            try
            {
                res = await rpcClient.RPCLocalCommandAsync("listaddressgroupings,0,true", null);

                var jsonread = JsonConvert.DeserializeObject<JObject>(res);

                if (jsonread["result"].Count<JToken>() > 0)
                {
                    var accnts = new List<JToken>();
                    foreach (var objlst in jsonread["result"])
                    {
                        foreach (var obj in objlst)
                            accnts.Add(obj);
                    }

                    foreach (var obj in accnts)
                    {
                        try
                        {
                            var cnt = obj.Count<JToken>();
                            var nad = new NeblioQTWalletAccountDetails();
                            var i = 0;

                            foreach (var o in obj)
                            {
                                switch (i)
                                {
                                    case 0:
                                        nad.Address = o.ToObject<string>();
                                        break;
                                    case 1:
                                        nad.Balance = o.ToObject<double>();
                                        break;
                                    case 2:
                                        if (cnt == 4)
                                        {
                                            nad.Name = o.ToObject<string>();
                                        }
                                        else
                                        {
                                            nad.tokenAccounts = parseTokenAccounts(o);
                                        }
                                        break;
                                    case 3:
                                        nad.tokenAccounts = parseTokenAccounts(o);
                                        break;
                                }

                                i++;
                            }

                            await LoadAccountObject(nad.Address, nad.Name, nad.Balance, nad.tokenAccounts);
                        }
                        catch (Exception ex)
                        {
                            log.Error("Exeption in List Accounts details", ex);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Exeption in ListAccounts", ex);
            }
        }

        /// <summary>
        /// This will load just the details to already added addresses in the Account Dictionary
        /// This communicates just with neblio API and it does not need QT Wallet and RPC
        /// </summary>
        /// <param name="withTx"></param>
        /// <returns></returns>
        private async Task LoadAccountsFromAPI(bool withTx = false)
        {
            foreach(var a in Accounts)
            {
                // run without await
                await LoadAccountObject(a.Value.Address, a.Value.Name, (double)a.Value.TotalBalance, null);
            }
        }

        public override async Task<IDictionary<string, IAccount>> ListAccounts(bool useRPC = true, bool withTx = false)
        {
            TotalBalance = 0;

            try
            {
                //if (useRPC)
                //    await ListAccountWithRPC(withTx);
                //else
                    await LoadAccountsFromAPI(withTx);

                //await CheckNewTxWaitingForDetails();
            
            }
            catch (Exception ex)
            {
                log.Error("Exeption in ListAccounts", ex);
            }

            return Accounts;
        }
        public override void RegisterAccountEvents(string address)
        {
            if (Accounts.TryGetValue(address, out var a))
            {
                if (a != null)
                {
                    a.ConfirmedTransaction += A_ConfirmedTransaction;
                    a.TxDetailsLoaded += A_TxDetailsLoaded;
                    a.DetailsLoaded += A_DetailsLoaded;
                }
            }
        }

        private void A_DetailsLoaded(object sender, IAccount e)
        {
            //Console.WriteLine("Account details loaded!");
        }

        private void A_TxDetailsLoaded(object sender, NewTransactionDTO dto)
        {
            NewTransaction?.Invoke(this, dto);
        }

        private void A_ConfirmedTransaction(object sender, NewTransactionDTO dto)
        {
            NewConfirmedTransactionDetailsReceived?.Invoke(this, dto);
        }

        // todo - find just in the already loaded tx in all accounts
        public override async Task<ITransaction> GetTxDetails(string txid)
        {
            var tx = await NeblioTransactionHelpers.TransactionInfoAsync(client, TransactionTypes.Neblio, txid);
            return tx;
        }

        private async Task<IAccount> GetTransactionsDetails(string accountAddr, GetAddressResponse accnt, IAccount acc)
        {
            foreach (var item in accnt.Transactions)
            {
                //Console.WriteLine($"Transaction: {item}");

                var tr = await NeblioTransactionHelpers.TransactionInfoAsync(client, TransactionTypes.Neblio, item, acc.Address);

                if (tr != null)
                {
                    acc.Transactions.TryAdd(tr.TxId, tr);
                }
            }

            return acc;
        }
    }
}
