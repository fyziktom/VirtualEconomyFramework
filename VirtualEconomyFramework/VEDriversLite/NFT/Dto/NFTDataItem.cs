using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace VEDriversLite.NFT.Dto
{
    /// <summary>
    /// Type of the storage where the data are stored. Usually it is IPFS.
    /// </summary>
    public enum DataItemStorageType
    {
        /// <summary>
        /// IPFS storage
        /// </summary>
        IPFS,
        /// <summary>
        /// Common url
        /// </summary>
        Url,
        /// <summary>
        /// Local storage
        /// </summary>
        Local
    }
    /// <summary>
    /// Item in the NFT gallery. It is usually some image with tags
    /// </summary>
    public class NFTDataItem
    {
        /// <summary>
        /// Hash of the transaction
        /// </summary>        
        public string Hash { get; set; } = string.Empty;
        /// <summary>
        /// Type of the storage where the data are stored. Usually it is IPFS.
        /// </summary>
        public DataItemStorageType Storage { get; set; } = DataItemStorageType.IPFS;
        /// <summary>
        /// Parsed tags from the TagsList
        /// </summary>
        public List<string> TagsList { get; set; } = new List<string>();
        /// <summary>
        /// Loaded data as byte array
        /// </summary>
        [JsonIgnore]
        public byte[] Data { get; set; } = new byte[0];
        /// <summary>
        /// Display flag for UI
        /// </summary>
        public bool IsMain { get; set; } = false;
    }
}
