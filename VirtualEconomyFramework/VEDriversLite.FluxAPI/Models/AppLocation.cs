using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.FluxAPI.Models
{
    /// <summary>
    /// App locations Dto
    /// </summary>
    public class AppLocation
    {
        /// <summary>
        /// IP and port of the App location
        /// </summary>
        [JsonProperty("ip", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string IP { get; set; } = string.Empty;
        /// <summary>
        /// Name of the App location
        /// </summary>
        [JsonProperty("name", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// App was broadcast At datetime
        /// </summary>
        [JsonProperty("broadcastedAt", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public DateTime BroadcastedAt { get; set; } = DateTime.MinValue;
        /// <summary>
        /// App will expire At datetime
        /// </summary>
        [JsonProperty("expireAt", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public DateTime ExpireAt { get; set; } = DateTime.MaxValue;
        /// <summary>
        /// App deployment hash
        /// </summary>
        [JsonProperty("hash", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Hash { get; set; } = string.Empty;

    }
}
