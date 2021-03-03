using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Nodes.Dto
{
    public class HTTPApiRequestNodeParams
    {
        /// <summary>
        /// Just IP address or host for example 127.0.0.1 if you want to call http://127.0.0.1/
        /// </summary>
        public string Host { get; set; } = "127.0.0.1";
        /// <summary>
        /// Port of service, for example 8080 for calling https://127.0.0.1:8080/
        /// </summary>
        public string Port { get; set; } = string.Empty;
        /// <summary>
        /// Full api comand with parameters for example "api/AddWallet/MyWalletName"
        /// </summary>
        public string ApiCommand { get; set; } = string.Empty;
        /// <summary>
        /// Any data for PUT and POST methods
        /// </summary>
        public string Data { get; set; } = string.Empty;
        /// <summary>
        /// if service uses login you can pass it here
        /// </summary>
        public string User { get; set; } = string.Empty;
        /// <summary>
        /// if service uses login you can pass it here
        /// </summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// if you set this true, it will create address in form https://127.0.0.1/
        /// </summary>
        public bool UseHttps { get; set; } = false;
    }
}
