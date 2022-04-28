using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Devices.Dto
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
        /// SSL classic user and pass
        /// </summary>
        SSL,
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
    public class CommonConnectionParams
    {
        /// <summary>
        /// IP for connection
        /// </summary>
        public string IP { get; set; } = string.Empty;
        /// <summary>
        /// URL for connection
        /// </summary>
        public string Url { get; set; } = string.Empty;
        /// <summary>
        /// PORT for connection
        /// </summary>
        public int Port { get; set; } = 80;
        /// <summary>
        /// Is communication encrypted
        /// </summary>
        public bool Encrypted { get; set; } = false;
        /// <summary>
        /// is commection secured
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
        /// Group Id of the device
        /// </summary>
        public string GroupId { get; set; } = string.Empty;
        /// <summary>
        /// Device Id of the device
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;
        /// <summary>
        /// User Id of the owner of the device
        /// </summary>
        public string UserId { get; set; } = string.Empty;
        /// <summary>
        /// Common data refresh interval in miliseconds
        /// Default value is 10s = 10000ms
        /// This is usually used for request type of the communication
        /// </summary>
        public int CommonRefreshInterval { get; set; } = 10000;
    }
}
