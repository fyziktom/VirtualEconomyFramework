using Binance.Net;
using VEDrivers.Economy.Exchanges;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VEconomy.Common;
using VEDrivers.Economy.Wallets;
using System.Collections.Concurrent;
using Microsoft.Extensions.Configuration;
using VEDrivers.Common;
using VEDrivers.Economy.Persons;
using VEDrivers.Economy;
using VEDrivers.Database;
using VEDrivers.Nodes;

namespace VEconomy
{
    public static class MainDataContext
    {
        public static bool WorkWithDb { get; set; } = true;
        public static string MQTTIP { get; set; }
        public static int MQTTPort { get; set; }
        public static MQTTConfig MQTT { get; set; }
        public static MQTTClient MQTTClient { get; internal set; }
        public static List<string> MQTTTopics { get; set; }
        public static QTRPCConfig QTRPConfig { get; set; }
        public static QTWalletRPCClient QTRPCClient { get; set; }

        public static IDbConnectorService DbService { get; set; }

        public static int WalletRefreshInterval { get; set; } = 1000; // todo to the appconfig

        public static BinanceSocketClient CommonBinanceSocketClient { get; internal set; }
        public static BinanceDataProvider ExchangeDataProvider { get; internal set; }
        public static IConfiguration CommonConfig { get; set; }

        public static IDictionary<string, IWallet> Wallets { get; internal set; }
        public static IDictionary<string, IAccount> Accounts { get; internal set; }
        public static IDictionary<string, INode> Nodes { get; internal set; }
        public static IDictionary<string, IOwner> Owners { get; internal set; }
        public static IDictionary<string, ICryptocurrency> Cryptocurrencies { get; internal set; }
    }
}
