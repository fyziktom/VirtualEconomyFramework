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

            MainDataContext.DbService = new DbConnectorService();

            MainDataContext.QTRPConfig = new QTRPCConfig();
            settings.GetSection("QTRPC").Bind(MainDataContext.QTRPConfig);
            MainDataContext.QTRPCClient = new QTWalletRPCClient(MainDataContext.QTRPConfig);
            NeblioTransactionHelpers.qtRPCClient = new QTWalletRPCClient(MainDataContext.QTRPConfig);

            MainDataContext.Wallets = new ConcurrentDictionary<string, IWallet>();
            MainDataContext.Accounts = new ConcurrentDictionary<string, IAccount>();
            MainDataContext.Nodes = new ConcurrentDictionary<string, INode>();
            MainDataContext.Cryptocurrencies = new ConcurrentDictionary<string, ICryptocurrency>();
            MainDataContext.Owners = new ConcurrentDictionary<string, IOwner>();

            // fill default Cryptocurrency, Owner and Wallet

            try { 
                var neblio = new NeblioCryptocurrency();
                MainDataContext.Cryptocurrencies.TryAdd("Neblio", new NeblioCryptocurrency());
            }
            catch(Exception ex)
            {
                log.Error("Cannot get details about cryptocurrency, please check the internet connection or firewall!");
            }

            var owner = new Owner() { Id = Guid.NewGuid(), Name = "John", SurName = "Doe" };
            MainDataContext.Owners.TryAdd("Default", owner);

            // load data from database
            DbEconomyContext.ConnectString = settings["ConnectionStrings:VEFrameworkDb"];  //we are not using DI for DbContext
            // load or create default wallet if db is not avaiable
            if (!WalletHandler.LoadWalletsFromDb())
                WalletHandler.UpdateWallet(Guid.NewGuid(), owner.Id, "NeblioWallet", WalletTypes.Neblio, "127.0.0.1", 6326);

            if (!NodeHandler.LoadNodesFromDb())
                Console.WriteLine("No nodes in Db, continue with empty list of nodes");

            if (!WalletHandler.ReloadAccounts())
                Console.WriteLine("Cannot reload accounts");
        }

        protected override async Task ExecuteAsync(CancellationToken stopToken)
        {
            try
            {
                MainDataContext.QTRPCClient.InitClients();
                NeblioTransactionHelpers.qtRPCClient.InitClients();
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
                            if (MainDataContext.MQTTClient != null)
                            {
                                if (start && MainDataContext.MQTTClient.IsConnected)
                                {
                                    await MainDataContext.MQTTClient.PostObjectAsJSONString("VEF/Start", "Virtual Economy Framework started");
                                    start = false;
                                }
                                else if (!start && MainDataContext.MQTTClient.IsConnected)
                                {
                                    // wait for specified time - time interval you can set i appsetting.json
                                    await Task.Delay(MainDataContext.WalletRefreshInterval);

                                    // this will refresh all wallets data without loading transaction details
                                    await WalletHandler.RefreshWallets();

                                    if (!WalletHandler.ReloadAccounts())
                                        Console.WriteLine("Cannot reload accounts");

                                    //these commands publish main dictionaries to MQTT each interval
                                    await MainDataContext.MQTTClient.PostObjectAsJSON<IDictionary<string, IWallet>>(
                                                                        "VEF/Wallets", MainDataContext.Wallets);

                                    await MainDataContext.MQTTClient.PostObjectAsJSON<IDictionary<string, IAccount>>(
                                                                        "VEF/Accounts", MainDataContext.Accounts);

                                    await MainDataContext.MQTTClient.PostObjectAsJSON<IDictionary<string, INode>>(
                                                                       "VEF/Nodes", MainDataContext.Nodes);

                                    await MainDataContext.MQTTClient.PostObjectAsJSON<IDictionary<string, ICryptocurrency>>(
                                                                        "VEF/Cryptocurrencies", MainDataContext.Cryptocurrencies);

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
