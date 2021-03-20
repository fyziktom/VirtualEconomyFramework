using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Database;
using VEDrivers.Economy.DTO;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Transactions;
using VEDrivers.Nodes.Dto;
using VEDrivers.Nodes.Handlers;

namespace VEDrivers.Economy.Wallets.Handlers
{
    public class BasicWalletHandler : CommonWalletHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static BasicNodeHandler NodeHandler = new BasicNodeHandler();
        /// <summary>
        /// After start all tx are new in accounts. Thats why it is ignored after start until first load of accounts of all wallets is done
        /// TODO: not good, need to be changed because of Tx which came during the system down, etc.
        /// </summary>
        private bool firstLoadAfterStart = true;

        public override async Task<string> UpdateWallet(Guid id, Guid ownerid, string walletName, WalletTypes type, string urlBase, int port)
        {
            IDbConnectorService dbservice = new DbConnectorService();

            if (EconomyMainContext.Wallets.TryGetValue(id.ToString(), out var wallet))
            {
                wallet.Name = walletName;
                if (ownerid != Guid.Empty)
                    wallet.Owner = ownerid;
                wallet.Type = type;
                wallet.BaseURL = urlBase;
                wallet.ConnectionPort = port;
                Console.WriteLine($"New wallet connection address: {wallet.ConnectionAddress}");

                if (EconomyMainContext.WorkWithDb)
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

                var wall = WalletFactory.GetWallet(id, ownerid, type, walletName, EconomyMainContext.WorkWithQTRPC, urlBase, port);

                if (wall != null)
                {
                    wall.NewTransaction += Wall_NewTransaction;
                    wall.NewTransactionDetailsReceived += WalletHandler_NewTransactionDetailsReceived;

                    EconomyMainContext.Wallets.Add(wall.Id.ToString(), wall);

                    if (EconomyMainContext.WorkWithDb)
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

        private void WalletHandler_NewTransactionDetailsReceived(object sender, NewTransactionDTO data)
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
                    if (EconomyMainContext.MQTTClient.IsConnected)
                    {
                        EconomyMainContext.MQTTClient.PostObjectAsJSON<NewTransactionDTO>(
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

                            EconomyMainContext.MQTTClient.PostObjectAsJSON<object>(
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

        public override bool LoadWalletsFromDb()
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
                    EconomyMainContext.Wallets.Clear();
                    foreach (var w in wallets)
                    {
                        EconomyMainContext.Wallets.TryAdd(w.Id.ToString(), w);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                log.Error("Cannot Load and pair wallets and accounts.", ex);
                return false;
            }
        }

        private void Wall_NewTransaction(object sender, NewTransactionDTO data)
        {
            if (firstLoadAfterStart)
                return;

            var tokenReceived = false;
            IToken token = null;

            try
            {
                if (data.TransactionDetails != null)
                {
                    if (EconomyMainContext.MQTTClient.IsConnected)
                    {
                        EconomyMainContext.MQTTClient.PostObjectAsJSON<NewTransactionDTO>(
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

        public override async Task RefreshWallets()
        {
            foreach (var w in EconomyMainContext.Wallets)
            {
                await w.Value.ListAccounts(EconomyMainContext.WorkWithQTRPC);
            }

            if (EconomyMainContext.Wallets.Count > 0 && firstLoadAfterStart)
                firstLoadAfterStart = false;
        }

        public override bool ReloadAccounts()
        {
            EconomyMainContext.Accounts.Clear();

            foreach (var w in EconomyMainContext.Wallets)
            {
                foreach (var a in w.Value.Accounts)
                {
                    EconomyMainContext.Accounts.TryAdd(a.Value.Address, a.Value);
                }
            }

            return true;
        }
    }
}
