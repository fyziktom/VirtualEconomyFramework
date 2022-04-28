using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.NFT.Dto
{
    /// <summary>
    /// Dto for shorten informations for the NFT
    /// </summary>
    public class NFTHash
    {
        /// <summary>
        /// Main address where this NFT is located or it is on the SubAccount address related to this main account
        /// </summary>
        public string MainAddress { get; set; } = string.Empty;
        /// <summary>
        /// Sub Account address where this NFT is located
        /// </summary>
        public string SubAccountAddress { get; set; } = string.Empty;
        /// <summary>
        /// whole transaction hash
        /// </summary>
        public string TxId { get; set; } = string.Empty;
        /// <summary>
        /// Intex of the utxo
        /// </summary>
        public int Index { get; set; } = 0;
        /// <summary>
        /// Name of the NFT
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Description of the NFT
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Author dogecoin address
        /// </summary>
        public string AuthorDogeAddress { get; set; } = string.Empty;
        /// <summary>
        /// Image in the NFT
        /// </summary>
        public string Image { get; set; } = string.Empty;
        /// <summary>
        /// Another link in the NFT
        /// </summary>
        public string Link { get; set; } = string.Empty;
        /// <summary>
        /// Price of the NFT, usually in the Neblio
        /// </summary>
        public double Price { get; set; } = 0.0;
        /// <summary>
        /// Dogecoin price of the NFT
        /// </summary>
        public double DogePrice { get; set; } = 0.0;
        /// <summary>
        /// Dogeft info in the NFT - describe model of sell
        /// </summary>
        public DogeftInfo DogeftInfo { get; set; } = new DogeftInfo();
        /// <summary>
        /// Type of the NFT
        /// </summary>
        public NFTTypes Type { get; set; } = NFTTypes.Image;
        /// <summary>
        /// Shorten hash of the NFT
        /// </summary>
        public string ShortHash => $"{NeblioTransactionHelpers.ShortenTxId(TxId, false, 16)}:{Index}";

        /// <summary>
        /// Get shorten hash of the NFT
        /// </summary>
        /// <param name="txid"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static string GetShortHash(string txid, int index = 0)
        {
            //return String.Format("{0:X}", ($"{txid}:{index}").GetHashCode());
            return $"{NeblioTransactionHelpers.ShortenTxId(txid, false, 16)}:{index}";
        }
    }

}
