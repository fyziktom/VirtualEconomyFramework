using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Dto
{
    /// <summary>
    /// Dto for info about actual Token supply on address
    /// </summary>
    public class TokenSupplyDto
    {
        /// <summary>
        /// Symbol of token - up to 5 unique letters
        /// </summary>
        public string TokenSymbol { get; set; } = string.Empty;
        /// <summary>
        /// Token Id hash
        /// </summary>
        public string TokenId { get; set; } = string.Empty;
        /// <summary>
        /// Amount of tokens available
        /// </summary>
        public double Amount { get; set; } = 0.0;
        /// <summary>
        /// Token icon image url
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;
    }
}
