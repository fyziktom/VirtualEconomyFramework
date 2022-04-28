using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Dto
{
    /// <summary>
    /// Dto for info about owner of some kind of the tokens
    /// </summary>
    public class TokenOwnerDto
    {
        /// <summary>
        /// Address of the Owner
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Shorten Address of the Owner
        /// </summary>
        public string ShortenAddress { get; set; } = string.Empty;
        /// <summary>
        /// Amount of the tokens on the Owner Address
        /// </summary>
        public int AmountOfTokens { get; set; } = 0;
        /// <summary>
        /// Amount of the NFTs on the Owner Address
        /// </summary>
        public int AmountOfNFTs { get; set; } = 0;
    }
}
