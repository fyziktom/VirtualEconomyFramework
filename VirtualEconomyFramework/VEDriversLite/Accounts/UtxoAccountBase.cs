using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.Accounts.Dto;

namespace VEDriversLite.Accounts
{
    public class UtxoAccountBase
    {
        /// <summary>
        /// Actual list of all Utxos on this address.
        /// </summary>
        [JsonIgnore]
        public List<Utxo> Utxos { get; set; } = new List<Utxo>();
    }
}
