using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.NFT.Dto
{
    /// <summary>
    /// Class offers way how to store the loaded NFT data for the recovering of the NFT after reload without requesting API
    /// </summary>
    public class NFTCacheDto
    {
        public string Address { get; set; } = string.Empty;
        public string Utxo { get; set; } = string.Empty;
        public int UtxoIndex { get; set; } = 0;
        public NFTTypes NFTType { get; set; } = NFTTypes.Image;
        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        /// <summary>
        /// Last time when the NFT was loaded from the cash
        /// this helps to remove old not used NFTs from the cache
        /// </summary>
        public DateTime LastAccess { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// indicate when the NFT was saved in the cash first time
        /// </summary>
        public DateTime FirstSave { get; set; } = DateTime.UtcNow;

        public int NumberOfReads { get; set; } = 0;
    }
}
