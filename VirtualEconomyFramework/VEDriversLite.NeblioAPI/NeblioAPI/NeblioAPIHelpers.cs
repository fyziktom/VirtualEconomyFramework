using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common;
//using VEDriversLite.Events;
//using VEDriversLite.Dto;
//using VEDriversLite.Neblio;
//using VEDriversLite.Security;

namespace VEDriversLite.NeblioAPI
{
    /// <summary>
    /// Main Helper class for the Neblio Blockchain Transactions
    /// </summary>
    public static class NeblioAPIHelpers
    {
        private static readonly HttpClient httpClient = new HttpClient();
        private static IClient _client;
        private static readonly string BaseURL = "https://ntp1node.nebl.io/";

        public const string VENFTTokenId = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8";
        public const string NeblioImageLink = "https://ve-framework.com/ipfs/QmPUvBN4qKvGyKKhADBJKSmNC7JGnr3Rwf5ndENGMfpX54";
        public const string DogecoinImageLink = "https://ve-framework.com/ipfs/QmRp3eyUeqctcgBFcRuBa7uRWiABTXmLBeYuhLp8xLX1sy";
        public const string VENFTImageLink = "https://ve-framework.com/ipfs/QmZSdjuLTihuPzVwUKaHLtivw1HYhsyCdQFnVLLCjWoVBk";
        public const string BDPImageLink = "https://ve-framework.com/ipfs/QmYMVuotTTpW24eJftpbUFgK7Ln8B4ox3ydbKCB6gaVwVB";
        public const string WDOGEImageLink = "https://ve-framework.com/ipfs/Qmc9xS9a8TnWmU7AN4dtsbu4vU6hpEXpMNAeUdshFfg1wT";
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
        /// Address of VEFramework.BlockchainIndexerServer API
        /// </summary>
        //#if DEBUG
        //public static string NewAPIAddress { get; set; } = "https://localhost:7267/";
        //#else
        public static string NewAPIAddress { get; set; } = "https://ve-framework.com/";
        //public static string NewAPIAddress { get; set; } = "http://localhost:5000/";
//#endif
        /// <summary>
        /// Check if the number of the confirmation is enough for doing transactions.
        /// It mainly usefull for UI stuff or console.
        /// </summary>
        /// <param name="confirmations"></param>
        /// <returns></returns>
        public static string IsEnoughConfirmationsForSend(int confirmations)
        {
            if (confirmations > MinimumConfirmations)
                return ">" + MinimumConfirmations.ToString();
            
            return confirmations.ToString();
        }
        
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
        /// <param name="len">Length of the result shortened tx hash</param>
        /// <returns></returns>
        public static string ShortenTxId(string txid, bool withDots = true, int len = 10)
        {
            if (string.IsNullOrEmpty(txid))
                return string.Empty;
            
            if (txid.Length < 10)
                return txid;

            string txids;
            if (withDots)
                txids = txid.Remove(len / 2, txid.Length - len / 2) + "....." + txid.Remove(0, txid.Length - len / 2);
            else
                txids = txid.Remove(len / 2, txid.Length - len / 2) + txid.Remove(0, txid.Length - len / 2);

            return txids;
        }
        
        /// <summary>
        /// Returns metadata in the token transction
        /// </summary>
        /// <param name="tokenid">token id hash</param>
        /// <param name="txid">tx id hash</param>
        /// <returns></returns>
        public static async Task<Dictionary<string, string>> GetTransactionMetadata(string tokenid, string txid, GetTransactionInfoResponse? txinfo = null)
        {
            if (string.IsNullOrEmpty(txid) && txinfo == null)
                return new Dictionary<string, string>();

            GetTokenMetadataResponse info = new GetTokenMetadataResponse();
            if (txinfo == null)
                txinfo = await GetTransactionInfo(txid);
            
            var metadata = string.Empty;
            if (txinfo.Vout.Any(o => o.ScriptPubKey.Type == "nulldata"))
                metadata = txinfo.Vout.FirstOrDefault(o => o.ScriptPubKey.Type == "nulldata")?.ScriptPubKey.Asm ?? string.Empty;
            
            var meta = ParseCustomMetadata(metadata);
            
            return meta;
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
                var meta = metadata.Replace("OP_RETURN ", string.Empty).Trim();

                var customData = string.Empty;
                if (meta.Contains("789c")) // start of the custom data
                {
                    var customDataStart = meta.Split("789c");
                    var length = customDataStart[0].Length;
                    customData = meta.Substring(length, meta.Length - length).Trim();
                }

                if (string.IsNullOrEmpty(customData))
                    return new Dictionary<string, string>();

                var customDecompressed = StringExt.Decompress(StringExt.HexStringToBytes(customData));
                var metadataString = Encoding.UTF8.GetString(customDecompressed);

                var resp = new Dictionary<string, string>();

                var userData = JsonConvert.DeserializeObject<MetadataOfUtxo>(metadataString);

                if (userData != null)
                {
                    foreach (var o in userData.UserData.Meta)
                    {
                        var od = JsonConvert.DeserializeObject<IDictionary<string, string>>(o.ToString());
                        if (od != null && od.Count > 0)
                        {
                            var of = od.First();
                            if (!resp.ContainsKey(of.Key))
                                resp.Add(of.Key, of.Value);
                        }
                    }
                }

                return resp;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot parse custom metadata from the transaction. " + ex.Message + "; original input metadata: " + metadata);
            }
            return new Dictionary<string, string>();
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
            {
                throw new Exception("Wrong input transaction for broadcast.");
            }
        }



        public class BroadcastTransactionResponseDto
        {
            public BroadcastDataResponseDto data { get; set; } = new BroadcastDataResponseDto();
        }
        public class BroadcastDataResponseDto
        {
            public string network { get; set; } = "neblio";
            public string txid { get; set; } = string.Empty;
        }

        public static async Task<string> BroadcastTransactionVEAPI(string txhex)
        {
            var client = new HttpClient();

            var obj = new
            {
                tx_hex = txhex,
                network = "neblio"
            };

            var cnt = JsonConvert.SerializeObject(obj);
            var url = $"{NeblioAPIHelpers.NewAPIAddress.Trim('/')}/api/BroadcastTransaction";

            using (var content = new StringContent(cnt, System.Text.Encoding.UTF8, "application/json"))
            {
                httpClient.DefaultRequestHeaders.Add("mode", "no-cors");
                httpClient.DefaultRequestHeaders.Add("Accept", "*/*");

                HttpResponseMessage result = await client.PostAsync(url, content);
                if (result.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    var returnStr = await result.Content.ReadAsStringAsync();

                    if (returnStr != null)
                    {
                        try
                        {
                            var resp = JsonConvert.DeserializeObject<BroadcastTransactionResponseDto>(returnStr);
                            if (resp != null)
                                return resp.data.txid;
                        }
                        catch (Exception ex)
                        {
                            await Console.Out.WriteLineAsync("Cannot deserialize response from broadcast of the transaction for txhex: " + txhex + "Exception: " + ex.Message);
                            return "Cannot deserialize response from broadcast of the transaction for txhex: " + txhex + "Exception: " + ex.Message;
                        }
                    }
                }
            }

            return "Cannot broadcast the transaction with TXhex: " + txhex;
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
            
            if (fee < MinimumAmount)
                throw new Exception("Fee cannot be smaller than 10000 Sat.");
            
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
        public static async Task<GetAddressResponse> AddressInfoAsyncOld(string addr)
        {
            if (string.IsNullOrEmpty(addr))
                return new GetAddressResponse();
            
            var address = await GetClient().GetAddressAsync(addr);
            return address;
        }

        public static async Task<GetAddressResponse?> AddressInfoAsync(string addr)
        {
            var client = new HttpClient();
            var url = $"{NewAPIAddress.Trim('/')}/api/GetAddressTransactions/{addr}/0/1000";
            var res = await client.GetStringAsync(url);
            if (res != null)
            {
                var txs = JsonConvert.DeserializeObject<List<string>>(res);
                if (txs != null)
                {
                    var reso = new GetAddressResponse()
                    {
                        Transactions = txs
                    };
                    return reso;
                }
            }
            return null;
        }

        /// <summary>
        /// Get Utxos List from VE Indexer API
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static async Task<List<Utxos>> GetAddressUtxosListFromNewAPIAsync(string addr)
        {
            var ouxox = new List<Utxos>();
            var client = new HttpClient();
            var url = $"{NewAPIAddress.Trim('/')}/api/GetAddressUtxos/{addr}";
            var res = await client.GetStringAsync(url);
            if (res != null)
            {
                var utxosOrdered = JsonConvert.DeserializeObject<List<IndexedUtxoDto>>(res);
                if (utxosOrdered != null)
                    return ConvertIndexedUtxoToUtxo(utxosOrdered);
            }
            return ouxox;
        }

        /// <summary>
        /// Get Utxos List from VE Indexer API
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static List<Utxos> ConvertIndexedUtxoToUtxo(List<IndexedUtxoDto> utxos)
        {
            var ouxox = new List<Utxos>();
            if (utxos != null)
            {
                foreach (var ux in utxos)
                {
                    var nux = new Utxos()
                    {
                        Blockheight = ux.Blockheight,
                        Blocktime = ux.Blocktime,
                        Index = ux.Index,
                        Txid = ux.TransactionHash,
                        Value = ux.Value,
                        Tokens = new List<Tokens>()
                    };
                    if (ux.TokenUtxo)
                    {
                        var tok = new Tokens() { Amount = ux.TokenAmount, TokenId = ux.TokenId };
                        if (ux.Blockheight == -1 && ux.TokenAmount == 1 && !string.IsNullOrEmpty(ux.Metadata))
                        {
                            tok.AdditionalProperties = new Dictionary<string, object>()
                            {
                                { "metadata" , ux.Metadata }
                            };
                        }
                        nux.Tokens.Add(tok);
                    }

                    ouxox.Add(nux);
                }
            }
            return ouxox;
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
            
            if (TurnOnCache && AddressInfoCache.TryGetValue(addr, out var info))
            {
                if ((DateTime.UtcNow - info.Item1) < new TimeSpan(0, 0, 1))
                {
                    GetAddressInfoResponse ainfo = info.Item2;
                    return ainfo;
                }
                else
                {
                    var utxos = await GetAddressUtxosListFromNewAPIAsync(addr);
                    if (utxos != null)
                    {
                        var address = new GetAddressInfoResponse() { Utxos = utxos, Address = addr };
                        if (AddressInfoCache.TryRemove(addr, out info))
                            AddressInfoCache.TryAdd(addr, (DateTime.UtcNow, address));

                        return address;
                    }
                }
            }
            else
            {
                var utxos = await GetAddressUtxosListFromNewAPIAsync(addr);
                if (utxos != null)
                {
                    var address = new GetAddressInfoResponse() { Utxos = utxos, Address = addr };
                    if (TurnOnCache)
                        AddressInfoCache.TryAdd(addr, (DateTime.UtcNow, address));

                    return address;
                }
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
                var utxos = await GetAddressUtxosListFromNewAPIAsync(addr);
                if (utxos != null)
                    return utxos;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load address utxos. " + ex.Message);
            }
            return new List<Utxos>();
        }

        /// <summary>
        /// Returns list of all Utxos which contains some tokens
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="addressinfo"></param>
        /// <returns></returns>
        public static async Task<ICollection<Utxos>> GetAddressTokensUtxos(string addr, GetAddressInfoResponse? addressinfo = null)
        {
            if (string.IsNullOrEmpty(addr))
                return new List<Utxos>();
            
            if (addressinfo == null)
                addressinfo = await AddressInfoUtxosAsync(addr);
            
            var utxos = new List<Utxos>();
            if (addressinfo?.Utxos != null)
            {
                foreach (var u in addressinfo.Utxos)
                {
                    if (u != null && u.Tokens.Count > 0)
                        utxos.Add(u);
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
                return string.Empty;
            
            var tx = await GetTransactionInfo(txid);
            if (tx != null)
                return tx.Hex;
            else
                return string.Empty;
        }

        private static void AddToTransactionInfoCache(GetTransactionInfoResponse? txinfo)
        {
            if (txinfo == null) return;

            if (txinfo.Confirmations > MinimumConfirmations + 2)
                TransactionInfoCache.TryAdd(txinfo.Txid, txinfo);
        }

        /// <summary>
        /// Returns list of all Utxos which contains just one token, means amount = 1
        /// </summary>
        /// <param name="addr"></param>
        /// <param name="allowedTokens">Load just the allowed tokens</param>
        /// <param name="addressinfo"></param>
        /// <returns></returns>
        public static async Task<ICollection<Utxos>> GetAddressNFTsUtxos(string addr, List<string> allowedTokens, GetAddressInfoResponse? addressinfo = null)
        {
            if (string.IsNullOrEmpty(addr))
                return new List<Utxos>();
            
            if (addressinfo == null)
                addressinfo = await AddressInfoUtxosAsync(addr);
            
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
                                utxos.Add(u);
                        }
                    }
                }
            }

            if (utxos == null || utxos.Count == 0)
                return new List<Utxos>();
            
            var ouxox = utxos.OrderByDescending(u => u.Blocktime).ToList();
            return ouxox;
        }

        /// <summary>
        /// Does UTXO has enough confirmations? Based on the utxo block height and current block height.
        /// </summary>
        /// <param name="UtxoBlockHeight"></param>
        /// <param name="latestBlockCount"></param>
        /// <returns></returns>
        public static bool IsValidUtxo(double UtxoBlockHeight, double latestBlockCount)
        {
            var confirmation = latestBlockCount - UtxoBlockHeight;
            return confirmation > MinimumConfirmations;
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
        public static async Task<ICollection<Utxos>> GetAddressNeblUtxo(string address, double minAmount = 0.0001, double requiredAmount = 0.0001, GetAddressInfoResponse? addinfo = null, double latestBlockHeight = 0)
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
                return resp;

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
                        return resp;
                }
            }

            return new List<Utxos>();
            
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
        public static async Task<double> ValidateOneTokenNFTUtxo(string address, string tokenId, string txid, int indx, GetAddressInfoResponse? addinfo = null, double latestBlockHeight = 0)
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
                return -1;
            
            var uts = utxos.Where(u => (u.Txid == txid && u.Index == indx)); // you can have multiple utxos with same txid but different amount of tokens
            if (uts == null)
                return -1;
            
            if (latestBlockHeight == 0)
                latestBlockHeight = await NeblioTransactionsCache.LatestBlockHeight(utxos.Where(u => u.Blockheight.Value > 0)?.FirstOrDefault()?.Txid, address);
            
            foreach (var ut in uts.Where(u => u.Blockheight.Value > 0 && u.Tokens != null && u.Tokens.Count > 0))
            {
                var toks = ut.Tokens.ToArray();
                if (toks[0].TokenId == tokenId && toks[0].Amount == 1)
                {
                    double UtxoBlockHeight = ut.Blockheight != null ? ut.Blockheight.Value : 0;
                    if (IsValidUtxo(UtxoBlockHeight, latestBlockHeight))
                        return ((double)ut.Index);
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
        public static async Task<List<Utxos>> FindUtxoForMintNFT(string addr, string tokenId, int numberToMint = 1, double oneTokenSat = 10000, GetAddressInfoResponse? addinfo = null, double latestBlockHeight = 0)
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
                            break;
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
                    break;
            }

            if (res.Count > 10)// neblio API cannot handle more than 10 inputs
                return new List<Utxos>();
            
            return res;
        }

        /// <summary>
        /// Get token metadata from the specific transaction cache logic
        /// </summary>
        /// <param name="tokenid"></param>
        /// <param name="txid"></param>
        /// <param name="verbosity"></param>
        /// <returns></returns>
        public static async Task<GetTokenMetadataResponse> GetTokenMetadataOfUtxoCache(string tokenid, string txid, double verbosity = 0, GetTransactionInfoResponse? txinfo = null)
        {
            if (TurnOnCache && TokenTxMetadataCache.TryGetValue(txid, out var tinfo))
            {
                return tinfo;
            }
            else
            {
                if (txinfo == null)
                    txinfo = await GetTransactionInfo(txid);

                var metadata = string.Empty;
                if (txinfo.Vout.Any(o => o.ScriptPubKey.Type == "nulldata"))
                    metadata = txinfo.Vout.FirstOrDefault(o => o.ScriptPubKey.Type == "nulldata")?.ScriptPubKey.Asm ?? string.Empty;

                var meta = ParseCustomMetadata(metadata);
                var data = new GetTokenMetadataResponse()
                {
                    MetadataOfUtxo = new MetadataOfUtxo()
                    {
                        UserData = new UserData()
                        {
                            Meta = new List<object>()
                        }
                    }
                };

                if (data.MetadataOfUtxo.UserData.Meta != null)
                {
                    foreach (var d in meta)
                    {
                        var obj = new JObject
                        {
                            [d.Key] = d.Value
                        };
                        data.MetadataOfUtxo.UserData.Meta.Add(obj);
                    }
                }

                var tokinfo = new GetTokenMetadataResponse();
                if (TokenMetadataCache.TryGetValue(tokenid, out var ti))
                {
                    tokinfo = ti;
                }
                else
                {
                    var toi = await GetTokenMetadata(tokenid);
                    tokinfo = toi;
                }

                data.Divisibility = tokinfo.Divisibility;
                data.FirstBlock = tokinfo.FirstBlock;
                data.InitialIssuanceAmount = tokinfo.InitialIssuanceAmount;
                data.IssuanceTxid = tokinfo.IssuanceTxid;
                data.IssueAddress = tokinfo.IssueAddress;
                data.MetadataOfIssuance = tokinfo.MetadataOfIssuance;
                data.TokenId = tokenid;
                data.TokenName = tokinfo.TokenName;
                data.TotalSupply = tokinfo.TotalSupply;

                if (TurnOnCache)
                    TokenTxMetadataCache.TryAdd(txid, data);

                return data;
            }
        }

        public static async Task<GetTransactionInfoResponse> GetTransactionInfo(string txid)
        {
            if (string.IsNullOrEmpty(txid))
                return new GetTransactionInfoResponse();
            
            try
            {
                GetTransactionInfoResponse? tx = null;
                if (TurnOnCache && TransactionInfoCache.TryGetValue(txid, out var txinfo))
                {
                    tx = txinfo;
                }
                else
                {
                    var client = new HttpClient();
                    var url = $"{NewAPIAddress.Trim('/')}/api/GetTransactionInfo/{txid}";
                    var res = await client.GetStringAsync(url);
                    if (res != null)
                    {
                        tx = JsonConvert.DeserializeObject<GetTransactionInfoResponse>(res);
                        if (tx != null)
                        {
                            if (TurnOnCache)
                                AddToTransactionInfoCache(tx);
                            return tx;
                        }
                    }
                }

                if (tx != null)
                    return tx;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot load tx {txid} info. " + ex.Message);
            }
            return new GetTransactionInfoResponse();
}

        /// <summary>
        /// Get transaction info.
        /// </summary>
        /// <param name="txid">tx id hash</param>
        /// <returns>Neblio API GetTransactionInfo object</returns>
        public static async Task<GetTransactionInfoResponse> GetTransactionInfoOld(string txid)
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
        public static async Task<string> GetTransactionInternal(string txid, string mode, GetTransactionInfoResponse? txinfo = null)
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
                
                var data = string.Empty;
                if (mode == "sender")
                {
                    data = txinfo?.Vin.ToList()[0]?.PreviousOutput?.Addresses.ToList()[0];
                }
                else if (mode == "receiver" )
                {
                    data = txinfo?.Vout.ToList()[0]?.ScriptPubKey.Addresses.ToList()[0];
                }
                                
                return data ?? string.Empty;
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
        public static async Task<string> GetTransactionSender(string txid, GetTransactionInfoResponse? txinfo = null)
        {
            return await GetTransactionInternal(txid, "sender", txinfo);
        }

        /// <summary>
        /// Get transaction sender.
        /// </summary>
        /// <param name="txid">tx id hash</param>
        /// <param name="txinfo">if you already have txinfo object</param>
        /// <returns>Sender address</returns>
        public static async Task<string> GetTransactionReceiver(string txid, GetTransactionInfoResponse? txinfo = null)
        {
            return await GetTransactionInternal(txid, "receiver", txinfo);
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
                return null;

            try
            {
                GetTokenMetadataResponse? tokeninfo = null;
                if (TokenMetadataCache.TryGetValue(tokenId, out var ti))
                {
                    tokeninfo = ti;
                }
                else
                {
                    //tokeninfo = await GetClient().GetTokenMetadataAsync(tokenId, 0);
                    var client = new HttpClient();
                    var url = $"{NewAPIAddress.Trim('/')}/api/GetTokenMetadata/{tokenId}";
                    var res = await client.GetStringAsync(url);
                    if (!string.IsNullOrEmpty(res))
                    {
                        tokeninfo = JsonConvert.DeserializeObject<GetTokenMetadataResponse>(res);
                        if (tokeninfo != null)
                        {
                            if (TurnOnCache)
                                TokenMetadataCache.TryAdd(tokenId, tokeninfo);
                            return tokeninfo;
                        }
                    }
                }

                if (tokeninfo != null)
                    return tokeninfo;
                else
                    return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load token metadata. " + ex.Message);
                return new GetTokenMetadataResponse();
            }
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
                    try
                    {
                        var info = await GetTokenMetadata(tok);
                        if (info != null)
                            TokensInfo.Add(tok, info);
                    }
                    catch(Exception ex)
                    {
                        await Console.Out.WriteLineAsync("Cannot load token info." + ex.Message);
                    }
                }
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
            
            TokenSupplyDto t = new TokenSupplyDto();
            try
            {
                try
                {
                    var info = await GetTokenMetadata(tokenId);
                    t.TokenSymbol = info.MetadataOfIssuance?.Data.TokenName ?? string.Empty;

                    var tu = new tokenUrlCarrier();
                    if (TokenMetadataCache.TryGetValue(t.TokenId, out var tokcache))
                        tu = tokcache.MetadataOfIssuance?.Data.Urls?.FirstOrDefault();
                    else
                        tu = info.MetadataOfIssuance?.Data.Urls?.FirstOrDefault();

                    if (tu != null)
                        t.ImageUrl = tu.url.Replace("https://ntp1-icons.ams3.digitaloceanspaces.com", "https://ntp1-icons.nebl.io");
                    else if (tu == null && t.TokenId == VENFTTokenId)
                        t.ImageUrl = VENFTImageLink;

                    t.TokenSymbol = info.TokenName;
                    t.TokenId = tokenId;
                    return t;
                }
                catch (Exception ex)
                {
                    await Console.Out.WriteLineAsync("Cannot get token metadata info for token Id : " + tokenId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load token metadata. " + ex.Message);
            }
            return new TokenSupplyDto();
        }

        /// <summary>
        /// check actual supply for minting on some address. It is just for VENFT tokens now. 
        /// Function will also load token metadta if it has not loaded yet.
        /// </summary>
        /// <param name="address">address which has utxos</param>
        /// <param name="tokenId">Specify the tokenId</param>
        /// <param name="addressinfo">if you have already loaded address info with utxo list provide it to prevent unnecessary API requests</param>
        /// <returns></returns>
        public static async Task<(double, GetTokenMetadataResponse)> GetActualMintingSupply(string address, string tokenId, GetAddressInfoResponse? addressinfo)
        {
            var res = await GetAddressTokensUtxos(address, addressinfo);
            var utxos = new List<Utxos>();
            foreach (var r in res)
            {
                var toks = r.Tokens.ToArray()?[0];
                if (toks != null && toks.Amount > 1 && toks.TokenId == tokenId)
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
                                TokenSymbol = info.MetadataOfIssuance?.Data.TokenName ?? string.Empty,
                                TokenId = toks.TokenId
                            };

                            var tu = new tokenUrlCarrier();

                            if (TokenMetadataCache.TryGetValue(toks.TokenId, out var tokcache))
                                tu = tokcache.MetadataOfIssuance?.Data.Urls?.FirstOrDefault();
                            else
                                tu = info.MetadataOfIssuance?.Data.Urls?.FirstOrDefault();

                            if (tu != null)
                                t.ImageUrl = tu.url.Replace("https://ntp1-icons.ams3.digitaloceanspaces.com", "https://ntp1-icons.nebl.io");
                            else if (tu == null && t.TokenId == VENFTTokenId)
                                t.ImageUrl = VENFTImageLink;

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
                        if (utxs != null && utxs.Count > 0)
                        {
                            var us = utxs.ToList();
                            resp.Add(new TokenOwnerDto()
                            {
                                Address = h.Address,
                                ShortenAddress = shadd,
                                AmountOfNFTs = us.Count,
                                AmountOfTokens = (int)(h.Amount ?? 0)
                            });
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

        /// <summary>
        /// Return list of the transactions in the block
        /// </summary>
        /// <param name="addr"></param>
        /// <returns></returns>
        public static async Task<GetTxsResponse> GetTxsAsync(string addr, string block, double? pageNum = 100)
        {
            if (string.IsNullOrEmpty(addr) && string.IsNullOrEmpty(block))
            {
                return new GetTxsResponse();
            }

            var address = await GetClient().GetTxsAsync(addr, block, pageNum);
            return address;
        }

        /// <summary>
        /// Return Block info object
        /// </summary>
        /// <param name="block">Hash of block</param>
        /// <returns></returns>
        public static async Task<GetBlockResponse> GetBlock(string block)
        {
            if (string.IsNullOrEmpty(block))
            {
                return new GetBlockResponse();
            }

            var address = await GetClient().GetBlockAsync(block);
            return address;
        }
    }
}
