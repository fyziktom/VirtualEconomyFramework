using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Accounts.Dto
{
    public class Token
    {
        public string TokenId { get; set; } = string.Empty;
        public double Amount { get; set; } = 0.0;
        public string IssueTxId { get; set; } = string.Empty;
        public string Data { get; set; } = string.Empty;
    }
}
