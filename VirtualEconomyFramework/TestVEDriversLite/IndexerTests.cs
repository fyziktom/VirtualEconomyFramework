using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common;
using VEDriversLite.Indexer;
using VEDriversLite.NeblioAPI;

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
        public static void Indexer_GetAddressInfo(string param)
        {
            Indexer_GetAddressInfoAsync(param);
        }
        public static async Task Indexer_GetAddressInfoAsync(string param)
        {
            // param must be valid Neblio address
            var info = Node.GetAddressInfo(param);
            await Console.Out.WriteLineAsync($"Address {param} info:");
            await Console.Out.WriteLineAsync("");
            await Console.Out.WriteLineAsync("");
            await Console.Out.WriteLineAsync(JsonConvert.SerializeObject(info, Formatting.Indented));
            await Console.Out.WriteLineAsync("");
            await Console.Out.WriteLineAsync("");
        }

        [TestEntry]
        public static void Indexer_GetAddressTokenSupplies(string param)
        {
            Indexer_GetAddressTokenSuppliesAsync(param);
        }
        public static async Task Indexer_GetAddressTokenSuppliesAsync(string param)
        {
            // param must be valid Neblio address
            var tokens = Node.GetAddressTokenSupplies(param);
            await Console.Out.WriteLineAsync($"Address {param} Token supplies:");
            await Console.Out.WriteLineAsync("");
            await Console.Out.WriteLineAsync("");
            await Console.Out.WriteLineAsync(JsonConvert.SerializeObject(tokens, Formatting.Indented));
            await Console.Out.WriteLineAsync("");
            await Console.Out.WriteLineAsync("");
        }

        [TestEntry]
        public static void Indexer_GetAddressTransactionsList(string param)
        {
            Indexer_GetAddressTransactionsListAsync(param);
        }
        public static async Task Indexer_GetAddressTransactionsListAsync(string param)
        {
            var split = param.Split(new[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (split.Length < 3)
                await Console.Out.WriteLineAsync("Please provide: Address,skip,take");

            var address = split[0];
            var skip = Convert.ToInt32(split[1]);
            var take = Convert.ToInt32(split[2]);

            // param must be valid Neblio address
            var info = Node.GetAddressTransactions(address, skip, take);
            await Console.Out.WriteLineAsync($"Address {address} info:");
            await Console.Out.WriteLineAsync("");
            await Console.Out.WriteLineAsync("");
            await Console.Out.WriteLineAsync(JsonConvert.SerializeObject(info, Formatting.Indented));
            await Console.Out.WriteLineAsync("");
            await Console.Out.WriteLineAsync("");
        }

        
        [TestEntry]
        public static void AAAA_DeflateString(string param)
        {
            AAAA_DeflateStringAsync(param);
        }
        public static async Task AAAA_DeflateStringAsync(string param)
        {
            //var metadata = "OP_RETURN 4e54031001000100000209789c55535b4fdb3014fe2bc8cf501d3bbee669d30a1ad2c6c3c85eb64ec89793124193ca7150a1ea7f9fe3b61b4491e2ef724e9c93cf7b328d1897365952efc906e7e7ef3db9bb69484d529c901c2ef7a479dd62c699bdb81d9aef388e767d54eeec665632bbc497cee37bedf3941e879855eba4d2a6a220ac74c204a12df754864a29e7b9b5c6525dd1d0ba60a9102d38e6b8d5e05180a73ce81a4abf258e3e76dbd40d7d6eba5f912eac48bd22927127814aed85e541c9c0b940e9dc8a5cae888f681386079b8a97016357c0f3dd305a535983fc557ceb384cdb877347eab5114e32132a6c012855796fe88b33940f3d5b45ab1463ca4005c2896cd5c231cd55b10e716dfbeecdce5bfed8dbb88fbd032f05cfd6e173b1f5eed34b5b1ba04c6b0066b4519c154f9f475e2c5fbe5edf37d73f2e9a68fdd3c51237c3717ff967663dcf27cd42d7af4fa82b755472a0c02553e57da94b539879010b25ab2a7ff3cc0ffdfa2c50b10021401a73c8ca88fd38c453c7478c9b218706cf046eb6186d9ae25cc8d88257738df51e9f31be779ea8329913b3ebc6875d5ec3022e4ff035c32bfa1fbfcd38ebf45086db619fce1df8020ef92a41b9ddcc11ac4901dfbafee9bcbe1fa6e8f167da0d995152551533dc182fadc62a274d58dd6a665b04e9280d425ac5516806c87238356d8d90a003e6b0057bcc7f3e12375d1c53b3fb7760fe1c0e7f01e18f037e";

            var meta = param.Replace("OP_RETURN ", string.Empty).Trim();

            var customData = string.Empty;

            if (meta.Contains("789c"))
            {
                var customDataStart = meta.Split("789c");
                var length = customDataStart[0].Length;
                customData = meta.Substring(length, meta.Length - length).Trim();
            }


            try
            {
                var customDecompressed = StringExt.Decompress(StringExt.HexStringToBytes(customData));
                await Console.Out.WriteLineAsync(Encoding.UTF8.GetString(customDecompressed));
                var resp = new Dictionary<string, string>();

                var data = Encoding.UTF8.GetString(customDecompressed);
                var userData = JsonConvert.DeserializeObject<MetadataOfUtxo>(data);

                foreach (var o in userData.UserData.Meta)
                {
                    var od = JsonConvert.DeserializeObject<IDictionary<string, string>>(o.ToString());
                    if (od != null && od.Count > 0)
                    {
                        var of = od.First();
                        if (!resp.ContainsKey(of.Key))
                        {
                            resp.Add(of.Key, of.Value);
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                await Console.Out.WriteLineAsync(   "Exception: " + ex.Message);
            }
        }

        #region RPCTests

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

        #endregion

    }
}
