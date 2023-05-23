using Moq;
using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using VEDriversLite;
using VEDriversLite.Common;
using VEDriversLite.Indexer;
using VEDriversLite.Indexer.Dto;
using VEDriversLite.Neblio;
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT;
using VEDriversLite.NFT.Dto;
using VEDriversLite.Security;
using VEFrameworkUnitTest.Neblio.Common;
using Xunit;

namespace VEFrameworkUnitTest.Neblio
{
    public class VirtualNodetests
    {

        private VirtualNode getNode()
        {
            var node = new VirtualNode();
            node.QTRPConfig = new VEDriversLite.Common.QTRPCConfig();
            node.InitClient(node.QTRPConfig);

            return node;
        }


        [Fact]
        public async void GetSplitTokenTransaction_Valid_Test()
        {
            var resp = JsonConvert.DeserializeObject<RpcResponse>(NeblioTestHelpers.SplitNeblioTrokensTransaction);
            var transactionObject = JsonConvert.DeserializeObject<GetTransactionInfoResponse>(resp.Result.ToString());

            var dto = new
            {
                result = transactionObject
            };
            var mockresponse = JsonConvert.SerializeObject(dto);

            var node = getNode();
            var fakeRpc = new FakeQTWalletRPCClient(node.QTRPConfig);
            // load fake blocks info
            foreach (var item in NeblioTestHelpers.TestingBlocksInfo)
                fakeRpc.CommandFakeReposnes.TryAdd("getblock," + item.Key, item.Value);
            // load fake tx info
            fakeRpc.CommandFakeReposnes.TryAdd($"gettransaction,{transactionObject.Txid}", mockresponse);
            // load fake client to the node
            node.QTRPCClient = fakeRpc;

            var transaction = await node.GetTx(transactionObject.Txid);

            var inputCount = transaction.Vin.Count();
            var outputCount = transaction.Vout.Count();
            var outputs = transaction.Vout.ToList();

            Assert.Equal(2, inputCount);
            Assert.Equal(7, outputCount);
            Assert.Equal(NeblioTransactionHelpers.MinimumAmount, outputs[0].Value);
            Assert.Equal(20, outputs[0].Tokens.FirstOrDefault().Amount);
            Assert.Equal(20, outputs[1].Tokens.FirstOrDefault().Amount);
            Assert.Equal(20, outputs[2].Tokens.FirstOrDefault().Amount);
            Assert.Equal(20, outputs[3].Tokens.FirstOrDefault().Amount);
            Assert.Contains("OP_RETURN ", outputs[4].ScriptPubKey.Asm.ToString());
            Assert.Equal(1853860000, outputs[5].Value);
            Assert.Equal(5368, outputs[6].Tokens.FirstOrDefault().Amount);

        }

        [Fact]
        public async void GetBlockInfo_Valid_Test()
        {
            var node = getNode();
            var fakeRpc = new FakeQTWalletRPCClient(node.QTRPConfig);
            // load fake blocks info
            foreach (var item in NeblioTestHelpers.TestingBlocksInfo)
                fakeRpc.CommandFakeReposnes.TryAdd("getblock," + item.Key, item.Value);
            // load fake client to the node
            node.QTRPCClient = fakeRpc;

            var block = await node.GetBlock("c697856f16c8fa11197ba48a619687c6817359be62486822b7c92ee98a806fe2");

            Assert.Equal(5051444, block.Height);
            Assert.Equal("c697856f16c8fa11197ba48a619687c6817359be62486822b7c92ee98a806fe2", block.Hash);
            Assert.Equal(1684211602, block.Time);
            var txs = new List<string>()
            {
                "252b90d5eb44d6f52e19dc5cdf6f2f98aed99a162fd724f360bb606f3a575d7a",
                "0ed366a54f4c9802e478aadbc0d6c193a070a4ca49e8f9c1d149da85c7bf2a5d",
                "da367b2f207ae1d62ceef5acb365c49578ac055cc9941ba6cfcb7d86411d9950"
            };
            foreach (var tx in txs)
                Assert.True(block.Tx.Contains(tx));

        }

        [Fact]
        public void IsItPOSTx_Valid_Test()
        {
            var resp = JsonConvert.DeserializeObject<RpcResponse>(NeblioTestHelpers.tx252b90d5eb44d6f52e19dc5cdf6f2f98aed99a162fd724f360bb606f3a575d7a);
            var transactionObject = JsonConvert.DeserializeObject<GetTransactionInfoResponse>(resp.Result.ToString());

            Assert.True(VirtualNode.IsTxPOS(transactionObject));
        }

        [Fact]
        public void IsItPOSTx_False_Test()
        {
            var resp = JsonConvert.DeserializeObject<RpcResponse>(NeblioTestHelpers.txda367b2f207ae1d62ceef5acb365c49578ac055cc9941ba6cfcb7d86411d9950);
            var transactionObject = JsonConvert.DeserializeObject<GetTransactionInfoResponse>(resp.Result.ToString());

            Assert.False(VirtualNode.IsTxPOS(transactionObject));
        }

        [Fact]
        public async void GetLoadBlockTransactions_Valid_Test()
        {
            var node = getNode();
            var fakeRpc = new FakeQTWalletRPCClient(node.QTRPConfig);

            // Load fake tx data
            fakeRpc.CommandFakeReposnes.TryAdd($"gettransaction,252b90d5eb44d6f52e19dc5cdf6f2f98aed99a162fd724f360bb606f3a575d7a", NeblioTestHelpers.tx252b90d5eb44d6f52e19dc5cdf6f2f98aed99a162fd724f360bb606f3a575d7a);
            fakeRpc.CommandFakeReposnes.TryAdd($"gettransaction,0ed366a54f4c9802e478aadbc0d6c193a070a4ca49e8f9c1d149da85c7bf2a5d", NeblioTestHelpers.tx0ed366a54f4c9802e478aadbc0d6c193a070a4ca49e8f9c1d149da85c7bf2a5d);
            fakeRpc.CommandFakeReposnes.TryAdd($"gettransaction,da367b2f207ae1d62ceef5acb365c49578ac055cc9941ba6cfcb7d86411d9950", NeblioTestHelpers.txda367b2f207ae1d62ceef5acb365c49578ac055cc9941ba6cfcb7d86411d9950);

            // load fake blocks info
            foreach (var item in NeblioTestHelpers.TestingBlocksInfo)
                fakeRpc.CommandFakeReposnes.TryAdd("getblock," + item.Key, item.Value);
            // load fake client to the node
            node.QTRPCClient = fakeRpc;

            var block = await node.GetBlock("c697856f16c8fa11197ba48a619687c6817359be62486822b7c92ee98a806fe2");
            await node.GetBlockTransactions(block.Tx.ToList(), (double)block.Height, false);

            Assert.Single(node.Transactions);
            Assert.Single(node.Blocks);
            Assert.Empty(node.Blocks.Where(b => b.Value.Indexed));

            node.Transactions.Clear();
            await node.GetBlockTransactions(block.Tx.ToList(), (double)block.Height, true);

            Assert.Equal(3, node.Transactions.Count);
            Assert.Single(node.Blocks);
            Assert.Empty(node.Blocks.Where(b => b.Value.Indexed));

            node.Transactions.Clear();
            await node.LoadAllBlocksTransactions();

            Assert.Single(node.Transactions);
            Assert.Single(node.Blocks);
            Assert.Single(node.Blocks.Where(b => b.Value.Indexed));
        }

        [Fact]
        public async void ParseOutput_Valid_Test()
        {
            var resp = JsonConvert.DeserializeObject<RpcResponse>(NeblioTestHelpers.SplitNeblioTrokensTransaction);
            var transactionObject = JsonConvert.DeserializeObject<GetTransactionInfoResponse>(resp.Result.ToString());

            var node = getNode();
            var fakeRpc = new FakeQTWalletRPCClient(node.QTRPConfig);
            // load fake blocks info
            foreach (var item in NeblioTestHelpers.TestingBlocksInfo)
                fakeRpc.CommandFakeReposnes.TryAdd("getblock," + item.Key, item.Value);
            // load fake tx info
            fakeRpc.CommandFakeReposnes.TryAdd($"gettransaction,a953a7ba2d1e8bbaa23047b5a45f3c4de4ab71e0343430e563d190dff56db0a4", NeblioTestHelpers.SplitNeblioTrokensTransaction);
            // load fake client to the node
            node.QTRPCClient = fakeRpc;

            var transaction = await node.GetTx(transactionObject.Txid);

            node.Utxos.Clear();
            var outputs = transaction.Vout.ToList();
            var out_0 = outputs[0];
            Assert.NotNull(out_0);
            var time = TimeHelpers.UnixTimestampToDateTime((transaction.Time ?? 0.0) * 1000);

            node.ProcessOutput(outputs[0], transaction.Txid, transaction.Blocktime ?? 0.0, time, string.Empty);

            Assert.Single(node.Utxos);
            var utxo = node.Utxos.Values.FirstOrDefault();
            Assert.True(node.Utxos.ContainsKey($"{transaction.Txid}:{0}"));
            Assert.Equal(NeblioTransactionHelpers.MinimumAmount, utxo.Value);
            Assert.False(utxo.Used);
            Assert.Equal(20, utxo.TokenAmount);
            Assert.Equal("VENFT", utxo.TokenSymbol);
            Assert.Equal(NFTHelpers.TokenId, utxo.TokenId);
            Assert.Equal("Nh7712QcTBT49NA7f9sB3WqX5WjbGLUmo8", utxo.OwnerAddress);
            Assert.True(utxo.Indexed);

            node.ProcessOutput(outputs[0], transaction.Txid, transaction.Blocktime ?? 0.0, time, string.Empty);

            Assert.Single(node.Utxos);

            node.ProcessOutput(outputs[1], transaction.Txid, transaction.Blocktime ?? 0.0, time, string.Empty);

            Assert.Equal(2, node.Utxos.Count);
            Assert.True(node.Utxos.ContainsKey($"{transaction.Txid}:{1}"));
            if (node.Utxos.TryGetValue($"{transaction.Txid}:{1}", out var utxo1))
            {
                Assert.Equal(NeblioTransactionHelpers.MinimumAmount, utxo1.Value);
                Assert.False(utxo1.Used);
                Assert.Equal(20, utxo1.TokenAmount);
                Assert.Equal("VENFT", utxo1.TokenSymbol);
                Assert.Equal(NFTHelpers.TokenId, utxo1.TokenId);
                Assert.Equal("NPivBSuWnt55d4eZjU1iH3W2U6dMksnobo", utxo1.OwnerAddress);
                Assert.True(utxo1.Indexed);
            }
        }

        [Fact]
        public async void ParseInput_Valid_Test()
        {
            var resp = JsonConvert.DeserializeObject<RpcResponse>(NeblioTestHelpers.SplitNeblioTrokensTransaction);
            var transactionObject = JsonConvert.DeserializeObject<GetTransactionInfoResponse>(resp.Result.ToString());

            var node = getNode();
            var fakeRpc = new FakeQTWalletRPCClient(node.QTRPConfig);
            // load fake blocks info
            foreach (var item in NeblioTestHelpers.TestingBlocksInfo)
                fakeRpc.CommandFakeReposnes.TryAdd("getblock," + item.Key, item.Value);
            // load fake tx info
            fakeRpc.CommandFakeReposnes.TryAdd($"gettransaction,a953a7ba2d1e8bbaa23047b5a45f3c4de4ab71e0343430e563d190dff56db0a4", NeblioTestHelpers.SplitNeblioTrokensTransaction);
            // load fake client to the node
            node.QTRPCClient = fakeRpc;

            var transaction = await node.GetTx(transactionObject.Txid);

            node.Utxos.Clear();
            var inputs = transaction.Vin.ToList();
            var in_0 = inputs[0];
            Assert.NotNull(in_0);
            var time = TimeHelpers.UnixTimestampToDateTime((transaction.Time ?? 0.0) * 1000);

            node.ProcessInput(inputs[0], transaction.Txid);

            Assert.Single(node.Utxos);
            var utxo = node.Utxos.Values.FirstOrDefault();
            Assert.True(node.Utxos.ContainsKey($"{in_0.Txid}:{in_0.Vout}"));
            Assert.True(utxo.Used);
            Assert.True(utxo.TokenUtxo);
            Assert.Equal(5448, utxo.TokenAmount);
            Assert.Equal(NFTHelpers.TokenId, utxo.TokenId);
            Assert.Equal("Nh7712QcTBT49NA7f9sB3WqX5WjbGLUmo8", utxo.OwnerAddress);
            Assert.True(utxo.Indexed);

            node.ProcessInput(inputs[0], transaction.Txid);

            Assert.Single(node.Utxos);

            node.ProcessInput(inputs[1], transaction.Txid);

            Assert.Equal(2, node.Utxos.Count);
            var in_1 = inputs[1];
            Assert.True(node.Utxos.ContainsKey($"{in_1.Txid}:{in_1.Vout}"));
            if (node.Utxos.TryGetValue($"{in_1.Txid}:{in_1.Vout}", out var utxo1))
            {
                Assert.True(utxo1.Used);
                Assert.False(utxo1.TokenUtxo);
                Assert.Equal("Nh7712QcTBT49NA7f9sB3WqX5WjbGLUmo8", utxo1.OwnerAddress);
                Assert.True(utxo1.Indexed);
            }
        }

        [Fact]
        public async void ParseTokenMetadata_Valid_Test()
        {
            var resp = JsonConvert.DeserializeObject<RpcResponse>(NeblioTestHelpers.SplitNeblioTrokensTransaction);
            var transactionObject = JsonConvert.DeserializeObject<GetTransactionInfoResponse>(resp.Result.ToString());

            var node = getNode();
            var fakeRpc = new FakeQTWalletRPCClient(node.QTRPConfig);
            // load fake blocks info
            foreach (var item in NeblioTestHelpers.TestingBlocksInfo)
                fakeRpc.CommandFakeReposnes.TryAdd("getblock," + item.Key, item.Value);
            // load fake tx info
            fakeRpc.CommandFakeReposnes.TryAdd($"gettransaction,a953a7ba2d1e8bbaa23047b5a45f3c4de4ab71e0343430e563d190dff56db0a4", NeblioTestHelpers.SplitNeblioTrokensTransaction);
            // load fake client to the node
            node.QTRPCClient = fakeRpc;

            var transaction = await node.GetTx(transactionObject.Txid);

            node.Utxos.Clear();
            var inputs = transaction.Vin.ToList();
            var in_0 = inputs[0];
            Assert.NotNull(in_0);

            var tokMeta = node.ProcessTokensMetadata(inputs[0].Tokens.FirstOrDefault());

            Assert.Equal(NFTHelpers.TokenId, tokMeta.TokenId);
            Assert.Equal(7, tokMeta.Divisibility);
            //Assert.Equal((ulong)1000000000000000, tokMeta.InitialIssuanceAmount); // must be parsed from the issuance Tx
            Assert.Equal("VENFT", tokMeta.TokenName);
            //Assert.Equal("NTUqno5PP8feqZTGSaoGQBEpS4BJRRrFkC", tokMeta.IssueAddress); // must be parsed from the issuance Tx
            Assert.Equal("fyziktom", tokMeta.MetadataOfIssuance.Data.Issuer);
            Assert.Equal("VEFramework NFTs", tokMeta.MetadataOfIssuance.Data.Description);
            Assert.NotEqual("https://ntp1-icons.nebl.io/9560db5a3a83de661c7b5e5c36eef0e1470c9dd3.PNG", tokMeta.MetadataOfIssuance.Data.Urls.FirstOrDefault().url);
            Assert.Equal("https://ntp1-icons.ams3.digitaloceanspaces.com/9560db5a3a83de661c7b5e5c36eef0e1470c9dd3.PNG", tokMeta.MetadataOfIssuance.Data.Urls.FirstOrDefault().url);
            Assert.Equal("49ad447c7fc9dc3b8203a42b76f80dd5b79ccaa3cb7e9723e9ac05fb4c70c32c", tokMeta.IssuanceTxid);
        }


        [Fact]
        public async void GetAllAddressesTest_Valid_Test()
        {
            var add1 = Common.FakeDataGenerator.GetKeyAndAddress();
            var add2 = Common.FakeDataGenerator.GetKeyAndAddress();
            var add3 = Common.FakeDataGenerator.GetKeyAndAddress();

            var node = getNode();
            var utxos = new List<IndexedUtxo>()
            {
                new IndexedUtxo() { TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add1.Item1.ToString() },
                new IndexedUtxo() { TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add2.Item1.ToString() },
                new IndexedUtxo() { TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add3.Item1.ToString() }
            };
            foreach (var u in utxos)
                node.Utxos.TryAdd(u.TransactionHashAndN, u);

            var addresses = node.GetAllAddresses();

            Assert.Equal(3, addresses.Count);
            Assert.Contains(add1.Item1.ToString(), addresses);
            Assert.Contains(add2.Item1.ToString(), addresses);
            Assert.Contains(add3.Item1.ToString(), addresses);

            addresses.Clear();

            var utxo = new IndexedUtxo() { TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add1.Item1.ToString() };

            node.Utxos.TryAdd(utxo.TransactionHashAndN, utxo);
            Assert.Equal(4, node.Utxos.Count);

            addresses = node.GetAllAddresses();

            Assert.Equal(3, addresses.Count);
            Assert.Contains(add1.Item1.ToString(), addresses);
            Assert.Contains(add2.Item1.ToString(), addresses);
            Assert.Contains(add3.Item1.ToString(), addresses);
        }

        [Fact]
        public async void UpdateAddress_Valid_Test()
        {
            var add1 = Common.FakeDataGenerator.GetKeyAndAddress();
            var add2 = Common.FakeDataGenerator.GetKeyAndAddress();

            var node = getNode();
            var utxos = new List<IndexedUtxo>()
            {
                new IndexedUtxo() { TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add1.Item1.ToString() },
                new IndexedUtxo() { TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add1.Item1.ToString() },
                new IndexedUtxo() { TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add2.Item1.ToString() },
                new IndexedUtxo() { TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add2.Item1.ToString() },
                new IndexedUtxo() { TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add2.Item1.ToString() }
            };

            foreach (var u in utxos)
                node.Utxos.TryAdd(u.TransactionHashAndN, u);

            node.UpdateAddressInfo(add1.Item1.ToString(), true);
            if (node.Addresses.TryGetValue(add1.Item1.ToString(), out var a))
            {
                Assert.Equal(2, a.Utxos.Count());

                var utxo = new IndexedUtxo() { TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add1.Item1.ToString() };
                var utxo1 = new IndexedUtxo() { TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add1.Item1.ToString() };

                node.Utxos.TryAdd(utxo.TransactionHashAndN, utxo);
                node.Utxos.TryAdd(utxo1.TransactionHashAndN, utxo1);
                Assert.Equal(2, a.Utxos.Count());

                node.UpdateAddressInfo(add1.Item1.ToString(), true);
                Assert.Equal(4, a.Utxos.Count());

                var utxo2 = new IndexedUtxo() { TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add1.Item1.ToString() };
                var utxo3 = new IndexedUtxo() { TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add1.Item1.ToString() };

                node.Utxos.TryAdd(utxo2.TransactionHashAndN, utxo2);
                node.Utxos.TryAdd(utxo3.TransactionHashAndN, utxo3);
                Assert.Equal(4, a.Utxos.Count());

                // test the force update (force update override the cache refresh time)
                node.UpdateAddressInfo(add1.Item1.ToString(), false);
                Assert.Equal(4, a.Utxos.Count());

                node.UpdateAddressInfo(add1.Item1.ToString(), true);
                Assert.Equal(6, a.Utxos.Count());
            }
        }


        [Fact]
        public async void GetAddressUtxosObjects_Valid_Test()
        {
            var add1 = Common.FakeDataGenerator.GetKeyAndAddress().Item1.ToString();
            var add2 = Common.FakeDataGenerator.GetKeyAndAddress().Item1.ToString();

            var node = getNode();
            var utxos = new List<IndexedUtxo>()
            {
                new IndexedUtxo() { Used = false, TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add1 },
                new IndexedUtxo() { Used = false, TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add1 },
                new IndexedUtxo() { Used = false, TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add2 },
                new IndexedUtxo() { Used = false, TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add2 },
                new IndexedUtxo() { Used = false, TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add2 }
            };

            foreach (var u in utxos)
                node.Utxos.TryAdd(u.TransactionHashAndN, u);

            node.UpdateAddressInfo(add1, true);
            node.UpdateAddressInfo(add2, true);

            var address1Utxos = node.GetAddressUtxosObjects(add1);
            var address2Utxos = node.GetAddressUtxosObjects(add2);

            Assert.Equal(2, address1Utxos.Count());
            Assert.Equal(3, address2Utxos.Count());

            var utxo = new IndexedUtxo() { Used = true, TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add1 };
            var utxo1 = new IndexedUtxo() { Used = true, TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add2 };
            var utxo2 = new IndexedUtxo() { Used = true, TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add2 };

            node.Utxos.TryAdd(utxo.TransactionHashAndN, utxo);
            node.Utxos.TryAdd(utxo1.TransactionHashAndN, utxo1);
            node.Utxos.TryAdd(utxo2.TransactionHashAndN, utxo2);

            node.UpdateAddressInfo(add1, true);
            node.UpdateAddressInfo(add2, true);

            var notused_address1Utxos = node.GetAddressUtxosObjects(add1);
            var notused_address2Utxos = node.GetAddressUtxosObjects(add2);

            Assert.Equal(2, notused_address1Utxos.Count());
            Assert.Equal(3, notused_address2Utxos.Count());

            // add token Utxo
            var tutxo = new IndexedUtxo() { Used = false, TokenUtxo = true, TransactionHash = Guid.NewGuid().ToString(), OwnerAddress = add1 };
            node.Utxos.TryAdd(tutxo.TransactionHashAndN, tutxo);
            node.UpdateAddressInfo(add1, true);

            var token_address1Utxos = node.GetAddressTokenUtxosObjects(add1);

            Assert.Single(token_address1Utxos);
        }



        [Fact]
        public async void GetIndexedBlockFromBlockResponse_Valid_Test()
        {
            var add1 = Common.FakeDataGenerator.GetKeyAndAddress().Item1.ToString();
            var add2 = Common.FakeDataGenerator.GetKeyAndAddress().Item1.ToString();

            var node = getNode();

            var bl = NeblioTestHelpers.TestingBlocksInfo.FirstOrDefault().Value;
            var blockr = JsonConvert.DeserializeObject<RpcResponse>(bl);
            var block = JsonConvert.DeserializeObject<GetBlockResponse>(blockr.Result.ToString());
            Assert.NotNull(block);

            var indexedBlock = node.GetIndexedBlockFromResponse(block);

            Assert.Equal(block.Hash, indexedBlock.Hash);
            Assert.Equal(block.Height, indexedBlock.Height);
            Assert.Equal(block.Tx, indexedBlock.Transactions);
            Assert.Equal(block.Time, TimeHelpers.DateTimeToUnixTimestamp(indexedBlock.Time) / 1000);

        }

        [Fact]
        public async void ConvertIndexedUtxoToUtxosObject_Valid_Test()
        {
            var add1 = FakeDataGenerator.GetKeyAndAddress().Item1.ToString();

            var node = getNode();

            var nuxo = FakeDataGenerator.GetFakeNeblioUtxo(add1, value: 100000000);
            var tuxo = FakeDataGenerator.GetFakeNeblioTokenUtxo(add1, NFTHelpers.TokenId, amount: 1000);

            var utxos = new List<IndexedUtxo>()
            {
                new IndexedUtxo()
                {
                     Blockheight = nuxo.Blockheight ?? 0.0,
                     Blocktime = nuxo.Blocktime ?? 0.0,
                     Used = false,
                     TransactionHash = nuxo.Txid,
                     Index = (int)nuxo.Index,
                     Value = nuxo.Value ?? 0.0,
                     OwnerAddress = add1,
                     TokenUtxo = false
                },
                new IndexedUtxo()
                {
                     Blockheight = tuxo.Blockheight ?? 0.0,
                     Blocktime = tuxo.Blocktime ?? 0.0,
                     Used = false,
                     TransactionHash = tuxo.Txid,
                     Index = (int)tuxo.Index,
                     Value = tuxo.Value ?? 0.0,
                     OwnerAddress = add1,
                     TokenUtxo = true,
                     TokenAmount = 1000,
                     TokenId = NFTHelpers.TokenId,
                     TokenSymbol = "VENFT"
                }
            };

            var indexedUtxos = VirtualNode.ConvertIndexedUtxoToUtxo(utxos);
            Assert.Equal(2, indexedUtxos.Count());

            var nuxo1 = indexedUtxos.FirstOrDefault();
            Assert.Equal(nuxo.Txid, nuxo1.Txid);
            Assert.Equal(nuxo.Blockheight, nuxo1.Blockheight);
            Assert.Equal(nuxo.Value, nuxo1.Value);
            Assert.Equal(nuxo.Blocktime, nuxo1.Blocktime);

            var tuxo1 = indexedUtxos.LastOrDefault();
            Assert.Equal(tuxo.Txid, tuxo1.Txid);
            Assert.Equal(tuxo.Value, tuxo1.Value);
            Assert.Equal(tuxo.Blocktime, tuxo1.Blocktime);
            Assert.Single(tuxo1.Tokens);

            var toks = tuxo1.Tokens.FirstOrDefault();
            Assert.NotNull(toks);
            Assert.Equal(1000, toks.Amount);
            Assert.Equal(NFTHelpers.TokenId, toks.TokenId);

        }

        [Fact]
        public async void ProcessBroadcastedTransactionPureNeblio_Valid_Test()
        {
            var ownerInput = FakeDataGenerator.GetKeyAndAddress();
            var owner = ownerInput.Item1.ToString();
            var ownerKey = ownerInput.Item2;
            var add1 = FakeDataGenerator.GetKeyAndAddress().Item1.ToString();
            
            var node = getNode();
            var iuxos = new List<IndexedUtxo>();
            var hashes = NeblioTestHelpers.HashesOfTxs.Values.ToArray();
            for (int i = 0; i < 3; i++)
            {
                iuxos.Add(new IndexedUtxo()
                {
                    Index = (int)i,
                    TransactionHash = hashes[i],
                    Value = 1.0 * NeblioTransactionHelpers.FromSatToMainRatio,
                    OwnerAddress = owner
                }) ;
            }

            foreach (var u in iuxos)
                node.Utxos.TryAdd(u.TransactionHashAndN, u);

            node.UpdateAddressInfo(owner, true);

            var uuxos = VirtualNode.ConvertIndexedUtxoToUtxo(iuxos);

            var transaction = NeblioTransactionHelpers.GetNeblioTransactionObject(new SendTxData()
            {
                Amount = 0.05,
                SenderAddress = owner,
                ReceiverAddress = add1
            }, uuxos);

            var result = await NeblioTransactionHelpers.SignAndBroadcast(transaction, ownerKey, false, uuxos);

            var ownerUtxosBeforeBroadcast = node.GetAddressUtxosObjects(owner);
            var add1UtxosBeforeBroadcast = node.GetAddressUtxosObjects(add1);
            Assert.Equal(3, ownerUtxosBeforeBroadcast.Count());
            Assert.Empty(add1UtxosBeforeBroadcast);

            await node.ProcessBroadcastedTransaction(result);

            var ownerUtxosAfterBroadcast = node.GetAddressUtxosObjects(owner);
            var add1UtxosAfterBroadcast = node.GetAddressUtxosObjects(add1);
            Assert.Single(ownerUtxosAfterBroadcast);
            Assert.Single(add1UtxosAfterBroadcast);
            Assert.Equal(3, node.UsedUtxos.Count);
            Assert.Equal(5, node.Utxos.Count);

        }

        [Fact]
        public async void ProcessBroadcastedTransactionWithTokenLot_Valid_Test()
        {
            var ownerInput = FakeDataGenerator.GetKeyAndAddress();
            var owner = ownerInput.Item1.ToString();
            var ownerKey = ownerInput.Item2;
            var add1 = FakeDataGenerator.GetKeyAndAddress().Item1.ToString();

            var node = getNode();
            var nuxos = new List<IndexedUtxo>();
            var tuxos = new List<IndexedUtxo>();
            var hashes = NeblioTestHelpers.HashesOfTxs.Values.ToArray();
            var amountOfTokensToSend = 50;

            // nebl utxo to cover the fee
            nuxos.Add(new IndexedUtxo()
            {
                Index = 0,
                TransactionHash = hashes[0],
                Value = 1 * NeblioTransactionHelpers.FromSatToMainRatio,
                OwnerAddress = owner,
                Time = DateTime.UtcNow
            });
            // token utxos
            for (int i = 1; i < 4; i++)
            {
                tuxos.Add(new IndexedUtxo()
                {
                    Index = (int)i,
                    TransactionHash = hashes[i],
                    Value = NeblioTransactionHelpers.MinimumAmount,
                    OwnerAddress = owner,
                    TokenAmount = 100,
                    TokenId = NFTHelpers.TokenId,
                    TokenUtxo = true,
                    TokenSymbol = "VENFT",
                    Time = DateTime.UtcNow
                });
            }

            foreach (var u in nuxos)
                node.Utxos.TryAdd(u.TransactionHashAndN, u);
            foreach (var u in tuxos)
                node.Utxos.TryAdd(u.TransactionHashAndN, u);

            node.UpdateAddressInfo(owner, true);

            var unuxos = VirtualNode.ConvertIndexedUtxoToUtxo(nuxos);
            var utuxos = VirtualNode.ConvertIndexedUtxoToUtxo(tuxos);

            var transaction = NeblioTransactionHelpers.SendTokenLotNewAsync(new SendTokenTxData()
            {
                Amount = amountOfTokensToSend,
                Id = NFTHelpers.TokenId,
                SenderAddress = owner,
                ReceiverAddress = add1,
                Metadata = new Dictionary<string, string>() { { "test", "metadata" } }
            }, unuxos, utuxos);

            var alluxos = new List<Utxos>();
            foreach(var u in unuxos)
                alluxos.Add(u);
            foreach(var u in utuxos)
                alluxos.Add(u);

            var result = await NeblioTransactionHelpers.SignAndBroadcast(transaction, ownerKey, false, alluxos);

            var ownerUtxosBeforeBroadcast = node.GetAddressUtxosObjects(owner);
            var add1UtxosBeforeBroadcast = node.GetAddressUtxosObjects(add1);
            Assert.Equal(4, ownerUtxosBeforeBroadcast.Count());
            Assert.Empty(add1UtxosBeforeBroadcast);

            await node.ProcessBroadcastedTransaction(result);

            var ownerUtxosAfterBroadcast = node.GetAddressUtxosObjects(owner).ToList();
            var add1UtxosAfterBroadcast = node.GetAddressUtxosObjects(add1).ToList();
            Assert.Equal(2, ownerUtxosAfterBroadcast.Count);
            Assert.Single(add1UtxosAfterBroadcast);
            Assert.True(add1UtxosAfterBroadcast[0].TokenUtxo);
            Assert.Equal(amountOfTokensToSend, add1UtxosAfterBroadcast[0].TokenAmount);

            Assert.Equal(ownerUtxosBeforeBroadcast.Where(u => u.TokenUtxo)
                                                 .Select(u => u.TokenAmount).Sum() - amountOfTokensToSend, 
                         ownerUtxosAfterBroadcast.Where(u => u.TokenUtxo)
                                                 .Select(u => u.TokenAmount).Sum());

            Assert.Equal(NFTHelpers.TokenId, add1UtxosAfterBroadcast[0].TokenId);
            Assert.Equal("VENFT", add1UtxosAfterBroadcast[0].TokenSymbol);
            Assert.Equal(4, node.UsedUtxos.Count);
            Assert.Equal(7, node.Utxos.Count);

        }
    }
}
