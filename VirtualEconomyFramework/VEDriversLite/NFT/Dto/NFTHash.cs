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
        public string MainAddress { get; set; } = string.Empty;
        public string SubAccountAddress { get; set; } = string.Empty;
        public string TxId { get; set; } = string.Empty;
        public int Index { get; set; } = 0;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string AuthorDogeAddress { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public double Price { get; set; } = 0.0;
        public double DogePrice { get; set; } = 0.0;
        public DogeftInfo DogeftInfo { get; set; } = new DogeftInfo();
        public NFTTypes Type { get; set; } = NFTTypes.Image;
        public string ShortHash => $"{NeblioTransactionHelpers.ShortenTxId(TxId, false, 16)}:{Index}";

        public static string GetShortHash(string txid, int index = 0)
        {
            //return String.Format("{0:X}", ($"{txid}:{index}").GetHashCode());
            return $"{NeblioTransactionHelpers.ShortenTxId(txid, false, 16)}:{index}";
        }
    }

}
