using log4net;
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
                        tokeninfo = await TokenMetadataAsync(client, TokenTypes.NTP1, tokenin.TokenId, txid);
                    else
                        tokeninfo = await TokenMetadataAsync(_client, TokenTypes.NTP1, tokenin.TokenId, txid);
                }
                catch (Exception ex)
                {
                    //todo, usually wrong parsing of json
                    throw new Exception("Cannot load token info from Neblio API", ex);
                }

                if (tokenin != null)
                {
                    transaction.VinTokens.Add(new NeblioNTP1Token()
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
                        TimeStamp = transaction.TimeStamp
                    });
                }

                var addrto = string.Empty;
                var txinfodetails = info.Vout?.FirstOrDefault();
                if (txinfodetails != null)
                    transaction.Amount = (double)txinfodetails.Value / NeblioCrypto.FromSatToMainRatio;

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
                    }
                }
                else
                {
                    transaction.Direction = TransactionDirection.Incoming;
                }

                transaction.To.Add(addrto);
                if (tokenout != null)
                {
                    transaction.VoutTokens.Add(new NeblioNTP1Token()
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
                    });
                }
            }

            return transaction;
        }

        public static IToken TokenMetadata(TokenTypes type, string tokenid, string txid, object obj)
        {
            _client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            var transaction = TokenMetadataAsync(_client, type, tokenid, txid);
            return transaction.GetAwaiter().GetResult();
        }

        private class tokenUrlCarrier
        {
            public string name { get; set; } = string.Empty;
            public string url { get; set; } = string.Empty;
            public string mimeType { get; set; } = string.Empty;
        }

        public static async Task<IToken> TokenMetadataAsync(IClient client, TokenTypes type, string tokenid, string txid)
        {
            if (client == null)
            {
                client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            }

            IToken token = new NeblioNTP1Token();

            GetTokenMetadataResponse info = new GetTokenMetadataResponse();
            try
            {

                if (string.IsNullOrEmpty(txid))
                {
                    info = await client.GetTokenMetadataAsync(tokenid, 0);
                }
                else
                {
                    info = await client.GetTokenMetadataOfUtxoAsync(tokenid, txid, 0);
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
        public static async Task<string> SendNTP1TokenAPI(SendTokenTxData data, double fee = 20000)
        {
            var res = "ERROR";
            var dto = new SendTokenRequest();

            dto.Metadata = new Metadata2();
            dto.Metadata.UserData = new UserData3();
            dto.Metadata.UserData.Meta = new List<JObject>();

            if (!qtRPCClient.IsConnected)
                qtRPCClient.InitClients();

            if (qtRPCClient.IsConnected)
            {
                if (data.Metadata != null)
                {
                    foreach (var d in data.Metadata)
                    {
                        var obj = new JObject();
                        obj[d.Key] = d.Value;

                        dto.Metadata.UserData.Meta.Add(obj);
                    }
                }

                dto.Fee = fee;
                dto.Flags = new Flags2() { SplitChange = true };
                dto.From = new List<string>() { data.SenderAddress };
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
                    var hexToSign = await SendRawNTP1TxAsync(TransactionTypes.Neblio, dto);
                    // sign tx
                    res = await qtRPCClient.RPCLocalCommandSplitedAsync("signrawtransaction", new string[] { hexToSign });
                    // send tx

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
                catch(Exception ex)
                {
                    log.Error("Cannot send token transaction!", ex);
                    Console.WriteLine("Cannot send token transaction!");
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
    }
}
