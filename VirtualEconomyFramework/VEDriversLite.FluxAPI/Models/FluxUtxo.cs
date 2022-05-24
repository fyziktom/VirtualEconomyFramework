using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.FluxAPI.Models
{
    /// <summary>
    /// Flux Utxo details
    /// </summary>
    public class FluxUtxo : TransactionBase
    {
        /// <summary>
        /// Address
        /// </summary>
        [JsonProperty("adress", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Value in Satoshi
        /// </summary>
        [JsonProperty("satoshis", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public long Value { get; set; } = 0;
        /// <summary>
        /// Address in scriptPubKey form
        /// </summary>
        [JsonProperty("scriptPubKey", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string ScriptPubKey { get; set; } = string.Empty;
        /// <summary>
        /// Coinbase?
        /// </summary>
        [JsonProperty("coinbase", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public bool Coinbase { get; set; } = false;

    }
}
