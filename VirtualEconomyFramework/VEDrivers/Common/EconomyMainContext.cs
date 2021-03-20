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
using VEDrivers.Economy.Wallets;
using VEDrivers.Nodes;

namespace VEDrivers.Common
{
    public static class EconomyMainContext
    {
        public static bool WorkWithDb { get; set; } = true;
        public static bool WorkWithQTRPC {get; set; } = true;
        public static string MQTTIP { get; set; }
        public static int MQTTPort { get; set; }
        public static MQTTConfig MQTT { get; set; }
        public static MQTTClient MQTTClient { get; set; }
        public static MQTTServer MQTTServer { get; set; }
        public static bool MQTTServerIsStarted { get; set; }
        public static List<string> MQTTTopics { get; set; }
        public static QTRPCConfig QTRPConfig { get; set; } = new QTRPCConfig();
        public static QTWalletRPCClient QTRPCClient { get; set; }
        public static List<string> AccountsFromConfig { get; set; } = new List<string>();

        public static IDbConnectorService DbService { get; set; }

        public static int WalletRefreshInterval { get; set; } = 1000; // todo to the appconfig

        public static BinanceSocketClient CommonBinanceSocketClient { get; set; }
        public static BinanceDataProvider ExchangeDataProvider { get; set; }
        public static IConfiguration CommonConfig { get; set; }

        public static IDictionary<string, IWallet> Wallets { get; set; } = new ConcurrentDictionary<string, IWallet>();
        public static IDictionary<string, IAccount> Accounts { get; set; } = new ConcurrentDictionary<string, IAccount>();
        public static IDictionary<string, INode> Nodes { get; set; } = new ConcurrentDictionary<string, INode>();
        public static IDictionary<string, IOwner> Owners { get; set; } = new ConcurrentDictionary<string, IOwner>();
        public static IDictionary<string, ICryptocurrency> Cryptocurrencies { get; set; } = new ConcurrentDictionary<string, ICryptocurrency>();
    }
}
