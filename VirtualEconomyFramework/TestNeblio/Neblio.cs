using System;
using System.Linq;
using System.IO;
using System.Xml;
using System.Xml.XPath;
using System.Data.Common;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Threading;
using System.Text;
using System.Reflection;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Diagnostics;
using System.Runtime.InteropServices;
using VEDriversLite;
using Neblio.RestApi;
using VEDriversLite.Common;

namespace TestNeblio
{
    static class Neblio
    {
        public static HttpClient httpClient = new HttpClient();
        public static QTWalletRPCClient rpcClient = new QTWalletRPCClient();

        [TestEntry]
        public static void _(DbConnection conn, string param)
        {
            Console.WriteLine($"Connection: {conn.ConnectionString}, Param: {param}");
        }

        [TestEntry]
        public static void SetRPCConnectionParams(DbConnection conn, string param, object obj)
        {
            try
            {
                var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);

                if (split.Length < 2)
                    throw new Exception("Not enought parameters: please input address, username and password!");

                if (!string.IsNullOrEmpty(split[0]))
                {
                    rpcClient.ConnectionUrlBaseAddress = "127.0.0.1";
                    rpcClient.ConnectionPort = 6326;
                }

                if (!string.IsNullOrEmpty(split[1]))
                    rpcClient.User = "user";

                if (!string.IsNullOrEmpty(split[2]))
                    rpcClient.Pass = "password";

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during setting of RPC Local connection parameters: {ex}");
            }
        }

        [TestEntry]
        public static void RPCLocalCommand(DbConnection conn, string param, object obj)
        {
            try
            {
                RPCLocalCommandAsync(conn, param, obj).GetAwaiter().GetResult();
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Exception during call of RPCLocalCommand: {ex}");
            }
        }

        private static async Task RPCLocalCommandAsync(DbConnection conn, string param, object obj)
        {
            var res = await rpcClient.RPCLocalCommandAsync(param, null);

            Console.WriteLine($"result of request: {res}");
        }

        [TestEntry]
        public static void WalletInfo(DbConnection conn, string param, object obj)
        {
            var client = (IClient)new Client(httpClient) {BaseUrl="https://ntp1node.nebl.io" };
            WalletInfo(client, param).GetAwaiter().GetResult();

        }
        [TestEntry]
        public static void WalletInfoTest(DbConnection conn, string param, object obj)
        {
            var client = (IClient)new TestnetClient(httpClient) { BaseUrl = "https://ntp1node.nebl.io" }; ;
            WalletInfo(client, param).GetAwaiter().GetResult();
        }

        private static async Task WalletInfo(IClient client, string param)
        {
            var address = await client.GetAddressAsync(param);
            Console.WriteLine($"AddrStr                     = {address.AddrStr                 }   ");
            Console.WriteLine($"Balance                     = {address.Balance                 }   ");
            Console.WriteLine($"BalanceSat                  = {address.BalanceSat              }   ");
            Console.WriteLine($"TotalReceived               = {address.TotalReceived           }   ");
            Console.WriteLine($"TotalReceivedSat            = {address.TotalReceivedSat        }   ");
            Console.WriteLine($"TotalSent                   = {address.TotalSent               }   ");
            Console.WriteLine($"TotalSentSat                = {address.TotalSentSat            }   ");
            Console.WriteLine($"UnconfirmedBalance          = {address.UnconfirmedBalance      }   ");
            Console.WriteLine($"UnconfirmedBalanceSat       = {address.UnconfirmedBalanceSat   }   ");
            Console.WriteLine($"UnconfirmedTxAppearances    = {address.UnconfirmedTxAppearances}   ");
            Console.WriteLine($"TxAppearances               = {address.TxAppearances           }   ");
            foreach (var item in address.Transactions)
            {
                Console.WriteLine($"Transaction: {item}");
            }
            foreach (var item in address.AdditionalProperties)
            {
                Console.WriteLine($"Property: Key = {item.Key}, Value = {item.Value}");
            }
            Console.WriteLine();
        }

        [TestEntry]
        public static void NeblioInfo(DbConnection conn, string param, object obj)
        {
            NeblioInfoAsync(param).GetAwaiter().GetResult();
        }

        private static async Task NeblioInfoAsync(string param)
        {
            using var client = new HttpClient();
            var result = await client.GetStringAsync("https://explorer.nebl.io/ext/getmoneysupply");

            Console.WriteLine($"Supply = {result}");
            Console.WriteLine();
        }

    }
}