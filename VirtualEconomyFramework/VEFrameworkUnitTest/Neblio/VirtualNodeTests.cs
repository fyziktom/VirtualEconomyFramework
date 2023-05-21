﻿using Moq;
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
            foreach(var tx in txs)
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
                Assert.Equal(20, utxo1.TokenAmount);
                Assert.Equal("VENFT", utxo1.TokenSymbol);
                Assert.Equal(NFTHelpers.TokenId, utxo1.TokenId);
                Assert.Equal("NPivBSuWnt55d4eZjU1iH3W2U6dMksnobo", utxo1.OwnerAddress); 
                Assert.True(utxo1.Indexed);
            }
        }
    }
}
