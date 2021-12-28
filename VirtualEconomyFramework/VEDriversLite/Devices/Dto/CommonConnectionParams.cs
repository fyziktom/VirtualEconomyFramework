using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Devices.Dto
{
    public enum CommunitacionSecurityType
    {
        None,
        SSL,
        Bearer,
        JWT
    }
    public class CommonConnectionParams
    {
        public string IP { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public int Port { get; set; } = 80;
        public bool Encrypted { get; set; } = false;
        public bool Secured { get; set; } = false;
        public CommunitacionSecurityType SType { get; set; } = CommunitacionSecurityType.None;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string GroupId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        /// <summary>
        /// Common data refresh interval in miliseconds
        /// Default value is 10s = 10000ms
        /// This is usually used for request type of the communication
        /// </summary>
        public int CommonRefreshInterval { get; set; } = 10000;
    }
}
