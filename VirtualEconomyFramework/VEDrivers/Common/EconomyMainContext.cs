using Binance.Net;
using Microsoft.Extensions.Configuration;
using System;
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
        public static string MQTTIP { get; set; }
        public static int MQTTPort { get; set; }
        public static MQTTConfig MQTT { get; set; }
        public static MQTTClient MQTTClient { get; set; }
        public static List<string> MQTTTopics { get; set; }
        public static QTRPCConfig QTRPConfig { get; set; }
        public static QTWalletRPCClient QTRPCClient { get; set; }

        public static IDbConnectorService DbService { get; set; }

        public static int WalletRefreshInterval { get; set; } = 1000; // todo to the appconfig

        public static BinanceSocketClient CommonBinanceSocketClient { get; set; }
        public static BinanceDataProvider ExchangeDataProvider { get; set; }
        public static IConfiguration CommonConfig { get; set; }

        public static IDictionary<string, IWallet> Wallets { get; set; }
        public static IDictionary<string, IAccount> Accounts { get; set; }
        public static IDictionary<string, INode> Nodes { get; set; }
        public static IDictionary<string, IOwner> Owners { get; set; }
        public static IDictionary<string, ICryptocurrency> Cryptocurrencies { get; set; }
    }
}
