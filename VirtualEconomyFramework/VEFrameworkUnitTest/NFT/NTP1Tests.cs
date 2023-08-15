using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Neblio;
using Xunit;
using VEDriversLite.Common;

namespace VEFrameworkUnitTest.NFT
{
    public class NTP1Tests
    {
        [Fact]
        public void ParseNTP1IssueTx()
        {
            
            var tx = new NTP1Transactions()
            {
                ntp1_opreturn = NTP1TestData.VENFTokenIssueNTP1Data
            };

            NTP1ScriptHelpers._NTP1ParseScript(tx); //No metadata

            Assert.NotNull(tx);
            Assert.Equal("VENFT", tx.tokenSymbol);
            Assert.Equal(Convert.ToString(1000000000000000), Convert.ToString(tx.tokenIssueAmount));
            Assert.Single(tx.ntp1_instruct_list);
            Assert.Equal(Convert.ToString(1000000000000000), Convert.ToString(tx.ntp1_instruct_list.FirstOrDefault().amount));
            Assert.Equal(0, tx.ntp1_instruct_list.FirstOrDefault().vout_num);

            tx = new NTP1Transactions()
            {
                ntp1_opreturn = NTP1TestData.MediGrowTokenIssueNTP1Data
            };

            NTP1ScriptHelpers._NTP1ParseScript(tx); //No metadata

            Assert.NotNull(tx);
            Assert.Equal("MGA", tx.tokenSymbol);
            Assert.Equal((ulong)333, tx.tokenIssueAmount);
            Assert.Single(tx.ntp1_instruct_list);
            Assert.Equal((ulong)333, tx.ntp1_instruct_list.FirstOrDefault().amount);
            Assert.Equal(0, tx.ntp1_instruct_list.FirstOrDefault().vout_num);
        }

        [Fact]
        public void ParseNFTTransaction()
        {

            var tx = new NTP1Transactions()
            {
                ntp1_opreturn = NTP1TestData.NFTOneTokenOutput
            };

            NTP1ScriptHelpers._NTP1ParseScript(tx); //No metadata

            Assert.NotNull(tx);
            Assert.Single(tx.ntp1_instruct_list);
            Assert.Equal((ulong)1, tx.ntp1_instruct_list.FirstOrDefault().amount);
            Assert.Equal(0, tx.ntp1_instruct_list.FirstOrDefault().vout_num);
        }
        [Fact]
        public void ParseMultiTokenOutput()
        {

            var tx = new NTP1Transactions()
            {
                ntp1_opreturn = NTP1TestData.NFTMultiTokenOutput
            };

            NTP1ScriptHelpers._NTP1ParseScript(tx); //No metadata

            Assert.NotNull(tx);
            Assert.Equal(10, tx.ntp1_instruct_list.Count);
            for (var output = 0; output < tx.ntp1_instruct_list.Count; output++)
            {
                Assert.Equal((ulong)200, tx.ntp1_instruct_list[output].amount);
                Assert.Equal(output, tx.ntp1_instruct_list[output].vout_num);
            }
        }

        [Fact]
        public void CreateOneOutputTokenOutputNTP1Data()
        {
            var metacomprimed = StringExt.Compress(Encoding.UTF8.GetBytes(NTP1TestData.TestTextForMetadata));

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

            Assert.NotNull(ti_script);
            Assert.Equal("4e54031001000d00000026789cf34dcc4e5528c94855c84d2d494c492c4954482f4a4d2c51484c4fcccc530400a6ee0ab5", ti_script);
            
        }

        [Fact]
        public void CreateMultiTokenOutputNTP1Data()
        {
            var metacomprimed = StringExt.Compress(Encoding.UTF8.GetBytes(NTP1TestData.TestTextForMetadata));

            List<NTP1Instructions> TiList = new List<NTP1Instructions>();
            //Now make the transfer instruction
            for (int i = 0; i < 10; i++)
            {
                NTP1Instructions ti = new NTP1Instructions();
                ti.amount = Convert.ToUInt64("100");
                ti.vout_num = i;
                TiList.Add(ti);
            }
            //Create the hex op_return
            string ti_script = NTP1ScriptHelpers._NTP1CreateTransferScript(TiList, metacomprimed); //No metadata

            Assert.NotNull(ti_script);
            Assert.Equal("4e5403100a00201201201202201203201204201205201206201207201208201209201200000026789cf34dcc4e5528c94855c84d2d494c492c4954482f4a4d2c51484c4fcccc530400a6ee0ab5", ti_script);

        }

        [Fact]
        public void CreateIssueScriptNTP1Data()
        {
            var metacomprimed = StringExt.Compress(Encoding.UTF8.GetBytes(NTP1TestData.TestTextForMetadata));
            List<NTP1Instructions> TiList = new List<NTP1Instructions>();
            //Now make the transfer instruction
            NTP1Instructions ti = new NTP1Instructions();
            ti.amount = Convert.ToUInt64(1000000000000000);
            ti.vout_num = 0;
            TiList.Add(ti);

            var flags = new IssuanceFlags()
            {
                Divisibility = 7,
                AggregationPolicy = AggregationPolicy.Aggregatable,
                Locked = true
            };

            //Create the hex op_return
            string ti_script = NTP1ScriptHelpers._NTP1CreateIsseueScript(TiList, metacomprimed, "VENFT", flags); //No metadata

            Assert.NotNull(ti_script);
            Assert.Equal("4e54030156454e4654201f0100201ff000000026789cf34dcc4e5528c94855c84d2d494c492c4954482f4a4d2c51484c4fcccc530400a6ee0ab5", ti_script);

            var tx1 = new NTP1Transactions() { ntp1_opreturn = ti_script };
            NTP1ScriptHelpers._NTP1ParseScript(tx1); //No metadata

            var customDecompressed1 = StringExt.Decompress(tx1.metadata);
            var metadataString1 = Encoding.UTF8.GetString(customDecompressed1);

            Assert.Equal(NTP1TestData.TestTextForMetadata, metadataString1);
            Assert.Equal((ulong)1000000000000000, tx1.tokenIssueAmount);
            Assert.Equal("VENFT", tx1.tokenSymbol);
        }

        [Fact]
        public void NTP1AmountHexToNumber()
        {
            var res = NTP1ScriptHelpers._NTP1ByteArrayToNum(NTP1ScriptHelpers.UnhexToByteArray("69892a92"));
            Assert.Equal((ulong)999901700, res);
            
            res = NTP1ScriptHelpers._NTP1ByteArrayToNum(NTP1ScriptHelpers.UnhexToByteArray("c007b60b6f687a"));
            Assert.Equal((ulong)8478457292922, res);
            res = NTP1ScriptHelpers._NTP1ByteArrayToNum(NTP1ScriptHelpers.UnhexToByteArray("40ef54"));
            Assert.Equal((ulong)38290000, res);
            res = NTP1ScriptHelpers._NTP1ByteArrayToNum(NTP1ScriptHelpers.UnhexToByteArray("201f"));
            Assert.Equal((ulong)1000000000000000, res);

            res = NTP1ScriptHelpers._NTP1ByteArrayToNum(NTP1ScriptHelpers.UnhexToByteArray("60b0b460"));
            Assert.Equal((ulong)723782, res);
            res = NTP1ScriptHelpers._NTP1ByteArrayToNum(NTP1ScriptHelpers.UnhexToByteArray("5545e1"));
            Assert.Equal((ulong)871340, res);
            
        }

        [Fact]
        public void NumberToHexNTP1AmountTests()
        {
            var res = NTP1ScriptHelpers._NTP1NumToByteArray(999901700);
            Assert.Equal("69892a92", NTP1ScriptHelpers.ConvertByteArrayToHexString(res));
            res = NTP1ScriptHelpers._NTP1NumToByteArray(8478457292922);
            Assert.Equal("c007b60b6f687a", NTP1ScriptHelpers.ConvertByteArrayToHexString(res));
            res = NTP1ScriptHelpers._NTP1NumToByteArray(38290000);
            Assert.Equal("40ef54", NTP1ScriptHelpers.ConvertByteArrayToHexString(res));
            res = NTP1ScriptHelpers._NTP1NumToByteArray(1000000000000000);
            Assert.Equal("201f", NTP1ScriptHelpers.ConvertByteArrayToHexString(res));
            res = NTP1ScriptHelpers._NTP1NumToByteArray(723782);
            Assert.Equal("60b0b460", NTP1ScriptHelpers.ConvertByteArrayToHexString(res));
            res = NTP1ScriptHelpers._NTP1NumToByteArray(871340);
            Assert.Equal("5545e1", NTP1ScriptHelpers.ConvertByteArrayToHexString(res));

        }

        [Fact]
        public void UnhexString()
        {
            var res = NTP1ScriptHelpers.UnhexToByteArray("69892a92");
            Assert.Equal(new byte[4] { 105, 137, 42, 146 }, res);

        }

        [Fact]
        public void CalculateAmountSizeTest()
        {
            var res = NTP1ScriptHelpers._CalculateAmountSize(NTP1ScriptHelpers.UnhexToByteArray("11")[0]);
            Assert.Equal((int)1u, res);

            res = NTP1ScriptHelpers._CalculateAmountSize(NTP1ScriptHelpers.UnhexToByteArray("2012")[0]);
            Assert.Equal((int)2u, res);

            res = NTP1ScriptHelpers._CalculateAmountSize(NTP1ScriptHelpers.UnhexToByteArray("4bb3c1")[0]);
            Assert.Equal((int)3u, res);

            res = NTP1ScriptHelpers._CalculateAmountSize(NTP1ScriptHelpers.UnhexToByteArray("68c7e5b3")[0]);
            Assert.Equal((int)4u, res);

            res = NTP1ScriptHelpers._CalculateAmountSize(NTP1ScriptHelpers.UnhexToByteArray("8029990f1a")[0]);
            Assert.Equal((int)5u, res);

            res = NTP1ScriptHelpers._CalculateAmountSize(NTP1ScriptHelpers.UnhexToByteArray("a09c47f7b1a1")[0]);
            Assert.Equal((int)6u, res);

            res = NTP1ScriptHelpers._CalculateAmountSize(NTP1ScriptHelpers.UnhexToByteArray("c0a60eea1aa8fd")[0]);
            Assert.Equal((int)7u, res);

        }
    }
}

