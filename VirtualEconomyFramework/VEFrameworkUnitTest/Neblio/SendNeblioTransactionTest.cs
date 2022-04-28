using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VEDriversLite;
using VEDriversLite.Neblio;
using VEDriversLite.NeblioAPI;
using VEDriversLite.Security;
using Xunit;

namespace VEFrameworkUnitTest.Neblio
{
    public class SendNeblioTransactionTest
    {

        /// <summary>
        /// Unit test method to verify if sendTransaction method is working as expected with correct parameters.
        /// </summary>
        [Fact]
        public async void GetNeblioTransaction_Valid_Test()
        {
            NeblioTransactionHelpers.GetClient(Common.NeblioTestHelpers.Client.Object);
            NeblioTransactionHelpers.TurnOnCache = false;

            //Arrange           

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

            var res = Common.FakeDataGenerator.GetKeyAndAddress();

            string address = res.Item1.ToString();
            string key = res.Item2.ToString();

            GetAddressInfoResponse addressObject = Common.FakeDataGenerator.GetAddressWithNeblUtxos(address, 10, 1000000);

            SendTxData sendTxData = new SendTxData()
            {
                ReceiverAddress = address,
                SenderAddress = address,
                Amount = 0.02,
                CustomMessage = "test",
                Password = ""
            };

            Common.NeblioTestHelpers.Client.Setup(x => x.GetAddressInfoAsync(It.IsAny<string>())).ReturnsAsync(addressObject);
            Common.NeblioTestHelpers.Client.Setup(x => x.GetTransactionInfoAsync(It.IsAny<string>())).ReturnsAsync(transactionObject);
            
            var AccountKey = new EncryptionKey(key);

            var transaction = NeblioTransactionHelpers.GetNeblioTransactionObject(sendTxData, AccountKey, addressObject.Utxos);

            var inputCount = transaction.Inputs.Count();
            var outputCount = transaction.Outputs.Count();

            Assert.Equal(10, inputCount);
            Assert.Equal(3, outputCount);            
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result if Data object is null.
        /// </summary>
        [Fact]
        public void SendNeblioTransaction_Data_Null_Test()
        {
            string message = "Data cannot be null!";
            var exception = Assert.Throws<Exception>(() => NeblioTransactionHelpers.GetNeblioTransactionObject(null, null, new List<Utxos>()));
            Assert.Equal(message, exception.Message);
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result if Key object is null.
        /// </summary>
        [Fact]
        public void SendNeblioTransaction_Key_Null_Test()
        {
            SendTxData txData = new SendTxData();
            string message = "Account cannot be null!";
            var exception = Assert.Throws<Exception>(() => NeblioTransactionHelpers.GetNeblioTransactionObject(txData, null, new List<Utxos>()));
            Assert.Equal(message, exception.Message);
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result if address is not correct.
        /// </summary>
        [Fact]
        public void SendNeblioTransaction_InvalidAddress_Test()
        {
            SendTxData txData = new SendTxData();
            EncryptionKey encryptionKey = new EncryptionKey("Test");

            string message = "Cannot send transaction. cannot create receiver address!";
            var exception = Assert.Throws<Exception>(() => NeblioTransactionHelpers.GetNeblioTransactionObject(txData, encryptionKey, new List<Utxos>()));
            Assert.Equal(message, exception.Message);
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result if Utxos are null.
        /// </summary>
        [Fact]
        public void GetNeblioTransaction_TransactionObject_Error_Test()
        {
            var res = Common.FakeDataGenerator.GetKeyAndAddress();

            string address = res.Item1.ToString();
            string key = res.Item2.ToString();
            var AccountKey = new EncryptionKey(key);

            SendTxData sendTxData = new SendTxData()
            {
                ReceiverAddress = address,
                SenderAddress = address,
                Amount = 100,
                CustomMessage = "test",
                Password = ""
            };

            string message = "Cannot create the transaction object.";
            var exception = Assert.Throws<Exception>(() => NeblioTransactionHelpers.GetNeblioTransactionObject(sendTxData, AccountKey, null));
            Assert.Equal(message, exception.Message);
        }

        ///// <summary>
        ///// Unit test method to verify if system is returning an error result on Output creation when there is an exception.
        ///// </summary>
        //[Fact]
        //public void SendNeblioTransaction_Outputs_Error_Test()
        //{

        //    var res = Common.FakeDataGenerator.GetKeyAndAddress();

        //    string address = res.Item1.ToString();
        //    string key = res.Item2.ToString();
        //    var AccountKey = new EncryptionKey(key);

        //    GetAddressInfoResponse addressObject = Common.FakeDataGenerator.GetAddressWithNeblUtxos(address, 10, 1000000);

        //    SendTxData sendTxData = new SendTxData()
        //    {
        //        ReceiverAddress = address,
        //        SenderAddress = address,
        //        Amount = 100,
        //        CustomMessage = "test",
        //        Password = ""
        //    };

        //    //NeblioTransactionHelpers.FromSatToMainRatio = 0;

        //    string message = "Exception during creating outputs. ";
        //    var exception = Assert.Throws<Exception>(() => NeblioTransactionHelpers.GetNeblioTransactionObject(sendTxData, AccountKey, addressObject.Utxos));
        //    Assert.Contains(message, exception.Message);
        //}

        /// <summary>
        /// Unit test method to verify if system is returning an error result when password is empty.
        /// </summary>
        [Fact]
        public void GetNeblioTransaction_PasswordNotFilled_Error_Test()
        {
            //Arrange           
        
            string address = "NPvfpRCmDNcJjCZvDuAB9QsFC32gVThWdh";
            GetAddressInfoResponse addressObject = Common.FakeDataGenerator.GetAddressWithNeblUtxos(address, 10, 1000000);

            string password = "";

            SendTxData sendTxData = new SendTxData()
            {
                ReceiverAddress = address,
                SenderAddress = address,
                Amount = 100,
                CustomMessage = "test",
                Password = password
            };

            string Key = "A1eqrHg6TNBF8IGRhAsOHaLykDRRb56oF65J+DssDC9gThelreGQSubhvl3O7ZvHJ168W/mfzRlSItzMcdRaJw==";

            var AccountKey = new EncryptionKey(Key)
            {
                IsEncrypted = true
            };

            string message = "Cannot send token transaction. Password is not filled and key is encrypted or unlock account!";
            var exception = Assert.Throws<Exception>(() => NeblioTransactionHelpers.GetNeblioTransactionObject(sendTxData, AccountKey, addressObject.Utxos));
            Assert.Equal(message, exception.Message);
        }

        /// <summary>
        /// Unit test method to verify if system is returning an error result if broadcast failed.
        /// </summary>
        [Fact]
        public void SendNeblioTransaction_CannotSignTransaction_Error_Test()
        {
            //Arrange           
            
            var res = Common.FakeDataGenerator.GetKeyAndAddress();

            string address = res.Item1.ToString();
            string key = res.Item2.ToString();
            var AccountKey = new EncryptionKey(key);

            // to load to the API which are requested during sign procedure - wrong not spendable coins
            GetAddressInfoResponse addressObject = Common.FakeDataGenerator.GetAddressWithNeblUtxos(address, 10, 1000000);
            // to load to the Send tx command as input utxos - fake actual spendable utxos on the address
            GetAddressInfoResponse addressObject1 = Common.FakeDataGenerator.GetAddressWithNeblUtxos(address, 10, 1000000);

            SendTxData sendTxData = new SendTxData()
            {
                ReceiverAddress = address,
                SenderAddress = address,
                Amount = 100,
                CustomMessage = "test",
                Password = ""
            };
            
            var k = NeblioTransactionHelpers.GetAddressAndKey(AccountKey);
            var key1 = k.Item2;

            string message = "Exception during signing tx";
            var transaction = NeblioTransactionHelpers.GetNeblioTransactionObject(sendTxData, AccountKey, addressObject1.Utxos);

            var exception = Assert.ThrowsAsync<Exception>(async ()=> await NeblioTransactionHelpers.SignAndBroadcastTransaction(transaction, key1));
            
            Assert.Contains(message, exception.Result.Message);
        }

        /// <summary>
        /// Unit test method to verify inputs in transactions
        /// </summary>
        [Fact]
        public void SendNeblioTransaction_CorrectInputsInTx_Test()
        {
            //Arrange           

            var res = Common.FakeDataGenerator.GetKeyAndAddress();

            string address = res.Item1.ToString();
            string key = res.Item2.ToString();
            var AccountKey = new EncryptionKey(key);

            GetAddressInfoResponse addressObject = Common.FakeDataGenerator.GetAddressWithNeblUtxos(address, 10, Convert.ToInt32(1*NeblioTransactionHelpers.FromSatToMainRatio));

            SendTxData sendTxData = new SendTxData()
            {
                ReceiverAddress = address,
                SenderAddress = address,
                Amount = 1, 
                CustomMessage = "test",
                Password = ""
            };

            foreach(var u in addressObject.Utxos)
                 u.Index = 2;

            var transaction = NeblioTransactionHelpers.GetNeblioTransactionObject(sendTxData, AccountKey, addressObject.Utxos);

            Assert.Equal(10, transaction.Inputs.Count);
            Assert.Equal(2, (int)transaction.Inputs[0].PrevOut.N);
            Assert.Equal(NBitcoin.uint256.Parse(addressObject.Utxos.FirstOrDefault().Txid), 
                                                transaction.Inputs[0].PrevOut.Hash);
        }

        /// <summary>
        /// Unit test method to verify if outputs are correct
        /// </summary>
        [Fact]
        public void SendNeblioTransaction_CorrectNumberOfOutputsInTx_Test()
        {
            //Arrange           

            var res = Common.FakeDataGenerator.GetKeyAndAddress();

            string address = res.Item1.ToString();
            string key = res.Item2.ToString();
            var AccountKey = new EncryptionKey(key);

            GetAddressInfoResponse addressObject = Common.FakeDataGenerator.GetAddressWithNeblUtxos(address, 10, 100000000);

            SendTxData sendTxData = new SendTxData()
            {
                ReceiverAddress = address,
                SenderAddress = address,
                Amount = 1,
                CustomMessage = "",
                Password = ""
            };

            var expectedFee = 0.0002;
            var totInputs = 0.0;
            foreach (var utxo in addressObject.Utxos)
                totInputs += utxo.Value.Value;

            totInputs /= NeblioTransactionHelpers.FromSatToMainRatio;

            var transaction = NeblioTransactionHelpers.GetNeblioTransactionObject(sendTxData, AccountKey, addressObject.Utxos);

            // we can expect use of two outputs for this tx. One to cover amount. second for rest of money back to source
            Assert.Equal(2, transaction.Outputs.Count);
            // Amount in the 0 output must be same as requested amount to send
            Assert.Equal(sendTxData.Amount, Convert.ToDouble(transaction.Outputs[0].Value.ToUnit(NBitcoin.MoneyUnit.BTC)));
            // 8.9998 is the rest after send 1 and fee 0.0002 in this tx - lots of inputs
            Assert.Equal(totInputs - sendTxData.Amount - expectedFee, 
                         Convert.ToDouble(transaction.Outputs[1].Value.ToUnit(NBitcoin.MoneyUnit.BTC)));
        }


        /// <summary>
        /// Unit test method to verify if system is adding message to tx correctly
        /// </summary>
        [Fact]
        public void SendNeblioTransaction_MessageInNeblioTx_Test()
        {
            //Arrange           

            var res = Common.FakeDataGenerator.GetKeyAndAddress();

            string address = res.Item1.ToString();
            string key = res.Item2.ToString();
            var AccountKey = new EncryptionKey(key);

            GetAddressInfoResponse addressObject = Common.FakeDataGenerator.GetAddressWithNeblUtxos(address, 10, 100000000);

            SendTxData sendTxData = new SendTxData()
            {
                ReceiverAddress = address,
                SenderAddress = address,
                Amount = 1,
                CustomMessage = "VEFramework",
                Password = ""
            };

            var expectedFee = 0.0002;
            var opreturnValue = 0.0001;
            var totInputs = 0.0;
            foreach (var utxo in addressObject.Utxos)
                totInputs += utxo.Value.Value;

            totInputs /= NeblioTransactionHelpers.FromSatToMainRatio;

            var transaction = NeblioTransactionHelpers.GetNeblioTransactionObject(sendTxData, AccountKey, addressObject.Utxos);

            // we can expect use of three outputs for this tx.
            // One to cover amount. 
            // Second is for message (OP_RETURN)
            // Third for rest of money back to source
            Assert.Equal(3, transaction.Outputs.Count);

            // Match encoded message
            var msgoutput = "OP_RETURN 56454672616d65776f726b";
            Assert.Equal(msgoutput, transaction.Outputs[1].ScriptPubKey.ToString());
            // 8.9998 is the rest after send 1, fee 0.0002 and 0.0001 for save data in this tx - lots of inputs
            Assert.Equal(totInputs - sendTxData.Amount - expectedFee - opreturnValue,
                         Convert.ToDouble(transaction.Outputs[2].Value.ToUnit(NBitcoin.MoneyUnit.BTC)));
        }

    }
}
