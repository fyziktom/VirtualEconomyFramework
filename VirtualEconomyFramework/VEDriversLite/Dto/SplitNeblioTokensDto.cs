using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.NFT;

namespace VEDriversLite.Dto
{
    /// <summary>
    /// Split Neblio tokens Dto.
    /// </summary>
    public class SplitNeblioTokensDto
    {
        /// <summary>
        /// Addresses of the receivers
        /// </summary>
        public List<string> receivers { get; set; } = new List<string>();
        /// <summary>
        /// Number of the lots
        /// </summary>
        public int lots { get; set; } = 2;
        /// <summary>
        /// Amount of the tokens
        /// </summary>
        public int amount { get; set; } = 25;
        /// <summary>
        /// Token Id
        /// </summary>
        public string tokenId { get; set; } = NFTHelpers.TokenId;
        /// <summary>
        /// Total amount of all lots together
        /// </summary>
        public int TotalAmount => amount * lots;
    }
}
