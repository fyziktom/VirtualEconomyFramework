using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.NFT.Dto
{
    public class NFTCacheDto
    {
        public string Address { get; set; } = string.Empty;
        public string Utxo { get; set; } = string.Empty;
        public int UtxoIndex { get; set; } = 0;
        public NFTTypes NFTType { get; set; } = NFTTypes.Image;
        public IDictionary<string, string> Metadata { get; set; } = new Dictionary<string, string>();

        public DateTime LastAccess { get; set; } = DateTime.UtcNow;
        public DateTime FirstSave { get; set; } = DateTime.UtcNow;

        public int NumberOfReads { get; set; } = 0;
    }
}
