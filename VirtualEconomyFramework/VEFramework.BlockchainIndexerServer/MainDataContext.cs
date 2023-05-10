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
using VEDriversLite.Indexer;

namespace VEFramework.BlockchainIndexerServer
{
    public static class MainDataContext
    {
        /// <summary>
        /// Main account for minting NFTs
        /// </summary>
        public static VirtualNode Node { get; set; } = new VirtualNode();
        public static int NumberOfBlocksInHistory { get; set; } = 10000;
        public static double LatestLoadedBlock { get; set; } = 0;
        public static double OldestBlockToLoad { get; set; } = 0;
        public static bool StartFromTheLatestBlock { get; set; } = true;
    }
}
