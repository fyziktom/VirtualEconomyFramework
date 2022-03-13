using VEDriversLite.NeblioAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Common
{
    public class QTWalletResponseDto
    {
        public string result { get; set; }
        public string id { get; set; }
    }

    public class QTWalletRPCClient
    {
        public QTWalletRPCClient(string baseurl = "127.0.0.1", int port = 6326)
        {
            ConnectionUrlBaseAddress = baseurl;
            ConnectionPort = port;

            Console.WriteLine("Connection Wallet Address setted to:" + ConnectionAddress);
        }

        public QTWalletRPCClient(QTRPCConfig cfg)
        {
            ConnectionUrlBaseAddress = cfg.Host;
            ConnectionPort = cfg.Port;
            User = cfg.User;
            Pass = cfg.Pass;

            Console.WriteLine("Connection Wallet Address setted to:" + ConnectionAddress);
        }

        public string ConnectionUrlBaseAddress { get; set; } = "127.0.0.1";
        public int ConnectionPort { get; set; } = 6326;

        private string _connectionAddress = "http://127.0.0.1:6326/";
        public string ConnectionAddress
        {
            get
            {
                _connectionAddress = $"http://{ConnectionUrlBaseAddress}:{ConnectionPort}/";
                return _connectionAddress;
            }
        }

        private bool _isConnected = false;
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
        }

        private string user = "user";
        private string pass = "password";
        public string User
        {
            get
            {
                return user;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    user = value;
                }
                else
                {
                    throw new Exception("Cannot be null or empty!");
                }
            }
        }
        public string Pass
        {
            get
            {
                return pass;
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    pass = value;
                }
                else
                {
                    throw new Exception("Cannot be null or empty!");
                }
            }
        }

        public bool ClientInitialized { get; set; } = false;

        private JsonClient jsonClient;
        private HttpClient httpClient;

        public void InitClients()
        {
            try
            {
                httpClient = new HttpClient();

                var byteArray = new UTF8Encoding().GetBytes($"{user}:{pass}");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                jsonClient = new JsonClient(httpClient);
                jsonClient.BaseUrl = ConnectionAddress;

                ClientInitialized = true;

                _isConnected = true;
            }
            catch (Exception ex)
            {
                ClientInitialized = false;
                _isConnected = false;
                Console.WriteLine($"Exception: {ex}");
            }
        }

        /// <summary>
        /// Function for call of RPC command. Parameters is string splitted with ','
        /// First is command, then goes the parameters. If parameters must cointain ',' please use function RPCLocalCommandSplitedAsync
        /// </summary>
        /// <param name="param">First is command, then goes the parameters.</param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public async Task<string> RPCLocalCommandAsync(string param, object obj)
        {
            var split = param.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

            if (split.Length < 1)
                throw new Exception("Not enought parameters: please input as minimum RPC command name!");

            //var addr = "NPWBL3i8ZQ8tmhDtrixXwYd93nofmunvhA";
            var command = split[0].TrimStart().TrimEnd();

            string[] parameters = new string[split.Length - 1];

            for (int i = 0; i < split.Length - 1; i++)
                parameters[i] = split[i + 1].TrimStart().TrimEnd();

            if (!ClientInitialized)
                InitClients();

            if (!ClientInitialized)
                throw new Exception("Cannot init RPC communication clients.");

            var res = await QTWalletRPC.ProcessRequest(Guid.NewGuid().ToString(), command, parameters, jsonClient);

            //Console.WriteLine($"result of request: {res}");

            return res;
        }

        /// <summary>
        /// This function require already splitted command and parameters
        /// This is for the case that some of the parameters must contain ',' which is used as separator
        /// </summary>
        /// <param name="command">RPC command name</param>
        /// <param name="parameters">string array of parameters</param>
        /// <returns></returns>
        public async Task<string> RPCLocalCommandSplitedAsync(string command, string[] parameters)
        {
            if (parameters != null)
            {
                foreach (var par in parameters)
                    par.TrimStart().TrimEnd();
            }
            else
            {
                return "ERROR - RPC Params cannot be empty";
            }

            if (!ClientInitialized)
                InitClients();

            if (!ClientInitialized)
                throw new Exception("Cannot init RPC communication clients.");

            var res = await QTWalletRPC.ProcessRequest(Guid.NewGuid().ToString(), command, parameters, jsonClient);

            //Console.WriteLine($"result of request: {res}");

            return res;
        }
    }
}
