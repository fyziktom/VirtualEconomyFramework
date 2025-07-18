using NBitcoin;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Events;
using VEDriversLite.Dto;
using VEDriversLite.Neblio;
using VEDriversLite.NeblioAPI;
using VEDriversLite.Security;
using VEDriversLite.Common;
using static Google.Protobuf.WellKnownTypes.Field;
using static System.Net.WebRequestMethods;

namespace VEDriversLite
{

    /// <summary>
    /// Main Helper class for the Neblio Blockchain Transactions
    /// </summary>
    public static class NeblioTransactionHelpers
    {
        /// <summary>
        /// NBitcoin Instance of Mainet Network of Neblio
        /// </summary>
        public static Network Network = NBitcoin.Altcoins.Neblio.Instance.Mainnet;

        /// <summary>
        /// Conversion ration for Neblio to convert from sat to 1 NEBL
        /// </summary>
        public const double FromSatToMainRatio = 100000000;

        /// <summary>
        /// Minimum amount in Satoshi on Neblio Blockchain
        /// </summary>
        public static long MinimumAmount = 10000;


        /// <summary>
        /// Main event info handler
        /// </summary>
        public static event EventHandler<IEventInfo> NewEventInfo;


        /// <summary>
        /// This function will calculate the fee based of the known lenght of the intputs and the outputs
        /// If there is the OP_RETURN output it is considered as the customMessage. Please fill it for token transactions.
        /// Token transaction also will add just for sure one output to calculation of the size for the case there will be some tokens back to original address
        /// </summary>
        /// <param name="numOfInputs">Number of input of the transaction "in" vector</param>
        /// <param name="numOfOutputs">Number of outpus of the transaction "out" vector</param>
        /// <param name="customMessageInOPReturn">Custom message - "OP_RETURN" output</param>
        /// <param name="isTokenTransaction">Token transaction will add another output for getting back the tokens</param>
        /// <returns></returns>
        public static double CalcFee(int numOfInputs, int numOfOutputs, string customMessageInOPReturn, bool isTokenTransaction)
        {
            if (numOfInputs <= 0)
                numOfInputs = 1;
            if (numOfOutputs <= 0)
                numOfOutputs = 1;

            const string exceptionMessage = "Cannot send transaction bigger than 4kB on Neblio network!";
            var basicFee = MinimumAmount;

            // inputs
            var blankInput = 41;
            var inputSignature = 56;
            var signedInput = blankInput + inputSignature;

            // outputs
            var outputWithAddress = 34;
            var emptyOpReturn = 11; // OP_RETURN with custom message with 10 characters had 21 bytes...etc.

            //common properties in each transaction
            var commonPropertiesSize = 214;

            var expectedSize = signedInput * numOfInputs + outputWithAddress * numOfOutputs + commonPropertiesSize;

            // add custom message if there is some
            if (!string.IsNullOrEmpty(customMessageInOPReturn))
            {
                var zipstr = StringExt.ZipStr(customMessageInOPReturn);
                expectedSize += emptyOpReturn + zipstr.Length;
            }

            // Expected outputs for the rest of the coins/tokens
            expectedSize += outputWithAddress; // NEBL
            if (isTokenTransaction)
            {
                expectedSize += outputWithAddress;
            }

            double size_m = ((double)expectedSize / 1024);
            if (size_m > 3.9)
            {
                throw new Exception(exceptionMessage);
            }

            size_m = Math.Ceiling(size_m);

            var fee = basicFee + (int)(size_m) * basicFee;

            if (isTokenTransaction)
            {
                fee += basicFee;
            }

            return fee;
        }

        private static async Task<(BitcoinAddress, BitcoinSecret)> GetAddressAndKeyInternal(EncryptionKey ekey, string password)
        {
            var key = string.Empty;
            const string message = "Cannot send token transaction. Password is not filled and key is encrypted or unlock account!";
            const string exceptionMessage = "Cannot send token transaction. cannot create keys!";

            if (ekey != null)
            {
                if (ekey.IsLoaded)
                {
                    if (ekey.IsEncrypted && string.IsNullOrEmpty(password) && !ekey.IsPassLoaded)
                    {
                        throw new Exception(message);
                    }
                    else if (!ekey.IsEncrypted)
                    {
                        key = await ekey.GetEncryptedKey();
                    }
                    else if (ekey.IsEncrypted && (!string.IsNullOrEmpty(password) || ekey.IsPassLoaded))
                    {
                        if (ekey.IsPassLoaded)
                        {
                            key = await ekey.GetEncryptedKey(string.Empty);
                        }
                        else
                        {
                            key = await ekey.GetEncryptedKey(password);
                        }
                    }

                    if (string.IsNullOrEmpty(key))
                    {
                        throw new Exception(message);
                    }
                }
            }

            if (!string.IsNullOrEmpty(key))
            {
                try
                {
                    BitcoinSecret loadedkey = Network.CreateBitcoinSecret(key);
                    BitcoinAddress addressForTx = loadedkey.GetAddress(ScriptPubKeyType.Legacy);

                    return (addressForTx, loadedkey);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot send token transaction!", ex);
                    throw new Exception(exceptionMessage);
                }
            }
            else
            {
                throw new Exception(message);
            }
        }

        /// <summary>
        /// Function converts EncryptionKey (optionaly with password if it is not already loaded in ekey)
        /// and returns BitcoinAddress and BitcoinSecret classes from NBitcoin
        /// </summary>
        /// <param name="ekey"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<(BitcoinAddress, BitcoinSecret)> GetAddressAndKey(EncryptionKey ekey, string password)
        {
            return await GetAddressAndKeyInternal(ekey, password);
        }

        /// <summary>
        /// Function converts EncryptionKey (optionaly with password if it is not already loaded in ekey)
        /// and returns BitcoinAddress and BitcoinSecret classes from NBitcoin
        /// </summary>
        /// <param name="ekey"></param>
        /// <returns></returns>
        public static async Task<(BitcoinAddress, BitcoinSecret)> GetAddressAndKey(EncryptionKey ekey)
        {
            return await GetAddressAndKeyInternal(ekey, "");
        }
        /// <summary>
        /// This function will crate empty Transaction object based on Neblio network standard
        /// Then add the Neblio Inputs and sumarize their value
        /// </summary>
        /// <param name="nutxos">List of Neblio Utxos to use</param>
        /// <param name="address">Address of the owner</param>
        /// <returns>(NBitcoin Transaction object, sum of all inputs values in double)</returns>
        public static (Transaction, double) GetTransactionWithNeblioInputs(ICollection<Utxos> nutxos, BitcoinAddress address)
        {
            // create template for new tx from last one
            var transaction = Transaction.Create(Network);

            var allNeblInputCoins = 0.0; //this is because of the optimization. When we iterate through values we can sum them for later use
            try
            {
                // add inputs of tx
                foreach (var utxo in nutxos)
                {
                    transaction.Inputs.Add(new TxIn()
                    {
                        PrevOut = new OutPoint(uint256.Parse(utxo.Txid), (int)utxo.Index),
                        ScriptSig = address.ScriptPubKey,
                    });
                    allNeblInputCoins += (double)utxo.Value / FromSatToMainRatio;
                }
                return (transaction, allNeblInputCoins);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception during loading inputs. " + ex.Message);
            }

            return (null, 0);
        }

        /// <summary>
        /// This function will create empty Transaction object based on Neblio network standard
        /// Then it add Neblio intpus and tokens inputs and summarize their value
        /// </summary>
        /// <param name="nutxos"></param>
        /// <param name="tutxos"></param>
        /// <param name="address"></param>
        /// <returns>(Item1 - NBitcoin Transaction object, (Item2.Item1 - sum of all neblio inputs, Item2.Item2 - sum of all tokens)</returns>
        public static (Transaction, (double, double)) GetTransactionWithNeblioAndTokensInputs(ICollection<Utxos> nutxos, ICollection<Utxos> tutxos, BitcoinAddress address)
        {
            var txres = GetTransactionWithNeblioInputs(nutxos, address);
            var transaction = txres.Item1;

            var allNeblInputCoins = txres.Item2; //this is because of the optimization. When we iterate through values we can sum them for later use
            var allTokensInputs = 0.0; //this is because of the optimization. When we iterate through values we can sum them for later use
            TxInList inputs = new TxInList();

            try
            {
                // add inputs of tx
                foreach (var utxo in tutxos)
                {
                    inputs.Add(new TxIn()
                    {
                        PrevOut = new OutPoint(uint256.Parse(utxo.Txid), (int)utxo.Index),
                        ScriptSig = address.ScriptPubKey,
                    });
                    allNeblInputCoins += (double)utxo.Value / FromSatToMainRatio;
                    var toks = utxo.Tokens.FirstOrDefault();
                    if (toks != null && toks.Amount > 0)
                        allTokensInputs += toks.Amount ?? 0;
                }

                foreach (var inp in transaction.Inputs)
                    inputs.Add(inp);

                transaction.Inputs.Clear();
                transaction.Inputs.AddRange(inputs);

                return (transaction, (allNeblInputCoins, allTokensInputs));
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception during loading inputs. " + ex.Message);
            }

            return (null, (0.0, 0.0));
        }

        public static byte[] GetMetaComprimed(IDictionary<string, string> metadata)
        {
            if (metadata == null)
                metadata = new Dictionary<string, string>();

            var dto = new SendTokenRequest()
            {
                Metadata = new Metadata2() { UserData = new UserData3() { Meta = new List<JObject>() } }
            };
            foreach (var d in metadata)
                dto.Metadata.UserData.Meta.Add(new JObject { [d.Key] = d.Value });

            var metastring = JsonConvert.SerializeObject(dto.Metadata);
            var metacomprimed = StringExt.Compress(Encoding.UTF8.GetBytes(metastring));

            return metacomprimed;
        }

        /// <summary>
        /// Issue new tokens. Please check the IssueTokenTxData Dto for input parameters
        /// </summary>
        /// <param name="data"></param>
        /// <param name="nutxos"></param>
        /// <param name="fee"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<Transaction> IssueTokensAsync(IssueTokenTxData data, ICollection<Utxos> nutxos, double fee = 20000)
        {
            if (data.IssuanceMetadata == null)
                throw new Exception("Cannot send without metadata!");

            // load key and address
            BitcoinAddress addressForTx;

            // create receiver address
            BitcoinAddress recaddr;
            try
            {
                addressForTx = recaddr = BitcoinAddress.Create(data.SenderAddress, Network);
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            var txres = GetTransactionWithNeblioInputs(nutxos, addressForTx);
            var transaction = txres.Item1;
            var allNeblInputCoins = txres.Item2;

            var metastring = JsonConvert.SerializeObject(data.IssuanceMetadata);
            var metacomprimed = StringExt.Compress(Encoding.UTF8.GetBytes(metastring));

            var index = 0;
            List<NTP1Instructions> TiList = new List<NTP1Instructions>();
            //Now make the transfer instruction
            NTP1Instructions ti = new NTP1Instructions();
            ti.amount = data.Amount;
            ti.vout_num = 0;
            TiList.Add(ti);

            //Create the hex op_return
            string ti_script = NTP1ScriptHelpers._NTP1CreateIsseueScript(TiList, metacomprimed, data.IssuanceMetadata.Data.TokenName, data.Flags);

            try
            {
                fee = CalcFee(transaction.Inputs.Count, 2, metastring, true);

                var balanceinSat = Convert.ToUInt64(allNeblInputCoins * FromSatToMainRatio);
                var costOfIssuing = Convert.ToUInt64(10 * FromSatToMainRatio); // it cost 10 NEBL to issue new tokens

                if ((costOfIssuing + fee) > balanceinSat)
                    throw new Exception("Not enought spendable Neblio on the address.");

                var diffinSat = balanceinSat - costOfIssuing - Convert.ToUInt64(fee) - Convert.ToUInt64(MinimumAmount) - Convert.ToUInt64(MinimumAmount); // fee is already included in previous output, last is token carriers

                if (!string.IsNullOrEmpty(data.ReceiverAddress))
                    transaction.Outputs.Add(new Money(MinimumAmount), recaddr.ScriptPubKey); // send to receiver required amount
                else
                    transaction.Outputs.Add(new Money(MinimumAmount), addressForTx.ScriptPubKey); // send to receiver required amount

                var ti_b = NTP1ScriptHelpers.UnhexToByteArray(ti_script);
                transaction.Outputs.Add(new TxOut()
                {
                    Value = new Money(MinimumAmount),
                    ScriptPubKey = CreateOPRETURNScript(ti_b)
                });

                if (diffinSat > 0)
                    transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender addresss
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during adding outputs with payment in neblio." + ex.Message);
            }

            return transaction;
        }


        /// <summary>
        /// Send lot of the tokens. It means more than 1 token in one output. Please check the SendTokenTxData Dto for input parameters
        /// </summary>
        /// <param name="data"></param>
        /// <param name="nutxos"></param>
        /// <param name="tutxos"></param>
        /// <param name="fee"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static Transaction SendTokenLotNewAsync(SendTokenTxData data, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos, double fee = 20000)
        {
            if (data.Metadata == null || data.Metadata.Count == 0)
                throw new Exception("Cannot send without metadata!");

            // load key and address
            BitcoinSecret key;
            BitcoinAddress addressForTx;

            // create receiver address
            BitcoinAddress recaddr;
            try
            {
                addressForTx = BitcoinAddress.Create(data.SenderAddress, Network);
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            var tutxo = tutxos.FirstOrDefault();
            if (tutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            var txres = GetTransactionWithNeblioAndTokensInputs(nutxos, tutxos, addressForTx);
            var transaction = txres.Item1;
            var allNeblInputCoins = txres.Item2.Item1;

            var index = 0;
            List<NTP1Instructions> TiList = new List<NTP1Instructions>();
            //Now make the transfer instruction
            NTP1Instructions ti = new NTP1Instructions();
            ti.amount = Convert.ToUInt64(data.Amount);
            ti.vout_num = 0;
            TiList.Add(ti);

            var metacomprimed = GetMetaComprimed(data.Metadata);
            //Create the hex op_return
            string ti_script = NTP1ScriptHelpers._NTP1CreateTransferScript(TiList, metacomprimed); //No metadata

            var restOfToks = txres.Item2.Item2; //txres.Item2.Item2 holds all tokens in inputs tutxos
            restOfToks -= data.Amount;

            try
            {
                fee = CalcFee(transaction.Inputs.Count, 2, JsonConvert.SerializeObject(data.Metadata), true);

                var balanceinSat = Convert.ToUInt64(allNeblInputCoins * FromSatToMainRatio);

                if ((4 * MinimumAmount + fee) > balanceinSat)
                    throw new Exception("Not enought spendable Neblio on the address.");

                var diffinSat = balanceinSat - Convert.ToUInt64(fee) - Convert.ToUInt64(MinimumAmount) - Convert.ToUInt64(MinimumAmount); // fee is already included in previous output, last is token carriers
                if (restOfToks > 0)
                    diffinSat -= Convert.ToUInt64(MinimumAmount);

                transaction.Outputs.Add(new Money(MinimumAmount), recaddr.ScriptPubKey); // send to receiver required amount

                var ti_b = NTP1ScriptHelpers.UnhexToByteArray(ti_script);
                transaction.Outputs.Add(new TxOut()
                {
                    Value = new Money(MinimumAmount),
                    ScriptPubKey = CreateOPRETURNScript(ti_b)
                });

                if (diffinSat > 0)
                    transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender address

                if (restOfToks > 0) // just for minting new payment nft
                    transaction.Outputs.Add(new Money(MinimumAmount), addressForTx.ScriptPubKey); // add 10000 sat as carier of tokens which goes back

            }
            catch (Exception ex)
            {
                throw new Exception("Exception during adding outputs with payment in neblio." + ex.Message);
            }

            return transaction;
        }

        /// <summary>
        /// Create OP_RETURN output ScriptPubKey from the input data. 
        /// Data should not contain OP_RETURN byte. This byte is added in the function. PLease provide just the data which should be attached
        /// Data must be in not HEX form. The Op.GetPushOp function converts them during the loading.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static Script CreateOPRETURNScript(params byte[][] data)
        {
            if (data == null)
                throw new ArgumentNullException(nameof(data));

            Op[] ops = new Op[data.Length + 1];
            ops[0] = OpcodeType.OP_RETURN;

            for (int i = 0; i < data.Length; i++)
                ops[1 + i] = Op.GetPushOp(data[i]);

            var script = new Script(ops);

            return script;
        }


        /// <summary>
        /// Function will Mint NFT from lot of the tokens
        /// </summary>
        /// <param name="data">Mint data, please see MintNFTData class for the details</param>
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="tutxos">Optional input token utxo</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<Transaction> MintNFTTokenAsync(MintNFTData data, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            return await MintMultiNFTTokenAsyncInternal(data, 0, nutxos, tutxos, false);
        }

        /// <summary>
        /// Function will Mint NFT with the coppies
        /// </summary>
        /// <param name="data">Mint data, please see MintNFTData class for the details</param>
        /// <param name="coppies">0 or more coppies - with 0 input it is same as MintNFTTokenAsync</param>
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="tutxos">Optional input token utxo</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<Transaction> MintMultiNFTTokenAsync(MintNFTData data, int coppies, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            return await MintMultiNFTTokenAsyncInternal(data, coppies, nutxos, tutxos, true);
        }
        /// <summary>
        /// Function will Mint NFT with the coppies
        /// </summary>
        /// <param name="data">Mint data, please see MintNFTData class for the details</param>
        /// <param name="coppies">0 or more coppies - with 0 input it is same as MintNFTTokenAsync</param>
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="tutxos">Optional input token utxo</param>
        /// <param name="multiTokens">If there is the multi token it needs to check if there is no conflict</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<Transaction> MintMultiNFTTokenAsyncInternal(MintNFTData data, int coppies, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos, bool multiTokens, double fee = 20000)
        {
            if (data.Metadata == null || data.Metadata.Count == 0)
                throw new Exception("Cannot send without metadata!");

            List<BitcoinAddress> receiverAddreses = new List<BitcoinAddress>();

            // load key and address
            BitcoinSecret key;
            BitcoinAddress addressForTx = BitcoinAddress.Create(data.SenderAddress, Network);

            if (!string.IsNullOrEmpty(data.ReceiverAddress) || data.MultipleReceivers.Count > 0)
            {
                if (data.MultipleReceivers.Count == 0)
                    receiverAddreses.Add(BitcoinAddress.Create(data.ReceiverAddress, Network));
                else
                {
                    foreach (var a in data.MultipleReceivers)
                        receiverAddreses.Add(BitcoinAddress.Create(a, Network));
                }
            }

            var tutxo = tutxos.FirstOrDefault();
            if (tutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            var txres = GetTransactionWithNeblioAndTokensInputs(nutxos, tutxos, addressForTx);
            var transaction = txres.Item1;
            var allNeblInputCoins = txres.Item2.Item1;

            data.Metadata.Add(new KeyValuePair<string, string>("SourceUtxo", tutxo.Txid));
            data.Metadata.Add(new KeyValuePair<string, string>("NFT FirstTx", "true"));

            List<NTP1Instructions> TiList = new List<NTP1Instructions>();
            //Now make the transfer instruction

            var totalToksToSend = 0.0;
            if (data.MultipleReceivers.Count > 0 && coppies == 0)
            {
                var index = 0;
                foreach (var utxo in data.MultipleReceivers)
                {
                    NTP1Instructions ti = new NTP1Instructions();
                    ti.amount = 1;
                    ti.vout_num = index;
                    TiList.Add(ti);
                    totalToksToSend++;
                    index++;
                }
            }
            else if (data.MultipleReceivers.Count == 0 && coppies > 0)
            {
                for (var i = 0; i < coppies + 1; i++)
                {
                    NTP1Instructions ti = new NTP1Instructions();
                    ti.amount = 1;
                    ti.vout_num = i;
                    TiList.Add(ti);
                    totalToksToSend++;
                }
            }
            else if (data.MultipleReceivers.Count == 0 && coppies == 0)
            {
                NTP1Instructions ti = new NTP1Instructions();
                ti.amount = 1;
                ti.vout_num = 0;
                TiList.Add(ti);
                totalToksToSend++;
            }

            var metacomprimed = GetMetaComprimed(data.Metadata);

            //Create the hex op_return
            string ti_script = NTP1ScriptHelpers._NTP1CreateTransferScript(TiList, metacomprimed); //No metadata

            var restOfToks = txres.Item2.Item2; //txres.Item2.Item2 holds all tokens in inputs tutxos
            restOfToks -= totalToksToSend;

            try
            {
                fee = CalcFee(transaction.Inputs.Count, 2, JsonConvert.SerializeObject(data.Metadata), true);

                var balanceinSat = Convert.ToUInt64(allNeblInputCoins * FromSatToMainRatio);

                if ((4 * MinimumAmount + fee) > balanceinSat)
                    throw new Exception("Not enought spendable Neblio on the address.");

                var diffinSat = balanceinSat - Convert.ToUInt64(fee) - Convert.ToUInt64(MinimumAmount); // one MinimumAmount subtracted is because of OP_RETURN, others are subtracted below based on number of receivers

                if (restOfToks > 0)
                    diffinSat -= Convert.ToUInt64(MinimumAmount);

                if (receiverAddreses.Count > 0 && coppies == 0)
                {
                    foreach (var receiver in receiverAddreses)
                    {
                        diffinSat -= Convert.ToUInt64(MinimumAmount);
                        transaction.Outputs.Add(new Money(MinimumAmount), receiver.ScriptPubKey); // send to receiver NFT
                    }
                }
                else if (receiverAddreses.Count == 1 && coppies > 0)
                {
                    for (var i = 0; i < coppies + 1; i++)
                    {
                        diffinSat -= Convert.ToUInt64(MinimumAmount);
                        transaction.Outputs.Add(new Money(MinimumAmount), receiverAddreses.FirstOrDefault()?.ScriptPubKey); // send to receiver NFT
                    }
                }
                else if (receiverAddreses.Count == 0 && coppies > 0)
                {
                    for (var i = 0; i < coppies + 1; i++)
                    {
                        diffinSat -= Convert.ToUInt64(MinimumAmount);
                        transaction.Outputs.Add(new Money(MinimumAmount), addressForTx.ScriptPubKey); // send to receiver NFT
                    }
                }
                else
                {
                    diffinSat -= Convert.ToUInt64(MinimumAmount);
                    transaction.Outputs.Add(new Money(MinimumAmount), addressForTx.ScriptPubKey); // send to own NFT
                }

                var ti_b = NTP1ScriptHelpers.UnhexToByteArray(ti_script);
                transaction.Outputs.Add(new TxOut()
                {
                    Value = new Money(MinimumAmount),
                    ScriptPubKey = CreateOPRETURNScript(ti_b)
                });

                if (diffinSat > 0)
                    transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender address

                if (restOfToks > 0) // just for minting new payment nft
                    transaction.Outputs.Add(new Money(MinimumAmount), addressForTx.ScriptPubKey); // add 10000 sat as carier of tokens which goes back

            }
            catch (Exception ex)
            {
                throw new Exception("Exception during adding outputs with payment in neblio." + ex.Message);
            }

            return transaction;
        }


        /// <summary>
        /// Send Tokens to multiple receivers with different amounts of the tokens
        /// </summary>
        /// <param name="data">Mint data, please see SendTokenTxData class for the details</param>
        /// <param name="coppies">0 or more coppies - with 0 input it is same as MintNFTTokenAsync</param>
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="tutxos">Optional input token utxo</param>
        /// <param name="multiTokens">If there is the multi token it needs to check if there is no conflict</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<Transaction> SendTokensToMultipleReceiversAsync(SendTokenTxData data, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos, double fee = 20000)
        {
            if (data.Metadata == null || data.Metadata.Count == 0)
                throw new Exception("Cannot send without metadata!");

            List<BitcoinAddress> receiverAddreses = new List<BitcoinAddress>();

            // load key and address
            BitcoinAddress addressForTx;
            addressForTx = BitcoinAddress.Create(data.SenderAddress, Network);

            if (!string.IsNullOrEmpty(data.ReceiverAddress) || data.MultipleReceivers.Count > 0)
            {
                if (data.MultipleReceivers.Count == 0)
                    receiverAddreses.Add(BitcoinAddress.Create(data.ReceiverAddress, Network));
                else
                {
                    foreach (var a in data.MultipleReceivers)
                        receiverAddreses.Add(BitcoinAddress.Create(a.Address, Network));
                }
            }

            var tutxo = tutxos.FirstOrDefault();
            if (tutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            var txres = GetTransactionWithNeblioAndTokensInputs(nutxos, tutxos, addressForTx);
            var transaction = txres.Item1;
            var allNeblInputCoins = txres.Item2.Item1;

            List<NTP1Instructions> TiList = new List<NTP1Instructions>();
            //Now make the transfer instruction

            var totalToksToSend = 0.0;
            if (data.MultipleReceivers.Count > 0)
            {
                var index = 0;
                foreach (var rcv in data.MultipleReceivers)
                {
                    NTP1Instructions ti = new NTP1Instructions();
                    ti.amount = (ulong)rcv.Amount;
                    ti.vout_num = index;
                    TiList.Add(ti);
                    totalToksToSend++;
                    index++;
                }
            }
            else if (data.MultipleReceivers.Count == 0)
            {
                NTP1Instructions ti = new NTP1Instructions();
                ti.amount = (ulong)data.Amount;
                ti.vout_num = 0;
                TiList.Add(ti);
                totalToksToSend++;
            }

            var totalToksCheck = 0;
            foreach (var t in TiList)
                totalToksCheck += (int)t.amount;
            if (data.Amount > 0 && totalToksCheck != data.Amount)
                throw new Exception("Total amount of tokens in Transfer instructions to send is not equal to the amount of tokens to send!");

            var metacomprimed = GetMetaComprimed(data.Metadata);

            //Create the hex op_return
            string ti_script = NTP1ScriptHelpers._NTP1CreateTransferScript(TiList, metacomprimed); //No metadata

            var restOfToks = txres.Item2.Item2; //txres.Item2.Item2 holds all tokens in inputs tutxos
            restOfToks -= totalToksToSend;

            try
            {
                fee = CalcFee(transaction.Inputs.Count, 2, JsonConvert.SerializeObject(data.Metadata), true);

                var balanceinSat = Convert.ToUInt64(allNeblInputCoins * FromSatToMainRatio);

                if ((4 * MinimumAmount + fee) > balanceinSat)
                    throw new Exception("Not enought spendable Neblio on the address.");

                var diffinSat = balanceinSat - Convert.ToUInt64(fee) - Convert.ToUInt64(MinimumAmount); // one MinimumAmount subtracted is because of OP_RETURN, others are subtracted below based on number of receivers

                if (restOfToks > 0)
                    diffinSat -= Convert.ToUInt64(MinimumAmount);

                if (receiverAddreses.Count > 0)
                {
                    foreach (var receiver in receiverAddreses)
                    {
                        diffinSat -= Convert.ToUInt64(MinimumAmount);
                        transaction.Outputs.Add(new Money(MinimumAmount), receiver.ScriptPubKey); // send to receiver NFT
                    }
                }
                else
                {
                    diffinSat -= Convert.ToUInt64(MinimumAmount);
                    transaction.Outputs.Add(new Money(MinimumAmount), addressForTx.ScriptPubKey); // send to own NFT
                }

                var ti_b = NTP1ScriptHelpers.UnhexToByteArray(ti_script);
                transaction.Outputs.Add(new TxOut()
                {
                    Value = new Money(MinimumAmount),
                    ScriptPubKey = CreateOPRETURNScript(ti_b)
                });

                if (diffinSat > 0)
                    transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender address

                if (restOfToks > 0) // just for minting new payment nft
                    transaction.Outputs.Add(new Money(MinimumAmount), addressForTx.ScriptPubKey); // add 10000 sat as carier of tokens which goes back

            }
            catch (Exception ex)
            {
                throw new Exception("Exception during adding outputs with payment in neblio." + ex.Message);
            }

            return transaction;
        }


        /// <summary>
        /// Wrap for SignAndBroadcast transaction function
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static async Task<string> SignAndBroadcastTransaction(Transaction transaction, BitcoinSecret key)
        {
            return await SignAndBroadcast(transaction, key);
        }
        /// <summary>
        /// Function will Split NTP1 tokens to smaller lots
        /// receiver list - If you input 0, split will be done to sender address, if you input 1 receiver split will be done to receiver (all inputs)
        /// if you will provide multiple receivers, the number of lots and receivers must match.
        /// </summary>
        /// <param name="receiver">List of receivers. </param>
        /// <param name="lots"></param>
        /// <param name="amount"></param>
        /// <param name="tokenId"></param>
        /// <param name="metadata"></param>
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="tutxos">Optional input token utxo</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<Transaction> SplitNTP1TokensAsync(string address, List<string> receiver, int lots, int amount, string tokenId,
                                                                      IDictionary<string, string> metadata,
                                                                      ICollection<Utxos> nutxos,
                                                                      ICollection<Utxos> tutxos)
        {

            List<BitcoinAddress> receiverAddreses = new List<BitcoinAddress>();

            // load key and address
            BitcoinSecret key;
            BitcoinAddress addressForTx = BitcoinAddress.Create(address, Network);
            if (receiver.Count > 0)
            {
                if (receiver.Count == 0)
                {
                    receiverAddreses.Add(addressForTx);
                }
                else if (receiver.Count == 1)
                {
                    var a = receiver.FirstOrDefault();
                    if (!string.IsNullOrEmpty(a))
                        receiverAddreses.Add(BitcoinAddress.Create(a, Network));
                }
                else
                {
                    foreach (var a in receiver)
                        receiverAddreses.Add(BitcoinAddress.Create(a, Network));
                }
            }

            if ((receiverAddreses.Count > 1 && lots > 1) && (receiverAddreses.Count != lots))
            {
                throw new Exception($"If you want to split coins to different receivers, the number of lots and receivers must match. Receivers {receiverAddreses.Count}, Lost {lots}. Some of input address may be wrong.");
            }

            var tutxo = tutxos.FirstOrDefault();
            if (tutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            var txres = GetTransactionWithNeblioAndTokensInputs(nutxos, tutxos, addressForTx);
            var transaction = txres.Item1;
            var allNeblInputCoins = txres.Item2.Item1;

            if (metadata == null)
                metadata = new Dictionary<string, string>();

            metadata.Add("TransactionType", "Token Split");

            List<NTP1Instructions> TiList = new List<NTP1Instructions>();
            //Now make the transfer instruction

            var totalToksToSend = 0.0;
            if (receiverAddreses.Count > 0)
            {
                var index = 0;
                foreach (var add in receiverAddreses)
                {
                    NTP1Instructions ti = new NTP1Instructions();
                    ti.amount = (ulong)amount;
                    ti.vout_num = index;
                    TiList.Add(ti);
                    totalToksToSend += amount;
                    index++;
                }
            }
            else if (receiverAddreses.Count == 0 || lots > 1)
            {
                for (var i = 0; i < lots; i++)
                {
                    NTP1Instructions ti = new NTP1Instructions();
                    ti.amount = (ulong)amount;
                    ti.vout_num = i;
                    TiList.Add(ti);
                    totalToksToSend += amount;
                }
            }

            var metacomprimed = GetMetaComprimed(metadata);
            //Create the hex op_return
            string ti_script = NTP1ScriptHelpers._NTP1CreateTransferScript(TiList, metacomprimed); //No metadata

            var restOfToks = txres.Item2.Item2; //txres.Item2.Item2 holds all tokens in inputs tutxos
            restOfToks -= totalToksToSend;

            try
            {
                var fee = CalcFee(transaction.Inputs.Count, 2, JsonConvert.SerializeObject(metadata), true);

                var balanceinSat = Convert.ToUInt64(allNeblInputCoins * FromSatToMainRatio);

                if ((4 * MinimumAmount + fee) > balanceinSat)
                    throw new Exception("Not enought spendable Neblio on the address.");

                var diffinSat = balanceinSat - Convert.ToUInt64(fee) - Convert.ToUInt64(MinimumAmount); // one MinimumAmount subtracted is because of OP_RETURN, others are subtracted below based on number of receivers

                if (restOfToks > 0)
                    diffinSat -= Convert.ToUInt64(MinimumAmount);

                if (receiverAddreses.Count > 0)
                {
                    foreach (var recv in receiverAddreses)
                    {
                        diffinSat -= Convert.ToUInt64(MinimumAmount);
                        transaction.Outputs.Add(new Money(MinimumAmount), recv.ScriptPubKey); // send to receiver
                    }
                }
                else if (receiverAddreses.Count == 1)
                {
                    diffinSat -= Convert.ToUInt64(MinimumAmount);
                    transaction.Outputs.Add(new Money(MinimumAmount), receiverAddreses[0].ScriptPubKey); // send to receiver
                }
                else
                {
                    for (var i = 0; i < lots; i++)
                    {
                        diffinSat -= Convert.ToUInt64(MinimumAmount);
                        transaction.Outputs.Add(new Money(MinimumAmount), addressForTx.ScriptPubKey); // send to own
                    }
                }

                var ti_b = NTP1ScriptHelpers.UnhexToByteArray(ti_script);
                transaction.Outputs.Add(new TxOut()
                {
                    Value = new Money(MinimumAmount),
                    ScriptPubKey = CreateOPRETURNScript(ti_b)
                });

                if (diffinSat > 0)
                    transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender address

                if (restOfToks > 0) // just for minting new payment nft
                    transaction.Outputs.Add(new Money(MinimumAmount), addressForTx.ScriptPubKey); // add 10000 sat as carier of tokens which goes back

            }
            catch (Exception ex)
            {
                throw new Exception("Exception during adding outputs with payment in neblio." + ex.Message);
            }

            return transaction;
        }


        /// <summary>
        /// Function will sent exact NFT. 
        /// You must fill the input token utxo in data object!
        /// </summary>
        /// <param name="data">Send data, please see SendTokenTxData class for the details</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<Transaction> SendNFTTokenAsync(SendTokenTxData data, ICollection<Utxos> nutxos, double fee = 20000, GetAddressInfoResponse? addinfo = null, double latestblockheight = 0)
        {
            if (data.Metadata == null || data.Metadata.Count == 0)
                throw new Exception("Cannot send without metadata!");

            BitcoinAddress addressForTx;
            // create receiver address
            BitcoinAddress recaddr;
            try
            {
                addressForTx = BitcoinAddress.Create(data.SenderAddress, Network);
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            var nftutxo = data.sendUtxo.FirstOrDefault();
            var itt = nftutxo;
            var indx = 0;
            if (!string.IsNullOrEmpty(nftutxo) && nftutxo.Contains(':'))
            {
                var split = nftutxo.Split(':');
                if (split.Length > 1)
                {
                    itt = split[0];
                    indx = Convert.ToInt32(split[1]);
                }
            }

            var val = await NeblioAPIHelpers.ValidateOneTokenNFTUtxo(data.SenderAddress, data.Id, itt, indx, addinfo, latestblockheight);
            Utxos tutxo;
            if (val == -1)
                throw new Exception("Cannot send transaction, nft utxo is not spendable!");
            else
            {
                tutxo = new Utxos()
                {
                    Index = val,
                    Txid = itt,
                    Value = 10000,
                    Tokens = new List<Tokens>() { new Tokens() { Amount = 1, TokenId = data.Id } }
                };
            }
            if (nutxos == null || nutxos.Count == 0)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            var txres = GetTransactionWithNeblioAndTokensInputs(nutxos, new List<Utxos>() { tutxo }, addressForTx);
            var transaction = txres.Item1;
            var allNeblInputCoins = txres.Item2.Item1;

            List<NTP1Instructions> TiList = new List<NTP1Instructions>();
            //Now make the transfer instruction
            NTP1Instructions ti = new NTP1Instructions();
            ti.amount = 1;
            ti.vout_num = 0;
            TiList.Add(ti);

            var metacomprimed = GetMetaComprimed(data.Metadata);
            //Create the hex op_return
            string ti_script = NTP1ScriptHelpers._NTP1CreateTransferScript(TiList, metacomprimed); //No metadata

            try
            {
                fee = CalcFee(transaction.Inputs.Count, 2, JsonConvert.SerializeObject(data.Metadata), true);

                var balanceinSat = Convert.ToUInt64(allNeblInputCoins * FromSatToMainRatio);

                if ((4 * MinimumAmount + fee) > balanceinSat)
                    throw new Exception("Not enought spendable Neblio on the address.");

                var diffinSat = balanceinSat - Convert.ToUInt64(fee) - Convert.ToUInt64(MinimumAmount); // just for the OP_RETURN, NFT has own MinimumAmount from Input

                transaction.Outputs.Add(new Money(MinimumAmount), recaddr.ScriptPubKey); // send to receiver required amount

                var ti_b = NTP1ScriptHelpers.UnhexToByteArray(ti_script);
                transaction.Outputs.Add(new TxOut()
                {
                    Value = new Money(MinimumAmount),
                    ScriptPubKey = CreateOPRETURNScript(ti_b)
                });

                if (diffinSat > 0)
                    transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender address

            }
            catch (Exception ex)
            {
                throw new Exception("Exception during adding outputs with payment in neblio." + ex.Message);
            }

            return transaction;
        }

        /// <summary>
        /// Function will send standard Neblio transaction
        /// </summary>
        /// <param name="data">Send data, please see SendTxData class for the details</param>
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static Transaction GetNeblioTransactionObject(SendTxData data, ICollection<Utxos> nutxos)
        {
            if (data == null)
                throw new Exception("Data cannot be null!");

            // create receiver address
            BitcoinAddress addressForTx;
            BitcoinAddress recaddr;
            try
            {
                addressForTx = BitcoinAddress.Create(data.SenderAddress, Network);
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception)
            {
                throw new Exception("Cannot send transaction. cannot create address!");
            }

            var tx = GetTransactionWithNeblioInputs(nutxos, addressForTx);
            if (tx.Item1 == null)
                throw new Exception("Cannot create the transaction object.");

            var transaction = tx.Item1;
            var allNeblInputCoins = tx.Item2;
            try
            {
                var fee = CalcFee(transaction.Inputs.Count, 1, data.CustomMessage, false);

                var diff = (allNeblInputCoins - data.Amount) - (fee / FromSatToMainRatio);

                // create outputs
                transaction.Outputs.Add(new Money(Convert.ToInt64(data.Amount * FromSatToMainRatio)), recaddr.ScriptPubKey); // send to receiver required amount

                if (!string.IsNullOrEmpty(data.CustomMessage))
                {
                    diff -= (MinimumAmount / FromSatToMainRatio); // 10000 sat is need as value for minimal output even if it holds the OP_RETURN
                    var bytes = Encoding.UTF8.GetBytes(data.CustomMessage);
                    transaction.Outputs.Add(new TxOut()
                    {
                        Value = MinimumAmount,
                        ScriptPubKey = TxNullDataTemplate.Instance.GenerateScriptPubKey(bytes)
                    });
                }
                transaction.Outputs.Add(new Money(Convert.ToInt64(diff * FromSatToMainRatio)), addressForTx.ScriptPubKey); // get diff back to sender address
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during creating outputs. " + ex.Message);
            }

            return transaction;
        }

        /// <summary>
        /// Function will send standard Neblio transaction
        /// </summary>
        /// <param name="receivers"></param>
        /// <param name="lots"></param>
        /// <param name="amount"></param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<Transaction> SplitNeblioCoinTransactionAPIAsync(string address, List<string> receivers, int lots, double amount, ICollection<Utxos> nutxos)
        {
            if (address == null)
                throw new Exception("Address cannot be null!");

            if ((receivers.Count > 1 && lots > 1) && (receivers.Count != lots))
                throw new Exception($"If you want to split coins to different receivers, the number of lots and receivers must match. Receivers {receivers.Count}, Lost {lots}.");

            if (lots < 2 || lots > 25)
                throw new Exception("Count must be bigger than 2 and lower than 25.");

            var addressForTx = BitcoinAddress.Create(address, Network);
            List<BitcoinAddress> receiversAddresses = new List<BitcoinAddress>();

            if (receivers != null && receivers.Count > 0)
            {
                foreach (var r in receivers)
                {
                    try
                    {
                        receiversAddresses.Add(BitcoinAddress.Create(r, Network));
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Cannot load one of the receivers");
                    }
                }
            }

            if ((receiversAddresses.Count > 1 && lots > 1) && (receiversAddresses.Count != lots))
                throw new Exception($"If you want to split coins to different receivers, the number of lots and receivers must match. Receivers {receiversAddresses.Count}, Lost {lots}. Some of input address may be wrong.");

            var tx = GetTransactionWithNeblioInputs(nutxos, addressForTx);
            if (tx.Item1 == null)
                throw new Exception("Cannot create the transaction object.");

            var transaction = tx.Item1;
            var allNeblInputCoins = tx.Item2;

            try
            {
                var fee = CalcFee(transaction.Inputs.Count, 1, "", false);

                var totalAmount = 0.0;
                for (int i = 0; i < lots; i++)
                    totalAmount += amount;

                var all = Convert.ToUInt64(allNeblInputCoins * FromSatToMainRatio);
                var amountinSat = Convert.ToUInt64(totalAmount * FromSatToMainRatio);
                if (amountinSat > all)
                    throw new Exception("Not enought neblio for splitting.");

                var diffinSat = Convert.ToUInt64(all) - amountinSat - Convert.ToUInt64(fee);
                var splitinSat = Convert.ToUInt64(amount * FromSatToMainRatio);
                // create outputs

                if (receivers.Count == 0)
                {
                    for (int i = 0; i < lots; i++)
                        transaction.Outputs.Add(new Money(splitinSat), addressForTx.ScriptPubKey); // add all new splitted coins                   
                }
                else if (receivers.Count == 1)
                {
                    for (int i = 0; i < lots; i++)
                        transaction.Outputs.Add(new Money(splitinSat), receiversAddresses[0].ScriptPubKey); // add all new splitted coins                   
                }
                else if (receivers.Count > 1)
                {
                    for (int i = 0; i < lots; i++)
                        transaction.Outputs.Add(new Money(splitinSat), receiversAddresses[i].ScriptPubKey); // add all new splitted coins
                }

                transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender address
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during creating outputs. " + ex.Message);
            }

            return transaction;

        }



        /// <summary>
        /// This function will send Neblio payment together with the token whichc carry some metadata
        /// </summary>
        /// <param name="data">Mint data, please see MintNFTData class for the details</param>
        /// <param name="neblAmount">Amount of Neblio to send</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="paymentUtxoToReturn">If you returning some payment fill this</param>
        /// <param name="paymentUtxoIndexToReturn">If you returning some payment fill this</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<Transaction> SendNTP1TokenWithPaymentAPIAsync(SendTokenTxData data, double neblAmount, ICollection<Utxos> nutxos, string paymentUtxoToReturn = null, int paymentUtxoIndexToReturn = 0)
        {
            if (data.Metadata == null || data.Metadata.Count == 0)
                throw new Exception("Cannot send without metadata!");

            // load key and address
            BitcoinSecret key;
            BitcoinAddress addressForTx;
            // create receiver address
            BitcoinAddress recaddr;
            try
            {
                addressForTx = BitcoinAddress.Create(data.SenderAddress, Network);
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            Utxos tutxo;
            if (paymentUtxoToReturn == null)
            {
                var tutxos = await NeblioAPIHelpers.FindUtxoForMintNFT(data.SenderAddress, data.Id, 5);
                if (tutxos == null || tutxos.Count == 0)
                    throw new Exception("Cannot send transaction, cannot load sender token utxos, for buying you need at least 5 VENFT lot!");

                tutxo = tutxos.FirstOrDefault();
                if (tutxo == null)
                    throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");
            }
            else
            {
                tutxo = new Utxos()
                {
                    Txid = paymentUtxoToReturn,
                    Index = paymentUtxoIndexToReturn,
                    Value = 0.0001,
                    Tokens = new List<Tokens>()
                      {
                          new Tokens()
                          {
                              TokenId = data.Id,
                              Amount = 1
                          }
                      }
                };
            }

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            var txres = GetTransactionWithNeblioAndTokensInputs(nutxos, new List<Utxos> { tutxo }, addressForTx);
            var transaction = txres.Item1;
            var allNeblInputCoins = txres.Item2.Item1;

            List<NTP1Instructions> TiList = new List<NTP1Instructions>();
            //Now make the transfer instruction
            NTP1Instructions ti = new NTP1Instructions();
            ti.amount = 1;
            ti.vout_num = 0;
            TiList.Add(ti);

            var metacomprimed = GetMetaComprimed(data.Metadata);
            //Create the hex op_return
            string ti_script = NTP1ScriptHelpers._NTP1CreateTransferScript(TiList, metacomprimed); //No metadata

            var restOfToks = txres.Item2.Item2; //txres.Item2.Item2 holds all tokens in inputs tutxos
            restOfToks -= data.Amount;

            try
            {
                var fee = CalcFee(transaction.Inputs.Count, 4, JsonConvert.SerializeObject(data.Metadata), true);

                var amountinSat = Convert.ToUInt64(neblAmount * FromSatToMainRatio);
                var balanceinSat = Convert.ToUInt64(allNeblInputCoins * FromSatToMainRatio);

                if (((amountinSat + (ulong)(4 * MinimumAmount)) + fee) > balanceinSat)
                    throw new Exception("Not enought spendable Neblio on the address.");

                var diffinSat = balanceinSat - amountinSat - Convert.ToUInt64(fee) - Convert.ToUInt64(MinimumAmount) - Convert.ToUInt64(MinimumAmount); // fee is already included in previous output, last is token carriers
                if (restOfToks > 0)
                    diffinSat -= Convert.ToUInt64(MinimumAmount);

                transaction.Outputs.Add(new Money(MinimumAmount), recaddr.ScriptPubKey); // send to receiver required amount
                transaction.Outputs.Add(new Money(amountinSat), recaddr.ScriptPubKey); // send to receiver required amount of neblio

                var ti_b = NTP1ScriptHelpers.UnhexToByteArray(ti_script);
                transaction.Outputs.Add(new TxOut()
                {
                    Value = new Money(MinimumAmount),
                    ScriptPubKey = CreateOPRETURNScript(ti_b)
                });

                if (diffinSat > 0)
                    transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender address

                if (restOfToks > 0) // just for minting new payment nft
                    transaction.Outputs.Add(new Money(MinimumAmount), addressForTx.ScriptPubKey); // add 10000 sat as carier of tokens which goes back

            }
            catch (Exception ex)
            {
                throw new Exception("Exception during adding outputs with payment in neblio." + ex.Message);
            }

            return transaction;
        }

        /// <summary>
        /// This function will send Neblio payment together with the token whichc carry some metadata
        /// </summary>
        /// <param name="data">Mint data, please see MintNFTData class for the details</param>
        /// <param name="neblAmount">Amount of Neblio to send</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="tutxos">Optional list of the token utxos </param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<Transaction> SendNTP1TokenLotWithPaymentAPIAsync(SendTokenTxData data, double neblAmount, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            if (data.Metadata == null || data.Metadata.Count == 0)
                throw new Exception("Cannot send without metadata!");

            // load key and address
            BitcoinSecret key;
            BitcoinAddress addressForTx;

            // create receiver address
            BitcoinAddress recaddr;
            try
            {
                addressForTx = BitcoinAddress.Create(data.SenderAddress, Network);
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            if (tutxos == null || tutxos.Count == 0)
                throw new Exception("Cannot send transaction, cannot load sender token utxos!");

            var tutxo = tutxos.FirstOrDefault();
            if (tutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl token utxo!");

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            var txres = GetTransactionWithNeblioAndTokensInputs(nutxos, tutxos, addressForTx);
            var transaction = txres.Item1;
            var allNeblInputCoins = txres.Item2.Item1;

            List<NTP1Instructions> TiList = new List<NTP1Instructions>();
            //Now make the transfer instruction
            NTP1Instructions ti = new NTP1Instructions();
            ti.amount = (ulong)data.Amount;
            ti.vout_num = 0;
            TiList.Add(ti);

            var metacomprimed = GetMetaComprimed(data.Metadata);
            //Create the hex op_return
            string ti_script = NTP1ScriptHelpers._NTP1CreateTransferScript(TiList, metacomprimed); //No metadata

            var restOfToks = txres.Item2.Item2; //txres.Item2.Item2 holds all tokens in inputs tutxos
            restOfToks -= data.Amount;

            try
            {
                var fee = CalcFee(transaction.Inputs.Count, 4, JsonConvert.SerializeObject(data.Metadata), true);

                var amountinSat = Convert.ToUInt64(neblAmount * FromSatToMainRatio);
                var balanceinSat = Convert.ToUInt64(allNeblInputCoins * FromSatToMainRatio);

                if (((amountinSat + (ulong)(4 * MinimumAmount)) + fee) > balanceinSat)
                    throw new Exception("Not enought spendable Neblio on the address.");

                var diffinSat = balanceinSat - amountinSat - Convert.ToUInt64(fee) - Convert.ToUInt64(MinimumAmount) - Convert.ToUInt64(MinimumAmount); // fee is already included in previous output, last is token carriers
                if (restOfToks > 0)
                    diffinSat -= Convert.ToUInt64(MinimumAmount);

                transaction.Outputs.Add(new Money(MinimumAmount), recaddr.ScriptPubKey); // send to receiver required amount of tokens

                transaction.Outputs.Add(new Money(amountinSat), recaddr.ScriptPubKey); // send to receiver required amount of neblio

                var ti_b = NTP1ScriptHelpers.UnhexToByteArray(ti_script);
                transaction.Outputs.Add(new TxOut()
                {
                    Value = new Money(MinimumAmount),
                    ScriptPubKey = CreateOPRETURNScript(ti_b)
                });

                if (diffinSat > 0)
                    transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender address

                if (restOfToks > 0) // just for minting new payment nft
                    transaction.Outputs.Add(new Money(MinimumAmount), addressForTx.ScriptPubKey); // add 10000 sat as carier of tokens which goes back

            }
            catch (Exception ex)
            {
                throw new Exception("Exception during adding outputs with payment in neblio." + ex.Message);
            }

            return transaction;
        }

        /// <summary>
        /// Function will sign transaction with provided key and broadcast with Neblio API
        /// </summary>
        /// <param name="transaction">NBitcoin Transaction object</param>
        /// <param name="key">NBitcoin Key - must contain Private Key</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<string> SignAndBroadcast(Transaction transaction, BitcoinSecret key, bool withBroadcast = true, List<Utxos> addrutxos = null)
        {
            BitcoinAddress address = key.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network);

            // add coins
            List<ICoin> coins = new List<ICoin>();
            try
            {
                if (addrutxos == null)
                    addrutxos = await NeblioAPIHelpers.GetAddressUtxosListFromNewAPIAsync(address.ToString());

                // add all spendable coins of this address
                foreach (var inp in addrutxos)
                {
                    if (transaction.Inputs.FirstOrDefault(i => (i.PrevOut.Hash == uint256.Parse(inp.Txid)) && i.PrevOut.N == (uint)inp.Index) != null)
                    {
                        coins.Add(new Coin(uint256.Parse(inp.Txid), (uint)inp.Index, new Money((long)inp.Value), address.ScriptPubKey));
                    }
                }

                // add signature to inputs before signing
                foreach (var inp in transaction.Inputs)
                {
                    inp.ScriptSig = address.ScriptPubKey;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during loading inputs. " + ex.Message);
            }

            // sign
            try
            {
                var tx = transaction.ToString();
                var txhx = transaction.ToHex();

                transaction.Sign(key, coins);

                var sx = transaction.ToString();

                bool end = false;
                if (end)
                {
                    return string.Empty;
                }

                if (tx == sx)
                {
                    throw new Exception("Transaction was not signed. Probably not spendable source.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during signing tx! " + ex.Message);
            }

            bool endNow = false;
            // broadcast            
            var txhex = transaction.ToHex();

            if (withBroadcast)
            {
                //var res = await NeblioAPIHelpers.BroadcastSignedTransaction(txhex);
                var res = await NeblioAPIHelpers.BroadcastTransactionVEAPI(txhex);
                return res;
            }
            return txhex;
        }

        //////////////////////////////////////
        #region Multi Token Input Tx
        /// <summary>
        /// Transaction which sends multiple tokens from input to different outputs. For example process of the send Ordered NFT and NFT Receipt in one tx.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="nutxos"></param>
        /// <param name="fee"></param>
        /// <param name="isMintingOfCopy"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<Transaction> SendMultiTokenAPIAsync(SendTokenTxData data, ICollection<Utxos> nutxos, double fee = 20000, bool isMintingOfCopy = false, GetAddressInfoResponse? addinfo = null, double latestblockheight = 0)
        {
            // load key and address
            BitcoinSecret key;
            BitcoinAddress addressForTx;
            BitcoinAddress recaddr;
            try
            {
                addressForTx = BitcoinAddress.Create(data.SenderAddress, Network);
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            SendTokenRequest dto;
            dto = NeblioAPIHelpers.GetSendTokenObject(1, 20000, addressForTx.ToString(), data.Id);

            var tutxos = new List<Utxos>();
            // load utxos list if exists, other case leave it to Neblio API
            if (data.sendUtxo.Count > 0)
            {
                var first = true;
                foreach (var it in data.sendUtxo)
                {
                    var itt = it;
                    var indx = 0;
                    if (it.Contains(':'))
                    {
                        var splt = it.Split(':');
                        if (splt.Length > 1)
                        {
                            itt = splt[0];
                            indx = Convert.ToInt32(splt[1]);
                        }
                    }

                    double voutstate = -1;

                    try
                    {
                        if (first && isMintingOfCopy)
                            first = false;
                        else
                            voutstate = await NeblioAPIHelpers.ValidateOneTokenNFTUtxo(data.SenderAddress, data.Id, itt, indx, addinfo, latestblockheight);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Cannot validate utxo for multitoken payment.");
                    }

                    if ((!isMintingOfCopy && voutstate != -1) || (!first && isMintingOfCopy && voutstate != -1))
                    {
                        tutxos.Add(new Utxos()
                        {
                            Index = (int)voutstate,
                            Txid = itt,
                            Tokens = new List<Tokens>() { new Tokens() { Amount = 1, TokenId = data.Id } },
                            Value = 10000
                        });
                    }
                }
            }
            else
            {
                throw new Exception("This kind of transaction requires Token input utxo list.");
            }

            if (isMintingOfCopy)
            {
                // if not utxo provided, check the un NFT tokens sources. These with more than 1 token
                var utxs = await NeblioAPIHelpers.FindUtxoForMintNFT(data.SenderAddress, data.Id, 5, addinfo: addinfo, latestBlockHeight: latestblockheight);
                var ut = utxs?.FirstOrDefault();
                if (ut != null)
                    tutxos.Add(ut);
                else
                    throw new Exception("Cannot find utxo for minting NFT token. Wait for enough confirmation after previous transaction.");
            }

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            var txres = GetTransactionWithNeblioAndTokensInputs(nutxos, tutxos, addressForTx);
            var transaction = txres.Item1;
            var allNeblInputCoins = txres.Item2.Item1;

            List<NTP1Instructions> TiList = new List<NTP1Instructions>();
            //Now make the transfer instruction

            var totalToksToSend = 0.0;

            NTP1Instructions ti = new NTP1Instructions();
            ti.amount = (ulong)data.Amount;
            ti.vout_num = 0;
            TiList.Add(ti);
            totalToksToSend = data.Amount;

            NTP1Instructions ti1 = new NTP1Instructions();
            ti1.amount = 1;
            ti1.vout_num = 1;
            TiList.Add(ti1);
            totalToksToSend++;

            var metacomprimed = GetMetaComprimed(data.Metadata);
            //Create the hex op_return
            string ti_script = NTP1ScriptHelpers._NTP1CreateTransferScript(TiList, metacomprimed); //No metadata
            var restOfToks = txres.Item2.Item2; //txres.Item2.Item2 holds all tokens in inputs tutxos
            restOfToks -= totalToksToSend;

            try
            {
                fee = CalcFee(transaction.Inputs.Count, 3, JsonConvert.SerializeObject(data.Metadata), true);

                var balanceinSat = Convert.ToUInt64(allNeblInputCoins * FromSatToMainRatio);

                if ((4 * MinimumAmount + fee) > balanceinSat)
                    throw new Exception("Not enought spendable Neblio on the address.");

                var diffinSat = balanceinSat - Convert.ToUInt64(fee) - Convert.ToUInt64(MinimumAmount); // one MinimumAmount subtracted is because of OP_RETURN, others are subtracted below based on number of receivers

                if (restOfToks > 0)
                    diffinSat -= Convert.ToUInt64(MinimumAmount);

                if (!string.IsNullOrEmpty(data.ReceiverAddress))
                {
                    diffinSat -= Convert.ToUInt64(MinimumAmount);
                    transaction.Outputs.Add(new Money(MinimumAmount), recaddr.ScriptPubKey); // send to receiver
                }
                else
                {
                    diffinSat -= Convert.ToUInt64(MinimumAmount);
                    transaction.Outputs.Add(new Money(MinimumAmount), addressForTx.ScriptPubKey); // send to own
                }

                diffinSat -= Convert.ToUInt64(MinimumAmount);
                transaction.Outputs.Add(new Money(MinimumAmount), addressForTx.ScriptPubKey); // send to own

                var ti_b = NTP1ScriptHelpers.UnhexToByteArray(ti_script);
                transaction.Outputs.Add(new TxOut()
                {
                    Value = new Money(MinimumAmount),
                    ScriptPubKey = CreateOPRETURNScript(ti_b)
                });

                if (diffinSat > 0)
                    transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender address

                if (restOfToks > 0) // just for minting new payment nft
                    transaction.Outputs.Add(new Money(MinimumAmount), addressForTx.ScriptPubKey); // add 10000 sat as carier of tokens which goes back

            }
            catch (Exception ex)
            {
                throw new Exception("Exception during adding outputs with payment in neblio." + ex.Message);
            }

            return transaction;
        }

        //////////////////////////////////////
        /// <summary>
        /// Destroy of the NFT. It merge the NFT with the minting lot
        /// 1VENFT + 10VENFT => 11 VENFT
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ekey"></param>
        /// <param name="nutxos"></param>
        /// <param name="fee"></param>
        /// <param name="mintingUtxo"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<Transaction> DestroyNFTAsync(SendTokenTxData data, ICollection<Utxos> nutxos, double fee = 20000, Utxos mintingUtxo = null, GetAddressInfoResponse? addinfo = null, double latestblockheight = 0)
        {
            // load key and address
            BitcoinSecret key;
            BitcoinAddress addressForTx;
            BitcoinAddress recaddr;
            try
            {
                addressForTx = BitcoinAddress.Create(data.SenderAddress, Network);
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            SendTokenRequest dto;
            dto = NeblioAPIHelpers.GetSendTokenObject(1, 20000, addressForTx.ToString(), data.Id);

            var tutxos = new List<Utxos>();
            // load utxos list if exists, other case leave it to Neblio API
            if (data.sendUtxo.Count > 0)
            {
                foreach (var it in data.sendUtxo)
                {
                    var itt = it;
                    var indx = 0;
                    if (it.Contains(':'))
                    {
                        var splt = it.Split(':');
                        if (splt.Length > 1)
                        {
                            itt = splt[0];
                            indx = Convert.ToInt32(splt[1]);
                        }
                    }

                    double voutstate = -1;

                    try
                    {
                        voutstate = await NeblioAPIHelpers.ValidateOneTokenNFTUtxo(data.SenderAddress, data.Id, itt, indx, addinfo, latestblockheight);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Cannot validate utxo for multitoken payment.");
                    }

                    if (voutstate != -1)
                    {
                        tutxos.Add(new Utxos()
                        {
                            Index = (int)voutstate,
                            Txid = itt,
                            Tokens = new List<Tokens>() { new Tokens() { Amount = 1, TokenId = data.Id } },
                            Value = 10000
                        });
                    }
                }
            }
            else
            {
                throw new Exception("This kind of transaction requires Token input utxo list.");
            }

            if (mintingUtxo == null)
            {
                // if not utxo provided, check the un NFT tokens sources. These with more than 1 token
                var utxs = await NeblioAPIHelpers.FindUtxoForMintNFT(data.SenderAddress, data.Id, 5, addinfo: addinfo, latestBlockHeight: latestblockheight);
                var ut = utxs?.FirstOrDefault();
                if (ut != null)
                    tutxos.Add(ut);
                else
                    throw new Exception("Cannot find utxo for minting NFT token. Wait for enough confirmation after previous transaction.");

            }
            else
            {
                tutxos.Add(mintingUtxo);
            }

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            var txres = GetTransactionWithNeblioAndTokensInputs(nutxos, tutxos, addressForTx);
            var transaction = txres.Item1;
            var allNeblInputCoins = txres.Item2.Item1;

            List<NTP1Instructions> TiList = new List<NTP1Instructions>();
            //Now make the transfer instruction

            var totalToksToSend = txres.Item2.Item2;

            NTP1Instructions ti = new NTP1Instructions();
            ti.amount = (ulong)totalToksToSend;
            ti.vout_num = 0;
            TiList.Add(ti);

            var metacomprimed = GetMetaComprimed(data.Metadata);
            //Create the hex op_return
            string ti_script = NTP1ScriptHelpers._NTP1CreateTransferScript(TiList, metacomprimed); //No metadata

            try
            {
                fee = CalcFee(transaction.Inputs.Count, 3, JsonConvert.SerializeObject(data.Metadata), true);

                var balanceinSat = Convert.ToUInt64(allNeblInputCoins * FromSatToMainRatio);

                if ((4 * MinimumAmount + fee) > balanceinSat)
                    throw new Exception("Not enought spendable Neblio on the address.");

                var diffinSat = balanceinSat - Convert.ToUInt64(fee) - Convert.ToUInt64(MinimumAmount); // one MinimumAmount subtracted is because of OP_RETURN, others are subtracted below based on number of receivers

                if (!string.IsNullOrEmpty(data.ReceiverAddress))
                {
                    diffinSat -= Convert.ToUInt64(MinimumAmount);
                    transaction.Outputs.Add(new Money(MinimumAmount), recaddr.ScriptPubKey); // send to receiver
                }
                else
                {
                    diffinSat -= Convert.ToUInt64(MinimumAmount);
                    transaction.Outputs.Add(new Money(MinimumAmount), addressForTx.ScriptPubKey); // send to own
                }

                var ti_b = NTP1ScriptHelpers.UnhexToByteArray(ti_script);
                transaction.Outputs.Add(new TxOut()
                {
                    Value = new Money(MinimumAmount),
                    ScriptPubKey = CreateOPRETURNScript(ti_b)
                });

                if (diffinSat > 0)
                    transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender address

            }
            catch (Exception ex)
            {
                throw new Exception("Exception during adding outputs with payment in neblio." + ex.Message);
            }

            return transaction;
        }

        #endregion


        ///////////////////////////////////////////
        // Tools for addresses

        /// <summary>
        /// Find last send transaction by some address.
        /// This is usefull to obtain address public key from signature of input.
        /// </summary>
        /// <param name="address">Searched address</param>
        /// <returns>NBitcoin Transaction object</returns>
        public static async Task<Transaction> GetLastSentTransaction(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return null;
            }

            try
            {
                var addinfo = await NeblioAPIHelpers.GetClient().GetAddressAsync(address);
                if (addinfo == null || addinfo.Transactions.Count == 0)
                {
                    return null;
                }

                foreach (var t in addinfo.Transactions)
                {
                    var th = await NeblioAPIHelpers.GetTxHex(t);
                    var ti = Transaction.Parse(th, Network);
                    if (ti != null)
                    {
                        if (ti.Inputs[0].ScriptSig.GetSignerAddress(Network).ToString() == address)
                        {
                            return ti;
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load tx info. " + ex.Message);
                return null;
            }
        }



        /// <summary>
        /// Check if the private key is valid for the Neblio Network
        /// </summary>
        /// <param name="privatekey"></param>
        /// <returns></returns>
        public static BitcoinSecret IsPrivateKeyValid(string privatekey)
        {
            try
            {
                if (string.IsNullOrEmpty(privatekey) || privatekey.Length < 52 || privatekey[0] != 'T')
                {
                    return null;
                }

                var sec = new BitcoinSecret(privatekey, Network);

                if (sec != null)
                {
                    return sec;
                }
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }
        /// <summary>
        /// Parse the Neblio address from the private key
        /// </summary>
        /// <param name="privatekey"></param>
        /// <returns></returns>
        public static string GetAddressFromPrivateKey(string privatekey)
        {
            try
            {
                var p = IsPrivateKeyValid(privatekey);
                if (p != null)
                {
                    var address = p.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network);
                    if (address != null)
                    {
                        return address.ToString();
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Validate if the Neblio address is the correct
        /// </summary>
        /// <param name="neblioAddress"></param>
        /// <returns></returns>
        public static string ValidateNeblioAddress(string neblioAddress)
        {
            try
            {
                if (string.IsNullOrEmpty(neblioAddress) || neblioAddress.Length < 34 || neblioAddress[0] != 'N')
                {
                    return string.Empty;
                }

                BitcoinAddress address = BitcoinAddress.Create(neblioAddress, Network);
                if (!string.IsNullOrEmpty(address.ToString()))
                {
                    return address.ToString();
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return string.Empty;
        }


        /// <summary>
        /// Returns sended amount of neblio in some transaction. It counts the outputs which was send to input address
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="address">expected address where was nebl send in this tx</param>
        /// <returns></returns>
        public static double GetSendAmount(GetTransactionInfoResponse tx, string address)
        {
            BitcoinAddress addr;
            try
            {
                addr = BitcoinAddress.Create(address, Network);
            }
            catch (Exception)
            {
                const string exceptionMessage = "Cannot get amount of transaction. cannot create receiver address!";
                throw new Exception(exceptionMessage);
            }

            var vinamount = 0.0;
            foreach (var vin in tx.Vin)
            {
                if (vin.Addr == address)
                {
                    vinamount += ((double)vin.Value / FromSatToMainRatio);
                }
            }

            var amount = 0.0;
            foreach (var vout in tx.Vout)
            {
                if (vout.ScriptPubKey.Hex == addr.ScriptPubKey.ToHex())
                {
                    amount += ((double)vout.Value / FromSatToMainRatio);
                }
            }

            amount -= vinamount;

            return Math.Abs(amount);
        }

        /// <summary>
        /// Parse message from the OP_RETURN data in the tx
        /// </summary>
        /// <param name="txinfo"></param>
        /// <returns></returns>
        public static (bool, string) ParseNeblioMessage(GetTransactionInfoResponse txinfo)
        {
            if (txinfo == null)
            {
                return (false, "No input data provided.");
            }

            if (txinfo.Vout == null || txinfo.Vout.Count == 0)
            {
                return (false, "No outputs in transaction.");
            }

            foreach (var o in txinfo.Vout)
            {
                if (!string.IsNullOrEmpty(o.ScriptPubKey.Asm) && o.ScriptPubKey.Asm.Contains("OP_RETURN"))
                {
                    var message = o.ScriptPubKey.Asm.Replace("OP_RETURN ", string.Empty);
                    var bytes = StringExt.HexStringToBytes(message);
                    var msg = Encoding.UTF8.GetString(bytes);
                    return (true, msg);
                }
            }

            return (false, string.Empty);
        }

        /// <summary>
        /// Parse and decompress the custom metadata from the OP_RETURN output
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public static Dictionary<string, string> ParseCustomMetadata(string metadata)
        {
            try
            {
                var meta = NeblioAPIHelpers.ParseCustomMetadata(metadata);
                return meta;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot parse custom metadata from the transaction. " + ex.Message + "; original input metadata: " + metadata);
            }
            return new Dictionary<string, string>();
        }

        public static string? GetAddressStringFromSignedScriptPubKey(ScriptSig script)
        {
            return GetAddressFromSignedScriptPubKey(script)?.ToString();
        }
        public static BitcoinAddress? GetAddressFromSignedScriptPubKey(ScriptSig script)
        {
            if (script == null)
                return null;
            if (string.IsNullOrEmpty(script.Asm))
                return null;
            return GetAddressFromSignedScriptPubKey(script.Asm);
        }
        public static BitcoinAddress? GetAddressFromSignedScriptPubKey(string scriptAsm)
        {
            if (string.IsNullOrEmpty(scriptAsm))
                return null;

            var split = scriptAsm.Split(' ');
            if (split.Length >= 2)
            {
                var pubkey = split[split.Length - 1];
                BitcoinAddress add = null;
                try
                {
                    PubKey pk = new PubKey(pubkey);
                    add = pk.GetAddress(ScriptPubKeyType.Legacy, NeblioTransactionHelpers.Network);
                    return add;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Wrong public key input during parsing the Tx data. PubKey input:" + pubkey);
                }
            }
            return null;
        }

        public static PubKey? GetPubKeyFromSignedScriptPubKey(string scriptAsm)
        {
            if (string.IsNullOrEmpty(scriptAsm))
                return null;

            var split = scriptAsm.Split(' ');
            if (split.Length >= 2)
            {
                var pubkey = split[split.Length - 1];
                BitcoinAddress add = null;
                try
                {
                    var pk = new PubKey(pubkey);
                    return pk;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Wrong public key input during parsing the Tx data. PubKey input:" + pubkey);
                }
            }
            return null;
        }

    }
}
