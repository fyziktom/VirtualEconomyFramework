using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.Dto;
using VEDriversLite.Bookmarks;
using VEDriversLite.NFT;
using VEDriversLite.NeblioAPI;
using VEDriversLite.AI.OpenAI;

namespace VEBlazor.Demo.AI.MintFreeForAI
{
    public static class MainDataContext
    {
        /// <summary>
        /// Minted NFTs
        /// </summary>
        public static ConcurrentDictionary<string, INFT> MintedNFTs = new ConcurrentDictionary<string, INFT>();
        /// <summary>
        /// Main account for minting NFTs
        /// </summary>
        public static string MainAccount { get; set; } = string.Empty;
        /// <summary>
        /// Virtual AI assistant
        /// </summary>
        public static VirtualAssistant? Assistant { get; set; } = null;
    }
}
