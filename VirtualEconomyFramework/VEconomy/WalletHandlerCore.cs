using log4net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VEconomy.Common;
using VEDrivers.Common;
using VEDrivers.Database;
using VEDrivers.Economy;
using VEDrivers.Economy.Persons;
using VEDrivers.Economy.Transactions;
using VEDrivers.Economy.Wallets;
using VEDrivers.Nodes;

namespace VEconomy
{
    public class WalletHandlerCore : BackgroundService
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IConfiguration settings;
        private IHostApplicationLifetime lifetime;

        public WalletHandlerCore(IConfiguration settings, IHostApplicationLifetime lifetime)
        {
            this.settings = settings; //startup configuration in appsettings.json
            this.lifetime = lifetime;

            EconomyMainContext.WorkWithQTRPC = Convert.ToBoolean(settings.GetValue<bool>("UseRPC"));

            EconomyMainContext.WorkWithDb = Convert.ToBoolean(settings.GetValue<bool>("UseDatabase"));
            if (EconomyMainContext.WorkWithDb)
            {
                var constr = settings["ConnectionStrings:VEFrameworkDb"];  //we are not using DI for DbContext
                if (!string.IsNullOrEmpty(constr))
                {
                    DbEconomyContext.ConnectString = constr;
                    EconomyMainContext.DbService = new DbConnectorService();
                }
                else
                {
                    log.Error("Cannot connect to Db without connection string!");
                    Console.WriteLine("Cannot connect to Db without connection string!");
                    EconomyMainContext.WorkWithDb = false;
                }
            }

            settings.GetSection("QTRPC").Bind(EconomyMainContext.QTRPConfig);
            if (EconomyMainContext.QTRPConfig != null)
            {
                EconomyMainContext.QTRPCClient = new QTWalletRPCClient(EconomyMainContext.QTRPConfig);
                NeblioTransactionHelpers.qtRPCClient = new QTWalletRPCClient(EconomyMainContext.QTRPConfig);
            }

            // fill default Cryptocurrency, Owner and Wallet
            try { 
                EconomyMainContext.Cryptocurrencies.TryAdd("Neblio", new NeblioCryptocurrency());
            }
            catch(Exception ex)
            {
                log.Error("Cannot get details about cryptocurrency, please check the internet connection or firewall!");
            }

            var owner = new Owner() { Id = Guid.NewGuid(), Name = "John", SurName = "Doe" };
            EconomyMainContext.Owners.TryAdd("Default", owner);

            // load data from database
            // load or create default wallet if db is not avaiable
            if (EconomyMainContext.WorkWithDb)
            {
                if (!MainDataContext.WalletHandler.LoadWalletsFromDb())
                    MainDataContext.WalletHandler.UpdateWallet(Guid.NewGuid(), owner.Id, "NeblioWallet", WalletTypes.Neblio, "127.0.0.1", 6326).GetAwaiter().GetResult();

                if (!MainDataContext.NodeHandler.LoadNodesFromDb())
                    Console.WriteLine("No nodes in Db, continue with empty list of nodes");
            }
            else
            {
                var acc = new List<string>();
                settings.GetSection("Accounts").Bind(acc);
                if (acc != null)
                    EconomyMainContext.AccountsFromConfig = acc;

                var uid = Guid.NewGuid();
                MainDataContext.WalletHandler.UpdateWallet(uid, owner.Id, "NeblioWallet", WalletTypes.Neblio, "127.0.0.1", 6326).GetAwaiter().GetResult();
                if (EconomyMainContext.Wallets.TryGetValue(uid.ToString(), out var wallet))
                {
                    foreach(var a in EconomyMainContext.AccountsFromConfig)
                    {
                        MainDataContext.AccountHandler.UpdateAccount(a, uid, AccountTypes.Neblio, a).GetAwaiter().GetResult();
                    }
                }
            }

            if (!MainDataContext.WalletHandler.ReloadAccounts())
                Console.WriteLine("Cannot reload accounts");
        }

        protected override async Task ExecuteAsync(CancellationToken stopToken)
        {
            try
            {
                if (EconomyMainContext.WorkWithQTRPC)
                {
                    EconomyMainContext.QTRPCClient.InitClients();
                    NeblioTransactionHelpers.qtRPCClient.InitClients();
                }
            }
            catch(Exception ex)
            {
                log.Error("Cannot init QTRPC Client! Please check settings in appsetting.json", ex);
            }

            bool start = true;

            await Task.Delay(1);
            try
            {
                _ = Task.Run(async () =>
                {
                    while (!stopToken.IsCancellationRequested)
                    {
                        try
                        {
                            // first wait until MQTT client exists and it is connected to the broker
                            if (EconomyMainContext.MQTTClient != null)
                            {
                                if (start && EconomyMainContext.MQTTClient.IsConnected)
                                {
                                    await EconomyMainContext.MQTTClient.PostObjectAsJSONString("VEF/Start", "Virtual Economy Framework started");
                                    start = false;
                                }
                                else if (!start && EconomyMainContext.MQTTClient.IsConnected)
                                {
                                    // wait for specified time - time interval you can set i appsetting.json
                                    await Task.Delay(EconomyMainContext.WalletRefreshInterval);

                                    // this will refresh all wallets data without loading transaction details
                                    await MainDataContext.WalletHandler.RefreshWallets();

                                    if (!MainDataContext.WalletHandler.ReloadAccounts())
                                        Console.WriteLine("Cannot reload accounts");

                                    //these commands publish main dictionaries to MQTT each interval
                                    await EconomyMainContext.MQTTClient.PostObjectAsJSON<IDictionary<string, IWallet>>(
                                                                        "VEF/Wallets", EconomyMainContext.Wallets);

                                    await EconomyMainContext.MQTTClient.PostObjectAsJSON<IDictionary<string, IAccount>>(
                                                                        "VEF/Accounts", EconomyMainContext.Accounts);

                                    await EconomyMainContext.MQTTClient.PostObjectAsJSON<IDictionary<string, INode>>(
                                                                       "VEF/Nodes", EconomyMainContext.Nodes);

                                    await EconomyMainContext.MQTTClient.PostObjectAsJSON<IDictionary<string, ICryptocurrency>>(
                                                                        "VEF/Cryptocurrencies", EconomyMainContext.Cryptocurrencies);

                                    //await MainDataContext.MQTTClient.PostObjectAsJSON<IDictionary<string, IOwner>>(
                                    //                                    "VEF/Owners", MainDataContext.Owners);
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            log.Error("Error occured in WalletHandlerCore.", ex);
                            log.Info("Trying again within 1s.");
                            await Task.Delay(1000);
                        }
                    }
                    log.Info($"Virtual Economy Framework wallet handler task stopped");
                });

            }
            catch (Exception ex)
            {
                log.Fatal("Cannot start Virtual Economy server wallet handler", ex);
                lifetime.StopApplication();
            }

        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await base.StopAsync(cancellationToken);
        }
    }
}
