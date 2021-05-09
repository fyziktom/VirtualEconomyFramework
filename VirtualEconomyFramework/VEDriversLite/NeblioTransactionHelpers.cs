using NBitcoin;
using NBitcoin.Altcoins;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Builder;
using VEDriversLite.Events;
using VEDriversLite.NeblioAPI;
using VEDriversLite.Security;

namespace VEDriversLite
{
    public class TokenSupplyDto
    {
        public string TokenSymbol { get; set; } = string.Empty;
        public string TokenId { get; set; } = string.Empty;
        public double Amount { get; set; } = 0.0;
        public string ImageUrl { get; set; } = string.Empty;
    }
    public class TokenOwnerDto
    {
        public string Address { get; set; } = string.Empty;
        public string ShortenAddress { get; set; } = string.Empty;
        public int AmountOfTokens { get; set; } = 0;
        public int AmountOfNFTs { get; set; } = 0;
    }

    public class NeblioAPIScripPubKeyDto
    {
        public string asm { get; set; } = string.Empty;
        public string hex { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public int reqSigs { get; set; } = 0;
        public ICollection<string> addresses { get; set; }
    }

    public static class NeblioTransactionHelpers
    {
        private static HttpClient httpClient = new HttpClient();
        private static IClient _client;
        private static string BaseURL = "https://ntp1node.nebl.io/";
        public static double FromSatToMainRatio = 100000000;
        public static Network Network = NBitcoin.Altcoins.Neblio.Instance.Mainnet;
        public static string VENFTId = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8";
        public static int MinimumConfirmations = 4;

        public static event EventHandler<IEventInfo> NewEventInfo;
        private class SignResultDto
        {
            public string hex { get; set; }
            public bool complete { get; set; }
        }

        
        public static string ShortenAddress(string address)
        {
            var shortaddress = address.Substring(0, 3) + "..." + address.Substring(address.Length - 3);
            return shortaddress;
        }
        public static string ShortenTxId(string txid)
        {
            var txids = txid.Remove(5, txid.Length - 5) + "....." + txid.Remove(0, txid.Length - 5);
            return txids;
        }

        public static async Task<(BitcoinAddress, BitcoinSecret)> GetAddressAndKey(EncryptionKey ekey, string password = "")
        {
            var key = string.Empty;

            if (ekey != null)
            {
                if (ekey.IsLoaded)
                {
                    if (ekey.IsEncrypted && string.IsNullOrEmpty(password) && !ekey.IsPassLoaded)
                    {
                        throw new Exception("Cannot send token transaction. Password is not filled and key is encrypted or unlock account!");
                    }
                    else if (!ekey.IsEncrypted)
                    {
                        key = await ekey.GetEncryptedKey();
                    }
                    else if (ekey.IsEncrypted && (!string.IsNullOrEmpty(password) || ekey.IsPassLoaded))
                    {
                        if (ekey.IsPassLoaded)
                            key = await ekey.GetEncryptedKey(string.Empty);
                        else
                            key = await ekey.GetEncryptedKey(password);
                    }

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
            {
                throw new Exception("Cannot send token transaction. Password is not filled and key is encrypted or unlock account!");
            }
        }

        public static async Task<string> BroadcastSignedTransaction(string txhex)
        {
            try
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
                {
                    throw new Exception("Wrong input transaction for broadcast.");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot broadcast transaction. " + ex.Message);
            }
        }

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

        private static async Task<ICollection<string>> GetUtxoListForTx(string address, 
                                                                        string tokenId, 
                                                                        double amount, 
                                                                        ICollection<string> tokenUtxos, 
                                                                        string inputNeblUtxo, 
                                                                        double fee = 10000, 
                                                                        bool isItMintNFT = false, 
                                                                        bool isNFTtx = false, 
                                                                        bool sendEvenNeblUtxoNotFound = false)
        {
            var Sendutxo = new List<string>();

            // neblio API accept sendUtxo (both token and nebl) or from and it will find some unspecified spendable
            // if you correct format of sendUtxo is hash:index for example for token tx you must send this
            // 
            // "4213dfd34dca0b691a5c0e41080c098681e6f1be45935e7b36cad3559fe6b446:0" - token utxo
            // "255b33cfe724800bf158b63985ed2cafc39db897d3dfb475b7535cdec0b69923:1" - nebl utxo
            //
            // to simplify this in the function is used search for nebl utxo! it means if you want to send just one token you will fill just one utxo of this token

            if (tokenUtxos?.Count != 0)
            {
                if (!isItMintNFT)
                {
                    foreach (var it in tokenUtxos)
                    {
                        var itt = it;
                        if (it.Contains(':'))
                            itt = it.Split(':')[0];

                        (bool, double) voutstate;

                        if (!isNFTtx)
                        {
                            voutstate = await ValidateNeblioTokenUtxo(address, tokenId, itt);
                        }
                        else
                        {
                            voutstate = await ValidateOneTokenNFTUtxo(address, tokenId, itt);
                        }

                        if (voutstate.Item1)
                            Sendutxo.Add(itt + ":" + ((int)voutstate.Item2).ToString()); // copy received utxos and add item number of vout after validation
                    }
                }
                else
                {
                    // if isItMing flag is set the utxo was already found and checked by internal function
                    foreach (var u in tokenUtxos)
                    {
                        var itt = u;
                        if (u.Contains(':'))
                            itt = u.Split(':')[0];

                        var voutstate = await ValidateNeblioTokenUtxo(address, itt, tokenId, true);

                        if (voutstate.Item1)
                            Sendutxo.Add(itt + ":" + ((int)voutstate.Item2).ToString());

                    }
                }
            }
            else
            {
                // if not utxo provided, check the un NFT tokens sources. These with more than 1 token

                var utxs = await FindUtxoForMintNFT(address, tokenId, (int)amount);
                var ut = utxs.FirstOrDefault();
                if (ut != null)
                    Sendutxo.Add(ut.Txid + ":" + ((int)ut.Index).ToString());
            }

            if (Sendutxo.Count == 0)
                throw new Exception("Cannot find any spendable token source Utxos");

            NBitcoin.Altcoins.Neblio.NeblioTransaction neblUtxo = null;

            // need some neblio too
            // to be sure to have last tx request it from neblio network
            // set some minimum amount
            var utxos = await GetAddressNeblUtxo(address, 0.0001, 2 * (fee / FromSatToMainRatio)); // to be sure find at leas 2 times of expected fee
                                                                                                              // create raw Tx with NBitcoin

            if (utxos == null || utxos.Count == 0)
            {
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");
            }

            var found = false;
            if (!string.IsNullOrEmpty(inputNeblUtxo))
            {
                foreach (var u in utxos)
                {
                    if (u.Txid == inputNeblUtxo)
                    {
                        Sendutxo.Add(u.Txid + ":" + ((int)u.Index).ToString());
                        found = true;
                        break;
                    }
                }
            }

            if (!found && !sendEvenNeblUtxoNotFound && !string.IsNullOrEmpty(inputNeblUtxo))
                throw new Exception("Input Neblio Utxo is not spendable!");

            if (!found)
            {
                if (string.IsNullOrEmpty(inputNeblUtxo))
                {
                    foreach (var u in utxos)
                    {
                        if ((u.Value) >= fee)
                        {
                            Sendutxo.Add(u.Txid + ":" + ((int)u.Index).ToString());
                            break;
                        }
                    }
                }
            }

            return Sendutxo;

        }

        public static string SendNTP1TokenAPI(SendTokenTxData data, NeblioAccount account, double fee = 20000, bool isItMintNFT = false, bool isNFTtx = false)
        {
            var res = SendNTP1TokenAPIAsync(data, account, fee, isItMintNFT, isNFTtx).GetAwaiter().GetResult();
            return res;
        }
        public static async Task<string> SendNTP1TokenAPIAsync(SendTokenTxData data, NeblioAccount account, double fee = 20000, bool isItMintNFT = false, bool isNFTtx = false)
        {
            var res = "ERROR";

            // load key and address
            BitcoinSecret keyfromFile = null;
            BitcoinAddress addressForTx = null;
            try
            {
                var k = await GetAddressAndKey(account.AccountKey, data.Password);
                keyfromFile = k.Item2;
                addressForTx = k.Item1;
            }
            catch (Exception ex)
            {
                throw ex;
            }


            // create and init send token request dto for Neblio API
            var dto = new SendTokenRequest();
            try
            {
                dto = GetSendTokenObject(data.Amount, fee, data.ReceiverAddress, data.Id);

                if (data.Metadata != null)
                {
                    foreach (var d in data.Metadata)
                    {
                        var obj = new JObject();
                        obj[d.Key] = d.Value;
                        dto.Metadata.UserData.Meta.Add(obj);
                    }
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // load utxos list if exists, other case leave it to Neblio API
            if (data.sendUtxo.Count > 0)
            {
                dto.Sendutxo = await GetUtxoListForTx(account.Address,
                                                      data.Id,
                                                      data.Amount,
                                                      data.sendUtxo,
                                                      data.NeblUtxo, fee, isItMintNFT, isNFTtx, data.SendEvenNeblUtxoNotFound);

                if (dto.Sendutxo.Count < 2)
                    throw new Exception("Cannot load correct utxos. Loaded just one as input.");
            }
            else
            {
                dto.From = new List<string>() { data.SenderAddress };
            }

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

        public static string MintNFTToken(MintNFTData data, EncryptionKey ekey, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos, double fee = 20000)
        {
            var res = MintNFTTokenAsync(data, ekey, nutxos, tutxos, fee).GetAwaiter().GetResult();
            return res;
        }
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

            // create and init send token request dto for Neblio API
            var dto = new SendTokenRequest();
            try
            {
                dto = GetSendTokenObject(1, fee, data.SenderAddress, VENFTId);

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

            /*
            transaction.Inputs.RemoveAt(0);
            var inp = transaction.Inputs.FirstOrDefault();//save neblio input
            transaction.Inputs.Clear();
            transaction.Inputs.Add(new OutPoint(uint256.Parse(tutxo.Txid), (int)tutxo.Index));
            transaction.Inputs.Add(inp);
            */
            try
            {
                return await SignAndBroadcast(transaction, key, addressForTx);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

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
            var tutxo = string.Empty;
            var val = await ValidateOneTokenNFTUtxo(data.SenderAddress, VENFTId, nftutxo);
            if (!val.Item1)
                throw new Exception("Cannot send transaction, nft utxo is not spendable!");
            else
                tutxo = nftutxo + ":" + ((int)val.Item2).ToString();

            if (nutxos == null || nutxos.Count == 0)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxos!");

            var nutxo = nutxos.FirstOrDefault();
            if (nutxo == null)
                throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");

            // create and init send token request dto for Neblio API
            var dto = new SendTokenRequest();
            try
            {
                dto = GetSendTokenObject(1, fee, data.ReceiverAddress, VENFTId);

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

            // create and init send token request dto for Neblio API
            var dto = new SendTokenRequest();
            try
            {
                dto = GetSendTokenObject(data.Amount, fee, data.ReceiverAddress, VENFTId);

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

        public static string SendNeblioTransactionAPI(SendTxData data, EncryptionKey ekey, ICollection<Utxos> nutxos, double fee = 10000)
        {
            var res = SendNeblioTransactionAPIAsync(data, ekey, nutxos, fee).GetAwaiter().GetResult();
            return res;
        }
        public static async Task<string> SendNeblioTransactionAPIAsync(SendTxData data, EncryptionKey ekey, ICollection<Utxos> nutxos, double fee = 10000)
        {
            var res = "ERROR";

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

            // create template for new tx from last one
            var transaction = Transaction.Create(Network); // new NBitcoin.Altcoins.Neblio.NeblioTransaction(network.Consensus.ConsensusFactory);//neblUtxo.Clone();

            try
            {
                // add inputs of tx
                foreach (var utxo in nutxos)
                {
                    var txh = await GetTxHex(utxo.Txid);
                    if (Transaction.TryParse(txh, Network, out var txin))
                    {
                        transaction.Inputs.Add(txin, (int)utxo.Index);
                        transaction.Inputs.Last().ScriptSig = addressForTx.ScriptPubKey;
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during loading inputs. " + ex.Message);
            }

            try
            {
                var allNeblCoins = 0.0;
                foreach (var u in nutxos)
                    allNeblCoins += (double)u.Value;

                var amountinSat = Convert.ToUInt64(data.Amount * FromSatToMainRatio);
                var diffinSat = Convert.ToUInt64(allNeblCoins) - amountinSat - Convert.ToUInt64(fee);

                // create outputs
                transaction.Outputs.Add(new Money(amountinSat), recaddr.ScriptPubKey); // send to receiver required amount
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


        public static async Task<string> SendNTP1TokenWithPaymentAPIAsync(SendTokenTxData data, EncryptionKey ekey, double neblAmount, ICollection<Utxos> nutxos, double fee = 20000)
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

            var tutxos = await FindUtxoForMintNFT(data.SenderAddress, "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8", 5);
            if (tutxos == null || tutxos.Count == 0)
                throw new Exception("Cannot send transaction, cannot load sender token utxos, for buying you need at least 5 VENFT lot!");

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
                dto = GetSendTokenObject(1, fee, data.ReceiverAddress, VENFTId);

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

            ////

            ICollection<Utxos> neblutxos = null;
            try
            {
                // to be sure to have last tx request it from neblio network
                neblutxos = await GetAddressNeblUtxo(data.SenderAddress, (fee / FromSatToMainRatio), (neblAmount + 2*(fee/FromSatToMainRatio)));
                // create raw Tx with NBitcoin
                if (neblutxos == null)
                    throw new Exception("Cannot send transaction, cannot load sender address history!");
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot send transaction, cannot load sender address history! " + ex.Message);
            }

            try
            {
                transaction.Inputs.Clear();
                
                foreach(var u in dto.Sendutxo)
                {
                    var txh = await GetTxHex(u.Split(':')[0]);
                    if (Transaction.TryParse(txh, Network, out var txin))
                    {
                        transaction.Inputs.Add(txin, Convert.ToInt32(u.Split(':')[1]));
                    }
                }

                // remove token carrier, will be added later
                transaction.Outputs.RemoveAt(3);

                // add inputs of neblio utxo for payment part
                foreach (var u in neblutxos)
                {
                    if (!dto.Sendutxo.Any(ut => ((ut.Split(':')[0] == u.Txid) && ut.Split(':')[1] == ((int)u.Index).ToString())))
                    {
                        var txh = await GetTxHex(u.Txid);
                        if (Transaction.TryParse(txh, Network, out var txin))
                        {
                            transaction.Inputs.Add(txin, (int)u.Index);
                        }
                    }
                    else
                    {
                        // if the input is same as for nebl tx remove it and recalc + add it later
                        transaction.Outputs.RemoveAt(2);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during loading inputs. " + ex.Message);
            }

            try
            {
                var allNeblCoins = 0.0;
                foreach (var u in neblutxos)
                    allNeblCoins += (double)u.Value;

                var amountinSat = Convert.ToUInt64(neblAmount * FromSatToMainRatio);
                var balanceinSat = Convert.ToUInt64(allNeblCoins);

                if ((amountinSat + fee) > balanceinSat)
                    throw new Exception("Not enought spendable Neblio on the address.");

                var diffinSat = balanceinSat - amountinSat - Convert.ToUInt64(fee) - 10000; // fee is already included in previous output, last is token carrier

                // create outputs
                transaction.Outputs.Add(new Money(amountinSat), recaddr.ScriptPubKey); // send to receiver required amount
                transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender address
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

        private static async Task<string> SignAndBroadcast(Transaction transaction, BitcoinSecret key, BitcoinAddress address)
        {
            // add coins
            List<ICoin> coins = new List<ICoin>();
            try
            {
                var addrutxos = await GetAddressUtxosObjects(address.ToString());

                // add all spendable coins of this address
                foreach (var inp in addrutxos)
                    coins.Add(new Coin(uint256.Parse(inp.Txid), (uint)inp.Index, new Money((int)inp.Value), address.ScriptPubKey));

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

                transaction.Sign(key, coins);

                var sx = transaction.ToString();

                if (tx == sx)
                    throw new Exception("Transaction was not signed. Probably not spendable source.");
            }
            catch (Exception ex)
            {
                throw new Exception("Exception during signing tx! " + ex.Message);
            }

            // broadcast
            try
            {
                var txhex = transaction.ToHex();
                var res = await BroadcastSignedTransaction(txhex);
                return res;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //////////////////////////////////////
        #region Multi Token Input Tx

        public static async Task<string> SendMultiTokenAPIAsync(SendTokenTxData data, EncryptionKey ekey, ICollection<Utxos> nutxos, double fee = 10000)
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
                            Address = "NPWBL3i8ZQ8tmhDtrixXwYd93nofmunvhA",
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
                foreach (var it in data.sendUtxo)
                {
                    var itt = it;
                    if (it.Contains(':'))
                        itt = it.Split(':')[0];

                    (bool, double) voutstate;

                    try
                    {
                        voutstate = await ValidateOneTokenNFTUtxo(data.SenderAddress, data.Id, itt);
                    }
                    catch(Exception ex)
                    {
                        throw new Exception("Cannot validate utxo for multitoken payment.");
                    }
                    
                    if (voutstate.Item1)
                        dto.Sendutxo.Add(itt + ":" + ((int)voutstate.Item2).ToString()); // copy received utxos and add item number of vout after validation
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
        // calls of Neblio API and helpers

        public static async Task<string> SendRawNTP1TxAsync(SendTokenRequest data)
        {

            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
            }

            try
            {
                var info = await _client.SendTokenAsync(data);
                return info.TxHex;
            }
            catch(Exception ex)
            {
                throw new Exception("Cannot Create raw token tx. " + ex.Message);
            }
        }

        public static async Task<string> BroadcastNTP1TxAsync(BroadcastTxRequest data)
        {

            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
            }

            var info = await _client.BroadcastTxAsync(data);

            return info.Txid;
        }

        public static async Task<GetAddressResponse> AddressInfoAsync(string addr)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
            }

            var address = await _client.GetAddressAsync(addr);

            return address;
        }

        public static async Task<GetAddressInfoResponse> AddressInfoUtxosAsync(string addr)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
            }

            var address = await _client.GetAddressInfoAsync(addr);

            return address;
        }

        public static async Task<ICollection<Anonymous>> GetAddressUtxos(string addr)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
            }

            var utxos = await _client.GetAddressUtxosAsync(addr);

            return utxos;
        }

        public static async Task<ICollection<Utxos>> GetAddressUtxosObjects(string addr)
        {
            try
            {
                if (_client == null)
                {
                    _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
                }

                var addinfo = await _client.GetAddressInfoAsync(addr);

                return addinfo.Utxos;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load address utxos. " + ex.Message);
                return new List<Utxos>();
            }
        }

        public static async Task<ICollection<Utxos>> GetAddressTokensUtxos(string addr, GetAddressInfoResponse addressinfo = null)
        {
            if (addressinfo == null)
            {
                if (_client == null)
                {
                    _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
                }

                addressinfo = await _client.GetAddressInfoAsync(addr);
            }

            var utxos = new List<Utxos>();

            if (addressinfo?.Utxos != null)
            {
                foreach (var u in addressinfo.Utxos)
                {
                    if (u.Tokens.Count > 0)
                    {
                        utxos.Add(u);
                    }
                }
            }

            return utxos;
        }

        public static async Task<string> GetTxHex(string txid)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
            }

            var tx = await _client.GetTransactionInfoAsync(txid);

            if (tx != null)
                return tx.Hex;
            else
                return string.Empty;
        }

        public static async Task<ICollection<Utxos>> GetAddressNFTsUtxos(string addr, GetAddressInfoResponse addressinfo = null)
        {
            if (addressinfo == null)
            {
                if (_client == null)
                {
                    _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
                }

                addressinfo = await _client.GetAddressInfoAsync(addr);
            }
            var utxos = new List<Utxos>();

            if (addressinfo?.Utxos != null)
            {
                foreach (var u in addressinfo.Utxos)
                {
                    if (u.Tokens.Count > 0)
                    {
                        if (u.Tokens.ToArray()[0]?.Amount == 1)
                            utxos.Add(u);
                    }
                }
            }

            return utxos;
        }

        public static async Task<string> BroadcastNeblTxAsync(BroadcastTxRequest dto)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
            }

            var resp = await _client.SendTxAsync(dto);

            return resp?.Txid;
        }

        public static async Task<double> GetSendAmount(GetTransactionInfoResponse tx, string address)
        {
            BitcoinAddress addr = null;
            try
            {
                addr = BitcoinAddress.Create(address, Network);
            }
            catch (Exception ex)
            {
                throw new Exception("Cannot get amount of transaction. cannot create receiver address!");
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
            foreach(var vout in tx.Vout)
            {
                if (vout.ScriptPubKey.Hex == addr.ScriptPubKey.ToHex())
                {
                    amount += ((double)vout.Value / FromSatToMainRatio);
                }
            }

            amount -= vinamount;

            return amount;
        }

        public static async Task<ICollection<Utxos>> GetAddressNeblUtxo(string addr, double minAmount = 0.0001, double requiredAmount = 0.0001)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
            }

            var addinfo = await _client.GetAddressInfoAsync(addr);
            var utxos = addinfo.Utxos;

            var resp = new List<Utxos>();

            var founded = 0.0;

            utxos = utxos.OrderBy(u => u.Value).Reverse().ToList();

            if (utxos != null)
            {
                foreach (var utx in utxos)
                {
                    if (utx.Blockheight > 0)
                    {
                        if (utx.Tokens?.Count == 0)
                        {
                            if (utx.Value > 10000)
                            {
                                if (((double)utx.Value) > (minAmount * FromSatToMainRatio))
                                {
                                    try
                                    {
                                        var tx = await _client.GetTransactionInfoAsync(utx.Txid);

                                        if (tx != null)
                                        {
                                            if (tx.Confirmations > MinimumConfirmations && tx.Blockheight > 0)
                                            {
                                                resp.Add(utx);
                                                founded += ((double)utx.Value / FromSatToMainRatio);
                                                if (founded > requiredAmount)
                                                    return resp;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        ;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return resp;
        }

        public static async Task<(bool, double)> ValidateNeblioUtxo(string address, string txid)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
            }

            var info = await _client.GetAddressInfoAsync(address);

            if (info != null)
            {
                var uts = info.Utxos.Where(u => u.Txid == txid);
                if (uts != null)
                {
                    foreach (var ut in uts)
                    {
                        if (ut.Blockheight > 0)
                        {
                            if (ut != null)
                            {
                                var tx = await _client.GetTransactionInfoAsync(ut.Txid);
                                if (tx != null)
                                {
                                    if (tx.Confirmations > MinimumConfirmations && tx.Blockheight > 0)
                                    {
                                        return (true, (double)ut.Index);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return (false, 0);
        }

        public static async Task<(bool, double)> ValidateNeblioTokenUtxo(string address, string txid, string tokenId, bool isMint = false)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
            }

            var addinfo = await _client.GetAddressInfoAsync(address);
            var utxos = addinfo.Utxos;

            var resp = new List<Utxos>();

            if (utxos != null)
            {
                foreach (var ut in utxos)
                {
                    if (ut != null)
                    {
                        if (ut.Blockheight > 0)
                        {
                            if (ut.Tokens.Count > 0)
                            {
                                var toks = ut.Tokens.ToArray();
                                if (toks[0].TokenId == tokenId)
                                {
                                    if ((toks[0].Amount > 0 && !isMint) || (toks[0].Amount > 1 && isMint))
                                    {
                                        var tx = await _client.GetTransactionInfoAsync(ut.Txid);
                                        if (tx != null)
                                        {
                                            if (tx.Confirmations > MinimumConfirmations && tx.Blockheight > 0)
                                            {
                                                return (true, (double)ut.Index);

                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return (false, 0);
        }

        public static async Task<(bool, double)> ValidateOneTokenNFTUtxo(string address, string tokenId, string txid)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
            }

            var addinfo = await _client.GetAddressInfoAsync(address);
            var utxos = addinfo.Utxos;

            var resp = new List<Utxos>();

            if (utxos != null)
            {
                var uts = utxos.Where(u => u.Txid == txid); // you can have multiple utxos with same txid but different amount of tokens

                if (uts != null)
                {
                    foreach (var ut in uts)
                    {
                        if (ut.Blockheight > 0)
                        {
                            if (ut.Tokens != null)
                            {
                                if (ut.Tokens.Count > 0)
                                {
                                    var toks = ut.Tokens.ToArray();
                                    if (toks[0].TokenId == tokenId)
                                    {
                                        if (toks[0].Amount == 1)
                                        {
                                            var tx = await _client.GetTransactionInfoAsync(ut.Txid);
                                            if (tx != null)
                                            {
                                                if (tx.Confirmations > MinimumConfirmations && tx.Blockheight > 0)
                                                {
                                                    return (true, (double)ut.Index);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return (false, 0);
        }

        public static async Task<List<Utxos>> FindUtxoForMintNFT(string addr, string tokenId, int numberToMint = 1, double oneTokenSat = 10000)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
            }

            var addinfo = await _client.GetAddressInfoAsync(addr);
            var utxos = addinfo.Utxos;

            var resp = new List<Utxos>();
            var founded = 0.0;

            if (utxos != null)
            {
                utxos = utxos.OrderBy(u => u.Value).Reverse().ToList();

                foreach (var utx in utxos)
                {
                    if (utx.Blockheight > 0)
                    {
                        if (utx.Tokens.Count > 0)
                        {
                            //if (utx.Value == oneTokenSat)
                            {
                                var tok = utx.Tokens.ToArray()?[0];
                                if (tok != null)
                                {
                                    if (tok.TokenId == tokenId)
                                    {
                                        if (tok?.Amount > 1)
                                        {
                                            var tx = await _client.GetTransactionInfoAsync(utx.Txid);
                                            if (tx != null)
                                            {
                                                if (tx.Confirmations > MinimumConfirmations && tx.Blockheight > 0)
                                                {
                                                    founded += (double)tok.Amount;
                                                    resp.Add(utx);

                                                    if (founded > numberToMint)
                                                        return resp;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return resp;
        }

        public static async Task<Utxos> FindUtxoToSplit(string addr, string tokenId, int lotAmount = 100, double oneTokenSat = 10000)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
            }

            var addinfo = await _client.GetAddressInfoAsync(addr);
            var utxos = addinfo.Utxos;

            var resp = new List<Utxos>();
            var founded = 0.0;

            if (utxos != null)
            {

                utxos = utxos.OrderBy(u => u.Value).ToList();

                foreach (var utx in utxos)
                {
                    if (utx.Blockheight > 0)
                    {
                        if (utx.Tokens.Count > 0)
                        {
                            if (utx.Value == oneTokenSat)
                            {
                                var tok = utx.Tokens.ToArray()?[0];
                                if (tok != null)
                                {
                                    if (tok.TokenId == tokenId)
                                    {
                                        if (tok?.Amount > lotAmount)
                                        {
                                            var tx = await _client.GetTransactionInfoAsync(utx.Txid);
                                            if (tx != null)
                                            {
                                                if (tx.Confirmations > MinimumConfirmations && tx.Blockheight > 0)
                                                {
                                                    founded += (double)tok.Amount;

                                                    return utx;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }

        public static async Task<Dictionary<string, string>> GetTransactionMetadata(string tokenid, string txid)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
            }
            var resp = new Dictionary<string, string>();

            var info = await _client.GetTokenMetadataOfUtxoAsync(tokenid, txid, 0);
            
            if (info.MetadataOfUtxo != null)
            {
                if (info.MetadataOfUtxo.UserData.Meta.Count > 0)
                {
                    foreach (var o in info.MetadataOfUtxo.UserData.Meta)
                    {
                        var od = JsonConvert.DeserializeObject<IDictionary<string, string>>(o.ToString());

                        if (od != null)
                        {
                            if (od.Count > 0)
                            {
                                var of = od.First();
                                if (!resp.ContainsKey(of.Key))
                                    resp.Add(of.Key, of.Value);
                            }
                        }
                    }
                }
            }
            return resp;
        }

        public static async Task<Transaction> GetLastSentTransaction(string address)
        {
            try
            {
                if (_client == null)
                    _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };

                var addinfo = await _client.GetAddressAsync(address);
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

        public static async Task<GetTransactionInfoResponse> GetTransactionInfo(string txid)
        {
            try
            {
                if (_client == null)
                {
                    _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
                }
                
                var info = await _client.GetTransactionInfoAsync(txid);

                return info;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot load tx info. " + ex.Message);
                return new GetTransactionInfoResponse();
            }
        }

        public static async Task<List<string>> SplitTheTokens(NeblioAccount account, string address, string password, string tokenId, int lotAmount, int numberOfLots)
        {
            var listofFinalUtxo = new List<string>();

            if (lotAmount > 1)
            {
                for (int i = 0; i < numberOfLots; i++)
                {
                    try
                    {
                        Console.WriteLine($"Starting spliting lot number {i}!");

                        var utxoFound = false;
                        var attempts = 120;
                        Utxos sutx = null;
                        Utxos neblUtxo = null;

                        while (!utxoFound)
                        {
                            sutx = await NeblioTransactionHelpers.FindUtxoToSplit(address, tokenId, lotAmount + 2);
                            var neblUtxos = await NeblioTransactionHelpers.GetAddressNeblUtxo(address, 0.0001, 0.0002);
                            neblUtxo = neblUtxos?.FirstOrDefault();
                            if (sutx != null && neblUtxo != null)
                            {
                                utxoFound = true;
                                break;
                            }
                            else
                            {
                                Console.WriteLine($"Waiting for source Utxo for spliting for item {i} of {numberOfLots}!");
                                await Task.Delay(1000);
                                attempts--;
                                if (attempts < 0)
                                {
                                    Console.WriteLine("Too long to get utxo for spliting!");
                                    throw new Exception("Cannot find Utxos for spliting!");
                                }
                            }
                        }

                        if (sutx != null && neblUtxo != null)
                        {
                            Console.WriteLine("Utxo for spliting found, start spliting!");

                            var meta = new Dictionary<string, string>();
                            meta.Add("Split Coin Process", "true");

                            var sendutxos = new List<string>();

                            sendutxos.Add(sutx.Txid + ":" + sutx.Index);

                            var dto = new SendTokenTxData()
                            {
                                Amount = lotAmount,
                                Id = tokenId,
                                Metadata = meta,
                                Password = password,
                                SenderAddress = address,
                                ReceiverAddress = address,
                                sendUtxo = sendutxos,
                                UseRPCPrimarily = false
                            };

                            var resp = await NeblioTransactionHelpers.SendNTP1TokenAPIAsync(dto, account, isItMintNFT: true);

                            if (!string.IsNullOrEmpty(resp))
                            {
                                Console.WriteLine($"Succesfully splited in txid {resp}!");
                                listofFinalUtxo.Add(resp);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot split tokens! " + ex);
                    }
                }

                return listofFinalUtxo;
            }

            return null;
        }

        public static async Task<GetTokenMetadataResponse> GetTokenMetadata(string tokenId)
        {
            try
            {
                if (_client == null)
                {
                    _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
                }

                GetTokenMetadataResponse tokeninfo = new GetTokenMetadataResponse();
                tokeninfo = await _client.GetTokenMetadataAsync(tokenId, 0);

                return tokeninfo;
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot load token metadata. " + ex.Message);
                return null;
            }
        }

        public static async Task<(double, GetTokenMetadataResponse)> GetActualMintingSupply(string address, GetAddressInfoResponse addressinfo = null)
        {

            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
            }

            GetTokenMetadataResponse tokeninfo = new GetTokenMetadataResponse();
            tokeninfo = await _client.GetTokenMetadataAsync(VENFTId, 0);

            var res = await NeblioTransactionHelpers.GetAddressTokensUtxos(address, addressinfo);
            var utxos = new List<Utxos>();
            foreach (var r in res)
            {
                var toks = r.Tokens.ToArray()?[0];
                if (toks != null)
                {
                    if (toks.Amount > 1)
                    {
                        if (toks.TokenId == "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8")
                            utxos.Add(r);
                    }
                }
            }

            var totalAmount = 0.0;
            foreach (var u in utxos)
                totalAmount += (double)u.Tokens.ToArray()?[0]?.Amount;

            return (totalAmount, tokeninfo);
        }

        private class tokenUrlCarrier
        {
            public string name { get; set; } = string.Empty;
            public string url { get; set; } = string.Empty;
            public string mimeType { get; set; } = string.Empty;
        }
        public static async Task<Dictionary<string,TokenSupplyDto>> CheckTokensSupplies(string address, GetAddressInfoResponse addressinfo = null)
        {
            var resp = new Dictionary<string, TokenSupplyDto>();

            var res = await GetAddressTokensUtxos(address, addressinfo);
            var utxos = new List<Utxos>();
            foreach (var r in res)
            {
                var toks = r.Tokens.ToArray()?[0];
                if (toks != null)
                {
                    if (toks.Amount > 1)
                    {
                        GetTokenMetadataResponse tokeninfo = new GetTokenMetadataResponse();
                        tokeninfo = await _client.GetTokenMetadataAsync(toks.TokenId, 0);

                        if (!resp.TryGetValue(tokeninfo.MetadataOfIssuance.Data.TokenName, out var tk))
                        {

                            var t = new TokenSupplyDto();
                            t.TokenSymbol = tokeninfo.MetadataOfIssuance.Data.TokenName;
                            var tus = JsonConvert.DeserializeObject<List<tokenUrlCarrier>>(JsonConvert.SerializeObject(tokeninfo.MetadataOfIssuance.Data.Urls));

                            var tu = tus.FirstOrDefault();
                            if (tu != null)
                            {
                                t.ImageUrl = tu.url;
                            }

                            t.TokenId = toks.TokenId;
                            t.Amount += (double)toks.Amount;

                            resp.TryAdd(t.TokenSymbol, t);
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

        public static async Task<List<TokenOwnerDto>> GetTokenOwners(string tokenId)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = BaseURL };
            }

            GetTokenHoldersResponse tokenholders = new GetTokenHoldersResponse();
            tokenholders = await _client.GetTokenHoldersAsync("La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8");

            var resp = new List<TokenOwnerDto>();
            var hd = tokenholders.Holders.ToList().OrderBy(h => (double)h.Amount).Reverse().ToList();
            hd.RemoveAt(0);
            hd.RemoveAt(0);
            hd.RemoveAt(0);

            var i = 0;
            foreach (var h in hd)
            {
                try
                {
                    if (h.Address != "NPWBL3i8ZQ8tmhDtrixXwYd93nofmunvhA" &&
                        h.Address != "NeNE6a2YQCq4yBLoVbVpcCzx44jVEBLaUE" &&
                        h.Address != "NikErRpjtRXpryFRc3RkP5nxRzm1ApxFH8" &&
                        h.Address != "NZREfode8XxDHndeoLGEeQKhsfvjWfHXUU")
                    {
                        var shadd = h.Address.Substring(0, 3) + "..." + h.Address.Substring(h.Address.Length - 3);
                        var utxs = await GetAddressNFTsUtxos(h.Address);
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
                        if (i > 100)
                            break;
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Problem with reading address info. Address: " + h.Address + " - " + ex.Message);
                }
            }

            resp = resp.OrderBy(r => r.AmountOfNFTs).Reverse().ToList();

            if (resp.Count > 50)
            {
                resp.RemoveRange(49, resp.Count - 50 - 1);
            }

            return resp;
        }
    }
}
