using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite.NFT.Coruzant
{
    /// <summary>
    /// Abstract common class for Coruzant NFTs
    /// </summary>
    public abstract class CommonCoruzantNFT : CommonNFT
    {
        /// <summary>
        /// Link to the podcast
        /// </summary>
        public string PodcastLink { get; set; } = string.Empty;
    }
}
