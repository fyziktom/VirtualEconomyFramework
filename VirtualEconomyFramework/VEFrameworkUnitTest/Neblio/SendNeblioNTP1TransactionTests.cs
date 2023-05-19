using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.Common;
using VEDriversLite.Neblio;
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT;
using VEDriversLite.Security;
using VEFrameworkUnitTest.NFT;
using Xunit;

namespace VEFrameworkUnitTest.Neblio
{
    public class SendNeblioNTP1TransactionTests
    {
        [Fact]
        public async Task CreateOPReturnScriptTest()
        {
            var script = NeblioTransactionHelpers.CreateOPRETURNScript(NTP1ScriptHelpers.UnhexToByteArray(NTP1TestData.NFTOneTokenOutput));
            Assert.NotNull(script);
            Assert.Equal("OP_RETURN " + NTP1TestData.NFTOneTokenOutput, script.ToString());
        }

        [Fact]
        public async Task CreateTokenLotTransactionTest()
        {
            //Arrange           

            var res = Common.FakeDataGenerator.GetKeyAndAddress();

            string address = res.Item1.ToString();

            var res1 = Common.FakeDataGenerator.GetKeyAndAddress();
            string receiver = res1.Item1.ToString();

            var nuxo = Common.FakeDataGenerator.GetFakeNeblioUtxo(address, value:100000000);
            var tuxo = Common.FakeDataGenerator.GetFakeNeblioTokenUtxo(address, NFTHelpers.TokenId, amount:1000);
            var meta = Common.FakeDataGenerator.GetMetadata();
            SendTokenTxData sendTxData = new SendTokenTxData()
            {
                Id = NFTHelpers.TokenId,
                ReceiverAddress = receiver,
                SenderAddress = address,
                Amount = 100,
                Metadata = meta
            };

            var transaction = await NeblioTransactionHelpers.SendTokenLotNewAsync(sendTxData, new List<Utxos> { nuxo }, new List<Utxos> { tuxo });

            Assert.NotNull(transaction);
            Assert.Equal(2, transaction.Inputs.Count);
            Assert.Equal(4, transaction.Outputs.Count);
            var opo = transaction.Outputs.FirstOrDefault(o => o.ScriptPubKey.ToString().Contains("OP_RETURN"))?.ScriptPubKey?.ToString();
            Assert.NotNull(opo);
            var metadata = NeblioAPIHelpers.ParseCustomMetadata(opo);
            Assert.Equal(meta.Count, metadata.Count);
            foreach (var m in metadata)
                Assert.Equal(meta[m.Key], m.Value);

            var tx1 = new NTP1Transactions() { ntp1_opreturn = opo.Replace("OP_RETURN ",string.Empty) };
            NTP1ScriptHelpers._NTP1ParseScript(tx1);
            Assert.Single(tx1.ntp1_instruct_list);
            Assert.Equal((ulong)100, tx1.ntp1_instruct_list.FirstOrDefault().amount);

            Assert.Equal(10000, transaction.Outputs[0].Value.Satoshi);
            Assert.Equal(res1.Item1.ScriptPubKey.ToString(), transaction.Outputs[0].ScriptPubKey.ToString());
            Assert.Equal(10000, transaction.Outputs[1].Value.Satoshi);
            Assert.Equal(99950000, transaction.Outputs[2].Value.Satoshi);
            Assert.Equal(res.Item1.ScriptPubKey.ToString(), transaction.Outputs[2].ScriptPubKey.ToString());
            Assert.Equal(10000, transaction.Outputs[3].Value.Satoshi);
            Assert.Equal(res.Item1.ScriptPubKey.ToString(), transaction.Outputs[3].ScriptPubKey.ToString());
            Assert.Equal(100000000 - 20000, transaction.Outputs.Select(o => o.Value.Satoshi).Sum());
        }

        [Fact]
        public async Task CreateTokenLotAndNeblioTransactionTest()
        {
            //Arrange           

            var res = Common.FakeDataGenerator.GetKeyAndAddress();
            string address = res.Item1.ToString();

            var res1 = Common.FakeDataGenerator.GetKeyAndAddress();
            string receiver = res1.Item1.ToString();

            var nuxo = Common.FakeDataGenerator.GetFakeNeblioUtxo(address, value: 100000000);
            var tuxo = Common.FakeDataGenerator.GetFakeNeblioTokenUtxo(address, NFTHelpers.TokenId, amount: 1000);
            var meta = Common.FakeDataGenerator.GetMetadata();
            SendTokenTxData sendTxData = new SendTokenTxData()
            {
                Id = NFTHelpers.TokenId,
                ReceiverAddress = receiver,
                SenderAddress = address,
                Amount = 100,
                Metadata = meta
            };

            var transaction = await NeblioTransactionHelpers.SendNTP1TokenLotWithPaymentAPIAsync(sendTxData, 0.05, new List<Utxos> { nuxo }, new List<Utxos> { tuxo });

            Assert.NotNull(transaction);
            Assert.Equal(2, transaction.Inputs.Count);
            Assert.Equal(5, transaction.Outputs.Count);
            var opo = transaction.Outputs.FirstOrDefault(o => o.ScriptPubKey.ToString().Contains("OP_RETURN"))?.ScriptPubKey?.ToString();
            Assert.NotNull(opo);
            var metadata = NeblioAPIHelpers.ParseCustomMetadata(opo);
            Assert.Equal(meta.Count, metadata.Count);
            foreach (var m in metadata)
                Assert.Equal(meta[m.Key], m.Value);

            var tx1 = new NTP1Transactions() { ntp1_opreturn = opo.Replace("OP_RETURN ", string.Empty) };
            NTP1ScriptHelpers._NTP1ParseScript(tx1);
            Assert.Single(tx1.ntp1_instruct_list);
            Assert.Equal((ulong)100, tx1.ntp1_instruct_list.FirstOrDefault().amount);

            Assert.Equal(10000, transaction.Outputs[0].Value.Satoshi);
            Assert.Equal(res1.Item1.ScriptPubKey.ToString(), transaction.Outputs[0].ScriptPubKey.ToString());
            Assert.Equal(5000000, transaction.Outputs[1].Value.Satoshi);
            Assert.Equal(res1.Item1.ScriptPubKey.ToString(), transaction.Outputs[1].ScriptPubKey.ToString());
            Assert.Equal(10000, transaction.Outputs[2].Value.Satoshi);
            Assert.Equal(94950000, transaction.Outputs[3].Value.Satoshi);
            Assert.Equal(res.Item1.ScriptPubKey.ToString(), transaction.Outputs[3].ScriptPubKey.ToString());
            Assert.Equal(10000, transaction.Outputs[4].Value.Satoshi);
            Assert.Equal(res.Item1.ScriptPubKey.ToString(), transaction.Outputs[4].ScriptPubKey.ToString());
            Assert.Equal(100000000 - 20000, transaction.Outputs.Select(o => o.Value.Satoshi).Sum());
        }

        [Fact]
        public async Task CreateNFTPaymentAndNeblioTransactionTest()
        {
            //Arrange           

            var res = Common.FakeDataGenerator.GetKeyAndAddress();
            string address = res.Item1.ToString();

            var res1 = Common.FakeDataGenerator.GetKeyAndAddress();
            string receiver = res1.Item1.ToString();

            var nuxo = Common.FakeDataGenerator.GetFakeNeblioUtxo(address, value: 100000000);
            var tuxo = Common.FakeDataGenerator.GetFakeNeblioTokenUtxo(address, NFTHelpers.TokenId, amount: 1);
            var meta = Common.FakeDataGenerator.GetMetadata();
            SendTokenTxData sendTxData = new SendTokenTxData()
            {
                Id = NFTHelpers.TokenId,
                ReceiverAddress = receiver,
                SenderAddress = address,
                Amount = 100,
                Metadata = meta
            };

            var transaction = await NeblioTransactionHelpers.SendNTP1TokenWithPaymentAPIAsync(sendTxData, 0.05, new List<Utxos> { nuxo },  tuxo.Txid, (int)tuxo.Index);

            Assert.NotNull(transaction);
            Assert.Equal(2, transaction.Inputs.Count);
            Assert.Equal(4, transaction.Outputs.Count);
            var opo = transaction.Outputs.FirstOrDefault(o => o.ScriptPubKey.ToString().Contains("OP_RETURN"))?.ScriptPubKey?.ToString();
            Assert.NotNull(opo);
            var metadata = NeblioAPIHelpers.ParseCustomMetadata(opo);
            Assert.Equal(meta.Count, metadata.Count);
            foreach (var m in metadata)
                Assert.Equal(meta[m.Key], m.Value);

            var tx1 = new NTP1Transactions() { ntp1_opreturn = opo.Replace("OP_RETURN ", string.Empty) };
            NTP1ScriptHelpers._NTP1ParseScript(tx1);
            Assert.Single(tx1.ntp1_instruct_list);
            Assert.Equal((ulong)1, tx1.ntp1_instruct_list.FirstOrDefault().amount);

            Assert.Equal(10000, transaction.Outputs[0].Value.Satoshi);
            Assert.Equal(res1.Item1.ScriptPubKey.ToString(), transaction.Outputs[0].ScriptPubKey.ToString());
            Assert.Equal(5000000, transaction.Outputs[1].Value.Satoshi);
            Assert.Equal(res1.Item1.ScriptPubKey.ToString(), transaction.Outputs[0].ScriptPubKey.ToString());
            Assert.Equal(10000, transaction.Outputs[2].Value.Satoshi);
            Assert.Equal(94950000, transaction.Outputs[3].Value.Satoshi);
            Assert.Equal(res.Item1.ScriptPubKey.ToString(), transaction.Outputs[3].ScriptPubKey.ToString());
            Assert.Equal(100000000 - 30000, transaction.Outputs.Select(o => o.Value.Satoshi).Sum());
        }

        [Fact]
        public async Task CreateIssueTokensTransactionTest()
        {
            //Arrange           

            var res = Common.FakeDataGenerator.GetKeyAndAddress();
            string address = res.Item1.ToString();

            var res1 = Common.FakeDataGenerator.GetKeyAndAddress();
            string receiver = res1.Item1.ToString();

            var nuxo = Common.FakeDataGenerator.GetFakeNeblioUtxo(address, value: 2000000000); // 20 NEBL
            var meta = Common.FakeDataGenerator.GetMetadata();

            ulong amountOfTokens = 1000000;
            var tokenSymbol = "PEACE";
            var issuerNick = "fyziktom";
            var description = "Peace for all";
            var imageLink = "https://ve-framework.com/ipfs/QmP3uXtMK7A7V38xtANTGrJgK43ZZkmCLh4R9WFKFhwgpA";
            var imageFileName = "icon.png";

            var dto = await NFTHelpers.GetTokenIssueTxData(address, 
                                                           receiver, 
                                                           amountOfTokens, 
                                                           tokenSymbol, 
                                                           issuerNick, 
                                                           description, 
                                                           imageLink, 
                                                           imageFileName, 
                                                           "", 
                                                           meta);

            var transaction = await NeblioTransactionHelpers.IssueTokensAsync(dto, new List<Utxos> { nuxo });

            Assert.NotNull(transaction);
            Assert.Single(transaction.Inputs);
            Assert.Equal(3, transaction.Outputs.Count);
            var opo = transaction.Outputs.FirstOrDefault(o => o.ScriptPubKey.ToString().Contains("OP_RETURN"))?.ScriptPubKey?.ToString();
            Assert.NotNull(opo);
            var tx1 = new NTP1Transactions() { ntp1_opreturn = opo.Replace("OP_RETURN ", string.Empty) };
            NTP1ScriptHelpers._NTP1ParseScript(tx1);
            Assert.Single(tx1.ntp1_instruct_list);
            Assert.Equal((ulong)amountOfTokens, tx1.ntp1_instruct_list.FirstOrDefault().amount);

            var customDecompressed1 = StringExt.Decompress(tx1.metadata);
            var metadataString1 = Encoding.UTF8.GetString(customDecompressed1);
            var mi = JsonConvert.DeserializeObject<MetadataOfIssuance>(metadataString1);
            Assert.NotNull(mi);
            Assert.Equal(issuerNick, mi.Data.Issuer);
            Assert.Equal(description, mi.Data.Description);
            Assert.Equal(tokenSymbol, mi.Data.TokenName);
            Assert.Equal(imageLink, mi.Data.Urls.FirstOrDefault()?.url);

            Assert.Equal(10000, transaction.Outputs[0].Value.Satoshi);
            Assert.Equal(res1.Item1.ScriptPubKey.ToString(), transaction.Outputs[0].ScriptPubKey.ToString());
            Assert.Equal(10000, transaction.Outputs[1].Value.Satoshi);
            Assert.Equal(999950000, transaction.Outputs[2].Value.Satoshi);
            Assert.Equal(res.Item1.ScriptPubKey.ToString(), transaction.Outputs[2].ScriptPubKey.ToString());
            Assert.Equal(2000000000 - 1000000000 - 30000, transaction.Outputs.Select(o => o.Value.Satoshi).Sum());
        }

        [Fact]
        public async Task CreateNFTMintingTxTransactionTest()
        {
            //Arrange           

            var res = Common.FakeDataGenerator.GetKeyAndAddress();
            string address = res.Item1.ToString();

            var res1 = Common.FakeDataGenerator.GetKeyAndAddress();
            string receiver = res1.Item1.ToString();

            var nuxo = Common.FakeDataGenerator.GetFakeNeblioUtxo(address, value: 100000000);
            var tuxo = Common.FakeDataGenerator.GetFakeNeblioTokenUtxo(address, NFTHelpers.TokenId, amount: 100);
            var meta = Common.FakeDataGenerator.GetMetadata();
            MintNFTData sendTxData = new MintNFTData()
            {
                Id = NFTHelpers.TokenId,
                ReceiverAddress = receiver,
                SenderAddress = address,
                Metadata = meta
            };

            var transaction = await NeblioTransactionHelpers.MintMultiNFTTokenAsyncInternal(sendTxData, 
                                                                                            0, 
                                                                                            new List<Utxos> { nuxo }, 
                                                                                            new List<Utxos> { tuxo },
                                                                                            false);

            Assert.NotNull(transaction);
            Assert.Equal(2, transaction.Inputs.Count);
            Assert.Equal(4, transaction.Outputs.Count);
            var opo = transaction.Outputs.FirstOrDefault(o => o.ScriptPubKey.ToString().Contains("OP_RETURN"))?.ScriptPubKey?.ToString();
            Assert.NotNull(opo);
            var metadata = NeblioAPIHelpers.ParseCustomMetadata(opo);
            Assert.Equal(12, metadata.Count);
            foreach (var m in metadata)
                Assert.Equal(meta[m.Key], m.Value);
            Assert.Contains("NFT FirstTx", metadata.Keys);
            Assert.Contains("SourceUtxo", metadata.Keys);

            var tx1 = new NTP1Transactions() { ntp1_opreturn = opo.Replace("OP_RETURN ", string.Empty) };
            NTP1ScriptHelpers._NTP1ParseScript(tx1);
            Assert.Single(tx1.ntp1_instruct_list);
            Assert.Equal((ulong)1, tx1.ntp1_instruct_list.FirstOrDefault().amount);

            Assert.Equal(10000, transaction.Outputs[0].Value.Satoshi);
            Assert.Equal(res1.Item1.ScriptPubKey.ToString(), transaction.Outputs[0].ScriptPubKey.ToString());
            Assert.Equal(10000, transaction.Outputs[1].Value.Satoshi);
            Assert.Equal(99950000, transaction.Outputs[2].Value.Satoshi);
            Assert.Equal(res.Item1.ScriptPubKey.ToString(), transaction.Outputs[2].ScriptPubKey.ToString());
            Assert.Equal(10000, transaction.Outputs[3].Value.Satoshi);
            Assert.Equal(res.Item1.ScriptPubKey.ToString(), transaction.Outputs[3].ScriptPubKey.ToString());
            Assert.Equal(100000000 - 20000, transaction.Outputs.Select(o => o.Value.Satoshi).Sum());
        }

        [Fact]
        public async Task CreateNFTMintingWith5CopiesTransactionTest()
        {
            //Arrange           

            var res = Common.FakeDataGenerator.GetKeyAndAddress();
            string address = res.Item1.ToString();

            var res1 = Common.FakeDataGenerator.GetKeyAndAddress();
            string receiver = res1.Item1.ToString();

            var nuxo = Common.FakeDataGenerator.GetFakeNeblioUtxo(address, value: 100000000);
            var tuxo = Common.FakeDataGenerator.GetFakeNeblioTokenUtxo(address, NFTHelpers.TokenId, amount: 100);
            var meta = Common.FakeDataGenerator.GetMetadata();

            var numOfCopies = 5;
            MintNFTData sendTxData = new MintNFTData()
            {
                Id = NFTHelpers.TokenId,
                ReceiverAddress = receiver,
                SenderAddress = address,
                Metadata = meta
            };

            var transaction = await NeblioTransactionHelpers.MintMultiNFTTokenAsyncInternal(sendTxData,
                                                                                            numOfCopies,
                                                                                            new List<Utxos> { nuxo },
                                                                                            new List<Utxos> { tuxo },
                                                                                            false);

            Assert.NotNull(transaction);
            Assert.Equal(2, transaction.Inputs.Count);
            Assert.Equal(9, transaction.Outputs.Count);
            var opo = transaction.Outputs.FirstOrDefault(o => o.ScriptPubKey.ToString().Contains("OP_RETURN"))?.ScriptPubKey?.ToString();
            Assert.NotNull(opo);
            var metadata = NeblioAPIHelpers.ParseCustomMetadata(opo);
            Assert.Equal(12, metadata.Count);
            foreach (var m in metadata)
                Assert.Equal(meta[m.Key], m.Value);
            Assert.Contains("NFT FirstTx", metadata.Keys);
            Assert.Contains("SourceUtxo", metadata.Keys);

            var tx1 = new NTP1Transactions() { ntp1_opreturn = opo.Replace("OP_RETURN ", string.Empty) };
            NTP1ScriptHelpers._NTP1ParseScript(tx1);
            Assert.Equal(6, tx1.ntp1_instruct_list.Count);
            var index = 0;
            foreach (var ins in tx1.ntp1_instruct_list)
            {
                Assert.Equal((ulong)1, ins.amount);
                Assert.Equal(index, ins.vout_num);
            
                Assert.Equal(10000, transaction.Outputs[0].Value.Satoshi);
                Assert.Equal(res1.Item1.ScriptPubKey.ToString(), transaction.Outputs[index].ScriptPubKey.ToString());
                index++;
            }

            Assert.Equal(10000, transaction.Outputs[index].Value.Satoshi);
            Assert.Equal(99950000 - numOfCopies * 10000, transaction.Outputs[index + 1].Value.Satoshi);
            Assert.Equal(res.Item1.ScriptPubKey.ToString(), transaction.Outputs[index + 1].ScriptPubKey.ToString());
            Assert.Equal(10000, transaction.Outputs[index + 2].Value.Satoshi);
            Assert.Equal(res.Item1.ScriptPubKey.ToString(), transaction.Outputs[index + 2].ScriptPubKey.ToString());
            Assert.Equal(100000000 - 20000, transaction.Outputs.Select(o => o.Value.Satoshi).Sum());
        }

    }
}
