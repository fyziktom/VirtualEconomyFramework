using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common;
using VEDriversLite.Indexer;

namespace TestVEDriversLite
{
    public static class IndexerTests
    {
        public static VirtualNode Node { get; set; } = new VirtualNode();

        [TestEntry]
        public static void Indexer_InitRPC(string param)
        {
            Indexer_InitRPCAsync(param);
        }
        public static async Task Indexer_InitRPCAsync(string param)
        {
            var rpcsettings = new QTRPCConfig()
            {
                Host = "127.0.0.1",
                Port = 6326,
                User = "user",
                Pass = "password"
            };

            Node.InitClient(rpcsettings);

        }

        [TestEntry]
        public static void Indexer_IndexBlocks(string param)
        {
            Indexer_IndexBlocksAsync(param);
        }
        public static async Task Indexer_IndexBlocksAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 2)
                await Console.Out.WriteLineAsync("Please input: \n - number of Blocks to load \n - latest block number from which to load and -1 for load from latest block based on node");
            
            var numberOfBlocks = Convert.ToInt32(split[0]);
            var lastBlockNumber = Convert.ToInt32(split[1]);

            if (!Node.QTRPCClient.IsConnected)
                await Indexer_InitRPCAsync(null);

            if (lastBlockNumber <= 0)
                lastBlockNumber = await Node.GetLatestBlockNumber();
            
            var offset = lastBlockNumber - numberOfBlocks;
            var start = 0 + offset;
            var end = numberOfBlocks + start;

            var timeToGetBlocks = 0.0;
            var timeToGetTxs = 0.0;


            Console.WriteLine($"Getting records");
            var sp = new Stopwatch();
            sp.Start();

            await Node.GetIndexedBlocksByNumbersOffsetAndAmount(offset, numberOfBlocks);

            sp.Stop();
            Console.WriteLine("");
            Console.WriteLine("");
            timeToGetBlocks = sp.Elapsed.TotalSeconds;
            Console.WriteLine($"Total time to get the blocks: {timeToGetBlocks} s");
            Console.WriteLine("");
            Console.WriteLine("Getting Tx info...");
            sp.Reset();
            sp.Start();

            await Node.LoadAllBlocksTransactions();

            sp.Stop();
            Console.WriteLine("");
            Console.WriteLine("");
            timeToGetTxs += sp.Elapsed.TotalSeconds;
            Console.WriteLine($"Total time to get the records: {timeToGetTxs} s");
            Console.WriteLine("");

            var addresses = Node.GetAllAddresses();
            foreach (var address in addresses)
            {
                var utxos = Node.GetAddressUtxos(address);
                var utxosObjects = Node.GetAddressUtxosObjects(address);
                var tokenUtxosObjets = Node.GetAddressTokenUtxosObjects(address);

                if (tokenUtxosObjets.Any())
                {
                    Console.WriteLine("\tAddress: " + address + " Tokens Utxos:");
                    foreach (var tokens in tokenUtxosObjets)
                        Console.WriteLine($"\t\tAmount: {tokens.TokenAmount} of {tokens.TokenSymbol}");
                }
            }

            Console.WriteLine("");
            Console.WriteLine("");
            Console.WriteLine($"Time to get blocks: {timeToGetBlocks} s");
            Console.WriteLine($"Time to get txs: {timeToGetTxs} s");
            Console.WriteLine($"Total utxos {Node.Utxos.Count}");
            Console.WriteLine($"Total Used Utxos {Node.UsedUtxos.Count}");
            Console.WriteLine($"Total txrecords {Node.Transactions.Count}");
            Console.WriteLine($"Total blocks {Node.Blocks.Count}");

            Console.WriteLine("");
            Console.WriteLine("END");
        }
    }
}
