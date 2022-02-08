using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite;
using VEDriversLite.NeblioAPI;
using Xunit;

namespace VEFrameworkUnitTest.Neblio
{
    public class GetSendAmountTest
    {
        /// <summary>
        /// 
        /// </summary>
        private Mock<IClient> _client = new Mock<IClient>();

        public GetSendAmountTest()
        {
            NeblioTransactionHelpers.GetClient(_client.Object);
        }

        /// <summary>
        /// Unit test method to verify if system is returning amount correctly.
        /// </summary>
        [Fact]
        public void GetSendAmount_Valid_Test()
        {
            //Arrange
            var transactionId = "123";
            string address = "NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh";

            string sampleTransactionObject = "{\"hex\":\"\",\"txid\":\"cb2cec4a0c3c6df5bf033e7da61a58eedb9a28ff2407c11b247b35f05baff6fb\",\"version\":1.0,\"locktime\":0.0,\"vin\"" +
                                            ":[{\"txid\":\"3e1fb1d4dd68fa6cfc87546d46f84700d3ec8c4ad30379aeebbefbfc846e1a0c\",\"vout\":0.0,\"scriptSig\":" +
                                            "{\"asm\":\"304402202cfb5f83b09523cbbad30c645bc7732d8f373e9db7beded80d647b5a9f7c470302203a9d3b0c8179489457d1a195449b1152dbac4a93036c041721e9043448bb693801" +
                                            " 03e295cd7e4bdc5005d185abc88b896a92aa2f32697df7255ee9a9eb4a1f121817\",\"hex\":" +
                                            "\"47304402202cfb5f83b09523cbbad30c645bc7732d8f373e9db7beded80d647b5a9f7c470302203a9d3b0c8179489457d1a195449b1152dbac4a93036c041721e9043448bb6938012103e295cd7e4bdc5005d185abc88b896a92aa2f32697df7255ee9a9eb4a1f121817\"}" +
                                            ",\"sequence\":4294967295.0,\"previousOutput\":{\"asm\":\"OP_DUP OP_HASH160 2c026bcdd04c8154d7cc73841914ffe33fcb3568 OP_EQUALVERIFY OP_CHECKSIG\"," +
                                            "\"hex\":\"76a9142c026bcdd04c8154d7cc73841914ffe33fcb356888ac\",\"reqSigs\":1.0,\"type\":\"pubkeyhash\",\"addresses\":[\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\"]}," +
                                            "\"tokens\":[{\"tokenId\":\"La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8\",\"amount\":100.0,\"issueTxid\":\"49ad447c7fc9dc3b8203a42b76f80dd5b79ccaa3cb7e9723e9ac05fb4c70c32c\"," +
                                            "\"divisibility\":7.0,\"lockStatus\":true,\"aggregationPolicy\":\"aggregatable\"}],\"value\":10000.0,\"fixed\":true,\"doubleSpentTxid\":\"70db343273615e939b3a268a08f93974b13d335052059ddfa11d24f8f0533fbe\"}," +
                                            "{\"txid\":\"3e1fb1d4dd68fa6cfc87546d46f84700d3ec8c4ad30379aeebbefbfc846e1a0c\",\"vout\":2.0,\"scriptSig\":{\"asm\":" +
                                            "\"3044022058927def6c4edfeee8c69afe054762d588a18b5289a47d038805473b57aeb6bc02205f6ab7e14c62e81baab44046aea04cbbf41f56f9bbde57032289fe172fbc1ed701" +
                                            " 03e295cd7e4bdc5005d185abc88b896a92aa2f32697df7255ee9a9eb4a1f121817\",\"hex\":\"473044022058927def6c4edfeee8c69afe054762d588a18b5289a47d038805473b57aeb6bc02205f6ab7e14c62e81baab44046aea04cbbf41f56f9bbde57032289fe172fbc1ed7012103e295cd7e4bdc5005d185abc88b896a92aa2f32697df7255ee9a9eb4a1f121817\"}," +
                                            "\"sequence\":4294967295.0,\"previousOutput\":{\"asm\":\"OP_DUP OP_HASH160 2c026bcdd04c8154d7cc73841914ffe33fcb3568 OP_EQUALVERIFY OP_CHECKSIG\",\"hex\":\"76a9142c026bcdd04c8154d7cc73841914ffe33fcb356888ac\",\"reqSigs\":1.0,\"type\":\"pubkeyhash\",\"addresses\":[\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\"]}," +
                                            "\"tokens\":[],\"value\":5000000.0,\"fixed\":true,\"doubleSpentTxid\":\"70db343273615e939b3a268a08f93974b13d335052059ddfa11d24f8f0533fbe\"}],\"vout\":[{\"value\":10000.0,\"n\":0.0,\"scriptPubKey\":{\"asm\":\"OP_DUP OP_HASH160 2c026bcdd04c8154d7cc73841914ffe33fcb3568 OP_EQUALVERIFY OP_CHECKSIG\"" +
                                            ",\"hex\":\"76a9142c026bcdd04c8154d7cc73841914ffe33fcb356888ac\",\"reqSigs\":1.0,\"type\":\"pubkeyhash\",\"addresses\":[\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\"]},\"tokens\":[{\"tokenId\":\"La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8\",\"amount\":1.0,\"issueTxid\":\"49ad447c7fc9dc3b8203a42b76f80dd5b79ccaa3cb7e9723e9ac05fb4c70c32c\"" +
                                            ",\"divisibility\":7.0,\"lockStatus\":true,\"aggregationPolicy\":\"aggregatable\"}],\"used\":false,\"blockheight\":3627327.0,\"usedBlockheight\":null,\"usedTxid\":null},{\"value\":10000.0,\"n\":1.0,\"scriptPubKey\":{\"asm\":\"OP_DUP OP_HASH160 2c026bcdd04c8154d7cc73841914ffe33fcb3568 OP_EQUALVERIFY OP_CHECKSIG\"," +
                                            "\"hex\":\"76a9142c026bcdd04c8154d7cc73841914ffe33fcb356888ac\",\"reqSigs\":1.0,\"type\":\"pubkeyhash\",\"addresses\":[\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\"]},\"tokens\":[{\"tokenId\":\"La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8\",\"amount\":1.0,\"issueTxid\":\"49ad447c7fc9dc3b8203a42b76f80dd5b79ccaa3cb7e9723e9ac05fb4c70c32c\"" +
                                            ",\"divisibility\":7.0,\"lockStatus\":true,\"aggregationPolicy\":\"aggregatable\"}],\"used\":false,\"blockheight\":3627327.0,\"usedBlockheight\":null,\"usedTxid\":null},{\"value\":10000.0,\"n\":2.0,\"scriptPubKey\":{\"asm\":\"OP_DUP OP_HASH160 2c026bcdd04c8154d7cc73841914ffe33fcb3568 OP_EQUALVERIFY OP_CHECKSIG\",\"hex\"" +
                                            ":\"76a9142c026bcdd04c8154d7cc73841914ffe33fcb356888ac\",\"reqSigs\":1.0,\"type\":\"pubkeyhash\",\"addresses\":[\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\"]},\"tokens\":[{\"tokenId\":\"La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8\",\"amount\":1.0,\"issueTxid\":\"49ad447c7fc9dc3b8203a42b76f80dd5b79ccaa3cb7e9723e9ac05fb4c70c32c\"," +
                                            "\"divisibility\":7.0,\"lockStatus\":true,\"aggregationPolicy\":\"aggregatable\"}],\"used\":false,\"blockheight\":3627327.0,\"usedBlockheight\":null,\"usedTxid\":null},{\"value\":10000.0,\"n\":3.0,\"scriptPubKey\":{\"asm\":\"\",\"hex\":\"\",\"type\":\"nulldata\"},\"tokens\":[],\"used\":false,\"blockheight\":3627327.0," +
                                            "\"usedBlockheight\":null,\"usedTxid\":null},{\"value\":4930000.0,\"n\":4.0,\"scriptPubKey\":{\"asm\":\"OP_DUP OP_HASH160 2c026bcdd04c8154d7cc73841914ffe33fcb3568 OP_EQUALVERIFY OP_CHECKSIG\",\"hex\":\"76a9142c026bcdd04c8154d7cc73841914ffe33fcb356888ac\",\"reqSigs\":1.0,\"type\":\"pubkeyhash\",\"addresses\"" +
                                            ":[\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\"]},\"tokens\":[],\"used\":false,\"blockheight\":3627327.0,\"usedBlockheight\":null,\"usedTxid\":null},{\"value\":10000.0,\"n\":5.0,\"scriptPubKey\":{\"asm\":\"OP_DUP OP_HASH160 2c026bcdd04c8154d7cc73841914ffe33fcb3568 OP_EQUALVERIFY OP_CHECKSIG\",\"hex\":\"76a9142c026bcdd04c8154d7cc73841914ffe33fcb356888ac\"" +
                                            ",\"reqSigs\":1.0,\"type\":\"pubkeyhash\",\"addresses\":[\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\"]},\"tokens\":[{\"tokenId\":\"La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8\",\"amount\":97.0,\"issueTxid\":\"49ad447c7fc9dc3b8203a42b76f80dd5b79ccaa3cb7e9723e9ac05fb4c70c32c\",\"divisibility\":7.0,\"lockStatus\":true," +
                                            "\"aggregationPolicy\":\"aggregatable\"}],\"used\":false,\"blockheight\":3627327.0,\"usedBlockheight\":null,\"usedTxid\":null}],\"blocktime\":1640788684000.0,\"blockheight\":3627327.0,\"totalsent\":4980000.0,\"fee\":30000.0,\"blockhash\":\"45e0f23add155af31ffcc06304384406212465222782194866654ccf359b7ddd\",\"time\"" +
                                            ":1640788684000.0,\"confirmations\":65021.0,\"doubleSpent\":true,\"tries\":0}";

            GetTransactionInfoResponse transactionObject = Newtonsoft.Json.JsonConvert.DeserializeObject<GetTransactionInfoResponse>(sampleTransactionObject);

            _client.Setup(x => x.GetTransactionInfoAsync(transactionId)).ReturnsAsync(transactionObject);

            //ACT
            var txinfo = NeblioTransactionHelpers.GetTransactionInfo(transactionId).Result;
            var amount = NeblioTransactionHelpers.GetSendAmount(txinfo, address);

            //Assert  
            Assert.True(amount > 0);
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result for incorrect address and incorrect transactionId.
        /// </summary>
        [Fact]
        public void GetSendAmount_Exception_Test()
        {
            //Arrange
            var transactionId = "cb2cec4a0c3c6df5bf033e7da61a58eedb9a28ff2407c11b247b35f05baff6"; //Incorrect transactionId
            string address = "NPvfpRCmDNcJjCZvDuAB9QsFC32gVThW"; //Incorrect address
            string message = "Cannot get amount of transaction. cannot create receiver address!";

            var transactionObject = new GetTransactionInfoResponse()
            {
                Vin = new List<Vin>(),
                Vout = new List<Vout>()
            };

            _client.Setup(x => x.GetTransactionInfoAsync(transactionId)).ReturnsAsync(transactionObject);

            //ACT            
            var txinfo = NeblioTransactionHelpers.GetTransactionInfo(transactionId).Result;

            //Assert
            var exception = Assert.Throws<Exception>(() => NeblioTransactionHelpers.GetSendAmount(txinfo, address));
            Assert.Equal(message, exception.Message);
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result for incorrect address and incorrect transactionId.
        /// </summary>
        [Fact]
        public void GetSendAmount_EmptyInAndOutVectors_Test()
        {
            //Arrange
            var transactionId = "cb2cec4a0c3c6df5bf033e7da61a58eedb9a28ff2407c11b247b35f05baff6"; //Incorrect transactionId
            string address = "NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh"; 

            var emptyTransactionObject = new GetTransactionInfoResponse()
            {
                Vin = new List<Vin>(),
                Vout = new List<Vout>()
            };

            _client.Setup(x => x.GetTransactionInfoAsync(transactionId)).ReturnsAsync(emptyTransactionObject);

            //ACT            
            var txinfo = NeblioTransactionHelpers.GetTransactionInfo(transactionId).Result;
            double amount = NeblioTransactionHelpers.GetSendAmount(txinfo, address);

            //Assert
            Assert.Equal(0, amount);
        }
    }
}
