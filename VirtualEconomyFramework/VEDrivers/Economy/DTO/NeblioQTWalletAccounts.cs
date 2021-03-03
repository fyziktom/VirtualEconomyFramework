using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Economy.DTO
{
    public class NeblioQTWalletAccounts
    {
        public string id { get; set; } = string.Empty;
        public List<JObject> result { get; set; } 
    }

    public class NeblioQTWalletAccountDetails
    {
        public string Address { get; set; } = string.Empty;
        public double Balance { get; set; } = 0.0;
        public string Name { get; set; } = string.Empty;
        public List<NeblioQTWalletTokenAccountDetails> tokenAccounts { get; set; }
    }

    public class NeblioQTWalletTokenAccountDetails
    {
        public string Symbol { get; set; } = string.Empty;
        public double Balance { get; set; } = 0.0;
        public string TokenId { get; set; } = string.Empty;

    }
}
