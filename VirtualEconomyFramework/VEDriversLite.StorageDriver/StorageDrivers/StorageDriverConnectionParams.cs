using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.StorageDriver.StorageDrivers
{
    /// <summary>
    /// Security of the API
    /// </summary>
    public enum CommunitacionSecurityType
    {
        /// <summary>
        /// Without credentials
        /// </summary>
        None,
        /// <summary>
        /// Basic user and pass
        /// </summary>
        Basic,
        /// <summary>
        /// common Bearer token
        /// </summary>
        Bearer,
        /// <summary>
        /// JWT Token
        /// </summary>
        JWT
    }
    /// <summary>
    /// Main connection parameters for the IoT data driver to access the API
    /// </summary>
    public class StorageDriverConnectionParams
    {
        /// <summary>
        /// IP for connection
        /// </summary>
        public string IP { get; set; } = string.Empty;
        /// <summary>
        /// URL for connection to the API if exists
        /// </summary>
        public string APIUrl { get; set; } = string.Empty;
        /// <summary>
        /// URL for connection to the Gateway if exists
        /// </summary>
        public string GatewayURL { get; set; } = string.Empty;
        /// <summary>
        /// Path to the file system if exists
        /// </summary>
        public string FileStoragePath { get; set; } = string.Empty;
        /// <summary>
        /// PORT for connection to API
        /// </summary>
        public int APIPort { get; set; } = 5001;
        /// <summary>
        /// PORT for connection to Gateway
        /// </summary>
        public int GatewayPort { get; set; } = 80;
        /// <summary>
        /// Is communication encrypted with HTTPS
        /// </summary>
        public bool Encrypted { get; set; } = false;
        /// <summary>
        /// is commection secured with some pass/token
        /// </summary>
        public bool Secured { get; set; } = false;
        /// <summary>
        /// Type of security type like: SSL, Bearer, etc.
        /// </summary>
        public CommunitacionSecurityType SType { get; set; } = CommunitacionSecurityType.None;
        /// <summary>
        /// User name for the case of secured connection
        /// </summary>
        public string Username { get; set; } = string.Empty;
        /// <summary>
        /// Password name for the case of secured connection
        /// </summary>
        public string Password { get; set; } = string.Empty;
        /// <summary>
        /// Token for the case of secured connection
        /// </summary>
        public string Token { get; set; } = string.Empty;
        /// <summary>
        /// User Id
        /// </summary>
        public string UserId { get; set; } = string.Empty;
    }
}
