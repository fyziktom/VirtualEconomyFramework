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

        public NeblioWallet(Guid id, Guid owner, string name, string urlbase = "127.0.0.1", int port = 6326)
        {
            Owner = owner;
            Name = name;
            ConnectionUrlBaseAddress = urlbase;
            ConnectionPort = port;
            Id = id;
            rpcClient = new QTWalletRPCClient(ConnectionUrlBaseAddress, ConnectionPort);
            rpcClient.InitClients();
            Type = WalletTypes.Neblio;

            Accounts = new ConcurrentDictionary<string, IAccount>();

            Transactions = new ConcurrentDictionary<string, ITransaction>();

            NewWaitingTxForDetails = new ConcurrentDictionary<string, (bool, string)>();

            client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
        }

        public override event EventHandler<NewTransactionDTO> NewTransaction;
        public override event EventHandler<NewTransactionDTO> NewTransactionDetailsReceived;

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

        public override async Task<IDictionary<string, IAccount>> ListAccounts(bool withTx = false)
        {
            var res = "ERROR";
            var accounts = new List<NeblioQTWalletAccountDetails>();
            var addrInfos = new List<GetAddressResponse>();

            if (withTx)
                Transactions.Clear();

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

                            if (!string.IsNullOrEmpty(nad.Address))
                            {
                                if (!string.IsNullOrEmpty(nad.Name) || (nad.Balance != 0))
                                {
                                    if (Accounts.TryGetValue(nad.Address, out var a))
                                    {
                                        if (a != null)
                                        {
                                            var addrinfo = await WalletInfoAsync(client, nad.Address);

                                            a.TotalBalance = addrinfo.Balance;
                                            a.TotalUnconfirmedBalance = addrinfo.UnconfirmedBalance;

                                            TotalBalance += a.TotalBalance;

                                            if (addrinfo.Transactions.Count > a.NumberOfTransaction)
                                            {
                                                var dto = new NewTransactionDTO();
                                                dto.AccountAddress = a.Address;
                                                dto.WalletName = Name;
                                                dto.Type = TransactionTypes.Neblio;
                                                dto.TxId = addrinfo.Transactions?.ToList()?.Last();
                                                dto.OwnerId = Owner;

                                                var txdetailsrcvd = false;
                                                try
                                                {
                                                    dto.TransactionDetails = NeblioTransactionHelpers.TransactionInfo(dto.Type, dto.TxId, null);

                                                    if (dto.TransactionDetails != null)
                                                        if (dto.TransactionDetails.Confirmations > 0)
                                                            txdetailsrcvd = true;
                                                }
                                                catch (Exception ex)
                                                {
                                                    log.Error("Neblio Wallet cannot load tx details: ", ex);
                                                }

                                                if (dto.TransactionDetails != null)
                                                    NewTransaction?.Invoke(this, dto);

                                                //send after message about new tx
                                                if (txdetailsrcvd)
                                                    NewTransactionDetailsReceived?.Invoke(this, dto);
                                                else
                                                    NewWaitingTxForDetails.TryAdd(dto.TxId, (false, a.Address));

                                                a.NumberOfTransaction = addrinfo.Transactions.Count;
                                            }
                                            a.Tokens.Clear();
                                            foreach (var t in nad.tokenAccounts)
                                            {
                                                var tdetails = await NeblioTransactionHelpers.TokenMetadataAsync(client, TokenTypes.NTP1, t.TokenId);
                                                tdetails.ActualBalance = t.Balance;
                                                a.Tokens.Add(t.TokenId, tdetails);
                                            }

                                            if (withTx)
                                            {
                                                a.Transactions.Clear();
                                                a = await GetTransactionsDetails(nad.Address, addrinfo, a);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        /*
                                        var acc = AccountFactory.GetAccount(Guid.Empty, AccountTypes.Neblio, Owner, Id, nad.Name, nad.Address, nad.Balance);

                                        foreach (var t in nad.tokenAccounts)
                                        {
                                            acc.Tokens.Add(t.Address, TokenFactory.GetToken(TokenTypes.NTP1, string.Empty, t.Symbol, t.Balance));
                                        }

                                        var addrinfo = await WalletInfoAsync(client, nad.Address);

                                        acc.TotalBalance = addrinfo.Balance;
                                        acc.TotalUnconfirmedBalance = addrinfo.UnconfirmedBalance;

                                        //a.NumberOfTransaction = addrinfo.Transactions.Count();

                                        if (withTx)
                                        {
                                            acc = await GetTransactionsDetails(nad.Address, addrinfo, acc);
                                        }

                                        Accounts.TryAdd(nad.Address, acc);
                                        */
                                    }

                                }
                            }
                        }
                        catch (Exception ex) 
                        {
                            log.Error("Exeption in List Accounts details", ex);
                        }
                    }
                }

                await CheckNewTxWaitingForDetails();
            }
            catch (Exception ex)
            {
                log.Error("Exeption in ListAccounts", ex);
            }

            return Accounts;
        }

        private async Task CheckNewTxWaitingForDetails()
        {
            if (NewWaitingTxForDetails.Count == 0)
                return;

            Console.WriteLine("Checking New Tx waiting for details...");

            foreach (var tx in NewWaitingTxForDetails)
            {
                try
                {
                    var txd = await NeblioTransactionHelpers.TransactionInfoAsync(client, TransactionTypes.Neblio, tx.Key);

                    if (txd != null)
                    {
                        var dto = new NewTransactionDTO()
                        {
                            AccountAddress = tx.Value.Item2,
                            WalletName = Name,
                            Type = TransactionTypes.Neblio,
                            TxId = tx.Key,
                            OwnerId = Owner,
                            TransactionDetails = txd
                        };

                        if (txd.Confirmations > 0)
                        {
                            NewTransactionDetailsReceived?.Invoke(this, dto);
                            NewWaitingTxForDetails.TryRemove(tx);
                        }
                        else if (txd.Confirmations == 0 && !tx.Value.Item1)
                        {
                            NewTransaction?.Invoke(this, dto);
                            NewWaitingTxForDetails.TryRemove(tx);
                            NewWaitingTxForDetails.TryAdd(tx.Key, (true, tx.Value.Item2));
                        }
                    }
                }
                catch(Exception ex)
                {
                    log.Error("Cannot load new tx details.", ex);
                }
            }
        }

        public override async Task<ITransaction> GetTxDetails(string txid)
        {
            var tx = await NeblioTransactionHelpers.TransactionInfoAsync(client, TransactionTypes.Neblio, txid);
            return tx;
        }

        public GetAddressResponse WalletInfo(string addr, object obj)
        {
            var addrinfo = WalletInfoAsync(client, addr);
            return addrinfo.GetAwaiter().GetResult();
        }

        //todo move to transaction helpers and preprocess data to get just IAccount
        private async Task<GetAddressResponse> WalletInfoAsync(IClient client, string addr)
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

        private async Task<IAccount> GetTransactionsDetails(string accountAddr, GetAddressResponse accnt, IAccount acc)
        {
            foreach (var item in accnt.Transactions)
            {
                //Console.WriteLine($"Transaction: {item}");

                var tr = await NeblioTransactionHelpers.TransactionInfoAsync(client, TransactionTypes.Neblio, item);

                if (tr != null)
                {
                    acc.Transactions.TryAdd(tr.TxId, tr);

                    Transactions.TryAdd(item, tr);
                }
            }

            return acc;
        }
    }
}
