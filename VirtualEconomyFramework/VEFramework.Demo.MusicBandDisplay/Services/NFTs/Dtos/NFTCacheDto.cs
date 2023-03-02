using System;
using System.Collections.Generic;
using System.Text;

namespace VEFramework.Demo.MusicBandDisplay.Services.NFTs.Dtos
{
    /// <summary>
    /// Class offers way how to store the loaded NFT data for the recovering of the NFT after reload without requesting API
    /// </summary>
    public class NFTCacheDto
    {
        /// <summary>
        /// Address which owns NFT
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// NFT Utxo
        /// </summary>
        public string Utxo { get; set; } = string.Empty;
        /// <summary>
        /// NFT Utxo Index
        /// </summary>
        public int UtxoIndex { get; set; } = 0;
        /// <summary>
        /// NFT type
        /// </summary>
        public NFTTypes NFTType { get; set; } = NFTTypes.Image;
        /// <summary>
        /// Metadata of the NFT
        /// </summary>
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
        /// <summary>
        /// Number of read of this NFT, good for check if it is used and remove if not
        /// </summary>
        public int NumberOfReads { get; set; } = 0;
    }
}
