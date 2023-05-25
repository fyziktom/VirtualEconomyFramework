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
        /// <summary>
        /// Number of block to load at init load of the indexer.
        /// </summary>
        public static int NumberOfBlocksInHistory { get; set; } = 10000;
        /// <summary>
        /// The newest block loaded by the indexer.
        /// </summary>
        public static double LatestLoadedBlock { get; set; } = 0.0;
        /// <summary>
        /// Oldest block to load back to the history. Indexer loads blocks from the latest to the oldest. 
        /// In meanwhile when there are no new blocks it loads back the history to block 0 or number of block setted by this variable.
        /// </summary>
        public static double OldestBlockToLoad { get; set; } = 0.0;
        /// <summary>
        /// If this is true the indexer will start from the latest block
        /// </summary>
        public static bool StartFromTheLatestBlock { get; set; } = true;
        /// <summary>
        /// Actual oldest loaded block in the indexer
        /// </summary>
        public static double ActualOldestLoadedBlock { get; set; } = 0.0;
        /// <summary>
        /// Average time to index one block in the microseconds
        /// </summary>
        public static double AverageTimeToIndexBlock { get => AverageTimeToIndexBlockHistory.Count > 0 ? AverageTimeToIndexBlockHistory.Average() : 0.0; }
        /// <summary>
        /// History of stopwatch of indexing of blocks in microseconds
        /// </summary>
        public static List<double> AverageTimeToIndexBlockHistory { get; set; } = new List<double>();
        /// <summary>
        /// Blocks where are just PoS transactions
        /// It should be loaded from the file if it exists from some last loading
        /// </summary>
        public static Dictionary<string, int> PoSBlocks { get; set; } = new Dictionary<string, int>();
    }
}
