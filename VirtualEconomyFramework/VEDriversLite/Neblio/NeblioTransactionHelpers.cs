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

namespace VEDriversLite
{

    /// <summary>
    /// Main Helper class for the Neblio Blockchain Transactions
    /// </summary>
    public static class NeblioTransactionHelpers
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static IClient _client;
        private static readonly string BaseURL = "https://ntp1node.nebl.io/";

        /// <summary>
        /// Turn on and off of the cache for address info, transaction info and metadata of the transaction
        /// If it is true the cache is on. It is default state.
        /// Usually it is important to turn it off just for unit tests which uses simulated address and tx data
        /// </summary>
        public static bool TurnOnCache = true;
        /// <summary>
        /// Conversion ration for Neblio to convert from sat to 1 NEBL
        /// </summary>
        public const double FromSatToMainRatio = 100000000;
        /// <summary>
        /// Maximum number of outputs which carry some token in the Neblio transaction
        /// </summary>
        public static int MaximumTokensOutpus = 10;
        /// <summary>
        /// Maximum number of outputs without tokens in the Neblio transaction
        /// </summary>
        public static int MaximumNeblioOutpus = 25;
        /// <summary>
        /// NBitcoin Instance of Mainet Network of Neblio
        /// </summary>
        public static Network Network = NBitcoin.Altcoins.Neblio.Instance.Mainnet;
        /// <summary>
        /// Minimum number of confirmation to send the transaction
        /// </summary>
        public static int MinimumConfirmations = 2;
        /// <summary>
        /// Minimum amount in Satoshi on Neblio Blockchain
        /// </summary>
        public static long MinimumAmount = 10000;
        /// <summary>
        /// Tokens Info for all already loaded tokens
        /// </summary>
        public static Dictionary<string, GetTokenMetadataResponse> TokensInfo = new Dictionary<string, GetTokenMetadataResponse>();
        /// <summary>
        /// Transaction info cache. If the tx was already loaded it will remember it if it has more than MinimumConfirmations
        /// </summary>
        public static ConcurrentDictionary<string, GetTransactionInfoResponse> TransactionInfoCache = new ConcurrentDictionary<string, GetTransactionInfoResponse>();
        /// <summary>
        /// Token metadata cache. It is same all the time for the specific hash of the tx
        /// </summary>
        public static ConcurrentDictionary<string, GetTokenMetadataResponse> TokenTxMetadataCache = new ConcurrentDictionary<string, GetTokenMetadataResponse>();
        /// <summary>
        /// Address info cache. It will save for at least one second address info. If it is older, it will reqeuest new info.
        /// </summary>
        public static ConcurrentDictionary<string, (DateTime, GetAddressInfoResponse)> AddressInfoCache = new ConcurrentDictionary<string, (DateTime, GetAddressInfoResponse)>();

        /// <summary>
        /// Main event info handler
        /// </summary>
        public static event EventHandler<IEventInfo> NewEventInfo;

        /// <summary>
        /// Create short version of address, 3 chars on start...3 chars on end
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string ShortenAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
            {
                return string.Empty;
            }

            var shortaddress = address.Substring(0, 3) + "..." + address.Substring(address.Length - 3);
            return shortaddress;
        }
        /// <summary>
        /// Create short version of txid hash, 3 chars on start...3 chars on end
        /// </summary>
        /// <param name="txid"></param>
        /// <param name="withDots">default true. add .... between start and end of the tx hash</param>
        /// <param name="len">Length of the result shortened tx hash</param>
        /// <returns></returns>
        public static string ShortenTxId(string txid, bool withDots = true, int len = 10)
        {
            if (string.IsNullOrEmpty(txid))
            {
                return string.Empty;
            }

            if (txid.Length < 10)
            {
                return txid;
            }

            string txids;
            if (withDots)
            {
                txids = txid.Remove(len / 2, txid.Length - len / 2) + "....." + txid.Remove(0, txid.Length - len / 2);
            }
            else
            {
                txids = txid.Remove(len / 2, txid.Length - len / 2) + txid.Remove(0, txid.Length - len / 2);
            }
            return txids;
        }

        /// <summary>
        /// Load informations about allowed tokens.
        /// </summary>
        /// <param name="tokenIds">List of allowed tokens to works with.</param>
        /// <returns></returns>
        public static async Task LoadAllowedTokensInfo(List<string> tokenIds)
        {
            foreach (var tok in tokenIds)
            {
                if (!TokensInfo.TryGetValue(tok, out _))
                {
                    var info = await GetTokenMetadata(tok);
                    if (info != null)
                    {
                        TokensInfo.Add(tok, info);
                    }
                }
            }
        }

        /// <summary>
        /// Function converts EncryptionKey (optionaly with password if it is not already loaded in ekey)
        /// and returns BitcoinAddress and BitcoinSecret classes from NBitcoin
        /// </summary>
        /// <param name="ekey"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static (BitcoinAddress, BitcoinSecret) GetAddressAndKey(EncryptionKey ekey, string password)
        {
            return GetAddressAndKeyInternal(ekey, password);
        }

        /// <summary>
        /// Function converts EncryptionKey (optionaly with password if it is not already loaded in ekey)
        /// and returns BitcoinAddress and BitcoinSecret classes from NBitcoin
        /// </summary>
        /// <param name="ekey"></param>
        /// <returns></returns>
        public static (BitcoinAddress, BitcoinSecret) GetAddressAndKey(EncryptionKey ekey)
        {
            return GetAddressAndKeyInternal(ekey, "");
        }

        private static (BitcoinAddress, BitcoinSecret) GetAddressAndKeyInternal(EncryptionKey ekey, string password)
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
                        key = ekey.GetEncryptedKey();
                    }
                    else if (ekey.IsEncrypted && (!string.IsNullOrEmpty(password) || ekey.IsPassLoaded))
                    {
                        if (ekey.IsPassLoaded)
                        {
                            key = ekey.GetEncryptedKey(string.Empty);
                        }
                        else
                        {
                            key = ekey.GetEncryptedKey(password);
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
                    BitcoinSecret loadedkey = NeblioTransactionHelpers.Network.CreateBitcoinSecret(key);
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
        /// Function will take hex of signed transaction and broadcast it via Neblio API
        /// </summary>
        /// <param name="txhex"></param>
        /// <returns></returns>
        public static async Task<string> BroadcastSignedTransaction(string txhex)
        {
            if (!string.IsNullOrEmpty(txhex))
            {
                var bdto = new BroadcastTxRequest()
                {
                    TxHex = txhex
                };

                var txid = await BroadcastNTP1TxAsync(bdto);

                if (!string.IsNullOrEmpty(txid))
                {
                    return txid;
                }
                else
                {
                    throw new Exception("Cannot broadcast transaction.");
                }
            }
            else
            {
                throw new Exception("Wrong input transaction for broadcast.");
            }
        }

        /// <summary>
        /// Function prepares SendTokenRequest object. It is important to initialitze correct inside properties
        /// </summary>
        /// <param name="amount">Amount to send</param>
        /// <param name="fee">Fee - min 10000, with metadata you need at least 20000</param>
        /// <param name="receiver">Receiver of the amount</param>
        /// <param name="tokenId">Token Id hash</param>
        /// <returns></returns>
        public static SendTokenRequest GetSendTokenObject(double amount, double fee = 20000, string receiver = "", string tokenId = "")
        {
            if (amount == 0)
            {
                throw new Exception("Amount to send cannot be 0.");
            }

            if (string.IsNullOrEmpty(receiver))
            {
                throw new Exception("Receiver Address not provided.");
            }

            if (string.IsNullOrEmpty(tokenId))
            {
                throw new Exception("Token Id not provided.");
            }

            if (fee < MinimumAmount)
            {
                throw new Exception("Fee cannot be smaller than 10000 Sat.");
            }

            var dto = new SendTokenRequest
            {
                Metadata = new Metadata2()
            };
            dto.Metadata.AdditionalProperties = new Dictionary<string, object>();
            dto.Metadata.UserData = new UserData3
            {
                AdditionalProperties = new Dictionary<string, object>(),
                Meta = new List<JObject>()
            };
            dto.Fee = fee;
            dto.Flags = new Flags2() { SplitChange = true };
            dto.Sendutxo = new List<string>();
            dto.From = null;

            dto.To = new List<To>()
                    {
                        new To()
                        {
                            Address = receiver,
                            Amount = amount,
                            TokenId = tokenId
                        }
                    };

            return dto;
        }

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
            if (size_m > 4)
            {
                throw new Exception(exceptionMessage);
            }

            var fee = basicFee + (int)(size_m) * basicFee;

            if (isTokenTransaction)
            {
                fee += basicFee;
            }

            return fee;
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
        /// Function will Mint NFT from lot of the tokens
        /// </summary>
        /// <param name="data">Mint data, please see MintNFTData class for the details</param>
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="tutxos">Optional input token utxo</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<Transaction> MintNFTTokenAsync(MintNFTData data, EncryptionKey ekey, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            return await MintMultiNFTTokenAsyncInternal(data, 0, ekey, nutxos, tutxos, false);
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
        public static async Task<Transaction> MintMultiNFTTokenAsync(MintNFTData data, int coppies, EncryptionKey ekey, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            return await MintMultiNFTTokenAsyncInternal(data, coppies, ekey, nutxos, tutxos, true);
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
        public static async Task<Transaction> MintMultiNFTTokenAsyncInternal(MintNFTData data, int coppies, EncryptionKey ekey, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos,bool multiTokens)
        {
            if (data.Metadata == null || data.Metadata.Count == 0)
            {
                throw new Exception("Cannot send without metadata!");
            }

            List<BitcoinAddress> receiverAddreses = new List<BitcoinAddress>();

            // load key and address
            BitcoinSecret key;
            BitcoinAddress addressForTx;

            var k = GetAddressAndKey(ekey);            
            addressForTx = k.Item1;
            if (!string.IsNullOrEmpty(data.ReceiverAddress) || data.MultipleReceivers.Count > 0)
            {
                if (data.MultipleReceivers.Count == 0)
                    receiverAddreses.Add(BitcoinAddress.Create(data.ReceiverAddress, Network));
                else
                {
                    foreach(var a in data.MultipleReceivers)
                        receiverAddreses.Add(BitcoinAddress.Create(a, Network));
                }                    
            }

            if (tutxos == null || tutxos.Count == 0)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");
            }

            var tutxo = tutxos.FirstOrDefault();
            if (tutxo == null)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");
            }

            if (nutxos == null || nutxos.Count == 0)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");
            }

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");
            }

            data.Metadata.Add(new KeyValuePair<string, string>("SourceUtxo", tutxo.Txid));
            data.Metadata.Add(new KeyValuePair<string, string>("NFT FirstTx", "true"));

            var fee = CalcFee(2, 1 + coppies, JsonConvert.SerializeObject(data.Metadata), true);

            // create and init send token request dto for Neblio API
            SendTokenRequest dto;

            if (!string.IsNullOrEmpty(data.ReceiverAddress))
            {
                dto = GetSendTokenObject(1, fee, data.ReceiverAddress, data.Id);
            }
            else
            {
                dto = GetSendTokenObject(1, fee, data.SenderAddress, data.Id);
            }

            if (data.Metadata != null)
            {
                foreach (var d in data.Metadata)
                {
                    var obj = new JObject
                    {
                        [d.Key] = d.Value
                    };
                    dto.Metadata.UserData.Meta.Add(obj);
                }
            }

            if (coppies > 1)
            {
                for (int i = 1; i < coppies; i++)
                {
                    var dummykey = new Key();
                    var dummyadd = dummykey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network);
                    dto.To.Add(
                    new To()
                    {
                        Address = dummyadd.ToString(),
                        Amount = 1,
                        TokenId = data.Id
                    });
                }
            }

            if (dto.Metadata.UserData.Meta.Count == 0)
            {
                throw new Exception("Cannot mint NFT without any metadata");
            }
            
            dto.From = null;

            if (multiTokens)
            {
                //add all token utxos
                foreach (var t in tutxos)
                {
                    if (t.Txid != nutxo.Txid)
                    {
                        dto.Sendutxo.Add(t.Txid + ":" + ((int)t.Index).ToString());
                    }
                    else
                    {
                        if (t.Index != nutxo.Index)
                        {
                            dto.Sendutxo.Add(t.Txid + ":" + ((int)t.Index).ToString());
                        }
                    }
                }
            }
            else
            {
                if (tutxo.Txid == nutxo.Txid && tutxo.Index == nutxo.Index)
                {
                    throw new Exception("Same input for token and neblio. Wrong input.");
                }
                dto.Sendutxo.Add(tutxo.Txid + ":" + ((int)tutxo.Index).ToString());
            }            

            dto.Sendutxo.Add(nutxo.Txid + ":" + ((int)nutxo.Index).ToString());

            // create raw tx
            string hexToSign;
            try
            {
                //var dtostr = JsonConvert.SerializeObject(dto);
                hexToSign = await SendRawNTP1TxAsync(dto);
                if (string.IsNullOrEmpty(hexToSign))
                {
                    Console.WriteLine("Cannot get correct raw token hex.");
                    Console.WriteLine("Data: " + JsonConvert.SerializeObject(data));
                    Console.WriteLine("Dto: " + JsonConvert.SerializeObject(dto));
                    throw new Exception("Cannot get correct raw token hex.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Exception during sending raw token tx");
                Console.WriteLine("Data: " + JsonConvert.SerializeObject(data));
                Console.WriteLine("Dto: " + JsonConvert.SerializeObject(dto));
                throw new Exception("Exception during sending raw token tx" + ex.Message);
            }

            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(hexToSign, Network, out var transaction))
            {
                throw new Exception("Cannot parse token tx raw hex.");
            }

            if (multiTokens)
            {
                var i = 0;
                foreach (var output in transaction.Outputs)
                {
                    if (!output.ScriptPubKey.ToString().Contains("RETURN"))
                    {
                        if (receiverAddreses.Count > 0)
                        {
                            if (receiverAddreses.Count > 1)
                            { 
                                output.ScriptPubKey = receiverAddreses[i].ScriptPubKey;
                                i++;
                            }
                            else if (receiverAddreses.Count == 1)
                            {
                                output.ScriptPubKey = receiverAddreses[0].ScriptPubKey;
                            }
                            else
                            {
                                throw new Exception("Cannot send token, no receiver address.");
                            }
                        }
                        else
                        {
                            output.ScriptPubKey = addressForTx.ScriptPubKey;
                        }
                    }
                    else
                    {
                        break;
                    }
                }
            }
            return transaction;
        }

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
        /// <param name="fee">Fee - 20000 minimum</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<Transaction> SplitNTP1TokensAsync(List<string> receiver, int lots, int amount, string tokenId,
                                                              IDictionary<string, string> metadata,
                                                              EncryptionKey ekey,
                                                              ICollection<Utxos> nutxos,
                                                              ICollection<Utxos> tutxos)
        {
            if (metadata == null || metadata.Count == 0)
            {
                throw new Exception("Cannot send without metadata!");
            }

            if ((receiver.Count > 1 && lots > 1) && (receiver.Count != lots))
            {
                throw new Exception($"If you want to split coins to different receivers, the number of lots and receivers must match. Receivers {receiver.Count}, Lost {lots}.");
            }

            // load key and address
            BitcoinSecret key;
            BitcoinAddress addressForTx;
            List<BitcoinAddress> receiversAddresses = new List<BitcoinAddress>();

            var k = GetAddressAndKey(ekey);
            addressForTx = k.Item1;
            if (receiver.Count > 0)
            {
                foreach (var r in receiver)
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
            {
                throw new Exception($"If you want to split coins to different receivers, the number of lots and receivers must match. Receivers {receiversAddresses.Count}, Lost {lots}. Some of input address may be wrong.");
            }

            if (tutxos == null || tutxos.Count == 0)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");
            }

            var tutxo = tutxos.FirstOrDefault();
            if (tutxo == null)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");
            }

            if (nutxos == null || nutxos.Count == 0)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");
            }

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");
            }

            metadata.Add("TransactionType", "Token Split");

            var fee = CalcFee(2, lots, JsonConvert.SerializeObject(metadata), true);

            // create and init send token request dto for Neblio API
            SendTokenRequest dto;

            if (receiversAddresses.Count == 0)
            {
                var dummykey = new Key();
                var dummyadd = dummykey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network);
                dto = GetSendTokenObject(amount, fee, dummyadd.ToString(), tokenId);
            }
            else
            {
                dto = GetSendTokenObject(amount, fee, receiversAddresses[0].ToString(), tokenId);
            }

            if (metadata != null)
            {
                foreach (var d in metadata)
                {
                    var obj = new JObject
                    {
                        [d.Key] = d.Value
                    };
                    dto.Metadata.UserData.Meta.Add(obj);
                }
            }

            if (lots > 1)
            {
                for (int i = 1; i < lots; i++)
                {
                    if (receiversAddresses.Count == 0 || receiversAddresses.Count == 1)
                    {
                        var dummykey = new Key();
                        var dummyadd = dummykey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network);
                        dto.To.Add(
                        new To()
                        {
                            Address = dummyadd.ToString(),
                            Amount = amount,
                            TokenId = tokenId
                        });
                    }
                    else
                    {
                        dto.To.Add(
                        new To()
                        {
                            Address = receiversAddresses[i].ToString(),
                            Amount = amount,
                            TokenId = tokenId
                        });

                    }
                }
            }


            if (dto.Metadata.UserData.Meta.Count == 0)
            {
                throw new Exception("Cannot mint NFT without any metadata");
            }

            dto.From = null;

            //add all token utxos
            foreach (var t in tutxos)
            {
                if (t.Txid != nutxo.Txid)
                {
                    dto.Sendutxo.Add(t.Txid + ":" + ((int)t.Index).ToString());
                }
                else
                {
                    if (t.Index != nutxo.Index)
                    {
                        dto.Sendutxo.Add(t.Txid + ":" + ((int)t.Index).ToString());
                    }
                }
            }
            // add neblio utxo
            dto.Sendutxo.Add(nutxo.Txid + ":" + ((int)nutxo.Index).ToString());
            if (dto.Sendutxo.Count < 2)
            {
                throw new Exception("Not enouht inputs sources for the split transaction.");
            }

            // create raw tx
            string hexToSign;
            try
            {
                //var dtostr = JsonConvert.SerializeObject(dto);
                hexToSign = await SendRawNTP1TxAsync(dto);
                if (string.IsNullOrEmpty(hexToSign))
                {
                    throw new Exception("Cannot get correct raw token hex.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during sending raw token tx" + ex.Message);
            }

            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(hexToSign, Network, out var transaction))
            {
                throw new Exception("Cannot parse token tx raw hex.");
            }

            var j = 0;
            foreach (var output in transaction.Outputs)
            {
                if (!output.ScriptPubKey.ToString().Contains("RETURN"))
                {
                    if (receiversAddresses.Count == 0 || receiversAddresses.Count == 1)
                    {
                        if (receiversAddresses.Count == 1)
                        {
                            output.ScriptPubKey = receiversAddresses[0].ScriptPubKey;
                        }
                        else
                        {
                            output.ScriptPubKey = addressForTx.ScriptPubKey;
                        }
                    }
                    else
                    {
                        output.ScriptPubKey = receiversAddresses[j].ScriptPubKey;
                    }
                    j++;
                }
                else
                {
                    break;
                }
            }

            return transaction;
        }

        /// <summary>
        /// Function will sent exact NFT. 
        /// You must fill the input token utxo in data object!
        /// </summary>
        /// <param name="data">Send data, please see SendTokenTxData class for the details</param>
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="fee">Fee - 20000 minimum</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<Transaction> SendNFTTokenAsync(SendTokenTxData data, ICollection<Utxos> nutxos)
        {
            if (data.Metadata == null || data.Metadata.Count == 0)
            {
                throw new Exception("Cannot send without metadata!");
            }

            var nftutxo = data.sendUtxo.FirstOrDefault();
            var itt = nftutxo;
            var indx = 0;
            if (nftutxo.Contains(':'))
            {
                var splt = nftutxo.Split(':');
                if (splt.Length > 1)
                {
                    itt = splt[0];
                    indx = Convert.ToInt32(splt[1]);
                }
            }

            var val = await ValidateOneTokenNFTUtxo(data.SenderAddress, data.Id, itt, indx);
            string tutxo;
            if (val == -1)
            {
                throw new Exception("Cannot send transaction, nft utxo is not spendable!");
            }
            else
            {
                tutxo = nftutxo + ":" + ((int)val).ToString();
            }

            if (nutxos == null || nutxos.Count == 0)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");
            }

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");
            }

            var fee = CalcFee(2, 1, JsonConvert.SerializeObject(data.Metadata), true);

            // create and init send token request dto for Neblio API
            SendTokenRequest dto;

            dto = GetSendTokenObject(1, fee, data.ReceiverAddress, data.Id);

            if (data.Metadata != null)
            {
                foreach (var d in data.Metadata)
                {
                    var obj = new JObject
                    {
                        [d.Key] = d.Value
                    };
                    dto.Metadata.UserData.Meta.Add(obj);
                }
            }

            if (tutxo.Length < 3)
            {
                throw new Exception("Same input for token and neblio. Wrong input.");
            }

            if (tutxo == nutxo.Txid + ":" + ((int)nutxo.Index).ToString())
            {
                throw new Exception("Same input for token and neblio. Wrong input.");
            }

            dto.Sendutxo.Add(tutxo);
            dto.Sendutxo.Add(nutxo.Txid + ":" + ((int)nutxo.Index).ToString());

            // create raw tx
            string hexToSign;
            try
            {
                hexToSign = await SendRawNTP1TxAsync(dto);
                if (string.IsNullOrEmpty(hexToSign))
                {
                    throw new Exception("Cannot get correct raw token hex.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during sending raw token tx" + ex.Message);
            }

            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(hexToSign, Network, out var transaction))
            {
                throw new Exception("Cannot parse token tx raw hex.");
            }

            return transaction;

        }

        /// <summary>
        /// Function will send lot of tokens (means more than 1) to some address
        /// </summary>
        /// <param name="data">Send data, please see SendtokenTxTData class for the details</param>
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="tutxos">Optional input token utxo</param>
        /// <param name="fee">Fee - 20000 minimum</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<Transaction> SendTokenLotAsync(SendTokenTxData data, EncryptionKey ekey, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos, double fee = 20000)
        {
            if (data.Metadata == null || data.Metadata.Count == 0)
            {
                throw new Exception("Cannot send without metadata!");
            }

            // load key and address
            BitcoinSecret key;
            BitcoinAddress addressForTx;

            var k = GetAddressAndKey(ekey);
            key = k.Item2;
            addressForTx = k.Item1;


            var tutxo = tutxos.FirstOrDefault();
            if (tutxo == null)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");
            }

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");
            }

            fee = CalcFee(2, 1, JsonConvert.SerializeObject(data.Metadata), true);

            // create and init send token request dto for Neblio API
            SendTokenRequest dto;

            dto = GetSendTokenObject(data.Amount, fee, data.ReceiverAddress, data.Id);

            if (data.Metadata != null)
            {
                foreach (var d in data.Metadata)
                {
                    var obj = new JObject
                    {
                        [d.Key] = d.Value
                    };
                    dto.Metadata.UserData.Meta.Add(obj);
                }
            }

            //add all token utxos
            foreach (var t in tutxos)
            {
                if (t.Txid != nutxo.Txid)
                {
                    dto.Sendutxo.Add(t.Txid + ":" + ((int)t.Index).ToString());
                }
                else
                {
                    if (t.Index != nutxo.Index)
                    {
                        dto.Sendutxo.Add(t.Txid + ":" + ((int)t.Index).ToString());
                    }
                }
            }
            // add neblio utxo
            dto.Sendutxo.Add(nutxo.Txid + ":" + ((int)nutxo.Index).ToString());
            if (dto.Sendutxo.Count < 2)
            {
                throw new Exception("Not enouht inputs sources for the split transaction.");
            }

            // create raw tx
            string hexToSign;
            try
            {
                hexToSign = await SendRawNTP1TxAsync(dto);
                if (string.IsNullOrEmpty(hexToSign))
                {
                    throw new Exception("Cannot get correct raw token hex.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during sending raw token tx" + ex.Message);
            }

            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(hexToSign, Network, out var transaction))
            {
                throw new Exception("Cannot parse token tx raw hex.");
            }

            return transaction;
        }


        /// <summary>
        /// Function will send standard Neblio transaction
        /// </summary>
        /// <param name="data">Send data, please see SendTxData class for the details</param>
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="fee">Fee - 10000 minimum</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static Transaction GetNeblioTransactionObject(SendTxData data, EncryptionKey ekey, ICollection<Utxos> nutxos)
        {
            if (data == null)
            {
                throw new Exception("Data cannot be null!");
            }

            if (ekey == null)
            {
                throw new Exception("Account cannot be null!");
            }

            // create receiver address
            BitcoinAddress recaddr;
            try
            {
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            // load key and address
            BitcoinSecret key;
            BitcoinAddress addressForTx;

            var k = GetAddressAndKey(ekey, data.Password);
            key = k.Item2;
            addressForTx = k.Item1;


            var tx = GetTransactionWithNeblioInputs(nutxos, addressForTx);
            if (tx.Item1 == null)
            {
                throw new Exception("Cannot create the transaction object.");
            }

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
        /// <param name="sender"></param>
        /// <param name="receivers"></param>
        /// <param name="lots"></param>
        /// <param name="amount"></param>
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="fee">Fee - 10000 minimum</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<Transaction> SplitNeblioCoinTransactionAPIAsync(List<string> receivers, int lots, double amount, EncryptionKey ekey, ICollection<Utxos> nutxos)
        {
            if (ekey == null)
            {
                throw new Exception("Account cannot be null!");
            }

            if ((receivers.Count > 1 && lots > 1) && (receivers.Count != lots))
            {
                throw new Exception($"If you want to split coins to different receivers, the number of lots and receivers must match. Receivers {receivers.Count}, Lost {lots}.");
            }

            if (lots < 2 || lots > 25)
            {
                throw new Exception("Count must be bigger than 2 and lower than 25.");
            }

            // create receiver address
            BitcoinSecret key = null;
            BitcoinAddress addressForTx = null;
            List<BitcoinAddress> receiversAddresses = new List<BitcoinAddress>();
            try
            {
                var k = GetAddressAndKey(ekey);
                key = k.Item2;
                addressForTx = k.Item1;
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
            }
            catch (Exception)
            {
                throw new Exception("Cannot send transaction. cannot create sender address!");
            }

            if ((receiversAddresses.Count > 1 && lots > 1) && (receiversAddresses.Count != lots))
            {
                throw new Exception($"If you want to split coins to different receivers, the number of lots and receivers must match. Receivers {receiversAddresses.Count}, Lost {lots}. Some of input address may be wrong.");
            }

            var tx = GetTransactionWithNeblioInputs(nutxos, addressForTx);
            if (tx.Item1 == null)
            {
                throw new Exception("Cannot create the transaction object.");
            }

            var transaction = tx.Item1;
            var allNeblInputCoins = tx.Item2;

            try
            {
                var fee = CalcFee(transaction.Inputs.Count, 1, "", false);

                var totalAmount = 0.0;
                for (int i = 0; i < lots; i++)
                {
                    totalAmount += amount;
                }

                var all = Convert.ToUInt64(allNeblInputCoins * FromSatToMainRatio);
                var amountinSat = Convert.ToUInt64(totalAmount * FromSatToMainRatio);
                if (amountinSat > all)
                {
                    throw new Exception("Not enought neblio for splitting.");
                }

                var diffinSat = Convert.ToUInt64(all) - amountinSat - Convert.ToUInt64(fee);
                var splitinSat = Convert.ToUInt64(amount * FromSatToMainRatio);
                // create outputs

                if (receivers.Count == 0)
                {
                    for (int i = 0; i < lots; i++)
                    {
                        transaction.Outputs.Add(new Money(splitinSat), addressForTx.ScriptPubKey); // add all new splitted coins
                    }
                }
                else if (receivers.Count == 1)
                {
                    for (int i = 0; i < lots; i++)
                    {
                        transaction.Outputs.Add(new Money(splitinSat), receiversAddresses[0].ScriptPubKey); // add all new splitted coins
                    }
                }
                else if (receivers.Count > 1)
                {
                    for (int i = 0; i < lots; i++)
                    {
                        transaction.Outputs.Add(new Money(splitinSat), receiversAddresses[i].ScriptPubKey); // add all new splitted coins
                    }
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
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="neblAmount">Amount of Neblio to send</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="paymentUtxoToReturn">If you returning some payment fill this</param>
        /// <param name="paymentUtxoIndexToReturn">If you returning some payment fill this</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<Transaction> SendNTP1TokenWithPaymentAPIAsync(SendTokenTxData data, EncryptionKey ekey, double neblAmount, ICollection<Utxos> nutxos, string paymentUtxoToReturn = null, int paymentUtxoIndexToReturn = 0)
        {
            if (data.Metadata == null || data.Metadata.Count == 0)
            {
                throw new Exception("Cannot send without metadata!");
            }

            if (neblAmount == 0)
            {
                throw new Exception("Neblio amount cannot be 0 in Token+Nebl transaction.");
            }

            // load key and address
            BitcoinAddress addressForTx;

            var k = GetAddressAndKey(ekey);
            addressForTx = k.Item1;


            // create receiver address
            BitcoinAddress recaddr;
            try
            {
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            Utxos tutxo;
            if (paymentUtxoToReturn == null)
            {
                var tutxos = await FindUtxoForMintNFT(data.SenderAddress, data.Id, 5);
                if (tutxos == null || tutxos.Count == 0)
                {
                    throw new Exception("Cannot send transaction, cannot load sender token utxos, for buying you need at least 5 VENFT lot!");
                }

                tutxo = tutxos.FirstOrDefault();
                if (tutxo == null)
                {
                    throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");
                }
            }
            else
            {
                tutxo = new Utxos()
                {
                    Txid = paymentUtxoToReturn,
                    Index = paymentUtxoIndexToReturn
                };
            }
            if (nutxos == null || nutxos.Count == 0)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");
            }

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");
            }

            // create and init send token request dto for Neblio API
            var dto = new SendTokenRequest();

            dto = GetSendTokenObject(1, 50000, data.ReceiverAddress, data.Id); //set maximum fee

            if (data.Metadata != null)
            {
                foreach (var d in data.Metadata)
                {
                    var obj = new JObject
                    {
                        [d.Key] = d.Value
                    };
                    dto.Metadata.UserData.Meta.Add(obj);
                }
            }

            if (tutxo.Txid == nutxo.Txid && tutxo.Index == nutxo.Index)
            {
                throw new Exception("Same input for token and neblio. Wrong input.");
            }

            dto.Sendutxo.Add(tutxo.Txid + ":" + ((int)tutxo.Index).ToString());
            dto.Sendutxo.Add(nutxo.Txid + ":" + ((int)nutxo.Index).ToString());

            // create raw tx
            var hexToSign = string.Empty;

            hexToSign = await SendRawNTP1TxAsync(dto);
            if (string.IsNullOrEmpty(hexToSign))
            {
                throw new Exception("Cannot get correct raw token hex.");
            }


            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(hexToSign, Network, out var transaction))
            {
                throw new Exception("Cannot parse token tx raw hex.");
            }

            var allNeblInputCoins = 0.0; //this is because of the optimization. When we iterate through values we can sum them for later use

            try
            {
                transaction.Inputs.Clear();

                // add token input
                transaction.Inputs.Add(new TxIn()
                {
                    PrevOut = new OutPoint(uint256.Parse(tutxo.Txid), (int)tutxo.Index),
                    ScriptSig = addressForTx.ScriptPubKey,
                });

                // add inputs with Neblio to pay the payment for the NFT
                foreach (var utxo in nutxos)
                {
                    transaction.Inputs.Add(new TxIn()
                    {
                        PrevOut = new OutPoint(uint256.Parse(utxo.Txid), (int)utxo.Index),
                        ScriptSig = addressForTx.ScriptPubKey,
                    });
                    allNeblInputCoins += (double)utxo.Value / FromSatToMainRatio;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during loading inputs. " + ex.Message);
            }

            try
            {
                // remove token carrier, will be added later - just for minting new nft
                if (string.IsNullOrEmpty(paymentUtxoToReturn))
                {
                    transaction.Outputs.RemoveAt(3);
                }
                //remove old calculated output with the diff
                transaction.Outputs.RemoveAt(2);

                var fee = CalcFee(transaction.Inputs.Count, 2, JsonConvert.SerializeObject(data.Metadata), true);
                
                var amountinSat = Convert.ToUInt64(neblAmount * FromSatToMainRatio);
                var balanceinSat = Convert.ToUInt64(allNeblInputCoins * FromSatToMainRatio);

                if ((amountinSat + fee) > balanceinSat)
                {
                    throw new Exception("Not enought spendable Neblio on the address.");
                }

                var diffinSat = balanceinSat - amountinSat - Convert.ToUInt64(fee) - Convert.ToUInt64(MinimumAmount) - Convert.ToUInt64(MinimumAmount); // fee is already included in previous output, last is token carriers

                // create outputs
                transaction.Outputs.Add(new Money(amountinSat), recaddr.ScriptPubKey); // send to receiver required amount
                if (diffinSat > 0)
                {
                    transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender address
                }

                if (string.IsNullOrEmpty(paymentUtxoToReturn)) // just for minting new payment nft
                {
                    transaction.Outputs.Add(new Money(MinimumAmount), addressForTx.ScriptPubKey); // add 10000 sat as carier of tokens which goes back
                }
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
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="neblAmount">Amount of Neblio to send</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="tutxos">Optional list of the token utxos </param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<Transaction> SendNTP1TokenLotWithPaymentAPIAsync(SendTokenTxData data, EncryptionKey ekey, double neblAmount, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            if (data.Metadata == null || data.Metadata.Count == 0)
            {
                throw new Exception("Cannot send without metadata!");
            }

            if (neblAmount == 0)
            {
                throw new Exception("Neblio amount cannot be 0 in Token+Nebl transaction.");
            }

            // load key and address
            BitcoinAddress addressForTx;

            var k = GetAddressAndKey(ekey);
            addressForTx = k.Item1;

            // create receiver address
            BitcoinAddress recaddr;
            try
            {
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            if (tutxos == null || tutxos.Count == 0)
            {
                throw new Exception("Cannot send transaction, cannot load sender token utxos!");
            }

            var tutxo = tutxos.FirstOrDefault();
            if (tutxo == null)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");
            }

            if (nutxos == null || nutxos.Count == 0)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");
            }

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");
            }

            // create and init send token request dto for Neblio API
            var dto = new SendTokenRequest();

            double fee = 20000;
            dto = GetSendTokenObject(data.Amount, fee, data.ReceiverAddress, data.Id);

            if (data.Metadata != null)
            {
                foreach (var d in data.Metadata)
                {
                    var obj = new JObject
                    {
                        [d.Key] = d.Value
                    };
                    dto.Metadata.UserData.Meta.Add(obj);
                }
            }

            if (tutxo.Txid == nutxo.Txid && tutxo.Index == nutxo.Index)
            {
                throw new Exception("Same input for token and neblio. Wrong input.");
            }

            // add inputs to the transaction for Neblio API
            var numberOfUsedTutxos = 0;
            var loadedtokens = 0.0;
            foreach (var tu in tutxos)
            {
                if (loadedtokens < data.Amount)
                {
                    dto.Sendutxo.Add(tu.Txid + ":" + ((int)tu.Index).ToString());
                    loadedtokens += (double)tu.Tokens.ToList()[0].Amount;
                    numberOfUsedTutxos++;
                }
            }
            foreach (var u in nutxos)
            {
                dto.Sendutxo.Add(u.Txid + ":" + ((int)u.Index).ToString());
            }

            // create raw tx
            string hexToSign;

            hexToSign = await SendRawNTP1TxAsync(dto);
            if (string.IsNullOrEmpty(hexToSign))
            {
                throw new Exception("Cannot get correct raw token hex.");
            }


            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(hexToSign, Network, out var transaction))
            {
                throw new Exception("Cannot parse token tx raw hex.");
            }


            // this flag will check, remove, add token return output carrier, will be added later
            var outputForTokensBack = true;

            var allNeblInputCoins = 0.0; //this is because of the optimization. When we iterate through values we can sum them for later use

            try
            {
                transaction.Inputs.Clear();

                foreach (var utxo in tutxos)
                {
                    if (numberOfUsedTutxos > 0)
                    {
                        transaction.Inputs.Add(new TxIn()
                        {
                            PrevOut = new OutPoint(uint256.Parse(utxo.Txid), (int)utxo.Index),
                            ScriptSig = addressForTx.ScriptPubKey,
                        });
                        numberOfUsedTutxos--;
                    }
                }

                // add inputs with Neblio to pay the payment for the NFT
                foreach (var utxo in nutxos)
                {
                    transaction.Inputs.Add(new TxIn()
                    {
                        PrevOut = new OutPoint(uint256.Parse(utxo.Txid), (int)utxo.Index),
                        ScriptSig = addressForTx.ScriptPubKey,
                    });
                    allNeblInputCoins += (double)utxo.Value / FromSatToMainRatio;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during loading inputs. " + ex.Message);
            }

            try
            {
                // remove token return output carrier, will be added later
                // if the minting supply has more than needed we will need to return the rest to main address
                // Neblio API add this output automatically
                if (transaction.Outputs.Count > 3)
                {
                    transaction.Outputs.RemoveAt(3);
                }
                else
                {
                    outputForTokensBack = false;
                }

                //remove old calculated output with the diff
                if (transaction.Outputs.Count > 2)
                {
                    transaction.Outputs.RemoveAt(2);
                }

                var amountinSat = Convert.ToUInt64(neblAmount * FromSatToMainRatio);
                var balanceinSat = Convert.ToUInt64(allNeblInputCoins * FromSatToMainRatio);

                fee = CalcFee(transaction.Inputs.Count, 2, JsonConvert.SerializeObject(data.Metadata), true);

                if ((amountinSat + fee) > balanceinSat)
                {
                    throw new Exception("Not enought spendable Neblio on the address.");
                }

                var diffinSat = balanceinSat - amountinSat - Convert.ToUInt64(fee) - Convert.ToUInt64(MinimumAmount); // fee is already included in previous output, last is token carrier

                // create outputs
                transaction.Outputs.Add(new Money(amountinSat), recaddr.ScriptPubKey); // send to receiver required amount
                if (diffinSat > 0)
                {
                    transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender address
                }

                if (outputForTokensBack)
                {
                    transaction.Outputs.Add(new Money(MinimumAmount), addressForTx.ScriptPubKey); // add 10000 sat as carier of tokens which goes back
                }
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
        /// <param name="address">NBitcoin address - must match with the provided key</param>
        /// <returns>New Transaction Hash - TxId</returns>
        private static async Task<string> SignAndBroadcast(Transaction transaction, BitcoinSecret key)
        {
            BitcoinAddress address = key.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network);

            // add coins
            List<ICoin> coins = new List<ICoin>();
            try
            {
                var addrutxos = await GetAddressUtxosObjects(address.ToString());

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

            // broadcast            
            var txhex = transaction.ToHex();
            var res = await BroadcastSignedTransaction(txhex);
            return res;
        }

        //////////////////////////////////////
        #region Multi Token Input Tx
        /// <summary>
        /// Transaction which sends multiple tokens from input to different outputs. For example process of the send Ordered NFT and NFT Receipt in one tx.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ekey"></param>
        /// <param name="nutxos"></param>
        /// <param name="fee"></param>
        /// <param name="isMintingOfCopy"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public static async Task<Transaction> SendMultiTokenAPIAsync(SendTokenTxData data, EncryptionKey ekey, ICollection<Utxos> nutxos, double fee = 20000, bool isMintingOfCopy = false)
        {
            // load key and address
            BitcoinSecret keyfromFile;
            BitcoinAddress addressForTx;

            var k = GetAddressAndKey(ekey);
            keyfromFile = k.Item2;
            addressForTx = k.Item1;

            // create receiver address
            BitcoinAddress recaddr;
            try
            {
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            // create and init send token request dto for Neblio API
            var dto = new SendTokenRequest();

            dto = GetSendTokenObject(data.Amount, fee, data.ReceiverAddress, data.Id);

            dto.To.Add(
                    new To()
                    {
                        Address = data.SenderAddress,//"NPWBL3i8ZQ8tmhDtrixXwYd93nofmunvhA",
                        Amount = 1,
                        TokenId = data.Id
                    });

            if (data.Metadata != null)
            {
                foreach (var d in data.Metadata)
                {
                    var obj = new JObject
                    {
                        [d.Key] = d.Value
                    };

                    dto.Metadata.UserData.Meta.Add(obj);
                }
            }


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
                        {
                            dto.Sendutxo.Add(it);
                            first = false;
                            // skip
                        }
                        else
                        {
                            voutstate = await ValidateOneTokenNFTUtxo(data.SenderAddress, data.Id, itt, indx);
                        }
                    }
                    catch (Exception)
                    {
                        throw new Exception("Cannot validate utxo for multitoken payment.");
                    }

                    if ((!isMintingOfCopy && voutstate != -1) || (!first && isMintingOfCopy && voutstate != -1))
                    {
                        dto.Sendutxo.Add(itt + ":" + ((int)voutstate).ToString()); // copy received utxos and add item number of vout after validation
                    }
                }
            }
            else
            {
                throw new Exception("This kind of transaction requires Token input utxo list.");
            }

            if (dto.Sendutxo.Count < 2)
            {
                throw new Exception("This kind of transaction requires Token input utxo list with at least 2 one token utox");
            }

            // neblio utxo
            if (nutxos == null || nutxos.Count == 0)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");
            }

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");
            }

            dto.Sendutxo.Add(nutxo.Txid + ":" + ((int)nutxo.Index).ToString());

            fee = CalcFee(dto.Sendutxo.Count, 1, JsonConvert.SerializeObject(data.Metadata), true);
            dto.Fee = fee;

            // create raw tx
            var hexToSign = string.Empty;

            hexToSign = await SendRawNTP1TxAsync(dto);
            if (string.IsNullOrEmpty(hexToSign))
            {
                throw new Exception("Cannot get correct raw token hex.");
            }

            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(hexToSign, Network, out var transaction))
            {
                throw new Exception("Cannot parse token tx raw hex.");
            }

            if (isMintingOfCopy)
            {
                transaction.Inputs.Add(new TxIn()
                {
                    PrevOut = new OutPoint(uint256.Parse(data.sendUtxo.Last()), 0),
                });
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
        public static async Task<Transaction> DestroyNFTAsync(SendTokenTxData data, EncryptionKey ekey, ICollection<Utxos> nutxos, double fee = 20000, Utxos mintingUtxo = null)
        {

            // load key and address
            BitcoinAddress addressForTx;

            var k = GetAddressAndKey(ekey);
            addressForTx = k.Item1;

            // create receiver address
            BitcoinAddress recaddr;
            try
            {
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            fee = CalcFee(data.sendUtxo.Count, 1, JsonConvert.SerializeObject(data.Metadata), true);

            // create and init send token request dto for Neblio API
            var dto = new SendTokenRequest();

            // use just temporary address, will be changed to main address later after go through neblio API create tx command
            dto = GetSendTokenObject(data.Amount, fee, "NPWBL3i8ZQ8tmhDtrixXwYd93nofmunvhA", data.Id);

            if (data.Metadata != null)
            {
                foreach (var d in data.Metadata)
                {
                    var obj = new JObject
                    {
                        [d.Key] = d.Value
                    };

                    dto.Metadata.UserData.Meta.Add(obj);
                }
            }


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
                        voutstate = await ValidateOneTokenNFTUtxo(data.SenderAddress, data.Id, itt, indx);
                    }
                    catch (Exception)
                    {
                        throw new Exception("Cannot validate utxo for multitoken payment.");
                    }

                    if (voutstate != -1)
                    {                    
                        dto.Sendutxo.Add(itt + ":" + ((int)voutstate).ToString()); // copy received utxos and add item number of vout after validation
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
                var utxs = await FindUtxoForMintNFT(data.SenderAddress, data.Id, 5);
                var ut = utxs?.FirstOrDefault();
                if (ut != null)
                {
                    dto.Sendutxo.Add(ut.Txid + ":" + ((int)ut.Index).ToString());
                    dto.To.FirstOrDefault().Amount += (double)ut?.Tokens?.ToList().FirstOrDefault()?.Amount; // add minting Utxo amount
                }
                else
                    throw new Exception("Cannot find utxo for minting NFT token. Wait for enough confirmation after previous transaction.");

            }
            else
            {
                dto.Sendutxo.Add(mintingUtxo.Txid + ":" + ((int)mintingUtxo.Index).ToString());
                dto.To.FirstOrDefault().Amount += (double)mintingUtxo?.Tokens?.ToList().FirstOrDefault()?.Amount; // add minting Utxo amount
            }


            if (dto.Sendutxo.Count < 2)
            {
                throw new Exception("This kind of transaction requires Token input utxo list with at least 2 token utox (NFT + Minting).");
            }

            // neblio utxo
            if (nutxos == null || nutxos.Count == 0)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");
            }

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");
            }

            dto.Sendutxo.Add(nutxo.Txid + ":" + ((int)nutxo.Index).ToString());

            // create raw tx
            string hexToSign;

            hexToSign = await SendRawNTP1TxAsync(dto);
            if (string.IsNullOrEmpty(hexToSign))
            {
                throw new Exception("Cannot get correct raw token hex.");
            }


            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(hexToSign, Network, out var transaction))
            {
                throw new Exception("Cannot parse token tx raw hex.");
            }

            transaction.Outputs[0].ScriptPubKey = addressForTx.ScriptPubKey;

            return transaction;
        }

        #endregion


        ///////////////////////////////////////////
        // Tools for addresses
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
        /// Check if the number of the confirmation is enough for doing transactions.
        /// It mainly usefull for UI stuff or console.
        /// </summary>
        /// <param name="confirmations"></param>
        /// <returns></returns>
        public static string IsEnoughConfirmationsForSend(int confirmations)
        {
            if (confirmations > MinimumConfirmations)
            {
                return ">" + MinimumConfirmations.ToString();
            }

            return confirmations.ToString();
        }

        ///////////////////////////////////////////
        // calls of Neblio API and helpers

        /// <summary>
        /// Returns private client for Neblio API. If it is null, it will create new instance.
        /// </summary>
        /// <returns></returns>
        public static IClient GetClient()
        {
            if (_client == null)
            {
                _client = new Client(httpClient) { BaseUrl = BaseURL };
            }

            return _client;
        }

        /// <summary>
        /// This method is written for Unit tests to pass in Mock client object
        /// </summary>
        /// <param name="client"></param>
        public static void GetClient(IClient client)
        {
            _client = client;
        }

        /// <summary>
        /// Send request for creating RAW token transaction
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<string> SendRawNTP1TxAsync(SendTokenRequest data)
        {
            try
            {
                var info = await GetClient().SendTokenAsync(data);
                return info.TxHex;
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot Create raw token tx. " + ex.Message);
            }
        }

        /// <summary>
        /// Broadcast of signed transaction. Works for Neblio and Token transactions.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static async Task<string> BroadcastNTP1TxAsync(BroadcastTxRequest data)
        {
            var info = await GetClient().BroadcastTxAsync(data);
            return info.Txid;
        }

        /// <summary>
        /// Return Address info object. Contains list of all transactions
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static async Task<GetAddressResponse> AddressInfoAsync(string addr)
        {
            if (string.IsNullOrEmpty(addr))
            {
                return new GetAddressResponse();
            }

            var address = await GetClient().GetAddressAsync(addr);
            return address;
        }

        /// <summary>
        /// Return address info object. this object contains list of Utxos.
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static async Task<GetAddressInfoResponse> AddressInfoUtxosAsync(string addr)
        {
            if (string.IsNullOrEmpty(addr))
            {
                return new GetAddressInfoResponse();
            }

            if (TurnOnCache && AddressInfoCache.TryGetValue(addr, out var info))
            {
                if ((DateTime.UtcNow - info.Item1) < new TimeSpan(0, 0, 1))
                {
                    GetAddressInfoResponse ainfo = info.Item2;
                    return ainfo;
                }
                else
                {
                    var address = await GetClient().GetAddressInfoAsync(addr);
                    if (address != null)
                    {
                        if (AddressInfoCache.TryRemove(addr, out info))
                        {
                            AddressInfoCache.TryAdd(addr, (DateTime.UtcNow, address));
                        }

                        return address;
                    }
                }
            }
            else
            {
                var address = await GetClient().GetAddressInfoAsync(addr);
                if (address != null && TurnOnCache)
                    AddressInfoCache.TryAdd(addr, (DateTime.UtcNow, address));

                return address;
            }
            return new GetAddressInfoResponse();
        }

        /// <summary>
        /// Return list of Utxos object.
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static async Task<ICollection<Utxos>> GetAddressUtxosObjects(string addr)
        {
            if (string.IsNullOrEmpty(addr))
            {
                return new List<Utxos>();
            }

            try
            {
                var addinfo = await GetClient().GetAddressInfoAsync(addr);
                return addinfo.Utxos;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load address utxos. " + ex.Message);
                return new List<Utxos>();
            }
        }

        /// <summary>
        /// Returns list of all Utxos which contains some tokens
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="addressinfo"></param>
        /// <returns></returns>
        public static async Task<ICollection<Utxos>> GetAddressTokensUtxos(string addr, GetAddressInfoResponse addressinfo = null)
        {
            if (string.IsNullOrEmpty(addr))
            {
                return new List<Utxos>();
            }

            if (addressinfo == null)
            {
                addressinfo = await AddressInfoUtxosAsync(addr);
            }

            var utxos = new List<Utxos>();
            if (addressinfo?.Utxos != null)
            {
                foreach (var u in addressinfo.Utxos)
                {
                    if (u != null && u.Tokens.Count > 0)
                    {
                        utxos.Add(u);
                    }
                }
            }

            return utxos;
        }

        /// <summary>
        /// Return transaction Hex
        /// </summary>
        /// <param name="txid"></param>
        /// <returns></returns>
        public static async Task<string> GetTxHex(string txid)
        {
            if (string.IsNullOrEmpty(txid))
            {
                return string.Empty;
            }

            var tx = await GetTransactionInfo(txid);
            if (tx != null)
            {
                return tx.Hex;
            }
            else
            {
                return string.Empty;
            }
        }

        private static void AddToTransactionInfoCache(GetTransactionInfoResponse txinfo)
        {
            if (txinfo == null)
            {
                return;
            }

            if (txinfo.Confirmations > MinimumConfirmations + 2)
            {
                TransactionInfoCache.TryAdd(txinfo.Txid, txinfo);
            }
        }

        /// <summary>
        /// Returns list of all Utxos which contains just one token, means amount = 1
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="allowedTokens">Load just the allowed tokens</param>
        /// <param name="addressinfo"></param>
        /// <returns></returns>
        public static async Task<ICollection<Utxos>> GetAddressNFTsUtxos(string addr, List<string> allowedTokens, GetAddressInfoResponse addressinfo = null)
        {
            if (string.IsNullOrEmpty(addr))
            {
                return new List<Utxos>();
            }

            if (addressinfo == null)
            {
                addressinfo = await AddressInfoUtxosAsync(addr);
            }

            var utxos = new List<Utxos>();

            if (addressinfo?.Utxos != null)
            {
                foreach (var u in addressinfo.Utxos)
                {
                    if (u != null && u.Tokens != null)
                    {
                        foreach (var tok in u.Tokens)
                        {
                            if (allowedTokens.Contains(tok.TokenId) && tok.Amount == 1)
                            {
                                utxos.Add(u);
                            }
                        }
                    }
                }
            }

            if (utxos == null || utxos.Count == 0)
            {
                return new List<Utxos>();
            }

            var ouxox = utxos.OrderByDescending(u => u.Blocktime).ToList();
            return ouxox;
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
        /// Returns list of spendable utxos which together match some input required amount for some transaction
        /// </summary>
        /// <param name="address">address which has utxos for spend - sender in tx</param>
        /// <param name="addinfo">If you already have loaded addinfo pass it</param>
        /// <param name="latestBlockHeight"></param>
        /// <param name="minAmount">minimum amount of one utxo</param>
        /// <param name="requiredAmount">amount what must be collected even by multiple utxos</param>
        /// <returns></returns>
        public static async Task<ICollection<Utxos>> GetAddressNeblUtxo(string address, double minAmount = 0.0001, double requiredAmount = 0.0001, GetAddressInfoResponse addinfo = null, double latestBlockHeight = 0)
        {
            var founded = 0.0;
            var resp = new List<Utxos>();

            if (addinfo.Utxos.Count == 0)
            {
                try
                {
                    addinfo = await AddressInfoUtxosAsync(address);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot Obtain the Address info. " + ex.Message);
                }
            }

            var utxos = addinfo?.Utxos;
            if (utxos == null)
            {
                return resp;
            }

            if (latestBlockHeight == 0)
                latestBlockHeight = await NeblioTransactionsCache.LatestBlockHeight(utxos.Where(u => u.Blockheight.Value > 0)?.FirstOrDefault()?.Txid, address);
            
            utxos = utxos.OrderByDescending(u => u.Value).ToList();

            foreach (var ut in utxos.Where(u => u.Blockheight.Value > 0 && u.Value > MinimumAmount && u.Tokens?.Count == 0 && ((double)u.Value) > (minAmount * FromSatToMainRatio)))
            {
                double UtxoBlockHeight = ut.Blockheight != null ? ut.Blockheight.Value : 0;
                if (IsValidUtxo(UtxoBlockHeight, latestBlockHeight))
                {
                    resp.Add(ut);
                    founded += ((double)ut.Value / FromSatToMainRatio);
                    if (founded > requiredAmount)
                    {
                        return resp;
                    }
                }
            }

            return new List<Utxos>();
            
        }

        private static bool IsValidUtxo(double UtxoBlockHeight, double latestBlockCount)
        {
            var confirmation = latestBlockCount - UtxoBlockHeight;
            return confirmation > MinimumConfirmations;
        }

        /// <summary>
        /// Check if the NFT token is spendable. Means utxos with token amount = 1
        /// </summary>
        /// <param name="address">address which should have this utxo</param>
        /// <param name="addinfo">if you already have addinfo pass it</param>
        /// <param name="latestBlockHeight"></param>
        /// <param name="tokenId">input token id hash</param>
        /// <param name="txid">input txid hash</param>
        /// <param name="indx"></param>
        /// <returns>true and index of utxo</returns>
        public static async Task<double> ValidateOneTokenNFTUtxo(string address, string tokenId, string txid, int indx, GetAddressInfoResponse addinfo = null, double latestBlockHeight = 0)
        {
            if (addinfo == null)
            {
                try
                {
                    addinfo = await AddressInfoUtxosAsync(address);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot Obtain the Address info. " + ex.Message);
                }
            }

            var utxos = addinfo?.Utxos;
            if (utxos == null)
            {
                return -1;
            }

            var uts = utxos.Where(u => (u.Txid == txid && u.Index == indx)); // you can have multiple utxos with same txid but different amount of tokens
            if (uts == null)
            {
                return -1;
            }

            if (latestBlockHeight == 0)
                latestBlockHeight = await NeblioTransactionsCache.LatestBlockHeight(utxos.Where(u => u.Blockheight.Value > 0)?.FirstOrDefault()?.Txid, address);
            
            foreach (var ut in uts.Where(u => u.Blockheight.Value > 0 && u.Tokens != null && u.Tokens.Count > 0))
            {
                var toks = ut.Tokens.ToArray();
                if (toks[0].TokenId == tokenId && toks[0].Amount == 1)
                {
                    double UtxoBlockHeight = ut.Blockheight != null ? ut.Blockheight.Value : 0;
                    if (IsValidUtxo(UtxoBlockHeight, latestBlockHeight))
                    {
                        return ((double)ut.Index);
                    }
                }
            }

            return -1;
        }

        /// <summary>
        /// Find utxo which can be used for minting. It means it has token amount > 1
        /// </summary>
        /// <param name="addr">address which has utxos</param>
        /// <param name="tokenId">token id hash</param>
        /// <param name="numberToMint">number of tokens which will be minted - because of multimint</param>
        /// <param name="oneTokenSat">this is usually default. On Neblio all token tx should have value 10000sat</param>
        /// <param name="addinfo"></param>
        /// <param name="latestBlockHeight"></param>
        /// <returns></returns>
        public static async Task<List<Utxos>> FindUtxoForMintNFT(string addr, string tokenId, int numberToMint = 1, double oneTokenSat = 10000, GetAddressInfoResponse addinfo = null, double latestBlockHeight = 0)
        {
            if (addinfo == null)
            {
                try
                {
                    addinfo = await AddressInfoUtxosAsync(addr);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot Obtain the Address info. " + ex.Message);
                }
            }

            var utxos = addinfo?.Utxos;
            var resp = new List<Utxos>();
            var founded = 0.0;

            if (utxos == null)
                return resp;

            if (latestBlockHeight == 0)
                latestBlockHeight = await NeblioTransactionsCache.LatestBlockHeight(utxos.Where(u => u.Blockheight.Value > 0)?.FirstOrDefault()?.Txid, addr);
            
            utxos = utxos.OrderByDescending(u => u.Value).ToList();

            utxos = utxos.Where(u => u.Tokens != null).ToList();
            if (utxos == null)
                return resp;

            foreach (var ut in utxos.Where(u => u.Blockheight.Value > 0 && u.Tokens.Count > 0))
            {
                var tok = ut.Tokens.ToArray()?[0];
                if (tok != null && tok.TokenId == tokenId && tok?.Amount > 3)
                {
                    double UtxoBlockHeight = ut.Blockheight != null ? ut.Blockheight.Value : 0;
                    if (IsValidUtxo(UtxoBlockHeight, latestBlockHeight))
                    {
                        founded += (double)tok.Amount;
                        resp.Add(ut);
                        if (founded > numberToMint)
                        {
                            break;
                        }
                    }
                }
            }

            resp = resp.OrderByDescending(t => t.Tokens.ToArray()[0].Amount).ToList();
            founded = 0.0;
            var res = new List<Utxos>();
            foreach (var r in resp)
            {
                founded += (double)r.Tokens.ToList()[0].Amount;
                res.Add(r);
                if (founded > numberToMint)
                {
                    break;
                }
            }

            if (res.Count > 10)// neblio API cannot handle more than 10 inputs
            {
                return new List<Utxos>();
            }

            return res;
        }
        /// <summary>
        /// Get token metadata from the specific transaction cache logic
        /// </summary>
        /// <param name="tokenid"></param>
        /// <param name="txid"></param>
        /// <param name="verbosity"></param>
        /// <returns></returns>
        public static async Task<GetTokenMetadataResponse> GetTokenMetadataOfUtxoCache(string tokenid, string txid, double verbosity = 0)
        {
            if (TurnOnCache && TokenTxMetadataCache.TryGetValue(txid, out var tinfo))
            {
                return tinfo;
            }
            else
            {
                var info = await GetClient().GetTokenMetadataOfUtxoAsync(tokenid, txid, 0);
                if (info != null && TurnOnCache)
                    TokenTxMetadataCache.TryAdd(txid, info);

                return info;
            }
        }

        /// <summary>
        /// Returns metadata in the token transction
        /// </summary>
        /// <param name="tokenid">token id hash</param>
        /// <param name="txid">tx id hash</param>
        /// <returns></returns>
        public static async Task<Dictionary<string, string>> GetTransactionMetadata(string tokenid, string txid)
        {
            if (string.IsNullOrEmpty(txid))
            {
                return new Dictionary<string, string>();
            }

            var resp = new Dictionary<string, string>();
            var info = await GetTokenMetadataOfUtxoCache(tokenid, txid, 0);

            if (info.MetadataOfUtxo != null && info.MetadataOfUtxo.UserData.Meta.Count > 0)
            {
                foreach (var o in info.MetadataOfUtxo.UserData.Meta)
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

            return resp;
        }

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
                var addinfo = await GetClient().GetAddressAsync(address);
                if (addinfo == null || addinfo.Transactions.Count == 0)
                {
                    return null;
                }

                foreach (var t in addinfo.Transactions)
                {
                    var th = await GetTxHex(t);
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
        /// Get transaction info.
        /// </summary>
        /// <param name="txid">tx id hash</param>
        /// <returns>Neblio API GetTransactionInfo object</returns>
        public static async Task<GetTransactionInfoResponse> GetTransactionInfo(string txid)
        {
            if (string.IsNullOrEmpty(txid))
            {
                return new GetTransactionInfoResponse();
            }

            try
            {
                GetTransactionInfoResponse tx = null;
                if (TurnOnCache && TransactionInfoCache.TryGetValue(txid, out var txinfo))
                {
                    tx = txinfo;
                }
                else
                {
                    tx = await GetClient().GetTransactionInfoAsync(txid);
                    if (TurnOnCache)
                        AddToTransactionInfoCache(tx);
                }
                if (tx != null)
                {
                    return tx;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot load tx {txid} info. " + ex.Message);
            }
            return new GetTransactionInfoResponse();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="txid"></param>
        /// <param name="mode"></param>
        /// <param name="txinfo"></param>
        /// <returns></returns>
        public static async Task<string> GetTransactionInternal(string txid, string mode, GetTransactionInfoResponse txinfo = null)
        {
            if (string.IsNullOrEmpty(txid))
            {
                return string.Empty;
            }

            try
            {
                if (txinfo == null)
                {
                    var tx = await GetTransactionInfo(txid);
                    if (tx != null)
                    {
                        txinfo = tx;
                    }
                }

                var data = "";
                if (mode == "sender")
                {
                    data = txinfo.Vin.ToList()[0]?.PreviousOutput?.Addresses.ToList()[0];
                }
                else if (mode == "receiver" )
                {
                    data = txinfo.Vout.ToList()[0]?.ScriptPubKey.Addresses.ToList()[0];
                }
                                
                return data;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load tx info. " + ex.Message);
                return string.Empty;
            }
        }

        /// <summary>
        /// Get transaction sender.
        /// </summary>
        /// <param name="txid">tx id hash</param>
        /// <param name="txinfo">if you already have txinfo object</param>
        /// <returns>Sender address</returns>
        public static async Task<string> GetTransactionSender(string txid, GetTransactionInfoResponse txinfo = null)
        {
            return await GetTransactionInternal(txid, "sender", txinfo);
        }

        /// <summary>
        /// Get transaction sender.
        /// </summary>
        /// <param name="txid">tx id hash</param>
        /// <param name="txinfo">if you already have txinfo object</param>
        /// <returns>Sender address</returns>
        public static async Task<string> GetTransactionReceiver(string txid, GetTransactionInfoResponse txinfo = null)
        {
            return await GetTransactionInternal(txid, "receiver", txinfo);
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
                    var bytes = HexStringToBytes(message);
                    var msg = Encoding.UTF8.GetString(bytes);
                    return (true, msg);
                }
            }

            return (false, string.Empty);
        }

        /// <summary>
        /// Convert the hex string to bytes
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static byte[] HexStringToBytes(string hexString)
        {
            if (hexString == null)
            {
                throw new ArgumentNullException("hexString");
            }

            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException("hexString must have an even length", "hexString");
            }

            var bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string currentHex = hexString.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(currentHex, 16);
            }
            return bytes;
        }

        /// <summary>
        /// Token Metadata cache list
        /// </summary>
        public static ConcurrentDictionary<string, GetTokenMetadataResponse> TokenMetadataCache = new ConcurrentDictionary<string, GetTokenMetadataResponse>();
        /// <summary>
        /// Get token issue metadata. Contains image url, issuer, and other info
        /// </summary>
        /// <param name="tokenId">token id hash</param>
        /// <returns></returns>
        public static async Task<GetTokenMetadataResponse> GetTokenMetadata(string tokenId)
        {
            if (string.IsNullOrEmpty(tokenId))
            {
                return new GetTokenMetadataResponse();
            }

            try
            {
                GetTokenMetadataResponse tokeninfo = null;
                if (TokenMetadataCache.TryGetValue(tokenId, out var ti))
                {
                    tokeninfo = ti;
                }
                else
                {
                    tokeninfo = await GetClient().GetTokenMetadataAsync(tokenId, 0);
                    TokenMetadataCache.TryAdd(tokenId, tokeninfo);
                }
                if (tokeninfo != null)
                {
                    return tokeninfo;
                }
                else
                {
                    return new GetTokenMetadataResponse();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load token metadata. " + ex.Message);
                return new GetTokenMetadataResponse();
            }
        }

        /// <summary>
        /// Get token info. Contains image url, issuer, and other info
        /// </summary>
        /// <param name="tokenId">token id hash</param>
        /// <returns></returns>
        public static async Task<TokenSupplyDto> GetTokenInfo(string tokenId)
        {
            if (string.IsNullOrEmpty(tokenId))
            {
                return new TokenSupplyDto();
            }

            TokenSupplyDto t = new TokenSupplyDto();
            try
            {
                var info = await GetTokenMetadata(tokenId);
                t.TokenSymbol = info.MetadataOfIssuance.Data.TokenName;
                var tus = JsonConvert.DeserializeObject<List<tokenUrlCarrier>>(JsonConvert.SerializeObject(info.MetadataOfIssuance.Data.Urls));

                var tu = tus.FirstOrDefault();
                if (tu != null)
                {
                    t.ImageUrl = tu.url;
                }

                t.TokenSymbol = info.TokenName;
                t.TokenId = tokenId;
                return t;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load token metadata. " + ex.Message);
                return new TokenSupplyDto();
            }
        }

        /// <summary>
        /// check actual supply for minting on some address. It is just for VENFT tokens now. 
        /// Function will also load token metadta if it has not loaded yet.
        /// </summary>
        /// <param name="address">address which has utxos</param>
        /// <param name="tokenId">Specify the tokenId</param>
        /// <param name="addressinfo">if you have already loaded address info with utxo list provide it to prevent unnecessary API requests</param>
        /// <returns></returns>
        public static async Task<(double, GetTokenMetadataResponse)> GetActualMintingSupply(string address, string tokenId, GetAddressInfoResponse addressinfo)
        {
            var res = await NeblioTransactionHelpers.GetAddressTokensUtxos(address, addressinfo);
            var utxos = new List<Utxos>();
            foreach (var r in res)
            {
                var toks = r.Tokens.ToArray()?[0];
                if (toks != null && toks.Amount > 1)
                {
                    if (toks.TokenId == tokenId)
                    {
                        utxos.Add(r);
                    }
                }
            }

            var totalAmount = 0.0;
            foreach (var u in utxos)
            {
                totalAmount += (double)u.Tokens.ToArray()?[0]?.Amount;
            }

            if (TokensInfo.TryGetValue(tokenId, out var info))
            {
                return (totalAmount, info);
            }
            else
            {
                return (totalAmount, null);
            }
        }

        private class tokenUrlCarrier
        {
            public string name { get; set; } = string.Empty;
            public string url { get; set; } = string.Empty;
            public string mimeType { get; set; } = string.Empty;
        }
        /// <summary>
        /// Check supply of all tokens on address.
        /// </summary>
        /// <param name="address">address which has utxos</param>
        /// <param name="addressinfo">if you have already loaded address info with utxo list provide it to prevent unnecessary API requests</param>
        /// <returns></returns>
        public static async Task<Dictionary<string, TokenSupplyDto>> CheckTokensSupplies(string address, GetAddressInfoResponse addressinfo)
        {
            var resp = new Dictionary<string, TokenSupplyDto>();

            var res = await GetAddressTokensUtxos(address, addressinfo);
            var utxos = new List<Utxos>();
            foreach (var r in res)
            {
                var toks = r.Tokens.ToArray()?[0];

                if (toks != null && toks.Amount > 1)
                {
                    if (TokensInfo.TryGetValue(toks.TokenId, out var info))
                    {
                        if (!resp.TryGetValue(toks.TokenId, out var tk))
                        {
                            var t = new TokenSupplyDto
                            {
                                TokenSymbol = info.MetadataOfIssuance.Data.TokenName
                            };
                            var tus = JsonConvert.DeserializeObject<List<tokenUrlCarrier>>(JsonConvert.SerializeObject(info.MetadataOfIssuance.Data.Urls));

                            var tu = tus.FirstOrDefault();
                            if (tu != null)
                            {
                                t.ImageUrl = tu.url;
                            }

                            t.TokenId = toks.TokenId;
                            t.Amount += (double)toks.Amount;

                            resp.TryAdd(t.TokenId, t);
                        }
                        else
                        {
                            tk.Amount += (double)toks.Amount;
                        }
                    }
                }
            }

            return resp;
        }

        /// <summary>
        /// Return VENFT top owners. It eliminate some testing addresses.
        /// </summary>
        /// <param name="tokenId">Token Id hash</param>
        /// <returns></returns>
        public static async Task<List<Holders>> GetTokenOwnersList(string tokenId)
        {
            var tokenholders = await GetClient().GetTokenHoldersAsync(tokenId);

            var hd = tokenholders.Holders.ToList().OrderByDescending(h => (double)h.Amount).ToList();
            hd.RemoveRange(0, 3);

            return hd;
        }

        /// <summary>
        /// Return VENFT top owners. It eliminate some testing addresses.
        /// </summary>
        /// <param name="tokenId">Token Id hash</param>
        /// <returns></returns>
        public static async Task<List<TokenOwnerDto>> GetTokenOwners(string tokenId = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8")
        {
            var resp = new List<TokenOwnerDto>();
            var hd = await GetTokenOwnersList(tokenId);

            List<string> addressList = new List<string>() {"NPWBL3i8ZQ8tmhDtrixXwYd93nofmunvhA",
                                                           "NeNE6a2YQCq4yBLoVbVpcCzx44jVEBLaUE",
                                                           "NikErRpjtRXpryFRc3RkP5nxRzm1ApxFH8",
                                                           "NWpT6Wiri9ZAsjVSH8m7eX85Nthqa2J8aY",
                                                           "NWHozNL3B85PcTXhipmFoBMbfonyrS9WiR",
                                                           "NQhy34DCWjG969PSVWV6S8QSe1MEbprWh7",
                                                           "NST3h9Z2CMuHHgea5ewy1berTNMhUdXJya",
                                                           "NidaStEf81XCmWKuJ6G6fvsFSpvh3TgceD",
                                                           "NZREfode8XxDHndeoLGEeQKhsfvjWfHXUU"};

            var i = 0;
            foreach (var h in hd)
            {
                try
                {
                    if (!addressList.Contains(h.Address))
                    {
                        var shadd = h.Address.Substring(0, 3) + "..." + h.Address.Substring(h.Address.Length - 3);
                        var utxs = await GetAddressNFTsUtxos(h.Address, new List<string>() { tokenId });
                        if (utxs != null)
                        {
                            if (utxs.Count > 0)
                            {
                                var us = utxs.ToList();
                                resp.Add(new TokenOwnerDto()
                                {
                                    Address = h.Address,
                                    ShortenAddress = shadd,
                                    AmountOfNFTs = us.Count,
                                    AmountOfTokens = (int)h.Amount
                                });
                            }
                        }
                        i++;
                        if (i > 1000)
                        {
                            break;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Problem with reading address info. Address: " + h.Address + " - " + ex.Message);
                }
            }

            resp = resp.OrderByDescending(r => r.AmountOfNFTs).ToList();
            /*
            if (resp.Count > 50)
                resp.RemoveRange(49, resp.Count - 50 - 1);
            */
            return resp;
        }
    }
}
