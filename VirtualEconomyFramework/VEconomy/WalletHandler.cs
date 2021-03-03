using log4net;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using VEDrivers.Database;
using VEDrivers.Economy.DTO;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Transactions;
using VEDrivers.Economy.Wallets;
using VEDrivers.Nodes.Dto;

namespace VEconomy
{
    public static class WalletHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// After start all tx are new in accounts. Thats why it is ignored after start until first load of accounts of all wallets is done
        /// TODO: not good, need to be changed because of Tx which came during the system down, etc.
        /// </summary>
        private static bool firstLoadAfterStart = true;

        public static async Task<string> UpdateWallet(Guid id, Guid ownerid, string walletName, WalletTypes type, string urlBase, int port)
        {
            IDbConnectorService dbservice = new DbConnectorService();

            if (MainDataContext.Wallets.TryGetValue(id.ToString(), out var wallet))
            {
                wallet.Name = walletName;
                if (ownerid != Guid.Empty)
                    wallet.Owner = ownerid;
                wallet.Type = type;
                wallet.BaseURL = urlBase;
                wallet.ConnectionPort = port;
                Console.WriteLine($"New wallet connection address: {wallet.ConnectionAddress}");

                if (MainDataContext.WorkWithDb)
                {
                    if (!dbservice.SaveWallet(wallet))
                    {
                        Console.WriteLine("Cannot save Node to the db!");
                        return "Cannot save Node to the db!";
                    }
                }

                return "OK";
            }
            else
            {
                if (string.IsNullOrEmpty(id.ToString()))
                {
                    id = Guid.NewGuid();
                }

                if (string.IsNullOrEmpty(ownerid.ToString()))
                {
                    ownerid = Guid.NewGuid();
                }

                var wall = WalletFactory.GetWallet(id, ownerid, type, walletName, urlBase, port);

                if (wall != null)
                {
                    wall.NewTransaction += Wall_NewTransaction;
                    wall.NewTransactionDetailsReceived += WalletHandler_NewTransactionDetailsReceived;

                    MainDataContext.Wallets.Add(wall.Id.ToString(), wall);

                    if (MainDataContext.WorkWithDb)
                    {
                        if (!dbservice.SaveWallet(wall))
                        {
                            log.Error("Cannot save wallet to the Db!");
                            return "Cannot save wallet to the Db!";
                        }
                    }
                }

                return "OK";
            }
        }

        private static void WalletHandler_NewTransactionDetailsReceived(object sender, NewTransactionDTO data)
        {
            if (firstLoadAfterStart)
                return;

            var tokenReceived = false;
            IToken token = null;

            if (data.TransactionDetails.Confirmations > 0)
            {
                if (data.TransactionDetails.VinTokens.Count > 0)
                {
                    token = data.TransactionDetails?.VoutTokens?.FirstOrDefault();
                    if (token != null)
                        tokenReceived = true;
                }

                try
                {
                    if (MainDataContext.MQTTClient.IsConnected)
                    {
                        MainDataContext.MQTTClient.PostObjectAsJSON<NewTransactionDTO>(
                            $"VEF/NewTransactionWithDetails",
                            data, false).GetAwaiter().GetResult();


                        if (tokenReceived && token != null)
                        {
                            var o = new
                            {
                                data.AccountAddress,
                                data.WalletName,
                                data.Type,
                                data.TransactionDetails.Direction,
                                token,
                                data
                            };

                            MainDataContext.MQTTClient.PostObjectAsJSON<object>(
                                $"VEF/TokensReceived",
                                o, false).GetAwaiter().GetResult();

                            // Tx token Node trigger
                            var res = string.Empty;
                            if (data.TransactionDetails.Direction == TransactionDirection.Incoming)
                                res = NodeHandler.TriggerNodesActions(NodeActionTriggerTypes.TokenTxArrived, data, o).GetAwaiter().GetResult();
                            else
                                res = NodeHandler.TriggerNodesActions(NodeActionTriggerTypes.TokenTxSent, data, o).GetAwaiter().GetResult();
                        }
                        else
                        {
                            var o = new
                            {
                                data.AccountAddress,
                                data.WalletName,
                                data.Type,
                                data.TransactionDetails.Direction,
                                data
                            };

                            // Tx Node trigger
                            var res = string.Empty;
                            if (data.TransactionDetails.Direction == TransactionDirection.Incoming)
                                res = NodeHandler.TriggerNodesActions(NodeActionTriggerTypes.NewTxArrived, data, o).GetAwaiter().GetResult();
                            else
                                res = NodeHandler.TriggerNodesActions(NodeActionTriggerTypes.TxSent, data, o).GetAwaiter().GetResult();
                        }

                    }
                }
                catch (Exception ex)
                {
                    log.Error("Wallet handler cannot send MQTT post when new tx arrived, check MQTT Broker and connection.", ex);
                }
            }
        }

        public static bool LoadWalletsFromDb()
        {
            IDbConnectorService dbservice = new DbConnectorService();

            try
            {
                var wallets = dbservice.GetWallets();
                var accounts = dbservice.GetAccounts();

                if (wallets != null && accounts != null)
                {
                    // this function will load accounts to proper wallets
                    foreach (var w in wallets)
                    {
                        w.NewTransaction += Wall_NewTransaction;
                        w.NewTransactionDetailsReceived += WalletHandler_NewTransactionDetailsReceived;
                        if (w != null)
                        {
                            foreach (var a in accounts)
                            {
                                if (a.WalletId == w.Id)
                                    w.Accounts.TryAdd(a.Address, a);
                            }
                        }
                    }
                }

                if (wallets != null)
                {
                    //refresh main wallet dictionary
                    MainDataContext.Wallets.Clear();
                    foreach (var w in wallets)
                    {
                        MainDataContext.Wallets.TryAdd(w.Id.ToString(), w);
                    }
                }

                return true;
            }
            catch(Exception ex)
            {
                log.Error("Cannot Load and pair wallets and accounts.", ex);
                return false;
            }
        }

        private static void Wall_NewTransaction(object sender, NewTransactionDTO data)
        {
            if (firstLoadAfterStart)
                return;

            var tokenReceived = false;
            IToken token = null;

            try
            {
                if (data.TransactionDetails != null)
                {
                    if (MainDataContext.MQTTClient.IsConnected)
                    {
                        MainDataContext.MQTTClient.PostObjectAsJSON<NewTransactionDTO>(
                            $"VEF/NewTransaction",
                            data, false).GetAwaiter().GetResult();
                    }

                    if (data.TransactionDetails.VinTokens.Count > 0)
                    {
                        token = data.TransactionDetails?.VoutTokens?.FirstOrDefault();
                        if (token != null)
                            tokenReceived = true;
                    }
                
                    if (tokenReceived && token != null)
                    {
                        var o = new
                        {
                            data.AccountAddress,
                            data.WalletName,
                            data.Type,
                            data.TransactionDetails.Direction,
                            token,
                            data
                        };

                        // Tx token Node trigger
                        var res = string.Empty;
                        if (data.TransactionDetails.Direction == TransactionDirection.Incoming)
                            res = NodeHandler.TriggerNodesActions(NodeActionTriggerTypes.UnconfirmedTokenTxArrived, data, o).GetAwaiter().GetResult();
                        else
                            res = NodeHandler.TriggerNodesActions(NodeActionTriggerTypes.UnconfirmedTokenTxSent, data, o).GetAwaiter().GetResult();
                    }
                    else
                    {
                        var o = new
                        {
                            data.AccountAddress,
                            data.WalletName,
                            data.Type,
                            data.TransactionDetails.Direction,
                            data
                        };

                        // Tx Node trigger
                        var res = string.Empty;
                        if (data.TransactionDetails.Direction == TransactionDirection.Incoming)
                            res = NodeHandler.TriggerNodesActions(NodeActionTriggerTypes.UnconfirmedTxArrived, data, o).GetAwaiter().GetResult();
                        else
                            res = NodeHandler.TriggerNodesActions(NodeActionTriggerTypes.UnconfirmedTxSent, data, o).GetAwaiter().GetResult();
                    }
                }
            }
            catch (Exception ex)
            {
                log.Error("Wallet handler cannot send MQTT post when new tx arrived, check MQTT Broker and connection.", ex);
            }
        }

        public static async Task RefreshWallets()
        {
            foreach(var w in MainDataContext.Wallets)
            {
                await w.Value.ListAccounts();
            }

            if (MainDataContext.Wallets.Count > 0 && firstLoadAfterStart)
                firstLoadAfterStart = false;
        }

        public static bool ReloadAccounts()
        {
            MainDataContext.Accounts.Clear();

            foreach (var w in MainDataContext.Wallets)
            {
                foreach(var a in w.Value.Accounts)
                {
                    MainDataContext.Accounts.TryAdd(a.Value.Address, a.Value);
                }
            }

            return true;
        }
    }
}
