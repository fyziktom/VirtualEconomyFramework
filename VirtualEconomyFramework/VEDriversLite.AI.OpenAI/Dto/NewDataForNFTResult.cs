using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.AI.OpenAI.Dto
{
    /// <summary>
    /// Result object from creating of NFT data by ChatGPT
    /// </summary>
    public class NewDataForNFTResult
    {
        /// <summary>
        /// NFT Name
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// NFT Descritpion
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// NFT Tags
        /// </summary>
        public string Tags { get; set; } = string.Empty;
    }
}
