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
    /// <summary>
    /// RPC Wallet response dto
    /// </summary>
    public class QTWalletResponseDto
    {
        /// <summary>
        /// Result of the reqest. It contains the data
        /// </summary>
        public string result { get; set; }
        /// <summary>
        /// Id of request for case of async handling
        /// </summary>
        public string id { get; set; }
    }

    /// <summary>
    /// RPC client for QT Wallets
    /// </summary>
    public class QTWalletRPCClient : IQTWalletRPCClient
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseurl">connection url or IP of QT wallet RPC server</param>
        /// <param name="port">connection port of QT wallet RPC server</param>
        public QTWalletRPCClient(string baseurl = "127.0.0.1", int port = 6326, bool usessl = false)
        {
            ConnectionUrlBaseAddress = baseurl;
            ConnectionPort = port;
            SSL = usessl;
            Console.WriteLine("Connection Wallet Address setted to:" + ConnectionAddress);
        }
        /// <summary>
        /// Create client from config dto
        /// </summary>
        /// <param name="cfg"></param>
        public QTWalletRPCClient(QTRPCConfig cfg)
        {
            ConnectionUrlBaseAddress = cfg.Host;
            ConnectionPort = cfg.Port;
            User = cfg.User;
            Pass = cfg.Pass;
            SSL = cfg.SSL;
            //Console.WriteLine("Connection Wallet Address setted to:" + ConnectionAddress);
        }
        /// <summary>
        /// Base URL for connection to RPC server
        /// </summary>
        public string ConnectionUrlBaseAddress { get; set; } = "127.0.0.1";
        /// <summary>
        /// Base port for connection to RPC server
        /// </summary>
        public int ConnectionPort { get; set; } = 6326;

        private string _connectionAddress = "http://127.0.0.1:6326/";
        /// <summary>
        /// Connection address in "clickable form
        /// </summary>
        public string ConnectionAddress
        {
            get
            {
                var http = "http";
                if (SSL)
                    http = "https";

                if (ConnectionPort > 0)
                    _connectionAddress = $"{http}://{ConnectionUrlBaseAddress}:{ConnectionPort}/";
                else
                    _connectionAddress = $"{http}://{ConnectionUrlBaseAddress}/";
                return _connectionAddress;
            }
        }

        public bool SSL { get; set; } = false;

        private bool _isConnected = false;
        /// <summary>
        /// Is connected to RPC server flag
        /// </summary>
        public bool IsConnected
        {
            get
            {
                return _isConnected;
            }
        }

        private string user = "user";
        private string pass = "password";
        /// <summary>
        /// User for connection to RPC server
        /// </summary>
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
        /// <summary>
        /// Password for connection to RPC server
        /// </summary>
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
        /// <summary>
        /// Is set if the client is initialized
        /// </summary>
        public bool ClientInitialized { get; set; } = false;

        private JsonClient jsonClient;
        private HttpClient httpClient;

        /// <summary>
        /// Initialize client. You need to load connection info first - usually during the construction
        /// </summary>
        public virtual void InitClients()
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
        public virtual async Task<string> RPCLocalCommandAsync(string param, object obj)
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
        public virtual async Task<string> RPCLocalCommandSplitedAsync(string command, string[] parameters)
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
