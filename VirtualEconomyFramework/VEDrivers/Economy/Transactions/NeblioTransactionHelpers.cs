using log4net;
using NBitcoin;
using NBitcoin.Altcoins;
using Neblio.RestApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Economy.DTO;
using VEDrivers.Economy.Tokens;

namespace VEDrivers.Economy.Transactions
{
    public static class NeblioTransactionHelpers
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static HttpClient httpClient = new HttpClient();
        private static IClient _client;
        private static NeblioCryptocurrency NeblioCrypto = new NeblioCryptocurrency(false);
        public static QTWalletRPCClient qtRPCClient { get; set; }


        public static ITransaction TransactionInfo(TransactionTypes type, string txid, string sourceAddress, object obj)
        {
            _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            var transaction = TransactionInfoAsync(_client, type, txid, sourceAddress);
            return transaction.GetAwaiter().GetResult();
        }

        public static async Task<ITransaction> TransactionInfoAsync(IClient client, TransactionTypes type, string txid, string sourceAddress = "")
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            }

            GetTransactionInfoResponse info = null;

            try
            {
                if (client != null)
                    info = await client.GetTransactionInfoAsync(txid);
                else
                    info = await _client.GetTransactionInfoAsync(txid);
            }
            catch(Exception ex)
            {
                //todo, usually wrong parsing of json
                throw new Exception("Cannot load transaction info from Neblio API", ex);
            }

            if (info == null)
                return null;

            ITransaction transaction = TransactionFactory.GetTransaction(type, txid, string.Empty, string.Empty, true);

            DateTime time;

            if (info.Time != null)
            {
                time = TimeHelpers.UnixTimestampToDateTime((double)info.Time);
                transaction.TimeStamp = time;
            }

            var tokenin = new Tokens2();
            var addrfrom = string.Empty;
            try
            {
                addrfrom = info.Vin?.FirstOrDefault().PreviousOutput.Addresses.FirstOrDefault();
                transaction.From.Add(addrfrom);
                tokenin = info.Vin?.FirstOrDefault().Tokens?.ToList()?.FirstOrDefault();
                transaction.Confirmations = Convert.ToInt32((double)info.Confirmations);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            if (tokenin != null)
            {
                IToken tokeninfo = null;

                try
                {
                    if (client != null)
                        tokeninfo = await TokenMetadataAsync(TokenTypes.NTP1, tokenin.TokenId, txid);
                    else
                        tokeninfo = await TokenMetadataAsync(TokenTypes.NTP1, tokenin.TokenId, txid);
                }
                catch (Exception ex)
                {
                    //todo, usually wrong parsing of json
                    throw new Exception("Cannot load token info from Neblio API", ex);
                }

                if (tokenin != null)
                {
                    transaction.VinTokens.Add(new NeblioNTP1Token(string.Empty)
                    {
                        ActualBalance = tokenin.Amount,
                        Id = tokenin.TokenId,
                        MaxSupply = tokeninfo.MaxSupply,
                        Symbol = tokeninfo.Symbol,
                        Name = tokeninfo.Name,
                        IssuerName = tokeninfo.IssuerName,
                        Metadata = tokeninfo.Metadata,
                        ImageUrl = tokeninfo.ImageUrl,
                        MetadataAvailable = tokeninfo.MetadataAvailable,
                        TimeStamp = transaction.TimeStamp,
                        TxId = info.Txid
                    }); ;
                }

                var addrto = string.Empty;
                var txinfodetails = info.Vout?.ToList()?[0];
                if (txinfodetails != null)
                {
                    transaction.Amount = (double)txinfodetails.Value / NeblioCrypto.FromSatToMainRatio;
                }

                var tokenout = info.Vout?.ToList()[0]?.Tokens?.ToList()?.FirstOrDefault();
                if (tokenout == null)
                {
                    tokenout = info.Vout?.ToList()[1]?.Tokens?.ToList()?.FirstOrDefault();
                    if (tokenout != null)
                        addrto = info.Vout?.ToList()[1]?.ScriptPubKey?.Addresses?.ToList().FirstOrDefault();
                    else
                        return null;
                    
                    //transaction.Direction = TransactionDirection.Outgoing;
                }
                else
                {
                    //transaction.Direction = TransactionDirection.Incoming;
                    addrto = info.Vout?.ToList()[0]?.ScriptPubKey?.Addresses?.ToList().FirstOrDefault();
                }

                if (!string.IsNullOrEmpty(addrfrom) && !string.IsNullOrEmpty(sourceAddress) && !string.IsNullOrEmpty(addrto))
                {
                    if (addrfrom == sourceAddress)
                    {
                        transaction.Direction = TransactionDirection.Outgoing;
                    }
                    else if (addrto == sourceAddress)
                    {
                        transaction.Direction = TransactionDirection.Incoming;
                        transaction.To.Add(sourceAddress);
                    }
                }
                else
                {
                    transaction.Direction = TransactionDirection.Incoming;
                    transaction.To.Add(sourceAddress);
                }

                transaction.To.Add(addrto);
                if (tokenout != null)
                {
                    transaction.VoutTokens.Add(new NeblioNTP1Token(string.Empty)
                    {
                        ActualBalance = tokenout.Amount,
                        Id = tokenout.TokenId,
                        MaxSupply = tokeninfo.MaxSupply,
                        Symbol = tokeninfo.Symbol,
                        Name = tokeninfo.Name,
                        IssuerName = tokeninfo.IssuerName,
                        ImageUrl = tokeninfo.ImageUrl,
                        Metadata = tokeninfo.Metadata,
                        MetadataAvailable = tokeninfo.MetadataAvailable,
                        TimeStamp = transaction.TimeStamp,
                        TxId = info.Txid
                    });
                }
            }
            else // not token tx
            {
                var txinfodetails = info.Vout?.ToList()?[0];
                if (txinfodetails != null)
                {
                     transaction.Amount = (double)txinfodetails.Value / NeblioCrypto.FromSatToMainRatio;
                }

                var addrto = string.Empty;
                addrto = info.Vout?.ToList()[0]?.ScriptPubKey?.Addresses?.ToList().FirstOrDefault();

                if (!string.IsNullOrEmpty(addrfrom) && !string.IsNullOrEmpty(sourceAddress) && !string.IsNullOrEmpty(addrto))
                {
                    if (addrfrom == sourceAddress)
                    {
                        transaction.Direction = TransactionDirection.Outgoing;
                    }
                    else if (addrto == sourceAddress)
                    {
                        transaction.Direction = TransactionDirection.Incoming;
                        transaction.To.Add(sourceAddress);
                    }
                }
                else
                {
                    transaction.Direction = TransactionDirection.Incoming;
                    transaction.To.Add(sourceAddress);
                }
            }

            return transaction;
        }

        public static IToken TokenMetadata(TokenTypes type, string tokenid, string txid, object obj)
        {
            _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            var transaction = TokenMetadataAsync(type, tokenid, txid);
            return transaction.GetAwaiter().GetResult();
        }

        private class tokenUrlCarrier
        {
            public string name { get; set; } = string.Empty;
            public string url { get; set; } = string.Empty;
            public string mimeType { get; set; } = string.Empty;
        }

        public static async Task<IToken> TokenMetadataAsync(TokenTypes type, string tokenid, string txid)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            }

            IToken token = new NeblioNTP1Token("");

            GetTokenMetadataResponse info = new GetTokenMetadataResponse();
            try
            {

                if (string.IsNullOrEmpty(txid))
                {
                    info = await _client.GetTokenMetadataAsync(tokenid, 0);
                }
                else
                {
                    info = await _client.GetTokenMetadataOfUtxoAsync(tokenid, txid, 0);
                }
            }
            catch(Exception ex)
            {
                throw new Exception("Cannot load token info from Neblio API", ex);
            }

            token.MaxSupply = info.InitialIssuanceAmount;
            token.Symbol = info.MetadataOfIssuance.Data.TokenName;
            token.Name = info.MetadataOfIssuance.Data.Description;
            token.IssuerName = info.MetadataOfIssuance.Data.Issuer;
            token.Id = tokenid;
            token.TxId = txid;
            var tus = JsonConvert.DeserializeObject<List<tokenUrlCarrier>>(JsonConvert.SerializeObject(info.MetadataOfIssuance.Data.Urls));

            if (info.MetadataOfUtxo != null)
            {
                if (info.MetadataOfUtxo.UserData.Meta.Count > 0)
                {
                    token.MetadataAvailable = true;

                    foreach (var o in info.MetadataOfUtxo.UserData.Meta)
                    {
                        var od = JsonConvert.DeserializeObject<IDictionary<string, string>>(o.ToString());
                       
                        if (od != null)
                        {
                            if (od.Count > 0)
                            {
                                var of = od.First();
                                if (!token.Metadata.ContainsKey(of.Key))
                                    token.Metadata.Add(of.Key, of.Value);
                                //Console.WriteLine("metadataName: " + of.Key.ToString());
                                //Console.WriteLine("metadataContent: " + of.Value.ToString());
                            }
                        }
                    }
                }
                else
                {
                    token.MetadataAvailable = false;
                }
            }

            var tu = tus.FirstOrDefault();
            if (tu != null)
            {
                token.ImageUrl = tu.url;
            }
            
            return token;
        }

        public static async Task<string> SendNTP1Token(SendTokenTxData data)
        {
            var res = "ERROR";

            if (!qtRPCClient.IsConnected)
                qtRPCClient.InitClients();

            if (qtRPCClient.IsConnected)
            {
                // load metadata from dictionary to the required shape
                JArray array = new JArray();

                if (data.Metadata != null)
                {
                    foreach (var d in data.Metadata)
                    {
                        var job = new JObject();
                        job[d.Key] = d.Value;

                        array.Add(job);
                    }
                }

                JObject jo = new JObject();
                JObject j = new JObject();
                j["meta"] = array;
                jo["userData"] = j;

                var mtds = jo.ToString(Formatting.None, null);

                var i = 4;
                if (data.Metadata.Count == 0)
                    i = 3;

                string[] parameters = new string[i];

                parameters[0] = data.ReceiverAddress;
                parameters[1] = Convert.ToInt32(data.Amount).ToString();
                parameters[2] = data.Symbol;

                if (data.Metadata.Count > 0)
                {
                    parameters[3] = mtds;
                }

                res = await qtRPCClient.RPCLocalCommandSplitedAsync("sendntp1toaddress", parameters);
            }

            return res;
        }


        private class TokenTxRPCControlerResponse
        {
            public SignResultDto result { get; set; }
            public string id { get; set; }

        }
        private class SignResultDto
        {
            public string hex { get; set; }
            public bool complete { get; set; }
        }
        public static async Task<string> SendNTP1TokenAPI(SendTokenTxData data, double fee = 20000, bool isItMintNFT = false, bool isNFTtx = false)
        {
            var res = "ERROR";
            var dto = new SendTokenRequest();

            dto.Metadata = new Metadata2();
            dto.Metadata.UserData = new UserData3();
            dto.Metadata.UserData.Meta = new List<JObject>();

            if (!qtRPCClient.IsConnected)
                qtRPCClient.InitClients();

            if ((EconomyMainContext.WorkWithQTRPC && qtRPCClient.IsConnected) || !EconomyMainContext.WorkWithQTRPC)
            {
                if (data.Metadata != null)
                {
                    
                    dto.Fee = fee;

                    dto.Flags = new Flags2() { SplitChange = true };

                    // neblio API accept sendUtxo (both token and nebl) or from and it will find some unspecified spendable
                    // if you correct format of sendUtxo is hash:index for example for token tx you must send this
                    // 
                    // "4213dfd34dca0b691a5c0e41080c098681e6f1be45935e7b36cad3559fe6b446:0" - token utxo
                    // "255b33cfe724800bf158b63985ed2cafc39db897d3dfb475b7535cdec0b69923:1" - nebl utxo
                    //
                    // to simplify this in the function is used search for nebl utxo! it means if you want to send just one token you will fill just one utxo of this token

                    
                    dto.Sendutxo = new List<string>();

                    if (data.sendUtxo?.Count != 0)
                    {
                        if (!isItMintNFT)
                        {
                            foreach (var it in data.sendUtxo)
                            {
                                var itt = it;
                                if (it.Contains(":"))
                                    itt = it.Split(":")[0];

                                (bool,double) voutstate;

                                if (!isNFTtx)
                                {
                                    voutstate = await ValidateNeblioTokenUtxo(data.SenderAddress, itt);
                                }
                                else
                                {
                                   voutstate = await ValidateOneTokenNFTUtxo(data.SenderAddress, itt);
                                }

                                if (voutstate.Item1)
                                    dto.Sendutxo.Add(itt + ":" + voutstate.Item2); // copy received utxos and add item number of vout after validation
                            }
                        }
                        else
                        {
                            // if isItMing flag is set the utxo was already found and checked by internal function
                            foreach (var u in data.sendUtxo)
                            {
                                dto.Sendutxo.Add(u);
                            }
                        }
                    }
                    else
                    {
                        // if not utxo provided, check the un NFT tokens sources. These with more than 1 token
                        
                        var utxs = await FindUtxoForMintNFT(data.SenderAddress, data.Id, (int)data.Amount);

                        foreach (var u in utxs)
                        {
                            dto.Sendutxo.Add(u.Txid + ":" + u.Index);
                        }
                    }


                    if (dto.Sendutxo.Count == 0)
                        throw new Exception("Cannot find any spendable token source Utxos");

                    if (isItMintNFT && dto.Sendutxo.Count > 0)
                        data.Metadata.Add(new KeyValuePair<string, string>("SourceUtxo", dto.Sendutxo.ToArray()?[0].Split(":")[0]));

                    // need some neblio too
                    // to be sure to have last tx request it from neblio network
                    // set some minimum amount
                    var utxos = await GetAddressNeblUtxo(data.SenderAddress, 0.0001, 2*0.0001); // to be sure find at leas 2 times of expected fee
                    // create raw Tx with NBitcoin
                    NBitcoin.Altcoins.Neblio.NeblioTransaction neblUtxo = null;
                    if (utxos == null || utxos.Count == 0)
                    {
                        throw new Exception("Cannot send transaction, cannot load sender nebl utxo!");
                    }

                    var found = false;
                    if (!string.IsNullOrEmpty(data.NeblUtxo))
                    {
                        foreach(var u in utxos)
                        {
                            if (u.Txid == data.NeblUtxo)
                            {
                                dto.Sendutxo.Add(u.Txid + ":" + u.Index);
                                found = true;
                                break;
                            }
                        }
                    }

                    if (!found && !data.SendEvenNeblUtxoNotFound && !string.IsNullOrEmpty(data.NeblUtxo))
                        throw new Exception("Input Neblio Utxo is not spendable!");

                    if (!found)
                    {
                        if (string.IsNullOrEmpty(data.NeblUtxo))
                        {
                            foreach (var u in utxos)
                            {
                                if ((u.Value * NeblioCrypto.FromSatToMainRatio) >= 2 * fee)
                                {
                                    dto.Sendutxo.Add(u.Txid + ":" + u.Index);
                                    break;
                                }
                            }
                        }
                    }

                    foreach (var d in data.Metadata)
                    {
                        var obj = new JObject();
                        obj[d.Key] = d.Value;

                        dto.Metadata.UserData.Meta.Add(obj);
                    }

                    /* If you want to find possible utxos by neblio API. This can send away the NFT if you want to send just one token!
                    else
                    {
                        dto.From = new List<string>() { data.SenderAddress };
                    }
                    */

                    dto.To = new List<To>()
                    {
                        new To()
                        {
                            Address = data.ReceiverAddress,
                             Amount = data.Amount,
                              TokenId = data.Id
                        }
                    };

                    try
                    {
                        // create raw tx
                        var str = JsonConvert.SerializeObject(dto);

                        var hexToSign = await SendRawNTP1TxAsync(TransactionTypes.Neblio, dto);

                        if (!string.IsNullOrEmpty(hexToSign))
                        {
                            if (EconomyMainContext.WorkWithQTRPC)
                            {
                                // sign tx
                                res = await qtRPCClient.RPCLocalCommandSplitedAsync("signrawtransaction", new string[] { hexToSign });

                                var parsedRes = JsonConvert.DeserializeObject<TokenTxRPCControlerResponse>(res);

                                if (parsedRes != null)
                                {
                                    if (parsedRes.result.complete)
                                    {
                                        var bdto = new BroadcastTxRequest()
                                        {
                                            TxHex = parsedRes.result.hex
                                        };

                                        var txid = await BroadcastNTP1TxAsync(TransactionTypes.Neblio, bdto);

                                        res = txid;
                                    }

                                }
                            }
                            else
                            {
                                var key = string.Empty;
                                if (EconomyMainContext.Accounts.TryGetValue(data.SenderAddress, out var account))
                                {
                                    if (account.AccountKey != null)
                                    {
                                        if (account.AccountKey.IsLoaded)
                                        {
                                            if (account.AccountKey.IsEncrypted && string.IsNullOrEmpty(data.Password) && !account.AccountKey.IsPassLoaded)
                                            {
                                                throw new Exception("Cannot send token transaction. Password is not filled and key is encrypted or unlock account!");
                                            }
                                            else if (!account.AccountKey.IsEncrypted)
                                            {
                                                key = account.AccountKey.GetEncryptedKey();
                                            }
                                            else if (account.AccountKey.IsEncrypted && (!string.IsNullOrEmpty(data.Password) || account.AccountKey.IsPassLoaded))
                                            {
                                                if (account.AccountKey.IsPassLoaded)
                                                    key = account.AccountKey.GetEncryptedKey(string.Empty);
                                                else
                                                    key = account.AccountKey.GetEncryptedKey(data.Password);
                                            }

                                            if (string.IsNullOrEmpty(key))
                                                throw new Exception("Cannot send token transaction. Password is not filled and key is encrypted or unlock account!");

                                        }
                                    }
                                }

                                var network = NBitcoin.Altcoins.Neblio.Instance.Mainnet;
                                BitcoinSecret keyfromFile = null;
                                BitcoinAddress addressForTx = null;

                                if (!string.IsNullOrEmpty(key))
                                {
                                    try
                                    {
                                        keyfromFile = network.CreateBitcoinSecret(key);
                                        addressForTx = keyfromFile.GetAddress(ScriptPubKeyType.Legacy);
                                    }
                                    catch (Exception ex)
                                    {
                                        log.Error("Cannot send token transaction!", ex);
                                        //Console.WriteLine($"Cannot send token transaction, cannot create keys");
                                        throw new Exception("Cannot send token transaction. cannot create keys!");
                                    }

                                    try
                                    {
                                        if (Transaction.TryParse(hexToSign, network, out var transaction))
                                        {
                                            //var txrespToken = NBitcoin.Altcoins.Neblio.NeblioTransaction.GetNeblioTransaction(transaction.Inputs[0].PrevOut.Hash.ToString()); // not accessible in NBitcoin nuget now
                                            //var txrespNebl = NBitcoin.Altcoins.Neblio.NeblioTransaction.GetNeblioTransaction(transaction.Inputs[1].PrevOut.Hash.ToString()); // not accessible in NBitcoin nuget now
                                            var txrespTokenHex = await GetTxHex(transaction.Inputs[0].PrevOut.Hash.ToString());
                                            var txrespNeblHex = await GetTxHex(transaction.Inputs[1].PrevOut.Hash.ToString());

                                            // there is still some issue in parsing json from api. now need to reparse the hex.

                                            if (!Transaction.TryParse(txrespTokenHex, network, out var tx1)) // token
                                            {
                                                log.Error("Cannot load previous token transaction!");
                                                Console.WriteLine($"Cannot load previous token transaction!");
                                                return string.Empty;
                                            }

                                            if (!Transaction.TryParse(txrespNeblHex, network, out var tx2)) // nebl
                                            {
                                                log.Error("Cannot load previous token transaction!");
                                                Console.WriteLine($"Cannot load previous token transaction!");
                                                return string.Empty;
                                            }

                                            // load list of input coins for the source address
                                            // some of them must be spendable
                                            List<ICoin> list = new List<ICoin>();
                                            foreach (var to in tx1.Outputs)
                                            {
                                                if (to.ScriptPubKey == addressForTx.ScriptPubKey)
                                                    if (to.Value.Satoshi == 10000) // token tx
                                                        foreach (var d in dto.Sendutxo)
                                                        {
                                                            if (d.Split(":")[1] == (tx1.Outputs.IndexOf(to).ToString()))
                                                                list.Add(new Coin(tx1, Convert.ToUInt32(d.Split(":")[1])));
                                                        }
                                            }

                                            foreach (var to in tx2.Outputs)
                                            {
                                                if (to.ScriptPubKey == addressForTx.ScriptPubKey)
                                                    if (to.Value.Satoshi > 10000) // nebl 10000 is just tokens or fee, everything should be bigger thant this in neblio
                                                        foreach (var d in dto.Sendutxo)
                                                        {
                                                            if (d.Split(":")[1] == (tx2.Outputs.IndexOf(to).ToString()))
                                                                list.Add(new Coin(tx2, Convert.ToUInt32(d.Split(":")[1])));
                                                        }
                                            }

                                            transaction.Inputs[0].ScriptSig = addressForTx.ScriptPubKey;
                                            transaction.Inputs[1].ScriptSig = addressForTx.ScriptPubKey;

                                            // just for skpping in debug
                                            var a = false;
                                            if (a)
                                                return null;

                                            transaction.Sign(keyfromFile, list);

                                            var txhex = transaction.ToHex();

                                            if (!string.IsNullOrEmpty(txhex))
                                            {
                                                var bdto = new BroadcastTxRequest()
                                                {
                                                    TxHex = txhex
                                                };

                                                var txid = await BroadcastNTP1TxAsync(TransactionTypes.Neblio, bdto);

                                                res = txid;
                                            }
                                        }
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Exception during loading inputs or signing tx: {ex}");
                                        throw new Exception("Exception during loading inputs or signing tx!");
                                    }
                                }
                                else
                                {
                                    throw new Exception("Key wasnt provided. Cannot sign transaction without key!");
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message.ToString().Contains("Cannot send token transaction. Password is not filled and key is encrypted or unlock account!"))
                            throw new Exception(ex.Message.ToString());

                        if (ex.Message.ToString().Contains("Key wasnt provided. Cannot sign transaction without key!"))
                            throw new Exception(ex.Message.ToString());

                        if (ex.Message.ToString().Contains("Cannot send token transaction. cannot create keys!"))
                            throw new Exception(ex.Message.ToString());

                        log.Error("Cannot send token transaction!", ex);
                        throw new Exception(ex.Message);
                    }
                }
            }

            return res;
        }

        public static async Task<string> SendRawNTP1TxAsync(TransactionTypes type, SendTokenRequest data)
        {
            if (type != TransactionTypes.Neblio)
                return string.Empty;

            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            }

            var info = await _client.SendTokenAsync(data);

            return info.TxHex;
        }

        public static async Task<string> BroadcastNTP1TxAsync(TransactionTypes type, BroadcastTxRequest data)
        {
            if (type != TransactionTypes.Neblio)
                return string.Empty;

            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            }

            var info = await _client.BroadcastTxAsync(data);

            return info.Txid;
        }

        public static async Task<string> MintNFTToken(MintNFTData data, double fee = 30000)
        {
            try
            {
                var utxos = new List<string>();
                var mainUtxo = string.Empty;

                if (!string.IsNullOrEmpty(data.sendUtxo))
                {
                    utxos.Add(data.sendUtxo);
                }
                else
                {
                    var utxs = await FindUtxoForMintNFT(data.SenderAddress, "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8", 1);

                    if (utxs != null)
                    {
                        mainUtxo = utxs.FirstOrDefault()?.Txid;

                        foreach (var u in utxs)
                        {
                            utxos.Add(u.Txid + ":" + u.Index);
                        }
                    }
                    else
                    {
                        throw new Exception("Dont have any spendable source tokens to mint new NFT!");
                    }
                }

                //data.Metadata.Add(new KeyValuePair<string, string>("SourceUtxo", mainUtxo));

                data.Metadata.Add(new KeyValuePair<string, string>("NFT FirstTx", "true"));

                var dto = new SendTokenTxData()
                {
                    Amount = 1,
                    Id = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8",
                    Symbol = "VENFT",
                    Metadata = data.Metadata,
                    Password = data.Password,
                    SenderAddress = data.SenderAddress,
                    ReceiverAddress = data.SenderAddress,
                    sendUtxo = utxos,
                    SendEvenNeblUtxoNotFound = true,
                    UseRPCPrimarily = false
                };

                var resp = await SendNTP1TokenAPI(dto, fee: fee, isItMintNFT: true);

                return resp;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public static async Task<string> SendNeblioTransactionAPI(SendTxData data, double fee = 10000)
        {
            var res = "ERROR";

            if (EconomyMainContext.WorkWithQTRPC)
            {
                if (!qtRPCClient.IsConnected)
                    qtRPCClient.InitClients();
            }

            try
            {
                var key = string.Empty;
                if (EconomyMainContext.Accounts.TryGetValue(data.SenderAddress, out var account))
                {
                    if (!EconomyMainContext.WorkWithQTRPC)
                    {
                        if (account.AccountKey != null)
                        {
                            if (account.AccountKey.IsLoaded)
                            {
                                if (account.AccountKey.IsEncrypted && string.IsNullOrEmpty(data.Password) && !account.AccountKey.IsPassLoaded)
                                {
                                    throw new Exception("Cannot send transaction. Password is not filled and key is encrypted or unlock account!");
                                }
                                else if (!account.AccountKey.IsEncrypted)
                                {
                                    key = account.AccountKey.GetEncryptedKey();
                                }
                                else if (account.AccountKey.IsEncrypted && (!string.IsNullOrEmpty(data.Password) || account.AccountKey.IsPassLoaded))
                                {
                                    if (account.AccountKey.IsPassLoaded)
                                        key = account.AccountKey.GetEncryptedKey(string.Empty);
                                    else
                                        key = account.AccountKey.GetEncryptedKey(data.Password);
                                }

                                if (string.IsNullOrEmpty(key))
                                    throw new Exception("Cannot send transaction. Password is not filled and key is encrypted or unlock account!");

                            }
                        }
                    }
                }

                var network = NBitcoin.Altcoins.Neblio.Instance.Mainnet;
                BitcoinSecret keyfromFile = null;
                BitcoinAddress addressForTx = null;

                if (!EconomyMainContext.WorkWithQTRPC)
                {
                    if (string.IsNullOrEmpty(key))
                    {
                        throw new Exception("Key wasnt provided. Cannot sign transaction without key!");
                    }

                    try
                    {
                        keyfromFile = network.CreateBitcoinSecret(key);
                        addressForTx = keyfromFile.GetAddress(ScriptPubKeyType.Legacy);
                    }
                    catch (Exception ex)
                    {
                        log.Error("Cannot send transaction!", ex);
                        //Console.WriteLine($"Cannot send token transaction, cannot create keys");
                        throw new Exception("Cannot send transaction. cannot create keys!");
                    }


                }

                try
                {
                    BitcoinAddress recaddr = null;
                    try
                    {
                        recaddr = BitcoinAddress.Create(data.ReceiverAddress, network);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception("Cannot send transaction. cannot create receiver address!");
                    }

                    // to be sure to have last tx request it from neblio network
                    var utxos = await GetAddressNeblUtxo(data.SenderAddress, ((2 * fee)/NeblioCrypto.FromSatToMainRatio), (data.Amount * NeblioCrypto.FromSatToMainRatio));
                    // create raw Tx with NBitcoin
                    NBitcoin.Altcoins.Neblio.NeblioTransaction neblUtxo = null;
                    if (utxos == null)
                    {
                        throw new Exception("Cannot send transaction, cannot load sender address history!");
                    }

                    // create template for new tx from last one
                    var transaction = new NBitcoin.Altcoins.Neblio.NeblioTransaction(network.Consensus.ConsensusFactory);//neblUtxo.Clone();
                    List<ICoin> list = new List<ICoin>();

                    int inputIndex = 0;
                    // there is still some issue in parsing json from api. now need to reparse the hex.
                    foreach (var utxo in utxos)
                    {
                        //neblUtxo = NBitcoin.Altcoins.Neblio.NeblioTransaction.GetNeblioTransaction(utxo.Txid); // not accessible in NBitcoin nuget now
                        var neblUtxoHex = await GetTxHex(utxo.Txid);

                        if (!string.IsNullOrEmpty(neblUtxoHex))
                        {
                            //if (!Transaction.TryParse(neblUtxo.Hex, network, out var txin)) // nebl
                            if (!Transaction.TryParse(neblUtxoHex, network, out var txin)) // nebl
                            {
                                log.Error("Cannot load previous token transaction!");
                                Console.WriteLine($"Cannot load previous token transaction!");
                                return string.Empty;
                            }

                            transaction.Version = 1;
                            //create new input
                            //transaction.Inputs.Clear();
                            transaction.Inputs.Add(txin, (int)utxo.Index); // last output is return

                            // load list of input coins for the source address
                            // some of them must be spendable

                            
                            foreach (var to in txin.Outputs)
                            {
                                if (to.ScriptPubKey == addressForTx.ScriptPubKey)
                                    if (to.Value.Satoshi != 10000)
                                        list.Add(new Coin(txin, (uint)utxo.Index));
                            }
                            if (transaction.Inputs.Count > 0)
                            {
                                transaction.Inputs[inputIndex].ScriptSig = addressForTx.ScriptPubKey;
                                transaction.Inputs[inputIndex].PrevOut.N = (uint)(utxo.Index);
                            }
                            else
                            {
                                throw new Exception("Cannot send transaction, cannot find spendable coin!");
                            }

                            inputIndex++;
                        }
                    }

                    var allNeblCoins = 0.0;
                    foreach (var u in utxos)
                        allNeblCoins += (double)u.Value;

                    var amountinSat = Convert.ToUInt64(data.Amount * NeblioCrypto.FromSatToMainRatio);
                    var balanceinSat = Convert.ToUInt64(allNeblCoins);
                    var diffinSat = balanceinSat - amountinSat - Convert.ToUInt64(fee);

                    //var feescriptpubkey = new Script("OP_RETURN 4e5403100100010000001d789cab562a2d4e2d72492c4954b2aa56ca4d05d1d1b1b5b5006ab30885");
                    // create outputs

                    transaction.Outputs.Add(new Money(amountinSat), recaddr.ScriptPubKey); // send to receiver required amount
                    transaction.Outputs.Add(new Money(diffinSat), addressForTx.ScriptPubKey); // get diff back to sender address
                                                                                              //transaction.Outputs.Add(new Money(Convert.ToUInt64(fee)), feescriptpubkey); // fee

                    // sign tx
                    var txhex = string.Empty;
                    if (EconomyMainContext.WorkWithQTRPC)
                    {
                        // sign tx with QT wallet
                        res = await qtRPCClient.RPCLocalCommandSplitedAsync("signrawtransaction", new string[] { transaction.ToHex() });

                        var parsedRes = JsonConvert.DeserializeObject<TokenTxRPCControlerResponse>(res);

                        if (parsedRes != null)
                        {
                            if (parsedRes.result.complete)
                            {
                                txhex = parsedRes.result.hex;
                            }
                        }
                    }
                    else
                    {
                        transaction.Sign(keyfromFile, list);
                        txhex = transaction.ToHex();
                    }

                    // broadcast tx
                    if (!string.IsNullOrEmpty(txhex))
                    {
                        var bdto = new BroadcastTxRequest()
                        {
                            TxHex = txhex
                        };

                        var txid = await BroadcastNTP1TxAsync(TransactionTypes.Neblio, bdto);

                        res = txid;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception during loading inputs or signing tx: {ex}");
                }
            }
            catch (Exception ex)
            {
                if (ex.Message.ToString().Contains("Cannot send transaction. Password is not filled and key is encrypted or unlock account!"))
                    throw new Exception(ex.Message.ToString());

                if (ex.Message.ToString().Contains("Key wasnt provided. Cannot sign transaction without key!"))
                    throw new Exception(ex.Message.ToString());

                if (ex.Message.ToString().Contains("Cannot send transaction. cannot create keys!"))
                    throw new Exception(ex.Message.ToString());

                log.Error("Cannot send transaction!", ex);
                throw new Exception(ex.Message);
            }

            return res;
        }


        public static async Task<GetAddressResponse> AddressInfoAsync(string addr)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            }

            var address = await _client.GetAddressAsync(addr);

            /*
            Console.WriteLine($"AddrStr                     = {address.AddrStr                 }   ");
            Console.WriteLine($"Balance                     = {address.Balance                 }   ");
            Console.WriteLine($"BalanceSat                  = {address.BalanceSat              }   ");
            Console.WriteLine($"TotalReceived               = {address.TotalReceived           }   ");
            Console.WriteLine($"TotalReceivedSat            = {address.TotalReceivedSat        }   ");
            Console.WriteLine($"TotalSent                   = {address.TotalSent               }   ");
            Console.WriteLine($"TotalSentSat                = {address.TotalSentSat            }   ");
            Console.WriteLine($"UnconfirmedBalance          = {address.UnconfirmedBalance      }   ");
            Console.WriteLine($"UnconfirmedBalanceSat       = {address.UnconfirmedBalanceSat   }   ");
            Console.WriteLine($"UnconfirmedTxAppearances    = {address.UnconfirmedTxAppearances}   ");
            Console.WriteLine($"TxAppearances               = {address.TxAppearances           }   ");

            foreach (var item in address.AdditionalProperties)
            {
                Console.WriteLine($"Property: Key = {item.Key}, Value = {item.Value}");
            }
            Console.WriteLine();
            */
            return address;
        }

        public static async Task<ICollection<Anonymous>> GetAddressUtxos(string addr)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            }

            var utxos = await _client.GetAddressUtxosAsync(addr);

            return utxos;
        }

        public static async Task<ICollection<Utxos>> GetAddressTokensUtxos(string addr)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            }

            var utxosAll = await _client.GetAddressInfoAsync(addr);
            var utxos = new List<Utxos>();

            if (utxosAll.Utxos != null)
            {
                foreach (var u in utxosAll.Utxos)
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
                _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            }

            var tx = await _client.GetTransactionInfoAsync(txid);

            if (tx != null)
                return tx.Hex;
            else
                return string.Empty;
        }

        public static async Task<ICollection<Utxos>> GetAddressNFTsUtxos(string addr)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            }

            var utxosAll = await _client.GetAddressInfoAsync(addr);
            var utxos = new List<Utxos>();

            if (utxosAll.Utxos != null)
            {
                foreach (var u in utxosAll.Utxos)
                {
                    if (u.Tokens.Count == 1)
                    {
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
                _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            }

            var resp = await _client.SendTxAsync(dto);

            return resp?.Txid;
        }

        public static async Task<List<Utxos>> GetAddressNeblUtxo(string addr, double minAmount = 0.0001, double requiredAmount = 0.0001)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            }


            var addinfo = await _client.GetAddressInfoAsync(addr);
            var utxos = addinfo.Utxos;

            var resp = new List<Utxos>();

            var founded = 0.0;

            utxos = utxos.OrderBy(u => u.Value).Reverse().ToList();

            if (utxos != null)
            {
                foreach(var utx in utxos)
                {
                    if (utx.Blockheight > 0)
                    {
                        if (utx.Tokens.Count == 0)
                        {
                            if (((double)utx.Value) > (minAmount * NeblioCrypto.FromSatToMainRatio))
                            {
                                try
                                {
                                    var tx = await _client.GetTransactionInfoAsync(utx.Txid);

                                    if (tx != null)
                                    {
                                        if (tx.Confirmations > 1 && tx.Blockheight > 0)
                                        {
                                            resp.Add(utx);
                                            founded += (double)utx.Value;
                                            if (founded > requiredAmount)
                                                return resp;
                                        }
                                    }
                                }
                                catch(Exception ex)
                                {
                                    ;
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
                _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            }

            var info = await _client.GetAddressInfoAsync(address);
            
            if (info != null)
            {
                var uts = info.Utxos.Where(u => u.Txid == txid);
                if (uts != null) {
                    foreach (var ut in uts)
                    {
                        if (ut.Blockheight > 0)
                        {
                            if (ut != null)
                            {
                                var tx = await _client.GetTransactionInfoAsync(ut.Txid);
                                if (tx != null)
                                {
                                    if (tx.Confirmations > 1 && tx.Blockheight > 0)
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

        public static async Task<(bool, double)> ValidateNeblioTokenUtxo(string address, string txid, bool isMint = false)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
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
                                if ((toks[0].Amount > 0 && !isMint) || (toks[0].Amount > 1 && isMint))
                                { 
                                    var tx = await _client.GetTransactionInfoAsync(ut.Txid);
                                    if (tx != null)
                                    {
                                        if (tx.Confirmations > 1 && tx.Blockheight > 0)
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

            return (false, 0);
        }

        public static async Task<(bool, double)> ValidateOneTokenNFTUtxo(string address, string txid)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
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
                            if (ut.Tokens.Count > 0)
                            {
                                var toks = ut.Tokens.ToArray();
                                if (toks[0].Amount == 1)
                                {
                                    var tx = await _client.GetTransactionInfoAsync(ut.Txid);
                                    if (tx != null)
                                    {
                                        if (tx.Confirmations > 1 && tx.Blockheight > 0)
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

            return (false, 0);
        }

        public static async Task<List<Utxos>> FindUtxoForMintNFT(string addr, string tokenId, int numberToMint = 1, double oneTokenSat = 10000)
        {
            if (_client == null)
            {
                _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
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
                            if (utx.Value == oneTokenSat)
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
                                                if (tx.Confirmations > 1 && tx.Blockheight > 0)
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
                _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
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
                                                if (tx.Confirmations > 1 && tx.Blockheight > 0)
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

        public static async Task<List<string>> SplitTheTokens (string address, string password, string tokenId, int lotAmount, int numberOfLots)
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

                            var resp = await NeblioTransactionHelpers.SendNTP1TokenAPI(dto, isItMintNFT: true);

                            if (!string.IsNullOrEmpty(resp))
                            {
                                Console.WriteLine($"Succesfully splited in txid {resp}!");
                                listofFinalUtxo.Add(resp);
                            }

                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("Cannot split tokens!", ex);
                    }
                }

                return listofFinalUtxo;
            }

            return null;
        }

    }
}
