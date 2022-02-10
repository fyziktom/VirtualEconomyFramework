using NBitcoin;
using NBitcoin.Altcoins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Builder;
using VEDriversLite.Events;
using VEDriversLite.Neblio;
using VEDriversLite.NeblioAPI;
using VEDriversLite.Security;
using System.IO.Compression;
using System.IO;

namespace VEDriversLite
{
    /// <summary>
    /// Dto for info about actual Token supply on address
    /// </summary>
    public class TokenSupplyDto
    {
        /// <summary>
        /// Symbol of token - up to 5 unique letters
        /// </summary>
        public string TokenSymbol { get; set; } = string.Empty;
        /// <summary>
        /// Token Id hash
        /// </summary>
        public string TokenId { get; set; } = string.Empty;
        /// <summary>
        /// Amount of tokens available
        /// </summary>
        public double Amount { get; set; } = 0.0;
        /// <summary>
        /// Token icon image url
        /// </summary>
        public string ImageUrl { get; set; } = string.Empty;
    }
    /// <summary>
    /// Dto for info about owner of some kind of the tokens
    /// </summary>
    public class TokenOwnerDto
    {
        /// <summary>
        /// Address of the Owner
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Shorten Address of the Owner
        /// </summary>
        public string ShortenAddress { get; set; } = string.Empty;
        /// <summary>
        /// Amount of the tokens on the Owner Address
        /// </summary>
        public int AmountOfTokens { get; set; } = 0;
        /// <summary>
        /// Amount of the NFTs on the Owner Address
        /// </summary>
        public int AmountOfNFTs { get; set; } = 0;
    }

    public static class NeblioTransactionHelpers
    {
        private static HttpClient httpClient = new HttpClient();
        private static IClient _client;
        private static string BaseURL = "https://ntp1node.nebl.io/";
        /// <summary>
        /// Conversion ration for Neblio to convert from sat to 1 NEBL
        /// </summary>
        public static double FromSatToMainRatio = 100000000;
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
        public static Dictionary<string, GetTokenMetadataResponse> TokensInfo = new Dictionary<string, GetTokenMetadataResponse>();
        public static ConcurrentDictionary<string, GetTransactionInfoResponse> TransactionInfoCache = new ConcurrentDictionary<string, GetTransactionInfoResponse>();
        public static ConcurrentDictionary<string, GetTokenMetadataResponse> TokenTxMetadataCache = new ConcurrentDictionary<string, GetTokenMetadataResponse>();
        public static ConcurrentDictionary<string, (DateTime, GetAddressInfoResponse)> AddressInfoCache = new ConcurrentDictionary<string, (DateTime, GetAddressInfoResponse)>();

        public static event EventHandler<IEventInfo> NewEventInfo;

        /// <summary>
        /// Create short version of address, 3 chars on start...3 chars on end
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public static string ShortenAddress(string address)
        {
            if (string.IsNullOrEmpty(address))
                return string.Empty;
            var shortaddress = address.Substring(0, 3) + "..." + address.Substring(address.Length - 3);
            return shortaddress;
        }
        /// <summary>
        /// Create short version of txid hash, 3 chars on start...3 chars on end
        /// </summary>
        /// <param name="txid"></param>
        /// <param name="withDots">default true. add .... between start and end of the tx hash</param>
        /// <returns></returns>
        public static string ShortenTxId(string txid, bool withDots = true, int len = 10)
        {
            if (string.IsNullOrEmpty(txid))
                return string.Empty;
            if (txid.Length < 10)
                return txid;

            if (withDots)
            {
                var txids = txid.Remove(((int)(len / 2)), txid.Length - ((int)(len/2))) + "....." + txid.Remove(0, txid.Length - ((int)(len / 2)));
                return txids;
            }
            else
            {
                var txids = txid.Remove(((int)(len / 2)), txid.Length - ((int)(len / 2))) + txid.Remove(0, txid.Length - ((int)(len / 2)));
                return txids;
            }
        }

        /// <summary>
        /// Load informations about allowed tokens.
        /// </summary>
        /// <param name="tokenIds">List of allowed tokens to works with.</param>
        /// <returns></returns>
        public static async Task LoadAllowedTokensInfo(List<string> tokenIds)
        {
            foreach(var tok in tokenIds)
                if (!TokensInfo.TryGetValue(tok, out var tokinfo))
                {
                    var info = await GetTokenMetadata(tok);
                    if (info != null)
                        TokensInfo.Add(tok, info);
                }
        }

        /// <summary>
        /// Function converts EncryptionKey (optionaly with password if it is not already loaded in ekey)
        /// and returns BitcoinAddress and BitcoinSecret classes from NBitcoin
        /// </summary>
        /// <param name="ekey"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static async Task<(BitcoinAddress, BitcoinSecret)> GetAddressAndKey(EncryptionKey ekey, string password = "")
        {
            var key = string.Empty;

            if (ekey != null)
            {
                if (ekey.IsLoaded)
                {
                    if (ekey.IsEncrypted && string.IsNullOrEmpty(password) && !ekey.IsPassLoaded)
                        throw new Exception("Cannot send token transaction. Password is not filled and key is encrypted or unlock account!");
                    else if (!ekey.IsEncrypted)
                        key = await ekey.GetEncryptedKey();
                    else if (ekey.IsEncrypted && (!string.IsNullOrEmpty(password) || ekey.IsPassLoaded))
                        if (ekey.IsPassLoaded)
                            key = await ekey.GetEncryptedKey(string.Empty);
                        else
                            key = await ekey.GetEncryptedKey(password);

                    if (string.IsNullOrEmpty(key))
                        throw new Exception("Cannot send token transaction. Password is not filled and key is encrypted or unlock account!");

                }
            }

            BitcoinSecret loadedkey = null;
            BitcoinAddress addressForTx = null;
            if (!string.IsNullOrEmpty(key))
            {
                try
                {
                    loadedkey = NeblioTransactionHelpers.Network.CreateBitcoinSecret(key);
                    addressForTx = loadedkey.GetAddress(ScriptPubKeyType.Legacy);

                    return (addressForTx, loadedkey);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot send token transaction!", ex);
                    //Console.WriteLine($"Cannot send token transaction, cannot create keys");
                    throw new Exception("Cannot send token transaction. cannot create keys!");
                }
            }
            else
                throw new Exception("Cannot send token transaction. Password is not filled and key is encrypted or unlock account!");
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
                    return txid;
                else
                    throw new Exception("Cannot broadcast transaction.");
            }
            else
                throw new Exception("Wrong input transaction for broadcast.");
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
                throw new Exception("Amount to send cannot be 0.");
            if (string.IsNullOrEmpty(receiver))
                throw new Exception("Receiver Address not provided.");
            if (string.IsNullOrEmpty(tokenId))
                throw new Exception("Token Id not provided.");
            if (fee < 10000)
                throw new Exception("Fee cannot be smaller than 10000 Sat.");

            var dto = new SendTokenRequest();

            dto.Metadata = new Metadata2();
            dto.Metadata.AdditionalProperties = new Dictionary<string, object>();
            dto.Metadata.UserData = new UserData3();
            dto.Metadata.UserData.AdditionalProperties = new Dictionary<string, object>();
            dto.Metadata.UserData.Meta = new List<JObject>();
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
            var basicFee = 10000;

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
                expectedSize += emptyOpReturn + (int)((double)zipstr.Length);
            }

            // Expected outputs for the rest of the coins/tokens
            expectedSize += outputWithAddress; // NEBL
            if (isTokenTransaction) expectedSize += outputWithAddress;

            double size_m = ((double)expectedSize / 1024);
            if (size_m > 4)
                throw new Exception("Cannot send transaction bigger than 4kB on Neblio network!");

            var fee = basicFee + (int)(size_m) * basicFee;

            if (isTokenTransaction)
                fee += basicFee;
            
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
        /// <param name="fee">Fee - 20000 minimum</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<string> MintNFTTokenAsync(MintNFTData data, EncryptionKey ekey, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos, double fee = 20000)
        {
            var res = "ERROR";

            if (data.Metadata == null || data.Metadata.Count == 0)
                throw new Exception("Cannot send without metadata!");

            // load key and address
            BitcoinSecret key = null;
            BitcoinAddress addressForTx = null;
            try
            {
                var k = await GetAddressAndKey(ekey);
                key = k.Item2;
                addressForTx = k.Item1;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (tutxos == null || tutxos.Count == 0)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");

            var tutxo = tutxos.FirstOrDefault();
            if (tutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            if (nutxos == null || nutxos.Count == 0)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            data.Metadata.Add(new KeyValuePair<string, string>("SourceUtxo", tutxo.Txid));
            data.Metadata.Add(new KeyValuePair<string, string>("NFT FirstTx", "true"));

            fee = CalcFee(2, 1, JsonConvert.SerializeObject(data.Metadata), true);

            // create and init send token request dto for Neblio API
            var dto = new SendTokenRequest();
            try
            {
                if (string.IsNullOrEmpty(data.ReceiverAddress))
                    dto = GetSendTokenObject(1, fee, data.SenderAddress, data.Id);
                else
                    dto = GetSendTokenObject(1, fee, data.ReceiverAddress, data.Id);

                if (data.Metadata != null)
                    foreach (var d in data.Metadata)
                    {
                        var obj = new JObject();
                        obj[d.Key] = d.Value;
                        dto.Metadata.UserData.Meta.Add(obj);
                    }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (dto.Metadata.UserData.Meta.Count == 0)
                throw new Exception("Cannot mint NFT without any metadata");

            //dto.Sendutxo = null;
            //dto.From = new List<string>() { data.SenderAddress }; //null;
            dto.From = null;

            if (tutxo.Txid == nutxo.Txid && tutxo.Index == nutxo.Index)
                throw new Exception("Same input for token and neblio. Wrong input.");
            
            dto.Sendutxo.Add(tutxo.Txid + ":" + ((int)tutxo.Index).ToString());
            dto.Sendutxo.Add(nutxo.Txid + ":" + ((int)nutxo.Index).ToString());

            // create raw tx
            var hexToSign = string.Empty;
            try
            {
                hexToSign = await SendRawNTP1TxAsync(dto);
                if (string.IsNullOrEmpty(hexToSign))
                    throw new Exception("Cannot get correct raw token hex.");
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during sending raw token tx" + ex.Message);
            }

            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(hexToSign, Network, out var transaction))
                throw new Exception("Cannot parse token tx raw hex.");

            try
            {
                return await SignAndBroadcast(transaction, key, addressForTx);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        
        /// <summary>
        /// Function will Mint NFT with the coppies
        /// </summary>
        /// <param name="data">Mint data, please see MintNFTData class for the details</param>
        /// <param name="coppies">0 or more coppies - with 0 input it is same as MintNFTTokenAsync</param>
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="tutxos">Optional input token utxo</param>
        /// <param name="fee">Fee - 20000 minimum</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<string> MintMultiNFTTokenAsync(MintNFTData data, int coppies, EncryptionKey ekey, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos, double fee = 20000)
        {
            var res = "ERROR";

            if (data.Metadata == null || data.Metadata.Count == 0)
                throw new Exception("Cannot send without metadata!");

            // load key and address
            BitcoinSecret key = null;
            BitcoinAddress addressForTx = null;
            BitcoinAddress receiverAddres = null;
            try
            {
                var k = await GetAddressAndKey(ekey);
                key = k.Item2;
                addressForTx = k.Item1;
                if (!string.IsNullOrEmpty(data.ReceiverAddress))
                {
                    receiverAddres = BitcoinAddress.Create(data.ReceiverAddress, Network);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (tutxos == null || tutxos.Count == 0)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");

            var tutxo = tutxos.FirstOrDefault();
            if (tutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            if (nutxos == null || nutxos.Count == 0)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            data.Metadata.Add(new KeyValuePair<string, string>("SourceUtxo", tutxo.Txid));
            data.Metadata.Add(new KeyValuePair<string, string>("NFT FirstTx", "true"));

            fee = CalcFee(2, 1 + coppies, JsonConvert.SerializeObject(data.Metadata), true);

            // create and init send token request dto for Neblio API
            var dto = new SendTokenRequest();
            try
            {
                if (!string.IsNullOrEmpty(data.ReceiverAddress))
                    dto = GetSendTokenObject(1, fee, data.ReceiverAddress, data.Id);
                else
                    dto = GetSendTokenObject(1, fee, data.SenderAddress, data.Id);

                if (data.Metadata != null)
                    foreach (var d in data.Metadata)
                    {
                        var obj = new JObject();
                        obj[d.Key] = d.Value;
                        dto.Metadata.UserData.Meta.Add(obj);
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
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (dto.Metadata.UserData.Meta.Count == 0)
                throw new Exception("Cannot mint NFT without any metadata");

            //dto.Sendutxo = null;
            //dto.From = new List<string>() { data.SenderAddress }; //null;
            dto.From = null;

            //add all token utxos
            foreach (var t in tutxos)
            {
                if (t.Txid != nutxo.Txid)
                    dto.Sendutxo.Add(t.Txid + ":" + ((int)t.Index).ToString());
                else
                {
                    if (t.Index != nutxo.Index)
                        dto.Sendutxo.Add(t.Txid + ":" + ((int)t.Index).ToString());
                }
            }

            dto.Sendutxo.Add(nutxo.Txid + ":" + ((int)nutxo.Index).ToString());

            // create raw tx
            var hexToSign = string.Empty;
            try
            {
                //var dtostr = JsonConvert.SerializeObject(dto);
                hexToSign = await SendRawNTP1TxAsync(dto);
                if (string.IsNullOrEmpty(hexToSign))
                    throw new Exception("Cannot get correct raw token hex.");
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during sending raw token tx" + ex.Message);
            }

            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(hexToSign, Network, out var transaction))
                throw new Exception("Cannot parse token tx raw hex.");

            foreach(var output in transaction.Outputs)
            {
                if (!output.ScriptPubKey.ToString().Contains("RETURN"))
                {
                    if (!string.IsNullOrEmpty(data.ReceiverAddress))
                        output.ScriptPubKey = receiverAddres.ScriptPubKey;
                    else
                        output.ScriptPubKey = addressForTx.ScriptPubKey;
                }
                else
                {
                    break;
                }
            }

            try
            {
                return await SignAndBroadcast(transaction, key, addressForTx);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        
        /// <summary>
        /// Function will Split NTP1 tokens to smaller lots
        /// receiver list - If you input 0, split will be done to sender address, if you input 1 receiver split will be done to receiver (all inputs)
        /// if you will provide multiple receivers, the number of lots and receivers must match.
        /// </summary>
        /// <param name="receiver">List of receivers. </param>
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="tutxos">Optional input token utxo</param>
        /// <param name="fee">Fee - 20000 minimum</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<string> SplitNTP1TokensAsync(List<string> receiver, int lots, int amount, string tokenId, 
                                                              IDictionary<string,string> metadata, 
                                                              EncryptionKey ekey, 
                                                              ICollection<Utxos> nutxos, 
                                                              ICollection<Utxos> tutxos, 
                                                              double fee = 20000)
        {
            var res = "ERROR";

            if (metadata == null || metadata.Count == 0)
                throw new Exception("Cannot send without metadata!");
            if ((receiver.Count > 1 && lots > 1) && (receiver.Count != lots))
                throw new Exception($"If you want to split coins to different receivers, the number of lots and receivers must match. Receivers {receiver.Count}, Lost {lots}.");

            // load key and address
            BitcoinSecret key = null;
            BitcoinAddress addressForTx = null;
            List<BitcoinAddress> receiversAddresses = new List<BitcoinAddress>();
            try
            {
                var k = await GetAddressAndKey(ekey);
                key = k.Item2;
                addressForTx = k.Item1;
                if (receiver.Count > 0)
                {
                    foreach (var r in receiver)
                    {
                        try
                        {
                            receiversAddresses.Add(BitcoinAddress.Create(r, Network));
                        }
                        catch(Exception ex)
                        {
                            Console.WriteLine("Cannot load one of the receivers");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if ((receiversAddresses.Count > 1 && lots > 1) && (receiversAddresses.Count != lots))
                throw new Exception($"If you want to split coins to different receivers, the number of lots and receivers must match. Receivers {receiversAddresses.Count}, Lost {lots}. Some of input address may be wrong.");

            if (tutxos == null || tutxos.Count == 0)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");

            var tutxo = tutxos.FirstOrDefault();
            if (tutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            if (nutxos == null || nutxos.Count == 0)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            metadata.Add("TransactionType", "Token Split");

            fee = CalcFee(2, lots, JsonConvert.SerializeObject(metadata), true);

            // create and init send token request dto for Neblio API
            var dto = new SendTokenRequest();
            try
            {
                if (receiversAddresses.Count == 0)
                {
                    var dummykey = new Key();
                    var dummyadd = dummykey.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network);
                    dto = GetSendTokenObject(amount, fee, dummyadd.ToString(), tokenId);
                }
                else
                    dto = GetSendTokenObject(amount, fee, receiversAddresses[0].ToString(), tokenId);                    

                if (metadata != null)
                    foreach (var d in metadata)
                    {
                        var obj = new JObject();
                        obj[d.Key] = d.Value;
                        dto.Metadata.UserData.Meta.Add(obj);
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
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (dto.Metadata.UserData.Meta.Count == 0)
                throw new Exception("Cannot mint NFT without any metadata");

            //dto.Sendutxo = null;
            //dto.From = new List<string>() { data.SenderAddress }; //null;
            dto.From = null;

            //add all token utxos
            foreach (var t in tutxos)
            {
                if (t.Txid != nutxo.Txid)
                    dto.Sendutxo.Add(t.Txid + ":" + ((int)t.Index).ToString());
                else
                {
                    if (t.Index != nutxo.Index)
                        dto.Sendutxo.Add(t.Txid + ":" + ((int)t.Index).ToString());
                }
            }
            // add neblio utxo
            dto.Sendutxo.Add(nutxo.Txid + ":" + ((int)nutxo.Index).ToString());
            if (dto.Sendutxo.Count < 2)
                throw new Exception("Not enouht inputs sources for the split transaction.");

            // create raw tx
            var hexToSign = string.Empty;
            try
            {
                //var dtostr = JsonConvert.SerializeObject(dto);
                hexToSign = await SendRawNTP1TxAsync(dto);
                if (string.IsNullOrEmpty(hexToSign))
                    throw new Exception("Cannot get correct raw token hex.");
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during sending raw token tx" + ex.Message);
            }

            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(hexToSign, Network, out var transaction))
                throw new Exception("Cannot parse token tx raw hex.");

            var j = 0;
            foreach (var output in transaction.Outputs)
            {
                if (!output.ScriptPubKey.ToString().Contains("RETURN"))
                {
                    if (receiversAddresses.Count == 0 || receiversAddresses.Count == 1)
                    {
                        if (receiversAddresses.Count == 1)
                            output.ScriptPubKey = receiversAddresses[0].ScriptPubKey;
                        else
                            output.ScriptPubKey = addressForTx.ScriptPubKey;
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

            try
            {
                return await SignAndBroadcast(transaction, key, addressForTx);
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
        public static async Task<string> SendNFTTokenAsync(SendTokenTxData data, EncryptionKey ekey, ICollection<Utxos> nutxos, double fee = 20000)
        {
            var res = "ERROR";

            if (data.Metadata == null || data.Metadata.Count == 0)
                throw new Exception("Cannot send without metadata!");

            // load key and address
            BitcoinSecret keyfromFile = null;
            BitcoinAddress addressForTx = null;
            try
            {
                var k = await GetAddressAndKey(ekey);
                keyfromFile = k.Item2;
                addressForTx = k.Item1;
            }
            catch (Exception ex)
            {
                throw ex;
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
            var tutxo = string.Empty;
            var val = await ValidateOneTokenNFTUtxo(data.SenderAddress, data.Id, itt, indx);
            if (val != 0)
                throw new Exception("Cannot send transaction, nft utxo is not spendable!");
            else
                tutxo = nftutxo + ":" + ((int)val).ToString();

            if (nutxos == null || nutxos.Count == 0)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            fee = CalcFee(2, 1, JsonConvert.SerializeObject(data.Metadata), true);

            // create and init send token request dto for Neblio API
            var dto = new SendTokenRequest();
            try
            {
                dto = GetSendTokenObject(1, fee, data.ReceiverAddress, data.Id);

                if (data.Metadata != null)
                    foreach (var d in data.Metadata)
                    {
                        var obj = new JObject();
                        obj[d.Key] = d.Value;
                        dto.Metadata.UserData.Meta.Add(obj);
                    }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (tutxo.Length < 3)
                throw new Exception("Same input for token and neblio. Wrong input.");

            if (tutxo == nutxo.Txid + ":" + ((int)nutxo.Index).ToString())
                throw new Exception("Same input for token and neblio. Wrong input.");

            dto.Sendutxo.Add(tutxo);
            dto.Sendutxo.Add(nutxo.Txid + ":" + ((int)nutxo.Index).ToString());

            // create raw tx
            var hexToSign = string.Empty;
            try
            {
                hexToSign = await SendRawNTP1TxAsync(dto);
                if (string.IsNullOrEmpty(hexToSign))
                    throw new Exception("Cannot get correct raw token hex.");
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during sending raw token tx" + ex.Message);
            }

            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(hexToSign, Network, out var transaction))
                throw new Exception("Cannot parse token tx raw hex.");

            try
            {
                return await SignAndBroadcast(transaction, keyfromFile, addressForTx);
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
        public static async Task<string> SendTokenLotAsync(SendTokenTxData data, EncryptionKey ekey, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos, double fee = 20000)
        {
            var res = "ERROR";

            if (data.Metadata == null || data.Metadata.Count == 0)
                throw new Exception("Cannot send without metadata!");

            // load key and address
            BitcoinSecret key = null;
            BitcoinAddress addressForTx = null;
            try
            {
                var k = await GetAddressAndKey(ekey);
                key = k.Item2;
                addressForTx = k.Item1;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            var tutxo = tutxos.FirstOrDefault();
            if (tutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            fee = CalcFee(2, 1, JsonConvert.SerializeObject(data.Metadata), true);

            // create and init send token request dto for Neblio API
            var dto = new SendTokenRequest();
            try
            {
                dto = GetSendTokenObject(data.Amount, fee, data.ReceiverAddress, data.Id);

                if (data.Metadata != null)
                    foreach (var d in data.Metadata)
                    {
                        var obj = new JObject();
                        obj[d.Key] = d.Value;
                        dto.Metadata.UserData.Meta.Add(obj);
                    }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            //add all token utxos
            foreach (var t in tutxos)
            {
                if (t.Txid != nutxo.Txid)
                    dto.Sendutxo.Add(t.Txid + ":" + ((int)t.Index).ToString());
                else
                {
                    if (t.Index != nutxo.Index)
                        dto.Sendutxo.Add(t.Txid + ":" + ((int)t.Index).ToString());
                }
            }
            // add neblio utxo
            dto.Sendutxo.Add(nutxo.Txid + ":" + ((int)nutxo.Index).ToString());
            if (dto.Sendutxo.Count < 2)
                throw new Exception("Not enouht inputs sources for the split transaction.");

            // create raw tx
            var hexToSign = string.Empty;
            try
            {
                hexToSign = await SendRawNTP1TxAsync(dto);
                if (string.IsNullOrEmpty(hexToSign))
                    throw new Exception("Cannot get correct raw token hex.");
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during sending raw token tx" + ex.Message);
            }

            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(hexToSign, Network, out var transaction))
                throw new Exception("Cannot parse token tx raw hex.");

            try
            {
                return await SignAndBroadcast(transaction, key, addressForTx);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

                
        /// <summary>
        /// Function will send standard Neblio transaction
        /// </summary>
        /// <param name="data">Send data, please see SendTxData class for the details</param>
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="fee">Fee - 10000 minimum</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<string> SendNeblioTransactionAPIAsync(SendTxData data, EncryptionKey ekey, ICollection<Utxos> nutxos, double fee = 10000)
        {
            if (data == null)
                throw new Exception("Data cannot be null!");
            if (ekey == null)
                throw new Exception("Account cannot be null!");

            // create receiver address
            BitcoinAddress recaddr = null;
            try
            {
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            // load key and address
            BitcoinSecret key = null;
            BitcoinAddress addressForTx = null;
            try
            {
                var k = await GetAddressAndKey(ekey, data.Password);
                key = k.Item2;
                addressForTx = k.Item1;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            var tx = GetTransactionWithNeblioInputs(nutxos, addressForTx);
            if (tx.Item1 == null)
                throw new Exception("Cannot create the transaction object.");

            var transaction = tx.Item1;
            var allNeblInputCoins = tx.Item2;
            try
            {

                fee = CalcFee(transaction.Inputs.Count, 1, data.CustomMessage, false);

                var diff = (allNeblInputCoins - data.Amount) - (fee / FromSatToMainRatio);
                
                // create outputs
                transaction.Outputs.Add(new Money(Convert.ToInt64(data.Amount * FromSatToMainRatio)), recaddr.ScriptPubKey); // send to receiver required amount

                if (!string.IsNullOrEmpty(data.CustomMessage))
                {
                    diff -= (10000 / FromSatToMainRatio); // 10000 sat is need as value for minimal output even if it holds the OP_RETURN
                    var bytes = Encoding.UTF8.GetBytes(data.CustomMessage);
                    transaction.Outputs.Add(new TxOut()
                    {
                        Value = 10000,
                        ScriptPubKey = TxNullDataTemplate.Instance.GenerateScriptPubKey(bytes)
                    });
                }
                transaction.Outputs.Add(new Money(Convert.ToInt64(diff * FromSatToMainRatio)), addressForTx.ScriptPubKey); // get diff back to sender address
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during creating outputs. " + ex.Message);
            }

            try
            {
                return await SignAndBroadcast(transaction, key, addressForTx);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Function will send standard Neblio transaction
        /// </summary>
        /// <param name="data">Send data, please see SendTxData class for the details</param>
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="fee">Fee - 10000 minimum</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<string> SplitNeblioCoinTransactionAPIAsync(string sender, List<string> receivers, int lots, double amount, EncryptionKey ekey, ICollection<Utxos> nutxos, double fee = 20000)
        {
            var res = "ERROR";

            if (ekey == null)
                throw new Exception("Account cannot be null!");

            if ((receivers.Count > 1 && lots > 1) && (receivers.Count != lots))
                throw new Exception($"If you want to split coins to different receivers, the number of lots and receivers must match. Receivers {receivers.Count}, Lost {lots}.");

            if (lots < 2 || lots > 25)
                throw new Exception("Count must be bigger than 2 and lower than 25.");

            // create receiver address
            BitcoinSecret key = null;
            BitcoinAddress addressForTx = null;
            List<BitcoinAddress> receiversAddresses = new List<BitcoinAddress>();
            try
            {
                var k = await GetAddressAndKey(ekey);
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
                        catch (Exception ex)
                        {
                            Console.WriteLine("Cannot load one of the receivers");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot send transaction. cannot create sender address!");
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
                fee = CalcFee(transaction.Inputs.Count, 1, "", false);

                var totalAmount = 0.0;
                for (int i = 0; i < lots; i++)
                    totalAmount += amount;

                var amountinSat = Convert.ToUInt64(totalAmount * FromSatToMainRatio);
                if (amountinSat > allNeblInputCoins)
                    throw new Exception("Not enought neblio for splitting.");

                var diffinSat = Convert.ToUInt64(allNeblInputCoins) - amountinSat - Convert.ToUInt64(fee);
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

            try
            {
                return await SignAndBroadcast(transaction, key, addressForTx);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// This function will send Neblio payment together with the token whichc carry some metadata
        /// </summary>
        /// <param name="data">Mint data, please see MintNFTData class for the details</param>
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="neblAmount">Amount of Neblio to send</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="fee">Fee - 20000 minimum</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<string> SendNTP1TokenWithPaymentAPIAsync(SendTokenTxData data, 
                                                                          EncryptionKey ekey, 
                                                                          double neblAmount, 
                                                                          ICollection<Utxos> nutxos, 
                                                                          string paymentUtxoToReturn = null, 
                                                                          int paymentUtxoIndexToReturn = 0, 
                                                                          double fee = 20000)
        {
            if (data.Metadata == null || data.Metadata.Count == 0)
                throw new Exception("Cannot send without metadata!");
            if (neblAmount == 0)
                throw new Exception("Neblio amount cannot be 0 in Token+Nebl transaction.");

            // load key and address
            BitcoinSecret key = null;
            BitcoinAddress addressForTx = null;
            try
            {
                var k = await GetAddressAndKey(ekey);
                key = k.Item2;
                addressForTx = k.Item1;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // create receiver address
            BitcoinAddress recaddr = null;
            try
            {
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            Utxos tutxo = null;
            if (paymentUtxoToReturn == null)
            {
                var tutxos = await FindUtxoForMintNFT(data.SenderAddress, data.Id, 5);
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
                    Index = paymentUtxoIndexToReturn
                };
            }
            if (nutxos == null || nutxos.Count == 0)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            // create and init send token request dto for Neblio API
            var dto = new SendTokenRequest();
            try
            {
                dto = GetSendTokenObject(1, 50000, data.ReceiverAddress, data.Id); //set maximum fee

                if (data.Metadata != null)
                    foreach (var d in data.Metadata)
                    {
                        var obj = new JObject();
                        obj[d.Key] = d.Value;
                        dto.Metadata.UserData.Meta.Add(obj);
                    }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (tutxo.Txid == nutxo.Txid && tutxo.Index == nutxo.Index)
                throw new Exception("Same input for token and neblio. Wrong input.");

            dto.Sendutxo.Add(tutxo.Txid + ":" + ((int)tutxo.Index).ToString());
            dto.Sendutxo.Add(nutxo.Txid + ":" + ((int)nutxo.Index).ToString());

            // create raw tx
            var hexToSign = string.Empty;
            try
            {
                hexToSign = await SendRawNTP1TxAsync(dto);
                if (string.IsNullOrEmpty(hexToSign))
                    throw new Exception("Cannot get correct raw token hex.");

            }
            catch (Exception ex)
            {
                throw ex;
            }

            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(hexToSign, Network, out var transaction))
                throw new Exception("Cannot parse token tx raw hex.");

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
                    transaction.Outputs.RemoveAt(3);
                //remove old calculated output with the diff
                transaction.Outputs.RemoveAt(2);

                fee = CalcFee(transaction.Inputs.Count, 2, JsonConvert.SerializeObject(data.Metadata), true);

                var amountinSat = Convert.ToUInt64(neblAmount * FromSatToMainRatio);
                var balanceinSat = Convert.ToUInt64(allNeblInputCoins * FromSatToMainRatio);

                if ((amountinSat + fee) > balanceinSat)
                    throw new Exception("Not enought spendable Neblio on the address.");

                var diffinSat = balanceinSat - amountinSat - Convert.ToUInt64(fee) - 10000; // fee is already included in previous output, last is token carrier

                // create outputs
                transaction.Outputs.Add(new Money(amountinSat), recaddr.ScriptPubKey); // send to receiver required amount
                if (diffinSat > 0)
                    transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender address
                if (string.IsNullOrEmpty(paymentUtxoToReturn)) // just for minting new payment nft
                    transaction.Outputs.Add(new Money(10000), addressForTx.ScriptPubKey); // add 10000 sat as carier of tokens which goes back
            }
            catch(Exception ex)
            {
                throw new Exception("Exception during adding outputs with payment in neblio." + ex.Message);
            }

            try
            {
                return await SignAndBroadcast(transaction, key, addressForTx);
            }
            catch(Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// This function will send Neblio payment together with the token whichc carry some metadata
        /// </summary>
        /// <param name="data">Mint data, please see MintNFTData class for the details</param>
        /// <param name="ekey">Input EncryptionKey of the account</param>
        /// <param name="neblAmount">Amount of Neblio to send</param>
        /// <param name="nutxos">Optional input neblio utxo</param>
        /// <param name="fee">Fee - 20000 minimum</param>
        /// <returns>New Transaction Hash - TxId</returns>
        public static async Task<string> SendNTP1TokenLotWithPaymentAPIAsync(SendTokenTxData data, EncryptionKey ekey, double neblAmount, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos, double fee = 20000)
        {
            var res = "ERROR";

            if (data.Metadata == null || data.Metadata.Count == 0)
                throw new Exception("Cannot send without metadata!");

            if (neblAmount == 0)
                throw new Exception("Neblio amount cannot be 0 in Token+Nebl transaction.");

            // load key and address
            BitcoinSecret key = null;
            BitcoinAddress addressForTx = null;
            try
            {
                var k = await GetAddressAndKey(ekey);
                key = k.Item2;
                addressForTx = k.Item1;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // create receiver address
            BitcoinAddress recaddr = null;
            try
            {
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            if (tutxos == null || tutxos.Count == 0)
                throw new Exception("Cannot send transaction, cannot load sender token utxos!");
            var tutxo = tutxos.FirstOrDefault();
            if (tutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            if (nutxos == null || nutxos.Count == 0)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");
            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            // create and init send token request dto for Neblio API
            var dto = new SendTokenRequest();
            try
            {
                dto = GetSendTokenObject(data.Amount, fee, data.ReceiverAddress, data.Id);

                if (data.Metadata != null)
                    foreach (var d in data.Metadata)
                    {
                        var obj = new JObject();
                        obj[d.Key] = d.Value;
                        dto.Metadata.UserData.Meta.Add(obj);
                    }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            if (tutxo.Txid == nutxo.Txid && tutxo.Index == nutxo.Index)
                throw new Exception("Same input for token and neblio. Wrong input.");

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
                dto.Sendutxo.Add(u.Txid + ":" + ((int)u.Index).ToString());

            // create raw tx
            var hexToSign = string.Empty;
            try
            {
                hexToSign = await SendRawNTP1TxAsync(dto);
                if (string.IsNullOrEmpty(hexToSign))
                    throw new Exception("Cannot get correct raw token hex.");

            }
            catch (Exception ex)
            {
                throw ex;
            }

            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(hexToSign, Network, out var transaction))
                throw new Exception("Cannot parse token tx raw hex.");


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
                    transaction.Outputs.RemoveAt(3);
                else
                    outputForTokensBack = false;

                //remove old calculated output with the diff
                if (transaction.Outputs.Count > 2)
                    transaction.Outputs.RemoveAt(2);

                var amountinSat = Convert.ToUInt64(neblAmount * FromSatToMainRatio);
                var balanceinSat = Convert.ToUInt64(allNeblInputCoins * FromSatToMainRatio);

                fee = CalcFee(transaction.Inputs.Count, 2, JsonConvert.SerializeObject(data.Metadata), true);

                if ((amountinSat + fee) > balanceinSat)
                    throw new Exception("Not enought spendable Neblio on the address.");

                var diffinSat = balanceinSat - amountinSat - Convert.ToUInt64(fee) - 10000; // fee is already included in previous output, last is token carrier

                // create outputs
                transaction.Outputs.Add(new Money(amountinSat), recaddr.ScriptPubKey); // send to receiver required amount
                if (diffinSat > 0)
                    transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender address
                if (outputForTokensBack)
                    transaction.Outputs.Add(new Money(10000), addressForTx.ScriptPubKey); // add 10000 sat as carier of tokens which goes back
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during adding outputs with payment in neblio." + ex.Message);
            }

            try
            {
                return await SignAndBroadcast(transaction, key, addressForTx);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        /// <summary>
        /// Function will sign transaction with provided key and broadcast with Neblio API
        /// </summary>
        /// <param name="transaction">NBitcoin Transaction object</param>
        /// <param name="key">NBitcoin Key - must contain Private Key</param>
        /// <param name="address">NBitcoin address - must match with the provided key</param>
        /// <returns>New Transaction Hash - TxId</returns>
        private static async Task<string> SignAndBroadcast(Transaction transaction, BitcoinSecret key, BitcoinAddress address)
        {
            // add coins
            List<ICoin> coins = new List<ICoin>();
            try
            {
                var addrutxos = await GetAddressUtxosObjects(address.ToString());

                // add all spendable coins of this address
                foreach (var inp in addrutxos)
                {
                    if (transaction.Inputs.FirstOrDefault(i => (i.PrevOut.Hash == uint256.Parse(inp.Txid)) && i.PrevOut.N == (uint)inp.Index) != null)
                        coins.Add(new Coin(uint256.Parse(inp.Txid), (uint)inp.Index, new Money((int)inp.Value), address.ScriptPubKey));
                }

                // add signature to inputs before signing
                foreach (var inp in transaction.Inputs)
                    inp.ScriptSig = address.ScriptPubKey;
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
                    return string.Empty;

                if (tx == sx)
                    throw new Exception("Transaction was not signed. Probably not spendable source.");
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

        public static async Task<string> SendMultiTokenAPIAsync(SendTokenTxData data, EncryptionKey ekey, ICollection<Utxos> nutxos, double fee = 20000, bool isMintingOfCopy = false)
        {
            var res = "ERROR";

            // load key and address
            BitcoinSecret keyfromFile = null;
            BitcoinAddress addressForTx = null;
            try
            {
                var k = await GetAddressAndKey(ekey);
                keyfromFile = k.Item2;
                addressForTx = k.Item1;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // create receiver address
            BitcoinAddress recaddr = null;
            try
            {
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            // create and init send token request dto for Neblio API
            var dto = new SendTokenRequest();
            try
            {
                dto = GetSendTokenObject(data.Amount, fee, data.ReceiverAddress, data.Id);

                dto.To.Add(
                        new To()
                        {
                            Address = data.SenderAddress,//"NPWBL3i8ZQ8tmhDtrixXwYd93nofmunvhA",
                            Amount = 1,
                            TokenId = data.Id
                        });

                if (data.Metadata != null)
                    foreach (var d in data.Metadata)
                    {
                        var obj = new JObject();
                        obj[d.Key] = d.Value;

                        dto.Metadata.UserData.Meta.Add(obj);
                    }
            }
            catch (Exception ex)
            {
                throw ex;
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

                    double voutstate = 0;

                    try
                    {
                        if (first && isMintingOfCopy)
                        {
                            dto.Sendutxo.Add(it);
                            first = false;
                            // skip
                        }
                        else
                            voutstate = await ValidateOneTokenNFTUtxo(data.SenderAddress, data.Id, itt, indx);
                    }
                    catch(Exception ex)
                    {
                        throw new Exception("Cannot validate utxo for multitoken payment.");
                    }
                    
                    if ((!isMintingOfCopy && voutstate != 0) || (!first && isMintingOfCopy && voutstate != 0))
                        dto.Sendutxo.Add(itt + ":" + ((int)voutstate).ToString()); // copy received utxos and add item number of vout after validation

                }
            }
            else
            {
                throw new Exception("This kind of transaction requires Token input utxo list.");
            }

            if (dto.Sendutxo.Count < 2)
                throw new Exception("This kind of transaction requires Token input utxo list with at least 2 one token utox");

            // neblio utxo
            if (nutxos == null || nutxos.Count == 0)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");
            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");
            dto.Sendutxo.Add(nutxo.Txid + ":" + ((int)nutxo.Index).ToString());

            fee = CalcFee(dto.Sendutxo.Count, 1, JsonConvert.SerializeObject(data.Metadata), true);
            dto.Fee = fee;

            // create raw tx
            var hexToSign = string.Empty;
            try
            {
                hexToSign = await SendRawNTP1TxAsync(dto);
                if (string.IsNullOrEmpty(hexToSign))
                    throw new Exception("Cannot get correct raw token hex.");

            }
            catch (Exception ex)
            {
                throw ex;
            }
            
            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(hexToSign, Network, out var transaction))
                throw new Exception("Cannot parse token tx raw hex.");

            if (isMintingOfCopy)
            {
                transaction.Inputs.Add(new TxIn()
                {
                    PrevOut = new OutPoint(uint256.Parse(data.sendUtxo.Last()), 0),
                });
            }

            try
            {
                return await SignAndBroadcast(transaction, keyfromFile, addressForTx);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //////////////////////////////////////

        public static async Task<string> DestroyNFTAsync(SendTokenTxData data, EncryptionKey ekey, ICollection<Utxos> nutxos, double fee = 20000)
        {
            var res = "ERROR";

            // load key and address
            BitcoinSecret keyfromFile = null;
            BitcoinAddress addressForTx = null;
            try
            {
                var k = await GetAddressAndKey(ekey);
                keyfromFile = k.Item2;
                addressForTx = k.Item1;
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // create receiver address
            BitcoinAddress recaddr = null;
            try
            {
                recaddr = BitcoinAddress.Create(data.ReceiverAddress, Network);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot send transaction. cannot create receiver address!");
            }

            fee = CalcFee(data.sendUtxo.Count, 1, JsonConvert.SerializeObject(data.Metadata), true);

            // create and init send token request dto for Neblio API
            var dto = new SendTokenRequest();
            try
            {
                // use just temporary address, will be changed to main address later after go through neblio API create tx command
                dto = GetSendTokenObject(data.Amount, fee, "NPWBL3i8ZQ8tmhDtrixXwYd93nofmunvhA", data.Id);

                if (data.Metadata != null)
                    foreach (var d in data.Metadata)
                    {
                        var obj = new JObject();
                        obj[d.Key] = d.Value;

                        dto.Metadata.UserData.Meta.Add(obj);
                    }
            }
            catch (Exception ex)
            {
                throw ex;
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

                    double voutstate;

                    try
                    {
                        voutstate = await ValidateOneTokenNFTUtxo(data.SenderAddress, data.Id, itt, indx);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Cannot validate utxo for multitoken payment.");
                    }

                    if (voutstate != 0)
                        dto.Sendutxo.Add(itt + ":" + ((int)voutstate).ToString()); // copy received utxos and add item number of vout after validation
                }
            }
            else
            {
                throw new Exception("This kind of transaction requires Token input utxo list.");
            }

            // if not utxo provided, check the un NFT tokens sources. These with more than 1 token
            var utxs = await FindUtxoForMintNFT(data.SenderAddress, data.Id, 5);
            var ut = utxs?.FirstOrDefault();
            if (ut != null)
                dto.Sendutxo.Add(ut.Txid + ":" + ((int)ut.Index).ToString());

            
            dto.To.FirstOrDefault().Amount += (double)ut?.Tokens?.ToList().FirstOrDefault()?.Amount; // add minting Utxo amount
            
            if (dto.Sendutxo.Count < 2)
                throw new Exception("This kind of transaction requires Token input utxo list with at least 2 token utox (NFT + Minting).");

            // neblio utxo
            if (nutxos == null || nutxos.Count == 0)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");
            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");
            dto.Sendutxo.Add(nutxo.Txid + ":" + ((int)nutxo.Index).ToString());

            // create raw tx
            var hexToSign = string.Empty;
            try
            {
                hexToSign = await SendRawNTP1TxAsync(dto);
                if (string.IsNullOrEmpty(hexToSign))
                    throw new Exception("Cannot get correct raw token hex.");

            }
            catch (Exception ex)
            {
                throw ex;
            }

            // parse raw hex to NBitcoin transaction object
            if (!Transaction.TryParse(hexToSign, Network, out var transaction))
                throw new Exception("Cannot parse token tx raw hex.");

            transaction.Outputs[0].ScriptPubKey = addressForTx.ScriptPubKey;

            try
            {
                return await SignAndBroadcast(transaction, keyfromFile, addressForTx);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


        ///////////////////////////////////////////
        // Tools for addresses

        public static BitcoinSecret IsPrivateKeyValid(string privatekey)
        {
            try
            {
                if (string.IsNullOrEmpty(privatekey) || privatekey.Length < 52 || privatekey[0] != 'T')
                    return null;
                
                var sec = new BitcoinSecret(privatekey, Network);

                if (sec != null)
                    return sec;
            }
            catch (Exception)
            {
                return null;
            }
            return null;
        }
        public static string GetAddressFromPrivateKey(string privatekey)
        {
            try
            {
                var p = IsPrivateKeyValid(privatekey);
                if (p != null)
                {
                    var address = p.PubKey.GetAddress(ScriptPubKeyType.Legacy, Network);
                    if (address != null)
                        return address.ToString();
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return string.Empty;
        }

        public static string ValidateNeblioAddress(string neblioAddress)
        {
            try
            {
                if (string.IsNullOrEmpty(neblioAddress) || neblioAddress.Length < 34 || neblioAddress[0] != 'N')
                    return string.Empty;

                BitcoinAddress address = BitcoinAddress.Create(neblioAddress, Network);
                if (!string.IsNullOrEmpty(address.ToString()))
                    return address.ToString();
            }
            catch (Exception)
            {
                return string.Empty;
            }
            return string.Empty;
        }

        public static string IsEnoughConfirmationsForSend(int confirmations)
        {
            if (confirmations > MinimumConfirmations)
                return ">" + MinimumConfirmations.ToString();
           
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
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
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
            catch(Exception ex)
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
                return new GetAddressResponse();
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
                return new GetAddressInfoResponse();
            GetAddressInfoResponse ainfo = null;
            if (AddressInfoCache.TryGetValue(addr, out var info))
            {
                if ((DateTime.UtcNow - info.Item1) < new TimeSpan(0, 0, 1))
                {
                    ainfo = info.Item2;
                    return ainfo;
                }
                else
                {
                    var address = await GetClient().GetAddressInfoAsync(addr);
                    if (address != null)
                    {
                        if (AddressInfoCache.TryRemove(addr, out info))
                            AddressInfoCache.TryAdd(addr, (DateTime.UtcNow, address));
                        return address;
                    }
                }
            }
            else
            {
                var address = await GetClient().GetAddressInfoAsync(addr);
                if (address != null)
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
                return new List<Utxos>();
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
                return new List<Utxos>();

            if (addressinfo == null)
                addressinfo = await AddressInfoUtxosAsync(addr);

            var utxos = new List<Utxos>();
            if (addressinfo?.Utxos != null)
                foreach (var u in addressinfo.Utxos)
                    if (u != null && u.Tokens.Count > 0)
                        utxos.Add(u);

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
                return string.Empty;
            var tx = await GetTransactionInfo(txid);
            if (tx != null)
                return tx.Hex;
            else
                return string.Empty;
        }

        private static void AddToTransactionInfoCache(GetTransactionInfoResponse txinfo)
        {
            if (txinfo == null) return;
            if (txinfo.Confirmations > MinimumConfirmations + 2)
                TransactionInfoCache.TryAdd(txinfo.Txid, txinfo);                            
        }

        /// <summary>
        /// Returns list of all Utxos which contains just one token, means amount = 1
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="addressinfo"></param>
        /// <returns></returns>
        public static async Task<ICollection<Utxos>> GetAddressNFTsUtxos(string addr, List<string> allowedTokens, GetAddressInfoResponse addressinfo = null)
        {
            if (string.IsNullOrEmpty(addr))
                return new List<Utxos>();
            if (addressinfo == null)
                addressinfo = await AddressInfoUtxosAsync(addr);

            var utxos = new List<Utxos>();
            try
            {
                if (addressinfo?.Utxos != null)
                    foreach (var u in addressinfo.Utxos)
                        if (u != null && u.Tokens != null)
                            foreach (var tok in u.Tokens)
                                if (allowedTokens.Contains(tok.TokenId) && tok.Amount == 1)
                                    utxos.Add(u);
            }
            catch (Exception ex)
            {
            }
            if (utxos != null && utxos.Count > 0)
            {
                var ouxox = utxos.OrderBy(u => u.Blocktime).Reverse().ToList();
                return ouxox;
            }
            else
            {
                return new List<Utxos>();
            }
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
                throw new Exception("Cannot get amount of transaction. cannot create receiver address!");
            }

            var vinamount = 0.0;
            foreach (var vin in tx.Vin)
                if (vin.Addr == address)
                    vinamount += ((double)vin.Value / FromSatToMainRatio);

            var amount = 0.0;
            foreach(var vout in tx.Vout)
                if (vout.ScriptPubKey.Hex == addr.ScriptPubKey.ToHex())
                    amount += ((double)vout.Value / FromSatToMainRatio);

            amount -= vinamount;

            return amount;
        }     

        /// <summary>
        /// Returns list of spendable utxos which together match some input required amount for some transaction
        /// </summary>
        /// <param name="addr">address which has utxos for spend - sender in tx</param>
        /// <param name="minAmount">minimum amount of one utxo</param>
        /// <param name="requiredAmount">amount what must be collected even by multiple utxos</param>
        /// <returns></returns>
        public static async Task<ICollection<Utxos>> GetAddressNeblUtxo(string addr, double minAmount = 0.0001, double requiredAmount = 0.0001)
        {
            GetAddressInfoResponse addinfo = null;
            var resp = new List<Utxos>();

            addinfo = await AddressInfoUtxosAsync(addr);
            var utxos = addinfo.Utxos;
            if (utxos == null)
                return resp;
            utxos = utxos.OrderByDescending(u => u.Value).ToList();

            var founded = 0.0;
            double latestBlockHeight = await NeblioTransactionsCache.LatestBlockHeight(utxos.FirstOrDefault()?.Txid);
            foreach (var ut in utxos.Where(u => u.Blockheight > 0 && u.Value > 10000 && u.Tokens?.Count == 0 && ((double)u.Value) > (minAmount * FromSatToMainRatio)))
            {
                double UtxoBlockHeight = ut.Blockheight != null ? ut.Blockheight.Value : 0;
                if (IsValidUtxo(UtxoBlockHeight, latestBlockHeight))
                {
                    resp.Add(ut);
                    founded += ((double)ut.Value / FromSatToMainRatio);
                    if (founded > requiredAmount)
                        return resp;
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
        /// <param name="tokenId">input token id hash</param>
        /// <param name="txid">input txid hash</param>
        /// <returns>true and index of utxo</returns>
        public static async Task<double> ValidateOneTokenNFTUtxo(string address, string tokenId, string txid, int indx)
        {
            var addinfo = await AddressInfoUtxosAsync(address);
            var utxos = addinfo.Utxos;
            if (utxos == null)
                return 0;
            
            var uts = utxos.Where(u => (u.Txid == txid && u.Index == indx)); // you can have multiple utxos with same txid but different amount of tokens
            if (uts == null)
                return 0;

            double latestBlockHeight = await NeblioTransactionsCache.LatestBlockHeight(utxos.FirstOrDefault()?.Txid);
            foreach (var ut in uts.Where(u => u.Blockheight > 0 && u.Tokens != null && u.Tokens.Count > 0))
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
            
            return 0;
        }        

        /// <summary>
        /// Find utxo which can be used for minting. It means it has token amount > 1
        /// </summary>
        /// <param name="addr">address which has utxos</param>
        /// <param name="tokenId">token id hash</param>
        /// <param name="numberToMint">number of tokens which will be minted - because of multimint</param>
        /// <param name="oneTokenSat">this is usually default. On Neblio all token tx should have value 10000sat</param>
        /// <returns></returns>
        public static async Task<List<Utxos>> FindUtxoForMintNFT(string addr, string tokenId, int numberToMint = 1, double oneTokenSat = 10000)
        {
            var addinfo = await AddressInfoUtxosAsync(addr);
            var utxos = addinfo.Utxos;
            var resp = new List<Utxos>();
            var founded = 0.0;

            if (utxos == null)
                return resp;

            utxos = utxos.OrderByDescending(u => u.Value).ToList();
            double latestBlockHeight = await NeblioTransactionsCache.LatestBlockHeight(utxos.FirstOrDefault()?.Txid);

            foreach (var ut in utxos.Where(u => u.Blockheight > 0 && u.Tokens.Count > 0))
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
                            break;
                    }
                }
            }
                
            resp = resp.OrderBy(t => t.Tokens.ToArray()[0].Amount).Reverse().ToList();
            founded = 0.0;
            var res = new List<Utxos>();
            foreach(var r in resp)
            {
                founded += (double)r.Tokens.ToList()[0].Amount;
                res.Add(r);
                if (founded > numberToMint)
                    break;
            }

            if (res.Count > 10)// neblio API cannot handle more than 10 inputs
                return new List<Utxos>();

            return res;
        }
        
        public static async Task<GetTokenMetadataResponse> GetTokenMetadataOfUtxoCache(string tokenid, string txid, double verbosity = 0)
        {
            if (TokenTxMetadataCache.TryGetValue(txid, out var tinfo))
                return tinfo;
            else
            {
                var info = await GetClient().GetTokenMetadataOfUtxoAsync(tokenid, txid, 0);
                if (info != null)
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
                return new Dictionary<string, string>();

            var resp = new Dictionary<string, string>();
            var info = await GetTokenMetadataOfUtxoCache(tokenid, txid, 0);
            
            if (info.MetadataOfUtxo != null && info.MetadataOfUtxo.UserData.Meta.Count > 0)
                foreach (var o in info.MetadataOfUtxo.UserData.Meta)
                {
                    var od = JsonConvert.DeserializeObject<IDictionary<string, string>>(o.ToString());
                    if (od != null && od.Count > 0)
                    {
                        var of = od.First();
                        if (!resp.ContainsKey(of.Key))
                            resp.Add(of.Key, of.Value);
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
                return null;
            try
            {
                var addinfo = await GetClient().GetAddressAsync(address);
                if (addinfo == null || addinfo.Transactions.Count == 0)
                    return null;

                foreach (var t in addinfo.Transactions)
                {
                    var th = await GetTxHex(t);
                    var ti = Transaction.Parse(th, Network);
                    if (ti != null)
                        if (ti.Inputs[0].ScriptSig.GetSignerAddress(Network).ToString() == address)
                            return ti;
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
                return new GetTransactionInfoResponse();
            try
            {
                GetTransactionInfoResponse tx = null;
                if (TransactionInfoCache.TryGetValue(txid, out var txinfo))
                    tx = txinfo;
                else
                {
                    tx = await GetClient().GetTransactionInfoAsync(txid);
                    AddToTransactionInfoCache(tx);
                }
                if (tx != null)
                    return tx;
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Cannot load tx {txid} info. " + ex.Message);
            }
            return new GetTransactionInfoResponse();
        }

        /// <summary>
        /// Get transaction sender.
        /// </summary>
        /// <param name="txid">tx id hash</param>
        /// <returns>Sender address</returns>
        public static async Task<string> GetTransactionSender(string txid, GetTransactionInfoResponse txinfo = null)
        {
            if (string.IsNullOrEmpty(txid))
                return string.Empty;
            try
            {
                if (txinfo == null)
                    txinfo = await GetTransactionInfo(txid);
                if (txinfo == null)
                    return string.Empty;
                var send = txinfo.Vin.ToList()[0]?.PreviousOutput?.Addresses.ToList()[0];
                return send;
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
        /// <returns>Sender address</returns>
        public static async Task<string> GetTransactionReceiver(string txid, GetTransactionInfoResponse txinfo = null)
        {
            if (string.IsNullOrEmpty(txid))
                return string.Empty;
            try
            {
                if (txinfo == null)
                {
                    var tx = await GetTransactionInfo(txid);
                    if (tx != null) 
                        txinfo = tx;
                }

                if (txinfo != null)
                {
                    var rec = txinfo.Vout.ToList()[0]?.ScriptPubKey.Addresses.ToList()[0];
                    if (!string.IsNullOrEmpty(rec))
                        return rec;
                }
                return string.Empty;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load tx info. " + ex.Message);
                return string.Empty;
            }
        }

        public static async Task<(bool, string)> ParseNeblioMessage(GetTransactionInfoResponse txinfo)
        {
            if (txinfo == null)
                return (false, "No input data provided.");
            if (txinfo.Vout == null || txinfo.Vout.Count == 0)
                return (false, "No outputs in transaction.");

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
        public static byte[] HexStringToBytes(string hexString)
        {
            if (hexString == null)
                throw new ArgumentNullException("hexString");
            if (hexString.Length % 2 != 0)
                throw new ArgumentException("hexString must have an even length", "hexString");
            var bytes = new byte[hexString.Length / 2];
            for (int i = 0; i < bytes.Length; i++)
            {
                string currentHex = hexString.Substring(i * 2, 2);
                bytes[i] = Convert.ToByte(currentHex, 16);
            }
            return bytes;
        }


        public static ConcurrentDictionary<string, GetTokenMetadataResponse> TokenMetadataCache = new ConcurrentDictionary<string, GetTokenMetadataResponse>();
        /// <summary>
        /// Get token issue metadata. Contains image url, issuer, and other info
        /// </summary>
        /// <param name="tokenId">token id hash</param>
        /// <returns></returns>
        public static async Task<GetTokenMetadataResponse> GetTokenMetadata(string tokenId)
        {
            if (string.IsNullOrEmpty(tokenId))
                return new GetTokenMetadataResponse();
            try
            {
                GetTokenMetadataResponse tokeninfo = null;
                if (TokenMetadataCache.TryGetValue(tokenId, out var ti))
                    tokeninfo = ti;
                else
                {
                    tokeninfo = await GetClient().GetTokenMetadataAsync(tokenId, 0);
                    TokenMetadataCache.TryAdd(tokenId, tokeninfo);
                }
                if (tokeninfo != null)
                    return tokeninfo;
                else
                    return new GetTokenMetadataResponse();
            }
            catch(Exception ex)
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
                return new TokenSupplyDto();
            var t = new TokenSupplyDto();
            try
            {
                var info = await GetTokenMetadata(tokenId);
                t.TokenSymbol = info.MetadataOfIssuance.Data.TokenName;
                var tus = JsonConvert.DeserializeObject<List<tokenUrlCarrier>>(JsonConvert.SerializeObject(info.MetadataOfIssuance.Data.Urls));

                var tu = tus.FirstOrDefault();
                if (tu != null)
                    t.ImageUrl = tu.url;
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
        /// <param name="addressinfo">if you have already loaded address info with utxo list provide it to prevent unnecessary API requests</param>
        /// <returns></returns>
        public static async Task<(double, GetTokenMetadataResponse)> GetActualMintingSupply(string address, string tokenId, GetAddressInfoResponse addressinfo = null)
        {
            var res = await NeblioTransactionHelpers.GetAddressTokensUtxos(address, addressinfo);
            var utxos = new List<Utxos>();
            foreach (var r in res)
            {
                var toks = r.Tokens.ToArray()?[0];
                if (toks != null && toks.Amount > 1)
                    if (toks.TokenId == tokenId)
                        utxos.Add(r);
            }

            var totalAmount = 0.0;
            foreach (var u in utxos)
                totalAmount += (double)u.Tokens.ToArray()?[0]?.Amount;

            if (TokensInfo.TryGetValue(tokenId, out var info))
                return (totalAmount, info);
            else
                return (totalAmount, null);
        }

        private class tokenUrlCarrier
        {
            public string name { get; set; } = string.Empty;
            public string url { get; set; } = string.Empty;
            public string mimeType { get; set; } = string.Empty;
        }
        /// <summary>
        /// Check supply of all VENFT tokens on address.
        /// </summary>
        /// <param name="address">address which has utxos</param>
        /// <param name="addressinfo">if you have already loaded address info with utxo list provide it to prevent unnecessary API requests</param>
        /// <returns></returns>
        public static async Task<Dictionary<string,TokenSupplyDto>> CheckTokensSupplies(string address, GetAddressInfoResponse addressinfo = null)
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
                            var t = new TokenSupplyDto();
                            t.TokenSymbol = info.MetadataOfIssuance.Data.TokenName;
                            var tus = JsonConvert.DeserializeObject<List<tokenUrlCarrier>>(JsonConvert.SerializeObject(info.MetadataOfIssuance.Data.Urls));

                            var tu = tus.FirstOrDefault();
                            if (tu != null)
                                t.ImageUrl = tu.url;

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
        public static async Task<List<Holders>> GetTokenOwnersList(string tokenId = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8")
        {
            var tokenholders = await GetClient().GetTokenHoldersAsync(tokenId);

            var hd = tokenholders.Holders.ToList().OrderBy(h => (double)h.Amount).Reverse().ToList();
            hd.RemoveAt(0);
            hd.RemoveAt(0);
            hd.RemoveAt(0);

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

            var i = 0;
            foreach (var h in hd)
            {
                try
                {
                    if (h.Address != "NPWBL3i8ZQ8tmhDtrixXwYd93nofmunvhA" &&
                        h.Address != "NeNE6a2YQCq4yBLoVbVpcCzx44jVEBLaUE" &&
                        h.Address != "NikErRpjtRXpryFRc3RkP5nxRzm1ApxFH8" &&
                        h.Address != "NWHozNL3B85PcTXhipmFoBMbfonyrS9WiR" &&
                        h.Address != "NQhy34DCWjG969PSVWV6S8QSe1MEbprWh7" &&
                        h.Address != "NST3h9Z2CMuHHgea5ewy1berTNMhUdXJya" &&
                        h.Address != "NidaStEf81XCmWKuJ6G6fvsFSpvh3TgceD" &&
                        h.Address != "NZREfode8XxDHndeoLGEeQKhsfvjWfHXUU")
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
                            break;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Problem with reading address info. Address: " + h.Address + " - " + ex.Message);
                }
            }

            resp = resp.OrderBy(r => r.AmountOfNFTs).Reverse().ToList();
            /*
            if (resp.Count > 50)
                resp.RemoveRange(49, resp.Count - 50 - 1);
            */
            return resp;
        }
    }
}
