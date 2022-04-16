using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.NFT.Dto
{
    /// <summary>
    /// Info about sold NFT
    /// </summary>
    public class NFTSoldInfo
    {
        /// <summary>
        /// Payment transaction hash
        /// </summary>
        public string PaymentTxId { get; set; } = string.Empty;
        /// <summary>
        /// Total amount
        /// </summary>
        public double Amount { get; set; } = 0.0;
        /// <summary>
        /// Currency
        /// </summary>
        public string Currency { get; set; } = string.Empty;
        /// <summary>
        /// Platform where the NFT was sold
        /// </summary>
        public string Platform { get; set; } = string.Empty;
        /// <summary>
        /// Sell as multiple NFT
        /// </summary>
        public bool SellAsMultipleCopy { get; set; } = false;
        /// <summary>
        /// Maximum supply of the NFT
        /// </summary>
        public int MaxSupply { get; set; } = 0;
        /// <summary>
        /// Original NFT used for mint the copy
        /// </summary>
        public string OriginalNFTTemplate { get; set; } = string.Empty;
        /// <summary>
        /// Is this dto empty?
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public bool IsEmpty
        {
            get
            {
                var res = true;
                if (!string.IsNullOrEmpty(PaymentTxId))
                    res = false;
                if (!string.IsNullOrEmpty(Currency))
                    res = false;
                if (!string.IsNullOrEmpty(Platform))
                    res = false;
                if (Amount > 0)
                    res = false;
                return res;
            }
        }
    }
}
