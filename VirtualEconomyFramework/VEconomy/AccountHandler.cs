using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Database;
using VEDrivers.Economy.Wallets;

namespace VEconomy
{
    public static class AccountHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static async Task<string> UpdateAccount(string accountAddress, Guid walletId, AccountTypes type, string name, bool justInDb = true)
        {
            IDbConnectorService dbservice = new DbConnectorService();

            if (MainDataContext.Wallets.TryGetValue(walletId.ToString(), out var wallet))
            {
                if (wallet.Accounts.TryGetValue(accountAddress, out var accnt))
                {
                    accnt.Name = name;

                    if (MainDataContext.WorkWithDb)
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
                        if (MainDataContext.QTRPCClient.IsConnected)
                        {
                            // creating wallet in desktop QT Wallet

                            var accresp = new QTWalletResponseDto();

                            if (!justInDb)
                            {
                                var acc = await MainDataContext.QTRPCClient.RPCLocalCommandSplitedAsync("getnewaddress", new string[] { name });
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

                                if (MainDataContext.WorkWithDb && account != null)
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
    }
}
