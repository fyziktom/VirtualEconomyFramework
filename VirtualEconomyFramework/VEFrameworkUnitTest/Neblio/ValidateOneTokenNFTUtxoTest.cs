using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.Neblio;
using VEDriversLite.NeblioAPI;
using VEDriversLite;
using Xunit;
using Moq;

namespace VEFrameworkUnitTest.Neblio
{
    public class ValidateOneTokenNFTUtxoTest
    {
        private Mock<IClient> _client = new Mock<IClient>();

        public ValidateOneTokenNFTUtxoTest()
        {
            NeblioTransactionHelpers.GetClient(_client.Object);
            NeblioTransactionHelpers.TurnOnCache = false;
        }
        /// <summary>
        /// Unit test method to verify if system is returning an error result if an address is not having enough Neblio.
        /// </summary>
        [Fact]
        public async void ValidateOneTokenNFTUtxo_Valid_Test()
        {            
            #region AddressObject

            //AddressInfoResponse
            var addressResponseObject = "{\"address\":\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\",\"utxos\":[{\"index\":1.0,\"txid\":\"cc1d7ca4e918d35dbee5018340a28a6cc6b3ede9a1ad76ea2f793abc3150186f\"," +
                "\"blockheight\":3664058.0,\"blocktime\":1641909889000.0,\"scriptPubKey\":{\"asm\":\"OP_DUP OP_HASH160 2c026bcdd04c8154d7cc73841914ffe33fcb3568 OP_EQUALVERIFY OP_CHECKSIG\"" +
                ",\"hex\":\"76a9142c026bcdd04c8154d7cc73841914ffe33fcb356888ac\",\"reqSigs\":1,\"type\":\"pubkeyhash\",\"addresses\":[\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\"]},\"used\"" +
                ":false,\"value\":500000000.0,\"tokens\":[]},{\"index\":5.0,\"txid\":\"cb2cec4a0c3c6df5bf033e7da61a58eedb9a28ff2407c11b247b35f05baff6fb\",\"blockheight\":3627327.0," +
                "\"blocktime\":1640788684000.0,\"scriptPubKey\":{\"asm\":\"OP_DUP OP_HASH160 2c026bcdd04c8154d7cc73841914ffe33fcb3568 OP_EQUALVERIFY OP_CHECKSIG\",\"hex\":" +
                "\"76a9142c026bcdd04c8154d7cc73841914ffe33fcb356888ac\",\"reqSigs\":1,\"type\":\"pubkeyhash\",\"addresses\":[\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\"]},\"used\":false," +
                "\"value\":10000.0,\"tokens\":[{\"tokenId\":\"La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8\",\"amount\":97.0,\"issueTxid\":\"49ad447c7fc9dc3b8203a42b76f80dd5b79ccaa3cb7e9723e9ac05fb4c70c32c\"" +
                ",\"divisibility\":7.0,\"lockStatus\":true,\"aggregationPolicy\":\"aggregatable\"}]},{\"index\":4.0,\"txid\":\"cb2cec4a0c3c6df5bf033e7da61a58eedb9a28ff2407c11b247b35f05baff6fb\"," +
                "\"blockheight\":3627327.0,\"blocktime\":1640788684000.0,\"scriptPubKey\":{\"asm\":\"OP_DUP OP_HASH160 2c026bcdd04c8154d7cc73841914ffe33fcb3568 OP_EQUALVERIFY OP_CHECKSIG\",\"hex\"" +
                ":\"76a9142c026bcdd04c8154d7cc73841914ffe33fcb356888ac\",\"reqSigs\":1,\"type\":\"pubkeyhash\",\"addresses\":[\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\"]},\"used\":false,\"value\":" +
                "4930000.0,\"tokens\":[]},{\"index\":2.0,\"txid\":\"cb2cec4a0c3c6df5bf033e7da61a58eedb9a28ff2407c11b247b35f05baff6fb\",\"blockheight\":3627327.0,\"blocktime\":1640788684000.0," +
                "\"scriptPubKey\":{\"asm\":\"OP_DUP OP_HASH160 2c026bcdd04c8154d7cc73841914ffe33fcb3568 OP_EQUALVERIFY OP_CHECKSIG\",\"hex\":\"76a9142c026bcdd04c8154d7cc73841914ffe33fcb356888ac\"" +
                ",\"reqSigs\":1,\"type\":\"pubkeyhash\",\"addresses\":[\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\"]},\"used\":false,\"value\":10000.0,\"tokens\":[{\"tokenId\":\"La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8\"" +
                ",\"amount\":1.0,\"issueTxid\":\"49ad447c7fc9dc3b8203a42b76f80dd5b79ccaa3cb7e9723e9ac05fb4c70c32c\",\"divisibility\":7.0,\"lockStatus\":true,\"aggregationPolicy\":\"aggregatable\"}]}," +
                "{\"index\":1.0,\"txid\":\"cb2cec4a0c3c6df5bf033e7da61a58eedb9a28ff2407c11b247b35f05baff6fb\",\"blockheight\":3627327.0,\"blocktime\":1640788684000.0,\"scriptPubKey\":{\"asm\":" +
                "\"OP_DUP OP_HASH160 2c026bcdd04c8154d7cc73841914ffe33fcb3568 OP_EQUALVERIFY OP_CHECKSIG\",\"hex\":\"76a9142c026bcdd04c8154d7cc73841914ffe33fcb356888ac\",\"reqSigs\":1,\"type\":" +
                "\"pubkeyhash\",\"addresses\":[\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\"]},\"used\":false,\"value\":10000.0,\"tokens\":[{\"tokenId\":\"La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8\",\"amount\"" +
                ":1.0,\"issueTxid\":\"49ad447c7fc9dc3b8203a42b76f80dd5b79ccaa3cb7e9723e9ac05fb4c70c32c\",\"divisibility\":7.0,\"lockStatus\":true,\"aggregationPolicy\":\"aggregatable\"}]},{\"index\"" +
                ":0.0,\"txid\":\"cb2cec4a0c3c6df5bf033e7da61a58eedb9a28ff2407c11b247b35f05baff6fb\",\"blockheight\":3627327.0,\"blocktime\":1640788684000.0,\"scriptPubKey\":{\"asm\":\"OP_DUP OP_HASH160" +
                " 2c026bcdd04c8154d7cc73841914ffe33fcb3568 OP_EQUALVERIFY OP_CHECKSIG\",\"hex\":\"76a9142c026bcdd04c8154d7cc73841914ffe33fcb356888ac\",\"reqSigs\":1,\"type\":\"pubkeyhash\",\"addresses\"" +
                ":[\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\"]},\"used\":false,\"value\":10000.0,\"tokens\":[{\"tokenId\":\"La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8\",\"amount\":1.0,\"issueTxid\":" +
                "\"49ad447c7fc9dc3b8203a42b76f80dd5b79ccaa3cb7e9723e9ac05fb4c70c32c\",\"divisibility\":7.0,\"lockStatus\":true,\"aggregationPolicy\":\"aggregatable\"}]},{\"index\":3.0,\"txid\":" +
                "\"5f424d06bc12c96cb4f6deef300a43cb4cb65e2be022c02a791fa428808bf2e0\",\"blockheight\":3619186.0,\"blocktime\":1640539674000.0,\"scriptPubKey\":{\"asm\":\"OP_DUP OP_HASH160 " +
                "2c026bcdd04c8154d7cc73841914ffe33fcb3568 OP_EQUALVERIFY OP_CHECKSIG\",\"hex\":\"76a9142c026bcdd04c8154d7cc73841914ffe33fcb356888ac\",\"reqSigs\":1,\"type\":\"pubkeyhash\",\"addresses\"" +
                ":[\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\"]},\"used\":false,\"value\":10000.0,\"tokens\":[{\"tokenId\":\"La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8\",\"amount\":98.0,\"issueTxid\":" +
                "\"49ad447c7fc9dc3b8203a42b76f80dd5b79ccaa3cb7e9723e9ac05fb4c70c32c\",\"divisibility\":7.0,\"lockStatus\":true,\"aggregationPolicy\":\"aggregatable\"}]},{\"index\":2.0,\"txid\":" +
                "\"5f424d06bc12c96cb4f6deef300a43cb4cb65e2be022c02a791fa428808bf2e0\",\"blockheight\":3619186.0,\"blocktime\":1640539674000.0,\"scriptPubKey\":{\"asm\":\"OP_DUP OP_HASH160 " +
                "2c026bcdd04c8154d7cc73841914ffe33fcb3568 OP_EQUALVERIFY OP_CHECKSIG\",\"hex\":\"76a9142c026bcdd04c8154d7cc73841914ffe33fcb356888ac\",\"reqSigs\":1,\"type\":\"pubkeyhash\"," +
                "\"addresses\":[\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\"]},\"used\":false,\"value\":4870000.0,\"tokens\":[]},{\"index\":0.0,\"txid\":\"5f424d06bc12c96cb4f6deef300a43cb4cb65e2be022c02a791fa428808bf2e0\"" +
                ",\"blockheight\":3619186.0,\"blocktime\":1640539674000.0,\"scriptPubKey\":{\"asm\":\"OP_DUP OP_HASH160 2c026bcdd04c8154d7cc73841914ffe33fcb3568 OP_EQUALVERIFY OP_CHECKSIG\",\"hex\"" +
                ":\"76a9142c026bcdd04c8154d7cc73841914ffe33fcb356888ac\",\"reqSigs\":1,\"type\":\"pubkeyhash\",\"addresses\":[\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\"]},\"used\":false,\"value\":10000.0," +
                "\"tokens\":[{\"tokenId\":\"La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8\",\"amount\":1.0,\"issueTxid\":\"49ad447c7fc9dc3b8203a42b76f80dd5b79ccaa3cb7e9723e9ac05fb4c70c32c\",\"divisibility\":7.0" +
                ",\"lockStatus\":true,\"aggregationPolicy\":\"aggregatable\"}]},{\"index\":1.0,\"txid\":\"7ac89ab228f76ac4a85c1af5359ee299e439aaf4d858d859f09fcee54256f467\",\"blockheight\":3613121.0" +
                ",\"blocktime\":1640355136000.0,\"scriptPubKey\":{\"asm\":\"OP_DUP OP_HASH160 2c026bcdd04c8154d7cc73841914ffe33fcb3568 OP_EQUALVERIFY OP_CHECKSIG\",\"hex\":" +
                "\"76a9142c026bcdd04c8154d7cc73841914ffe33fcb356888ac\",\"reqSigs\":1,\"type\":\"pubkeyhash\",\"addresses\":[\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\"]},\"used\":false,\"value\"" +
                ":10000.0,\"tokens\":[{\"tokenId\":\"La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8\",\"amount\":1.0,\"issueTxid\":\"49ad447c7fc9dc3b8203a42b76f80dd5b79ccaa3cb7e9723e9ac05fb4c70c32c\"," +
                "\"divisibility\":7.0,\"lockStatus\":true,\"aggregationPolicy\":\"aggregatable\"}]},{\"index\":2.0,\"txid\":\"e0986671ec79ac674c7a7929ec9ec5fa0a130c221ddddfd06ff13c423343ce7a\"," +
                "\"blockheight\":3613117.0,\"blocktime\":1640354975000.0,\"scriptPubKey\":{\"asm\":\"OP_DUP OP_HASH160 2c026bcdd04c8154d7cc73841914ffe33fcb3568 OP_EQUALVERIFY OP_CHECKSIG\",\"hex\"" +
                ":\"76a9142c026bcdd04c8154d7cc73841914ffe33fcb356888ac\",\"reqSigs\":1,\"type\":\"pubkeyhash\",\"addresses\":[\"NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh\"]},\"used\":false,\"value\":50000.0,\"tokens\":[]}]}";

            #endregion

            GetAddressInfoResponse addressObject = Newtonsoft.Json.JsonConvert.DeserializeObject<GetAddressInfoResponse>(addressResponseObject);

            #region TransactionObject

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

            #endregion
            
            GetTransactionInfoResponse transactionObject = Newtonsoft.Json.JsonConvert.DeserializeObject<GetTransactionInfoResponse>(sampleTransactionObject);

            _client.Setup(x => x.GetAddressInfoAsync(It.IsAny<string>())).ReturnsAsync(addressObject);

            _client.Setup(x => x.GetTransactionInfoAsync(It.IsAny<string>())).ReturnsAsync(transactionObject);

            string address = "123";
            string tokenId = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8";
            string transactionId = "cb2cec4a0c3c6df5bf033e7da61a58eedb9a28ff2407c11b247b35f05baff6fb";
            int index = 1;

            var result = await NeblioTransactionHelpers.ValidateOneTokenNFTUtxo(address, tokenId, transactionId, index);

            Assert.Equal(index, result);
        }
    }
}
