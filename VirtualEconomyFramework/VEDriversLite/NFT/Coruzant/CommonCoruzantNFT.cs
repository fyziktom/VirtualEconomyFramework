using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite.NFT.Coruzant
{
    public abstract class CommonCoruzantNFT : CommonNFT
    {
        public string PodcastLink { get; set; } = string.Empty;
    }
}
