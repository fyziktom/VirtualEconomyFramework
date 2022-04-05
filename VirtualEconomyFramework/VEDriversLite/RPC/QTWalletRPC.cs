using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite.Common
{
    /// <summary>
    /// QT Wallet RPC client controler
    /// </summary>
    public static class QTWalletRPC
    {
        /// <summary>
        /// Main API functions
        /// </summary>
        public static Dictionary<string, Func<string, string, string[], JsonClient, Task<string>>> ApiFunctions = new Dictionary<string, Func<string, string, string[], JsonClient, Task<string>>>
        {
            { "getaccount", CommonOneParameterRequest },
            { "getnewaddress", CommonOneParameterRequest },
            { "dumpprivatekey", CommonOneParameterRequest },
            { "getwalletinfo", CommonNoParameterRequest },
            { "getntp1balances", CommonNoParameterRequest },
            { "getpeerinfo", CommonNoParameterRequest },
            { "getinfo", CommonNoParameterRequest },
            { "listtransactions", CommonMultipleParameterRequest },
            { "listreceivedbyaddress", CommonNoParameterRequest },
            { "listreceivedbyaccount", CommonNoParameterRequest },
            { "move", CommonMultipleParameterRequest },
            { "signrawtransaction", CommonMultipleParameterRequest },
            { "listaccounts", CommonMultipleParameterRequest },
            { "listunspent", CommonMultipleParameterRequest },
            { "listaddressgroupings", CommonMultipleParameterRequest },
            { "getaccountaddress", CommonOneParameterRequest },
            { "getaddressesbyaccount", CommonOneParameterRequest },
            { "sendntp1toaddress", CommonMultipleParameterRequest }
        };

        /// <summary>
        /// Find match for command in ApiDictionary and invoke function
        /// </summary>
        /// <param name="uid">Request UID</param>
        /// <param name="command">Command to do</param>
        /// <param name="args">set of arguments</param>
        /// <param name="jsonClient"></param>
        /// <returns></returns>
        public static async Task<string> ProcessRequest(string uid, string command, string[] args, JsonClient jsonClient)
        {
            var res = "CANNOT_FIND_FUNCTION_IN_LIST";

            if (!string.IsNullOrEmpty(uid) && !string.IsNullOrEmpty(command))
                if (ApiFunctions.ContainsKey(command))
                    res = await ApiFunctions[command].Invoke(uid, command, args, jsonClient);

            return res;
        }

        /// <summary>
        /// Common Api Function - Common call for command with multiple parameter
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="command"></param>
        /// <param name="args"></param>
        /// <param name="jsonClient"></param>
        /// <returns></returns>
        private static async Task<string> CommonMultipleParameterRequest(string uid, string command, string[] args, JsonClient jsonClient)
        {
            var res = "WRONG_INPUT_DATA";

            if (args.Length > 0)
            {
                var account = args[0];

                if (!string.IsNullOrEmpty(account))
                {
                    var rpcReq = new RpcRequest();
                    jsonClient.ReadResponseAsString = true;
                    rpcReq.Id = uid;
                    rpcReq.Method = command;
                    rpcReq.Params = new List<string>();

                    foreach (var a in args)
                        rpcReq.Params.Add(a);

                    try
                    {
                        var rpcRes = await jsonClient.RpcAsync(rpcReq);
                        //Console.WriteLine("result:" + rpcRes.Result);

                        res = JsonConvert.SerializeObject(rpcRes);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception occured during request with multiple parameter, command {command}: {ex}");
                    }
                }
                else
                {
                    res = "Address cannot be empty or null";
                }
            }

            return res;
        }

        /// <summary>
        /// Common Api Function - Common call for command with one parameter
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="command"></param>
        /// <param name="args"></param>
        /// <param name="jsonClient"></param>
        /// <returns></returns>
        private static async Task<string> CommonOneParameterRequest(string uid, string command, string[] args, JsonClient jsonClient)
        {
            var res = "WRONG_INPUT_DATA";

            if (args.Length > 0)
            {
                var account = args[0];

                if (!string.IsNullOrEmpty(account))
                {
                    var rpcReq = new RpcRequest();
                    jsonClient.ReadResponseAsString = true;
                    rpcReq.Id = uid;
                    rpcReq.Method = command;
                    rpcReq.Params = new List<string>() { account };

                    try
                    {
                        var rpcRes = await jsonClient.RpcAsync(rpcReq);
                        //Console.WriteLine("result:" + rpcRes.Result);

                        res = JsonConvert.SerializeObject(rpcRes);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception occured during request with one parameter, command {command}: {ex}");
                    }
                }
                else
                {
                    res = "Address cannot be empty or null";
                }
            }

            return res;
        }

        /// <summary>
        /// Common Api Function - Common request without parameter
        /// </summary>
        /// <param name="uid"></param>
        /// <param name="command"></param>
        /// <param name="args"></param>
        /// <param name="jsonClient"></param>
        /// <returns></returns>
        private static async Task<string> CommonNoParameterRequest(string uid, string command, string[] args, JsonClient jsonClient)
        {
            var res = "WRONG_INPUT_DATA";

            var rpcReq = new RpcRequest();
            jsonClient.ReadResponseAsString = true;
            rpcReq.Id = uid;
            rpcReq.Method = command;

            try
            {
                var rpcRes = await jsonClient.RpcAsync(rpcReq);
                //Console.WriteLine("result:" + rpcRes.Result);

                res = JsonConvert.SerializeObject(rpcRes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception occured during request without parameters, command {command}: {ex}");
            }

            return res;
        }

    }
}
