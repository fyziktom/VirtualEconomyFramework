using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Dto
{
    public class DepositAddress
    {
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public bool TakeAddressFromNFT { get; set; } = false;
        public string Currency { get; set; } = string.Empty;
        public double Percentage { get; set; } = 0.0;
    }
}
