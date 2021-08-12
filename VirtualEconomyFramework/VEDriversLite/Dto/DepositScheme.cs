using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Dto
{
    public class DepositScheme
    {
        public string Name { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string MainDepositAddress { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public Dictionary<string, DepositAddress> DepositAddresses { get; set; } = new Dictionary<string, DepositAddress>();
    }
}
