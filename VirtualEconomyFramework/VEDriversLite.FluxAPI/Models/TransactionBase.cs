using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.FluxAPI.Models
{
    /// <summary>
    /// Base for the transaction related object
    /// </summary>
    public class TransactionBase
    {        
        /// <summary>
        /// Transaction Id
        /// </summary>
        [JsonProperty("txid", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string TxId { get; set; } = string.Empty;
        /// <summary>
        /// Index
        /// </summary>
        [JsonProperty("voutIndex", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public int Index { get; set; } = 0;
        /// <summary>
        /// Block Height
        /// </summary>
        [JsonProperty("height", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public double Height { get; set; } = 0;
    }
}
