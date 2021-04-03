using Binance.Net;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Database;
using VEDrivers.Economy;
using VEDrivers.Economy.Exchanges;
using VEDrivers.Economy.Persons;
using VEDrivers.Economy.Tokens;
using VEDrivers.Economy.Wallets;
using VEDrivers.Nodes;

namespace VEDrivers.Common
{
    public static class EconomyMainContext
    {
        /// <summary>
        /// Application currecnt location, means where exe or dll is located
        /// </summary>
        public static string CurrentLocation { get; set; } = string.Empty;
        /// <summary>
        /// This will tells how many confirmation of the transaction is requeired to take tx as confirmed
        /// </summary>
        public static int NumberOfConfirmationsToAccept { get; set; } = 1;
        /// <summary>
        /// main port of the application with default value 8080
        /// </summary>
        public static int MainPort { get; set; } = 8080;
        /// <summary>
        /// set this true if the app should start browser automaticaly
        /// </summary>
        public static bool StartBrowserAtStart { get; set; } = false;
        /// <summary>
        /// set this true if application works with the db
        /// Corect connection string must be provided then
        /// </summary>
        public static bool WorkWithDb { get; set; } = true;
        /// <summary>
        /// if the db is loaded this flag is setted to true
        /// </summary>
        public static bool DbLoaded { get; set; } = true;
        /// <summary>
        /// MQTT connection parameters
        /// </summary>
        public static MQTTConfig MQTT { get; set; }
        /// <summary>
        /// global shared MQTT client
        /// </summary>
        public static MQTTClient MQTTClient { get; set; }
        /// <summary>
        /// application MQTT broker (not used primarly)
        /// the VEconomy usually starts ASP.NET integrated MQTTNet broker
        /// </summary>
        public static MQTTServer MQTTServer { get; set; }
        /// <summary>
        /// if the MQTT is started this is set to true - not implemented yet, setted hardcode
        /// </summary>
        public static bool MQTTServerIsStarted { get; set; }
        /// <summary>
        /// All MQTT topic to subscribe in global client
        /// </summary>
        public static List<string> MQTTTopics { get; set; }
        /// <summary>
        /// set this true if application works with the QT Wallet via RPC
        /// correct connection parameters to QT Wallet RPC server must be provided.
        /// QT Wallet must has activated RPC server
        /// </summary>
        public static bool WorkWithQTRPC { get; set; } = true;
        /// <summary>
        /// QT Wallet RPC parameters
        /// </summary>
        public static QTRPCConfig QTRPConfig { get; set; } = new QTRPCConfig();
        /// <summary>
        /// Global RPC client for communication with QT Wallet
        /// </summary>
        public static QTWalletRPCClient QTRPCClient { get; set; }
        /// <summary>
        /// the list of accounts provided in the appsetting.json file if the Db is not suppoerted and some accounts should be loaded after start
        /// </summary>
        public static List<string> AccountsFromConfig { get; set; } = new List<string>();

        /// <summary>
        /// global DbService provider
        /// </summary>
        public static IDbConnectorService DbService { get; set; }

        /// <summary>
        /// Main interval for refresh data about wallets and publish them to the MQTT
        /// </summary>
        public static int WalletRefreshInterval { get; set; } = 1000; // todo to the appconfig
        /// <summary>
        /// Global binance client
        /// </summary>
        public static BinanceSocketClient CommonBinanceSocketClient { get; set; }
        /// <summary>
        /// global binance data provider
        /// </summary>
        public static BinanceDataProvider ExchangeDataProvider { get; set; }
        /// <summary>
        /// global setting - content of appsetting.json
        /// </summary>
        public static IConfiguration CommonConfig { get; set; }

        /// <summary>
        /// Dictionary with all wallets
        /// Key - Guid format of wallet ID
        /// Value - IWallet object
        /// </summary>
        public static IDictionary<string, IWallet> Wallets { get; set; } = new ConcurrentDictionary<string, IWallet>();
        /// <summary>
        /// Dictionary with all accounts
        /// Key - Account address
        /// Value - IAccount object
        /// </summary>
        public static IDictionary<string, IAccount> Accounts { get; set; } = new ConcurrentDictionary<string, IAccount>();
        /// <summary>
        /// Dictionary with all nodes
        /// Key - Guid format of wallet ID
        /// Value - INode object
        /// </summary>
        public static IDictionary<string, INode> Nodes { get; set; } = new ConcurrentDictionary<string, INode>();
        /// <summary>
        /// Dictionary with all owners
        /// Key - Guid format of owner ID
        /// Value - IOwner object - not used now
        /// </summary>
        public static IDictionary<string, IOwner> Owners { get; set; } = new ConcurrentDictionary<string, IOwner>();
        /// <summary>
        /// Dictionary with all cryptocurrencies
        /// Key - Name of cryptocurrency
        /// Value - ICryptocurrency object, now just Neblio
        /// </summary>
        public static IDictionary<string, ICryptocurrency> Cryptocurrencies { get; set; } = new ConcurrentDictionary<string, ICryptocurrency>();

        /// <summary>
        /// This Token is used for sending messages
        /// </summary>
        public static NeblioNTP1Token MessagingToken { get; set; } = new NeblioNTP1Token() { Symbol = "MSGT", Id = "La3Fiunz84XRHDGb1HhQboH6x3TzhV2PMeRQNZ" };

        }
}
