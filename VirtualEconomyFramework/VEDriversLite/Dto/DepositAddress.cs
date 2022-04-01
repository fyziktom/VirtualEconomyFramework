using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Dto
{
    /// <summary>
    /// Deposit Address - it is address where shop will resend the payment for store/deposit after the NFT was send
    /// </summary>
    public class DepositAddress
    {
        /// <summary>
        /// Name of the Deposit address
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Deposit Address
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// If you dont have it take if from the NFT data
        /// </summary>
        public bool TakeAddressFromNFT { get; set; } = false;
        /// <summary>
        /// Name of the currency
        /// </summary>
        public string Currency { get; set; } = string.Empty;
        /// <summary>
        /// Percentage of the amount what should be send to the deposit address from whole received amount
        /// </summary>
        public double Percentage { get; set; } = 0.0;
    }
}
