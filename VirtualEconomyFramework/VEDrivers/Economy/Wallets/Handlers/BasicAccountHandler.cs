using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Database;
using VEDrivers.Economy.Tokens;

namespace VEDrivers.Economy.Wallets.Handlers
{
    public class BasicAccountHandler : CommonAccountHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public override async Task<string> UpdateAccount(string accountAddress, Guid walletId, AccountTypes type, string name, bool justInDb = true)
        {
            IDbConnectorService dbservice = new DbConnectorService();

            if (EconomyMainContext.Wallets.TryGetValue(walletId.ToString(), out var wallet))
            {
                if (wallet.Accounts.TryGetValue(accountAddress, out var accnt))
                {
                    accnt.Name = name;

                    if (EconomyMainContext.WorkWithDb)
                    {
                        if (!dbservice.SaveAccount(accnt))
                            return "Cannot save account to the Db!";
                    }

                    return "OK";
                }
                else
                {
                    Console.WriteLine("Account not found in actual wallet Accounts. New account will be created.");
                    // cannot be the same account name in one wallet
                    if (wallet.Accounts.Values.FirstOrDefault(a => a.Name == name) == null)
                    {
                        if (EconomyMainContext.WorkWithQTRPC)
                        {
                            if (EconomyMainContext.QTRPCClient.IsConnected)
                            {
                                // creating wallet in desktop QT Wallet

                                var accresp = new QTWalletResponseDto();

                                if (!justInDb)
                                {
                                    var acc = await EconomyMainContext.QTRPCClient.RPCLocalCommandSplitedAsync("getnewaddress", new string[] { name });
                                    accresp = JsonConvert.DeserializeObject<QTWalletResponseDto>(acc);
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(accountAddress))
                                        accresp = new QTWalletResponseDto() { result = accountAddress };
                                    else
                                        accresp = new QTWalletResponseDto() { result = "No Address Filled" };
                                }

                                // if success add to list of Accounts of specified wallet
                                if (accresp != null)
                                {
                                    var account = AccountFactory.GetAccount(Guid.Empty, type, wallet.Owner, walletId, name, accresp.result, 0);

                                    wallet.Accounts.TryAdd(account.Address, account);

                                    if (EconomyMainContext.WorkWithDb && account != null)
                                    {
                                        if (!dbservice.SaveAccount(account))
                                            return "Cannot save new account to the Db!";
                                    }

                                    return "OK";
                                }
                                else
                                {
                                    log.Error("Cannot create account - cannot get correct response from QTWalletRPC!");
                                    return "Cannot create account - cannot get correct response from QTWalletRPC!";
                                }
                            }
                            else
                            {
                                log.Error("Cannot create account - RPC is not connected, probably not configured!");
                                return "Cannot create account - RPC is not connected, probably not configured!";
                            }
                        }
                        else
                        {
                            // if not work with RPC you must fill address
                            if (!string.IsNullOrEmpty(accountAddress))
                            {
                                var account = AccountFactory.GetAccount(Guid.Empty, type, wallet.Owner, walletId, name, accountAddress, 0);
                                wallet.Accounts.TryAdd(account.Address, account);
                                return "OK";
                            }
                            else
                            {
                                log.Error("Cannot create account - RPC is disabled and accountAddress is empty!");
                                return "Cannot create account - RPC is disabled and accountAddress is empty!";
                            }
                        }
                    }
                    else
                    {

                        log.Error("Cannot create account - Name already exists!");
                        return "Cannot create account - Name already exists!";
                    }
                }
            }
            else
            {
                log.Error("Cannot create account - wallet not found");
                return "Cannot create account - wallet not found";
            }
        }
        public override IDictionary<string, IToken> FindTokenByMetadata(string account, string key, string value = "")
        {
            var result = new Dictionary<string, IToken>();
            try
            {
                if (EconomyMainContext.Accounts.TryGetValue(account, out var acc))
                {
                    foreach(var t in acc.Transactions)
                    {
                        var tx = t.Value;
                        if(tx.VinTokens != null)
                        {
                            var found = false;

                            foreach (var tok in tx.VinTokens)
                            {
                                if (tok.Metadata.TryGetValue(key, out var v))
                                {
                                    result.Add(t.Key, tok);
                                    found = true;
                                }
                            }

                            if (!found)
                            {
                                foreach (var tok in tx.VoutTokens)
                                {
                                    if (tok.Metadata.TryGetValue(key, out var v))
                                    {
                                        result.Add(t.Key, tok);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                log.Error("Cannot find token by metadata", ex);
            }

            return result;
        }
        
    }
}
