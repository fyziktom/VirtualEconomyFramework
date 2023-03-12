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
        public static ConcurrentDictionary<string, INFT> MintedNFT = new ConcurrentDictionary<string, INFT>();

        public static string MainAccount { get; set; } = string.Empty;
        public static VirtualAssistant? Assistant { get; set; } = null;
    }
}
