using log4net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

            // load from settings if app should start with the RPC, then it needs QT wallet
            EconomyMainContext.WorkWithQTRPC = Convert.ToBoolean(settings.GetValue<bool>("UseRPC"));

            // create folder for Accounts LastTx data
            // neblio account will store last data about processed tx.
            // this it important for recovery and start from specific tx which was last processed and not received (there can be lots of tx received during crash)
            //var loc = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            var appdataFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
            var destFolder = Path.Combine(appdataFolder, "VEFramework");
            EconomyMainContext.CurrentLocation = destFolder;
            FileHelpers.CheckOrCreateTheFolder(Path.Join(destFolder, "Accounts"));

            // this tells how many confirmations are needed for transaction to invoke confirmed event
            EconomyMainContext.NumberOfConfirmationsToAccept = settings.GetValue<int>("NumberOfConfirmationsToAccept", 1);

            // load from settings if app should start with the db
            EconomyMainContext.WorkWithDb = Convert.ToBoolean(settings.GetValue<bool>("UseDatabase"));

            EconomyMainContext.StartWithShops = Convert.ToBoolean(settings.GetValue<bool>("StartWithShops"));


            // owner are not implemented yet, so just create dummy one
            var owner = new Owner() { Id = Guid.NewGuid(), Name = "John", SurName = "Doe" };
            EconomyMainContext.Owners.TryAdd("Default", owner);
            var attempts = 60000; // 30 seconds wait for db connection then error
            // load data from database - wait for the connection - it is started with the web server so load must wait
            // load or create default wallet if db is not avaiable
            if (EconomyMainContext.WorkWithDb)
            {
                
                while (!EconomyMainContext.DbLoaded)
                {
                    Task.Delay(500).GetAwaiter().GetResult();
                    attempts--;
                    if (attempts <= 0)
                        throw new Exception("Cannot load the database withing 30 seconds even it is required. Turn off Db support in appseting.json or setup correct connection parameters!");
                }

                // try to load wallets from db. If not successfull create dummy one
                if (!MainDataContext.WalletHandler.LoadWalletsFromDb(EconomyMainContext.DbService).GetAwaiter().GetResult())
                    MainDataContext.WalletHandler.UpdateWallet(Guid.NewGuid(), owner.Id, "NeblioWallet", WalletTypes.Neblio, "127.0.0.1", 6326, EconomyMainContext.DbService).GetAwaiter().GetResult();

                // load nodes from db
                if (!MainDataContext.NodeHandler.LoadNodesFromDb(EconomyMainContext.DbService))
                    Console.WriteLine("No nodes in Db, continue with empty list of nodes");
            }
            else
            {
                // load preset list of account in setting file
                // this is used just when db is not setted up and you still need to start app with some setted accounts
                var acc = new List<string>();
                settings.GetSection("Accounts").Bind(acc);
                if (acc != null)
                    EconomyMainContext.AccountsFromConfig = acc;

                var uid = Guid.NewGuid();
                MainDataContext.WalletHandler.UpdateWallet(uid, owner.Id, "NeblioWallet", WalletTypes.Neblio, "127.0.0.1", 6326, EconomyMainContext.DbService).GetAwaiter().GetResult();
                if (EconomyMainContext.Wallets.TryGetValue(uid.ToString(), out var wallet))
                {
                    foreach (var a in EconomyMainContext.AccountsFromConfig)
                    {
                        MainDataContext.AccountHandler.UpdateAccount(a, uid, AccountTypes.Neblio, a, EconomyMainContext.DbService).GetAwaiter().GetResult();
                    }
                }
            }

            //if the RPC is setted up try to get the setting
           // if (EconomyMainContext.WorkWithQTRPC)
            //{
                settings.GetSection("QTRPC").Bind(EconomyMainContext.QTRPConfig);
                if (EconomyMainContext.QTRPConfig != null)
                {
                    EconomyMainContext.QTRPCClient = new QTWalletRPCClient(EconomyMainContext.QTRPConfig);
                    NeblioTransactionHelpers.qtRPCClient = new QTWalletRPCClient(EconomyMainContext.QTRPConfig);
                }
            //}

            // fill default Cryptocurrency
            try
            {
                EconomyMainContext.Cryptocurrencies.TryAdd("Neblio", new NeblioCryptocurrency());
            }
            catch (Exception ex)
            {
                log.Error("Cannot get details about cryptocurrency, please check the internet connection or firewall!");
            }

            // if the app should run the browser automaticaly load and set
            EconomyMainContext.StartBrowserAtStart = settings.GetValue<bool>("StartBrowserAtStart");

            // load main port of the app, default is 8080
            var mainport = settings.GetValue<int>("MainPort", 0);
            if (mainport != 0)
            {
                EconomyMainContext.MainPort = mainport;
            }

        }

        protected override async Task ExecuteAsync(CancellationToken stopToken)
        {
            bool start = true;

            await Task.Delay(1);

            if (!MainDataContext.WalletHandler.ReloadAccounts())
                Console.WriteLine("Cannot reload accounts");

            if (EconomyMainContext.StartBrowserAtStart)
                BrowserHelpers.OpenBrowser($"http://localhost:{EconomyMainContext.MainPort}/");

            try
            {
                if (EconomyMainContext.WorkWithQTRPC)
                {
                    EconomyMainContext.QTRPCClient.InitClients();
                    NeblioTransactionHelpers.qtRPCClient.InitClients();
                }
            }
            catch (Exception ex)
            {
                log.Error("Cannot init QTRPC Client! Please check settings in appsetting.json", ex);
            }

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
                                while (!EconomyMainContext.MQTTClient.IsConnected && !stopToken.IsCancellationRequested)
                                {
                                    // wait until client is started and connected to broker
                                    await Task.Delay(500);
                                }

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
