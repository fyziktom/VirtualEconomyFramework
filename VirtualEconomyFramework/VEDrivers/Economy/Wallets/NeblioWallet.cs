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
                            var addrinfo = await WalletInfoAsync(client, address);

                            a.TotalBalance = addrinfo.Balance;
                            a.TotalUnconfirmedBalance = addrinfo.UnconfirmedBalance;

                            TotalBalance += a.TotalBalance;

                            if (addrinfo.Transactions?.Count > a.NumberOfTransaction)
                            {
                                for (int k = (int)(a.NumberOfTransaction); k < addrinfo.Transactions.Count; k++)
                                {
                                    var dto = new NewTransactionDTO();
                                    dto.AccountAddress = a.Address;
                                    dto.WalletName = Name;
                                    dto.Type = TransactionTypes.Neblio;
                                    dto.TxId = addrinfo.Transactions.ToArray()[k];
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
                                    {
                                        if (Accounts.TryGetValue(dto.AccountAddress, out var acc))
                                        {
                                            acc.Transactions.TryRemove(dto.TxId, out var to);
                                            acc.Transactions.TryAdd(dto.TxId, dto.TransactionDetails);
                                        }
                                        Transactions.TryRemove(dto.TxId, out var t);
                                        Transactions.TryAdd(dto.TxId, dto.TransactionDetails);

                                        NewTransactionDetailsReceived?.Invoke(this, dto);
                                    }
                                    else
                                    {
                                        NewWaitingTxForDetails.TryAdd(dto.TxId, (false, a.Address));
                                    }

                                    a.NumberOfTransaction++;// = addrinfo.Transactions.Count;
                                }
                            }


                            if (tokacc != null)
                            {
                                a.Tokens.Clear();
                                foreach (var t in tokacc)
                                {
                                    var tdetails = await NeblioTransactionHelpers.TokenMetadataAsync(client, TokenTypes.NTP1, t.TokenId, string.Empty);
                                    tdetails.ActualBalance = t.Balance;
                                    a.Tokens.Add(t.TokenId, tdetails);
                                }
                            }
                            else
                            {
                                var tokens = new Dictionary<string, IToken>();

                                string url = "https://explorer.nebl.io/address/" + a.Address; //loading actual state of tokens from explorer
                                var webGet = new HtmlWeb();
                                var document = webGet.Load(url);

                                HtmlNodeCollection tl = document.DocumentNode.SelectNodes("//a");
                                foreach (HtmlAgilityPack.HtmlNode node in tl)
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
                                                    var bal = Convert.ToInt32(balanceLine.InnerText);

                                                    var tdetails = await NeblioTransactionHelpers.TokenMetadataAsync(client, TokenTypes.NTP1, tokid, string.Empty);
                                                    tdetails.ActualBalance = bal;
                                                    tokens.Add(tokid, tdetails);
                                                }
                                            }
                                        }
                                    }
                                }
                                /*
                                 * This part calculate available tokens from address tx. But does not work. It does not count if tokens are send to another addres after tx
                                if (a.NumberOfTransaction == addrinfo.Transactions.Count)
                                {
                                    // calculate how many tokens are on the account from all account transactions
                                    var tokens = new Dictionary<string, IToken>();

                                    foreach (var tx in a.Transactions.Values)
                                    {
                                        if (tx.Direction == TransactionDirection.Incoming)
                                        {
                                            if (tx.VoutTokens?.Count > 0)
                                            {
                                                var intoken = tx.VoutTokens.First();
                                                if (tokens.TryGetValue(intoken.Symbol, out var tok))
                                                {
                                                    tok.ActualBalance += intoken.ActualBalance;
                                                }
                                                else
                                                {
                                                    //var tk = TokenFactory.GetToken(TokenTypes.NTP1, intoken.Name, intoken.Symbol, (double)intoken.ActualBalance);
                                                    var tdetails = await NeblioTransactionHelpers.TokenMetadataAsync(client, TokenTypes.NTP1, intoken.Id, string.Empty);
                                                    tdetails.ActualBalance = intoken.ActualBalance;
                                                    tokens.Add(intoken.Symbol, tdetails);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (tx.VoutTokens?.Count > 0)
                                            {
                                                var outtoken = tx.VoutTokens.First();
                                                if (tokens.TryGetValue(outtoken.Symbol, out var tok))
                                                {
                                                    tok.ActualBalance = outtoken.ActualBalance;
                                                }
                                                else
                                                {
                                                    //var tk = TokenFactory.GetToken(TokenTypes.NTP1, outtoken.Name, outtoken.Symbol, -(double)outtoken.ActualBalance);
                                                    var tdetails = await NeblioTransactionHelpers.TokenMetadataAsync(client, TokenTypes.NTP1, outtoken.Id, string.Empty);
                                                    tdetails.ActualBalance = outtoken.ActualBalance;
                                                    tokens.Add(outtoken.Symbol, tdetails);
                                                }
                                            }
                                        }
                                    }
                                    */
                                    a.Tokens.Clear();
                                    foreach (var t in tokens)
                                    {
                                        a.Tokens.Add(t.Key, t.Value);
                                    }
                            }
                            
                            /*
                            if (withTx)
                            {
                                a.Transactions.Clear();
                                a = await GetTransactionsDetails(address, addrinfo, a);
                            }*/
                        }
                    }
                    ////////////////////////////////////////////////////
                }
            }
        }

        private async Task ListAccountWithRPC(bool withTx = false)
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

                            await LoadAccountObject(nad.Address, nad.Name, nad.Balance, nad.tokenAccounts);
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
        }

        private async Task LoadAccountsFromAPI(bool withTx = false)
        {
            foreach(var a in Accounts)
            {
                await LoadAccountObject(a.Value.Address, a.Value.Name, (double)a.Value.TotalBalance, null);
            }
        }

        public override async Task<IDictionary<string, IAccount>> ListAccounts(bool useRPC = true, bool withTx = false)
        {
            if (withTx)
                Transactions.Clear();

            TotalBalance = 0;

            try
            {
                if (useRPC)
                    await ListAccountWithRPC(withTx);
                else
                    await LoadAccountsFromAPI(withTx);

                //await CheckNewTxWaitingForDetails();
            
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

                            // add to tx dictionaries
                            if (Accounts.TryGetValue(dto.AccountAddress, out var acc))
                            {
                                acc.Transactions.TryRemove(dto.TxId, out var to);
                                acc.Transactions.TryAdd(tx.Key, txd);
                            }
                            Transactions.TryRemove(dto.TxId, out var t);
                            Transactions.TryAdd(tx.Key, txd);
                        }
                        else if (txd.Confirmations == 0 && !tx.Value.Item1)
                        {
                            NewTransaction?.Invoke(this, dto);
                            NewWaitingTxForDetails.TryRemove(tx);
                            NewWaitingTxForDetails.TryAdd(tx.Key, (true, tx.Value.Item2));

                            // add to tx dictionaries
                            if (Accounts.TryGetValue(dto.AccountAddress, out var acc))
                            {
                                acc.Transactions.TryRemove(dto.TxId, out var to);
                                acc.Transactions.TryAdd(tx.Key, txd);
                            }
                            Transactions.TryRemove(dto.TxId, out var t);
                            Transactions.TryAdd(tx.Key, txd);
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
