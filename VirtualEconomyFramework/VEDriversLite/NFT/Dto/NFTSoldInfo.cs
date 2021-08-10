using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.NFT.Dto
{
    public class NFTSoldInfo
    {
        public string PaymentTxId { get; set; } = string.Empty;
        public double Amount { get; set; } = 0.0;
        public string Currency { get; set; } = string.Empty;
        public string Platform { get; set; } = string.Empty;
        public bool SellAsMultipleCopy { get; set; } = false;
        public int MaxSupply { get; set; } = 0;
        public string OriginalNFTTemplate { get; set; } = string.Empty;
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
