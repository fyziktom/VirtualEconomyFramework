using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.NFT;

namespace VEDriversLite.Dto
{
    public class SplitNeblioTokensDto
    {
        public List<string> receivers { get; set; } = new List<string>();
        public int lots { get; set; } = 2;
        public int amount { get; set; } = 25;
        public string tokenId { get; set; } = NFTHelpers.TokenId;

        public int TotalAmount => amount * lots;
    }
}
