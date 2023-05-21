using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Common
{
    /// <summary>
    /// Interface for the QTWalletRPCClient and its mock
    /// </summary>
    public interface IQTWalletRPCClient
    {
        /// <summary>
        /// User for connection to RPC server
        /// </summary>
        string User { get; set; }
        /// <summary>
        /// Password for connection to RPC server
        /// </summary>
        string Pass { get; set; }
        /// <summary>
        /// Is set if the client is initialized
        /// </summary>
        bool ClientInitialized { get; set; }

        /// <summary>
        /// Base URL for connection to RPC server
        /// </summary>
        string ConnectionUrlBaseAddress { get; set; }
        /// <summary>
        /// Base port for connection to RPC server
        /// </summary>
        int ConnectionPort { get; set; }
        /// <summary>
        /// Connection address in "clickable form
        /// </summary>
        string ConnectionAddress { get; }
        /// <summary>
        /// Is connected to RPC server flag
        /// </summary>
        bool IsConnected { get; }
        ///<summary>
        /// Initialize client. You need to load connection info first - usually during the construction
        /// </summary>
        void InitClients();
        /// <summary>
        /// Function for call of RPC command. Parameters is string splitted with ','
        /// First is command, then goes the parameters. If parameters must cointain ',' please use function RPCLocalCommandSplitedAsync
        /// </summary>
        /// <param name="param">First is command, then goes the parameters.</param>
        /// <param name="obj"></param>
        /// <returns></returns>
        Task<string> RPCLocalCommandAsync(string param, object obj);
        /// <summary>
        /// This function require already splitted command and parameters
        /// This is for the case that some of the parameters must contain ',' which is used as separator
        /// </summary>
        /// <param name="command">RPC command name</param>
        /// <param name="parameters">string array of parameters</param>
        /// <returns></returns>
        Task<string> RPCLocalCommandSplitedAsync(string command, string[] parameters);
    }
}
