using Newtonsoft.Json;
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

        [TestEntry]
        public static void Indexer_GetAddressUtxos(string param)
        {
            Indexer_GetAddressUtxosAsync(param);
        }
        public static async Task Indexer_GetAddressUtxosAsync(string param)
        {
            // param must be valid Neblio address
            var utxosObjects = Node.GetAddressUtxosObjects(param);

            if (utxosObjects.Any())
            {
                Console.WriteLine("\tAddress: " + param + " Utxos:");
                foreach (var utxo in utxosObjects)
                {
                    Console.WriteLine($"\t\tUtxo: {utxo.TransactionHashAndN}");
                    if (!utxo.TokenUtxo)
                        Console.WriteLine($"\t\t\t Amount: {utxo.Value} of NEBL.");
                    else
                        Console.WriteLine($"\t\t\t This is token Utxo with {utxo.TokenAmount} of {utxo.TokenSymbol}");
                }
            }
        }

        [TestEntry]
        public static void Indexer_GetAddressTokenUtxos(string param)
        {
            Indexer_GetAddressTokenUtxosAsync(param);
        }
        public static async Task Indexer_GetAddressTokenUtxosAsync(string param)
        {
            var tokenUtxosObjets = Node.GetAddressTokenUtxosObjects(param);

            if (tokenUtxosObjets.Any())
            {
                Console.WriteLine("\tAddress: " + param + " Tokens Utxos:");
                foreach (var tokens in tokenUtxosObjets)
                {
                    Console.WriteLine($"\t\tUtxo: {tokens.TransactionHashAndN}");
                    Console.WriteLine($"\t\t\tAmount: {tokens.TokenAmount} of {tokens.TokenSymbol}");
                }
            }
        }

        [TestEntry]
        public static void Indexer_GetAddressTransactions(string param)
        {
            Indexer_GetAddressTransactionsAsync(param);
        }
        public static async Task Indexer_GetAddressTransactionsAsync(string param)
        {
            // param must be valid Neblio address
            var transactions = Node.GetAddressTransactions(param);

            if (transactions.Any())
            {
                Console.WriteLine("\tAddress: " + param + " transactions:");
                foreach (var tx in transactions)
                    Console.WriteLine($"\t\t{tx}");
            }
        }

        [TestEntry]
        public static void Indexer_BroadcastTx(string param)
        {
            Indexer_BroadcastTxAsync(param);
        }
        public static async Task Indexer_BroadcastTxAsync(string param)
        {
            var txid = Node.BroadcastRawTx(param);
            await Console.Out.WriteLineAsync($"Tx id is: {txid}");
        }

        [TestEntry]
        public static void Indexer_GetBlockByHash(string param)
        {
            Indexer_GetBlockByHashAsync(param);
        }
        public static async Task Indexer_GetBlockByHashAsync(string param)
        {
            var block = Node.GetBlock(param);
            await Console.Out.WriteLineAsync(JsonConvert.SerializeObject(block, Formatting.Indented));
        }

        [TestEntry]
        public static void Indexer_GetBlockByNumber(string param)
        {
            Indexer_GetBlockByNumberAsync(param);
        }
        public static async Task Indexer_GetBlockByNumberAsync(string param)
        {
            var num = Convert.ToInt32(param);
            var block = Node.GetBlockByNumber(num);
            await Console.Out.WriteLineAsync(JsonConvert.SerializeObject(block, Formatting.Indented));
        }

        [TestEntry]
        public static void Indexer_GetTx(string param)
        {
            Indexer_GetTxAsync(param);
        }
        public static async Task Indexer_GetTxAsync(string param)
        {
            var tx = Node.GetTx(param);
            await Console.Out.WriteLineAsync(JsonConvert.SerializeObject(tx, Formatting.Indented));
        }

        [TestEntry]
        public static void Indexer_GetLatestBlock(string param)
        {
            Indexer_GetLatestBlockAsync(param);
        }
        public static async Task Indexer_GetLatestBlockAsync(string param)
        {
            var block = Node.GetLatestBlockNumber();
            await Console.Out.WriteLineAsync($"Latest block has number: {block}");
        }
    }
}
