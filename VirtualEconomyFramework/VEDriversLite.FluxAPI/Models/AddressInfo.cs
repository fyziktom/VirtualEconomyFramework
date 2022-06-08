using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.FluxAPI.Models
{
    /// <summary>
    /// Basic Address info, list of the transctions
    /// </summary>
    public class AddressInfo
    {
        /// <summary>
        /// Address
        /// </summary>
        [JsonProperty("address", Required = Newtonsoft.Json.Required.DisallowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Index
        /// </summary>
        [JsonProperty("transactions", Required = Newtonsoft.Json.Required.AllowNull, NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore)]
        public List<TransactionBase> Transactions { get; set; } = new List<TransactionBase>();
    }
}
