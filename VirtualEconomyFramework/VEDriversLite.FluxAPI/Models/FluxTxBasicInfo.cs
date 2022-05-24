using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.FluxAPI.Models
{
    /// <summary>
    /// Basic information about the flux transaction
    /// </summary>
    public class FluxTxBasicInfo : TransactionBase
    {
        /// <summary>
        /// Version of transaction
        /// </summary>
        [JsonProperty("version", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int Version { get; set; } = 0;
        /// <summary>
        /// Type of Transaction
        /// </summary>
        [JsonProperty("type", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Type { get; set; } = string.Empty;
        /// <summary>
        /// Version of transaction
        /// </summary>
        [JsonProperty("updateType", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int UpdateType { get; set; } = 0;
        /// <summary>
        /// IP and port of the node
        /// </summary>
        [JsonProperty("ip", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string IP { get; set; } = string.Empty;
        /// <summary>
        /// Bench Tier
        /// </summary>
        [JsonProperty("benchTier", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string BenchTier { get; set; } = string.Empty;
        /// <summary>
        /// Collateral Hash
        /// </summary>
        [JsonProperty("collateralHash", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string CollateralHash { get; set; } = string.Empty;
        /// <summary>
        /// Collateral Index
        /// </summary>
        [JsonProperty("collateralIndex", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string CollateralIndex { get; set; } = string.Empty;
        /// <summary>
        /// Zel Address
        /// </summary>
        [JsonProperty("zelAdress", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string ZelAddress { get; set; } = string.Empty;
        /// <summary>
        /// Locked amount
        /// </summary>
        [JsonProperty("lockedAmount", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long LockedAmount { get; set; } = 0;

    }
}
