using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{
    public abstract class CommonNFT : INFT
    {
        public string TypeText { get; set; } = string.Empty;
        public NFTTypes Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string IconLink { get; set; } = string.Empty;
        public string ImageLink { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public string Utxo { get; set; } = string.Empty;
        public string SourceTxId { get; set; } = string.Empty;
        public string NFTOriginTxId { get; set; } = string.Empty;

        public abstract Task ParseOriginData();
    }
}
