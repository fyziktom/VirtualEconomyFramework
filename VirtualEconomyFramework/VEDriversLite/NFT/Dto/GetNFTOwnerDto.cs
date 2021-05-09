using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.NFT.Dto
{
    public class GetNFTOwnerDto
    {
        public string TxId { get; set; } = string.Empty;
        public INFT NFT { get; set; }
        public string Sender { get; set; } = string.Empty;
        public string Owner { get; set; } = string.Empty;
    }
}
