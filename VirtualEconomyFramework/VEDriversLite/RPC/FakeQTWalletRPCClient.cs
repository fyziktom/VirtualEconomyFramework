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
    /// RPC client for QT Wallets
    /// </summary>
    public class FakeQTWalletRPCClient : IQTWalletRPCClient
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="baseurl">connection url or IP of QT wallet RPC server</param>
        /// <param name="port">connection port of QT wallet RPC server</param>
        public FakeQTWalletRPCClient(string baseurl = "127.0.0.1", int port = 6326)
        {
            ConnectionUrlBaseAddress = baseurl;
            ConnectionPort = port;

            Console.WriteLine("Connection Wallet Address setted to:" + ConnectionAddress);
        }
        /// <summary>
        /// Create client from config dto
        /// </summary>
        /// <param name="cfg"></param>
        public FakeQTWalletRPCClient(QTRPCConfig cfg)
        {
            ConnectionUrlBaseAddress = cfg.Host;
            ConnectionPort = cfg.Port;
            User = cfg.User;
            Pass = cfg.Pass;

            Console.WriteLine("Connection Wallet Address setted to:" + ConnectionAddress);
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
                _connectionAddress = $"http://{ConnectionUrlBaseAddress}:{ConnectionPort}/";
                return _connectionAddress;
            }
        }

        private bool _isConnected = true;
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

        public Dictionary<string, string> CommandFakeReposnes { get; set; } = new Dictionary<string, string>();

        private JsonClient jsonClient;
        private HttpClient httpClient;

        /// <summary>
        /// Initialize client. You need to load connection info first - usually during the construction
        /// </summary>
        public virtual void InitClients()
        {
            return;
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
            if (CommandFakeReposnes.TryGetValue(param, out var result))
                return result;
            else
                return string.Empty;
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
            var input = command;
            if (parameters != null)
            {
                foreach (var par in parameters)
                {
                    par.TrimStart().TrimEnd();
                    input += "," + par;
                }
            }

            if (CommandFakeReposnes.TryGetValue(input, out var result))
                return result;
            else
                return string.Empty;
        }
    }
}
