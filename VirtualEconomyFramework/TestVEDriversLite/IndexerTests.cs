using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common;
using VEDriversLite.Indexer;
using VEDriversLite.Neblio;
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
            await Indexer_InitRPCAsync("");
            var tx = Node.GetTx(param);

            var txs = JsonConvert.SerializeObject(tx);
            try
            {
                var dtx = JsonConvert.DeserializeObject<GetTransactionInfoResponse>(txs);
                Console.WriteLine($"{JsonConvert.SerializeObject(dtx, Formatting.Indented)}");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Error Message: " + ex.Message);
            }
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
        
        
        [TestEntry]
        public static void Indexer_ProcessBroadcastedTransaction(string param)
        {
            Indexer_ProcessBroadcastedTransactionAsync(param);
        }
        public static async Task Indexer_ProcessBroadcastedTransactionAsync(string param)
        {
            // nft tx
            //var tx = "0100000042df5a64022ad46becb69bee2ad18eef1369124ffd7bd94776136f850374f9254d8cef4e76030000006a47304402207b09218c0b9c573dc964f80a3b91a42627a90210352447d47cda785b315f088002206d0e61d60b95f559ff6fc7dd31e50b25f06573effb44b4ecaa1a0d98a581e43d0121023e333150345a4211c172754f39616a55e3eb817bfde6abd5f6503e9ff77e9ceeffffffff2ad46becb69bee2ad18eef1369124ffd7bd94776136f850374f9254d8cef4e76020000006a4730440220413f13b3547bacc3bd3a6d32b211ea138e90b34b22695a4e7da14c717344e17302205d548b11c97c48ad5a553a21e8753c6d196b430f8644f03d84cd1277cb1f034d0121023e333150345a4211c172754f39616a55e3eb817bfde6abd5f6503e9ff77e9ceeffffffff0410270000000000001976a9143493831d5dba660b1e518d0244b305a2ff7078a888ac10270000000000003b6a394e5403100100190000002e789cab562a2d4e2d72492c4954b2aa56ca4d05d1d1d54a1001a590fc927cafd490d4e212a5dad8da5a0062590fae80cb816e000000001976a914e875bd9be3588ddb0ca5fbab1999419418bde9f988ac10270000000000001976a914e875bd9be3588ddb0ca5fbab1999419418bde9f988ac00000000";
            
            // multiple output tx
            var tx = "0100000051945b6402714882984e1fe10eb8ce8b60233dacd9c3f09545c7d6dc3cd34439b2bd738ae9030000006a47304402201cf739e0f98088d2dfb760d97aed81b6f5941fb512a831c27a0b0924c9ecd22f0220200872d043e5d6fff644133fbf175958d7a0ed2dcc2c09f95c632e08e4e65f700121023e333150345a4211c172754f39616a55e3eb817bfde6abd5f6503e9ff77e9ceeffffffff714882984e1fe10eb8ce8b60233dacd9c3f09545c7d6dc3cd34439b2bd738ae9020000006a47304402205db503324c3ba1b9f09540d239245d54da840a99f3b0f5522fdc8147f1be7c2502205c6e5b80775661b17399cdc1a73259792c0c217378c7026d68bfda46348e19c40121023e333150345a4211c172754f39616a55e3eb817bfde6abd5f6503e9ff77e9ceeffffffff0710270000000000001976a914e875bd9be3588ddb0ca5fbab1999419418bde9f988ac10270000000000001976a91429c96b3934f4274950b0dd6d2c6c5df3a39842b488ac10270000000000001976a91471d2bb76f0a9dbc62350b5c66b20fd940b38377588ac10270000000000001976a91447b1695d00159b8cb7ab9b56f55c5201087854de88ac1027000000000000896a4c864e54031004001401140214031400000075789c258c310bc2301046ff4ab85993bd9ba08e2e0d5dc4e12c272d6def8ee6229490ffaed5e983ef3d5e819c683da321340516daf75ee07f401c9027b749f6500f05bacbed1add49f58b06334d4d08f8946cfe4d477e99ef65093f33aec8097b1b85e3a6b4a7642276adcea3417dd4fa01968f293fa0a87f6e000000001976a914e875bd9be3588ddb0ca5fbab1999419418bde9f988ac10270000000000001976a914e875bd9be3588ddb0ca5fbab1999419418bde9f988ac00000000";
            await Indexer_InitRPCAsync("");
            await Node.ProcessBroadcastedTransaction(tx);
        }

        #endregion

        #region ntp1Tests

        [TestEntry]
        public static void Indexer_ParseNTP1Script(string param)
        {
            Indexer_ParseNTP1ScriptAsync(param);
        }
        public static async Task Indexer_ParseNTP1ScriptAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
                //param = "4e540310010020120000005f789cab562a2d4e2d72492c4954b2aa56ca4d05d1d1d54abea9c5c589e9a94a564a21198979d90a95f9a50a69f9450aa5c59979e90a61ae7e6e217a0a19252505c556fafa8949f9a5257a65a9ba7969257ac9f9b9fa4ab5b1b5b5007c711f15";
                // param = "4e5403100a00202201202202202203202204202205202206202207202208202209202200000071789cab562a2d4e2d72492c4954b2aa56ca4d05d1d1d54abea9c5c589e9a94a564a4ab53ad54a61ae7e6e210a8e050540818c929282622b7dfdc4a4fcd212bdb254ddbcb412bde4fc5c7db0ca90a2c4bce2c4e492ccfcbc90ca02900121f9d9a9790ac1053999254ab5b1b5b50085e726e9" // multi token output 10 x 200 tokens
                // param = "4e540310010001000002ee789c9553cd52e240107e95d99cb30a0822de5c5141161696586a8987493221212413677ab2048a07b13c71d8c31e7c813dec25f25edb49c09f5a6bab3c25fd4df7d77f5f2f3425996852a0dae1420b58f6bd5968bd53433bd44028a62df585662411431b51d2e71272ac47830cfb16091a5312a58f963b4d7f115bad578c28e2f329cdfd8e14b85ca0e7b14be1ac6f901c6d3269092f028f87f8d45fdf7b84674c4f0fef53910923a18210616e477ce2016153e683e001072e7444d159983ca64094e526901471213339e17263511227c1d34388e1791aa44c1f77c870ca6c3501468085c049d6672438b1e974bd4a1f09785182a1203c3f212125e96f5bacff9879659ff276da011d67d370012279b8bb1bb3cf8ec001fde0c2dfb178b0eb458edc1d0456d0b592e6553f0476723a1e5c56603093f3e0f2faf86aafd1b13c67d8af1e7bb38bb87e3a2fe6849b69030b2492df2c46f92646da614d1f692d2a5dfc1d6983e0fa88b5be97ef662783e6c4e95c9c8de59766cd7486a1188f4f8efdf396e7360c4bd8f533d11b69183bc49961c5185e42cba063f9d59380e6cd2dda6dd9a55e889643a7926119cf794b6ff37ea49d8fe4cd84f79fb4fdb134fc4aaf71d761815fa919c3891a74afbbb67974ced8113d9b471d35ec774c15b75a1f6ff7b6903c3ae1cc37aba6d9aa0b85d28d2e5fcb2f7be5647dcfe0e961be7d79be0083cd201339f7955d70a889dac89cab9ccedbc85c27407d1228993e0628c138c47b785fef11fa159ae7ff885e7fabfac2392f314e205db92157a06fb370db4d577e066d1b28fcb787a29378ae421b3344d374b55ead7fe23b073cc4173222818a6d2b3ab1d3156023010979fc82865452db7b4d33df207969f308a3fc4427ea9935ef4e99c94bec76c414638ba914f3a7d9593a7c5af411854c05143c5fede40b1872252c66ccda36aea15a2e538751e634eae67e8d35caa552b94269ad5adbaf30939a55ab4cf7ac3d56b7cbf5f241a974b057293167bf6a572db42af5578c1730e3c8a82d6f97cbbfd81bf3bb"; // nft one token output
                // VENFT token issuing should be 1000000000000000
                //param = "4e54030156454e4654201f0100201ff000000114789c6550cb6ec23010fc95c86748406942c9b52dbd454845bd541c1c7b036ee287ec0d8846f9f76e42a9507bb23c333b33bb3d931c392b7a86b60153720dac60ef2fe566c7664c42105e3954d64ce8c6137fb6be89880f24502174e089ab2f5faa41ab09eb7c1b58f1d13373355382a627983e4744178a2431e896f3910931d7218da53a28e4ad15c04d705c408885d5c93acb17b2ca78ca1f530979be14ab2a834ca43940bd80e5c36a21d652a6f1b67ca50cad34ec2e6e4ad5fc0089330736ec293c807ffe5954c3f852bf062e24dc7afb090269fac4db0efeec398f6e8d4f50dfd0a91a0de035ea0dbd1a7366bf969d7736c09de593078e249aee1659139550b5ca466785c7e82eefbfe97e18866fdd138c71"; 
                //BDP token issuing should be 1000000000000000
                // param = "4e5403014244502020201f0100201ff000000182789c7d52cb6edb3010fc1582bc26b263ab4ee25b03a3408036356a033d1439acc895c55a7c804bb5700dff7b9752520439f4447066776676c9b3349041aecf328723fa277028d7f261b39557d220e96463b6c1170c08375cbbed4123b39668c0c4447bfa638f3938c686d4935cff384b3fe958cdad23cc972ee748ebd9cce778735d18aac0d1b232f66033f44123788a2c4e950e6e7683f3c5a2d6cb95d1f5fdaabd6bdb39dcad16cbdbdbb6361fda7b6d1a63a06eaae80fece1acc3fd298eae0e0e382bf0e599cd09d3e6654687e5e47c473c71e136859fa83377ff827ec0f7438a6bf19ab961bc2c2a167c8cc74d79b2dbe5648bd7d53fd921c540f846769743e24c22b42277288a926843126ee8b38d3d8a38b550f51fd93d1ce88da62a395f85d503efefa83bb05ea8c7eda79d1869a1be26dd21e504e51599f266e0dba99e0bf51d9b25974ddbdfffb69e84e28d98414fb5df4213b2d554ba28e20bfab4d90bf50574673d7e46489e430af5f151a85d18d2f833de0df07cb95cfe02ae16ce19"; 
                // SPOTT token issuing should be 2000000789
                //param = "4e54030153504f545483b9acb8a8010083b9acb8a8f0000000c9789c958fbd6ac34010845fe5d8da4810210caaddc60ef8ba90e2a45bc96be97e72bb729c08bdbb4fa40aa471b9b333f3310b5823069a05248ce88fc62134707e3b690d3b20e6195316f4057588e718e4e427f2987f16b94b148582ff35a82da5f456a38ca87f22739a189af705fc5f4cd6f3751189dc94656bfa3121d97bdd55b1ba2275f73d57d75427b42f2e7cfafdadade330f5233bdbd2ed67fe62f175308e0a8a3d17be179690cc8045068f65263872a8bfe3068d7e80f56307af28461d9e1db1ae0ffbf06a88";
                // MGA Medigrowth token issuing should be 333
                param = "4e5403014d4741202034d0010034d0f00000018d789c9592516fda301485ff4a94e7b60969bb31a4a9a28192b0a5a430584a5521436e1c2bb11d7c9d0041fcf705d43d744fcb93757d8e8e3f1ddfa319134dccded1d43203f14c38983d3318f5cd2b3306dc28566826c5f90e624695dce9d4e8cf678dcc104b50ff2825364a89a0061fb11ccee7dbd1cce0f0d9eb8b4436e68ae4e5f9cd54eb027b96152b56c10d9592e670b391dc4a580e566c756c9ab27c3b5d3a3062d4aed72e5fac33e0a4b3f396c144a656c560f75062f11d53a298a04db83e14e7ec99becca7abbf187e680444100a1c84361a9968a0873634abe8fae9972f8a24eacfa2b17f77bf7f0a6afaad1bf16d88fe80b5a10989d20214a6ac408388d870659e93b56ca89aeeb10d565745fe38b56ff7c528fb4adcc715f245ec0d6b7b7e377cf4bd9f6db03e6a611b23cc896843f12ac7ab3179ad61bf769dac63cf06bfbbf5971f2fce2878c66596b5a1f8b43115a0be7c59a8642191e497bae608864c8ca41471bbb2b64e325d4ce693e1340c689dc2e0c55d55ce665a7a023d3ef90fccf7d3e9f40794c52053";
                // QSAT1 token issuing- should be 200000000
                //param = "4e5403015153415431202801002028f0000000b9789c4d8fbb0ec2300c45ffc533b4b4117d6d48cc480836c4e0264e8968d2a84e0754f5df498181c9bae7da3af20c0a03423343189ee44e68091a385f0ed70c36a088e5687c3083fba38679a27105314c63cfd0dc6670df5323e3ee07c7f008c17393a62ef86cbb369ca0659128d39980fd20091d7b94c4891c6caaeb5690d88932935ae7ba42a9f6b9564a932a6a9115455594755bed12efbae8b0c6d2f5e53f568b1da52b5eee51ce341e7f6f595ae7edbe2ccb1b31b44cc3"; 

            var tx = new NTP1Transactions()
            {
                ntp1_opreturn = param,
                tx_type = TxType.TxType_Transfer
            };

            NTP1ScriptHelpers._NTP1ParseScript(tx); //No metadata

            var customDecompressed = StringExt.Decompress(tx.metadata);
            var metadataString = Encoding.UTF8.GetString(customDecompressed);

            foreach(var inst in tx.ntp1_instruct_list)
                await Console.Out.WriteLineAsync($"Token amount in tx is: {inst.amount} on {inst.vout_num} output");

            await Console.Out.WriteLineAsync($"Metadata In the transaction are: {metadataString}");

        }

        [TestEntry]
        public static void Indexer_CreateNTP1Script(string param)
        {
            Indexer_CreateNTP1ScriptAsync(param);
        }
        public static async Task Indexer_CreateNTP1ScriptAsync(string param)
        {

            Metadata2 meta = new Metadata2()
            {
                UserData = new UserData3()
                {
                    Meta = new List<JObject>()
                    {
                        new JObject { ["test"] = "skriptu" },
                        new JObject { ["totoje"] = "cool" },
                    }
                }
            };

            var metastring = JsonConvert.SerializeObject(meta);
            await Console.Out.WriteLineAsync($"Metadata In the transaction before creating script: {metastring}");
            var metacomprimed = StringExt.Compress(Encoding.UTF8.GetBytes(metastring));

            var index = 0;
            List<NTP1Instructions> TiList = new List<NTP1Instructions>();
            //Now make the transfer instruction
            NTP1Instructions ti = new NTP1Instructions();
            ti.amount = Convert.ToUInt64("13");
            ti.vout_num = index;
            TiList.Add(ti);
            index++;

            //Create the hex op_return
            string ti_script = NTP1ScriptHelpers._NTP1CreateTransferScript(TiList, metacomprimed); //No metadata

            var tx = new NTP1Transactions()
            {
                ntp1_opreturn = ti_script
            };

            NTP1ScriptHelpers._NTP1ParseScript(tx); //No metadata

            var customDecompressed1 = StringExt.Decompress(tx.metadata);
            var metadataString = Encoding.UTF8.GetString(customDecompressed1);

            await Console.Out.WriteLineAsync($"Token amount in tx is: {tx.ntp1_instruct_list.FirstOrDefault().amount} on {tx.ntp1_instruct_list.FirstOrDefault().vout_num} output");
            await Console.Out.WriteLineAsync($"Metadata In the transaction are: {metadataString}");
            await Console.Out.WriteLineAsync($"OP_RETURN Script: {tx.ntp1_opreturn}");

        }


        [TestEntry]
        public static void Indexer_CreateIsuanceNTP1Script(string param)
        {
            Indexer_CreateIsuanceNTP1ScriptAsync(param);
        }
        public static async Task Indexer_CreateIsuanceNTP1ScriptAsync(string param)
        {
            if (string.IsNullOrEmpty(param))
                // VENFT token issuing should be 1000000000000000
                //param = "4e54030156454e4654201f0100201ff000000114789c6550cb6ec23010fc95c86748406942c9b52dbd454845bd541c1c7b036ee287ec0d8846f9f76e42a9507bb23c333b33bb3d931c392b7a86b60153720dac60ef2fe566c7664c42105e3954d64ce8c6137fb6be89880f24502174e089ab2f5faa41ab09eb7c1b58f1d13373355382a627983e4744178a2431e896f3910931d7218da53a28e4ad15c04d705c408885d5c93acb17b2ca78ca1f530979be14ab2a834ca43940bd80e5c36a21d652a6f1b67ca50cad34ec2e6e4ad5fc0089330736ec293c807ffe5954c3f852bf062e24dc7afb090269fac4db0efeec398f6e8d4f50dfd0a91a0de035ea0dbd1a7366bf969d7736c09de593078e249aee1659139550b5ca466785c7e82eefbfe97e18866fdd138c71"; 
                //BDP token issuing should be 1000000000000000
                 param = "4e5403014244502020201f0100201ff000000182789c7d52cb6edb3010fc1582bc26b263ab4ee25b03a3408036356a033d1439acc895c55a7c804bb5700dff7b9752520439f4447066776676c9b3349041aecf328723fa277028d7f261b39557d220e96463b6c1170c08375cbbed4123b39668c0c4447bfa638f3938c686d4935cff384b3fe958cdad23cc972ee748ebd9cce778735d18aac0d1b232f66033f44123788a2c4e950e6e7683f3c5a2d6cb95d1f5fdaabd6bdb39dcad16cbdbdbb6361fda7b6d1a63a06eaae80fece1acc3fd298eae0e0e382bf0e599cd09d3e6654687e5e47c473c71e136859fa83377ff827ec0f7438a6bf19ab961bc2c2a167c8cc74d79b2dbe5648bd7d53fd921c540f846769743e24c22b42277288a926843126ee8b38d3d8a38b550f51fd93d1ce88da62a395f85d503efefa83bb05ea8c7eda79d1869a1be26dd21e504e51599f266e0dba99e0bf51d9b25974ddbdfffb69e84e28d98414fb5df4213b2d554ba28e20bfab4d90bf50574673d7e46489e430af5f151a85d18d2f833de0df07cb95cfe02ae16ce19"; 
                // SPOTT token issuing should be 2000000789
                //param = "4e54030153504f545483b9acb8a8010083b9acb8a8f0000000c9789c958fbd6ac34010845fe5d8da4810210caaddc60ef8ba90e2a45bc96be97e72bb729c08bdbb4fa40aa471b9b333f3310b5823069a05248ce88fc62134707e3b690d3b20e6195316f4057588e718e4e427f2987f16b94b148582ff35a82da5f456a38ca87f22739a189af705fc5f4cd6f3751189dc94656bfa3121d97bdd55b1ba2275f73d57d75427b42f2e7cfafdadade330f5233bdbd2ed67fe62f175308e0a8a3d17be179690cc8045068f65263872a8bfe3068d7e80f56307af28461d9e1db1ae0ffbf06a88";
                // MGA Medigrowth token issuing should be 333
                //param = "4e5403014d4741202034d0010034d0f00000018d789c9592516fda301485ff4a94e7b60969bb31a4a9a28192b0a5a430584a5521436e1c2bb11d7c9d0041fcf705d43d744fcb93757d8e8e3f1ddfa319134dccded1d43203f14c38983d3318f5cd2b3306dc28566826c5f90e624695dce9d4e8cf678dcc104b50ff2825364a89a0061fb11ccee7dbd1cce0f0d9eb8b4436e68ae4e5f9cd54eb027b96152b56c10d9592e670b391dc4a580e566c756c9ab27c3b5d3a3062d4aed72e5fac33e0a4b3f396c144a656c560f75062f11d53a298a04db83e14e7ec99becca7abbf187e680444100a1c84361a9968a0873634abe8fae9972f8a24eacfa2b17f77bf7f0a6afaad1bf16d88fe80b5a10989d20214a6ac408388d870659e93b56ca89aeeb10d565745fe38b56ff7c528fb4adcc715f245ec0d6b7b7e377cf4bd9f6db03e6a611b23cc896843f12ac7ab3179ad61bf769dac63cf06bfbbf5971f2fce2878c66596b5a1f8b43115a0be7c59a8642191e497bae608864c8ca41471bbb2b64e325d4ce693e1340c689dc2e0c55d55ce665a7a023d3ef90fccf7d3e9f40794c52053";
                // QSAT1 token issuing- should be 200000000
                //param = "4e5403015153415431202801002028f0000000b9789c4d8fbb0ec2300c45ffc533b4b4117d6d48cc480836c4e0264e8968d2a84e0754f5df498181c9bae7da3af20c0a03423343189ee44e68091a385f0ed70c36a088e5687c3083fba38679a27105314c63cfd0dc6670df5323e3ee07c7f008c17393a62ef86cbb369ca0659128d39980fd20091d7b94c4891c6caaeb5690d88932935ae7ba42a9f6b9564a932a6a9115455594755bed12efbae8b0c6d2f5e53f568b1da52b5eee51ce341e7f6f595ae7edbe2ccb1b31b44cc3"; 

            var tx = new NTP1Transactions() { ntp1_opreturn = param };

            NTP1ScriptHelpers._NTP1ParseScript(tx); //No metadata

            var customDecompressed = StringExt.Decompress(tx.metadata);
            var metadataString = Encoding.UTF8.GetString(customDecompressed);

            await Console.Out.WriteLineAsync($"Token amount in tx is: {tx.ntp1_instruct_list.FirstOrDefault().amount} on {tx.ntp1_instruct_list.FirstOrDefault().vout_num} output");
            await Console.Out.WriteLineAsync($"Metadata In the transaction are: {metadataString}");
            await Console.Out.WriteLineAsync($"OP_RETURN Script: {tx.ntp1_opreturn}");
            await Console.Out.WriteLineAsync($"Issued Amount: {tx.tokenIssueAmount}");
            await Console.Out.WriteLineAsync($"Token symbol: {tx.tokenSymbol}");

            Console.WriteLine("");
            Console.WriteLine("");
            await Console.Out.WriteLineAsync($"Metadata In the transaction before creating script: {metadataString}");
            var toCompress = Encoding.UTF8.GetBytes(metadataString);
            var metacomprimed = StringExt.Compress(toCompress);
            var metac = NTP1ScriptHelpers.ConvertByteArrayToHexString(metacomprimed);

            var metacB = NTP1ScriptHelpers.ConvertHexStringToByteArray(metac);
                
            var customDecompressedC = StringExt.Decompress(metacB);
            var metadataStringC = Encoding.UTF8.GetString(customDecompressed);

            var txmeta = NTP1ScriptHelpers.ConvertByteArrayToHexString(tx.metadata);
            if (metac == txmeta)
                Console.WriteLine("Match!");

            if (metadataString == metadataStringC)
                 Console.WriteLine("Match!");

            List<NTP1Instructions> TiList = new List<NTP1Instructions>();
            //Now make the transfer instruction
            NTP1Instructions ti = new NTP1Instructions();
            ti.amount = Convert.ToUInt64(tx.tokenIssueAmount);
            ti.vout_num = 0;
            TiList.Add(ti);

            var flags = new IssuanceFlags() 
            { 
                Divisibility = 7, 
                AggregationPolicy = AggregationPolicy.Aggregatable, 
                Locked = true 
            };

            //Create the hex op_return
            string ti_script = NTP1ScriptHelpers._NTP1CreateIsseueScript(TiList, metacomprimed, tx.tokenSymbol, flags); //No metadata

            var tx1 = new NTP1Transactions() { ntp1_opreturn = ti_script };

            if (param == tx1.ntp1_opreturn)
                Console.WriteLine("Match!");

            NTP1ScriptHelpers._NTP1ParseScript(tx1); //No metadata

            var customDecompressed1 = StringExt.Decompress(tx1.metadata);
            var metadataString1 = Encoding.UTF8.GetString(customDecompressed1);

            if (metadataString == metadataString1)
                Console.WriteLine("Match!");

            await Console.Out.WriteLineAsync($"Token amount in tx is: {tx1.ntp1_instruct_list.FirstOrDefault().amount} on {tx1.ntp1_instruct_list.FirstOrDefault().vout_num} output");
            await Console.Out.WriteLineAsync($"Metadata In the transaction are: {metadataString}");
            await Console.Out.WriteLineAsync($"OP_RETURN Script: {tx1.ntp1_opreturn}");
            await Console.Out.WriteLineAsync($"Issued Amount: {tx1.tokenIssueAmount}");
            await Console.Out.WriteLineAsync($"Token symbol: {tx1.tokenSymbol}");

        }
        #endregion

    }
}
