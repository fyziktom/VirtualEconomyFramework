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
        private static IClient client;
        private static NeblioCryptocurrency NeblioCrypto = new NeblioCryptocurrency(false);
        public static QTWalletRPCClient qtRPCClient { get; set; }

        public static ITransaction TransactionInfo(TransactionTypes type, string txid, object obj)
        {
            client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            var transaction = TransactionInfoAsync(client, type, txid);
            return transaction.GetAwaiter().GetResult();
        }

        public static async Task<ITransaction> TransactionInfoAsync(IClient client, TransactionTypes type, string txid)
        {
            if (client == null)
            {
                client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            }

            var info = await client.GetTransactionInfoAsync(txid);

            ITransaction transaction = TransactionFactory.GetTranaction(type, txid);

            DateTime time;

            if (info.Time != null)
            {
                time = TimeHelpers.UnixTimestampToDateTime((double)info.Time);
                transaction.TimeStamp = time;
            }

            var tokenin = new Tokens2();
            try
            {
                transaction.From.Add(info.Vin?.FirstOrDefault().PreviousOutput.Addresses.FirstOrDefault());
                tokenin = info.Vin?.FirstOrDefault().Tokens?.ToList()?.FirstOrDefault();
                transaction.Confirmations = Convert.ToInt32((double)info.Confirmations);
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            if (tokenin != null)
            {
                var tokeninfo = await TokenMetadataAsync(client, TokenTypes.NTP1, tokenin.TokenId, txid);

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
                        TimeStamp = transaction.TimeStamp
                    });
                }

                var addr = "";
                var txinfodetails = info.Vout?.FirstOrDefault();
                if (txinfodetails != null)
                    transaction.Amount = (double)txinfodetails.Value / NeblioCrypto.FromSatToMainRatio;

                var tokenout = info.Vout?.ToList()[0]?.Tokens?.ToList()?.FirstOrDefault();
                if (tokenout == null)
                {
                    tokenout = info.Vout?.ToList()[1]?.Tokens?.ToList()?.FirstOrDefault();
                    if (tokenout != null)
                        addr = info.Vout?.ToList()[1]?.ScriptPubKey?.Addresses?.ToList().FirstOrDefault();
                    else
                        return null;

                    transaction.Direction = TransactionDirection.Outgoing;
                }
                else
                {
                    transaction.Direction = TransactionDirection.Incoming;
                    addr = info.Vout?.ToList()[0]?.ScriptPubKey?.Addresses?.ToList().FirstOrDefault();
                }

                if (string.IsNullOrEmpty(addr))
                    return null;

                transaction.To.Add(addr);
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
                        Metadata = tokeninfo.Metadata,
                        TimeStamp = transaction.TimeStamp
                    });
                }
            }
            /*
            Console.WriteLine($"Hex                         = {info.Hex                  }   ");
            Console.WriteLine($"Txid                        = {info.Txid                 }   ");
            Console.WriteLine($"Version                     = {info.Version              }   ");
            Console.WriteLine($"Locktime                    = {info.Locktime             }   ");
            Console.WriteLine($"Vin count                   = {info.Vin.Count            }   ");
            foreach (var vin in info.Vin)
            {
                Console.WriteLine($"    Txid                = {vin.Txid              }   ");
                Console.WriteLine($"    Vout                = {vin.Vout              }   ");
                Console.WriteLine($"    ScriptSig.Asm       = {vin.ScriptSig.Asm     }   ");
                Console.WriteLine($"    ScriptSig.Hex       = {vin.ScriptSig.Hex     }   ");
                foreach (var item in vin.ScriptSig.AdditionalProperties)
                {
                    Console.WriteLine($"    ScriptSig Property: Key = {item.Key}, Value = {item.Value}");
                }
                Console.WriteLine($"    sequence            = {vin.Sequence          }   ");
                Console.WriteLine($"    PreviousOutput      = {vin.PreviousOutput    }   ");
                Console.WriteLine($"    vin Tokens count    = {vin.Tokens.Count     }    ");
                foreach (var token in vin.Tokens) 
                {
                    Console.WriteLine($"    Id               = {token.TokenId             }  ");
                    Console.WriteLine($"    Amount           = {token.Amount              }  ");
                    Console.WriteLine($"    Props            = {token.AdditionalProperties } ");
                    Console.WriteLine($"    Div           = {token.Divisibility } ");
                }
                Console.WriteLine($"    Value               = {vin.Value             }   ");
                Console.WriteLine($"    sequence            = {vin.Sequence          }   ");
                foreach (var item in vin.AdditionalProperties)
                {
                    Console.WriteLine($"    Vin Property: Key = {item.Key}, Value = {item.Value}");
                }
                Console.WriteLine();
            }
            Console.WriteLine($"Vout count                  = {info.Vout.Count           }   ");
            foreach (var vout in info.Vout)
            {
                Console.WriteLine($"    value                  = {vout.Value                 }   ");
                Console.WriteLine($"    N                      = {vout.N                     }   ");
                Console.WriteLine($"    ScriptPubKey.Asm       = {vout.ScriptPubKey.Asm      }   ");
                Console.WriteLine($"    ScriptPubKey.Hex       = {vout.ScriptPubKey.Hex      }   ");
                Console.WriteLine($"    ScriptPubKey.ReqSigs   = {vout.ScriptPubKey.ReqSigs  }   ");
                Console.WriteLine($"    ScriptPubKey.Type      = {vout.ScriptPubKey.Type     }   ");

                if (vout.ScriptPubKey.Addresses != null)
                {
                    foreach (var item in vout.ScriptPubKey.Addresses) Console.WriteLine($"    ScriptPubKey address:     {vout.ScriptPubKey.Type     }   ");
                    Console.WriteLine($"    vout Tokens count      = {vout.Tokens.Count     }   ");
                }

                foreach (var token in vout.Tokens) 
                {
                    Console.WriteLine($"    Id               = {token.TokenId             }  ");
                    Console.WriteLine($"    Amount           = {token.Amount              }  ");
                    Console.WriteLine($"    Props            = {token.AdditionalProperties } ");
                    Console.WriteLine($"    Div           = {token.Divisibility } ");
                }
                Console.WriteLine($"    Used                   = {vout.Used             }   ");
                Console.WriteLine($"    Blockheight            = {vout.Blockheight      }   ");
                Console.WriteLine($"    UsedBlockheight        = {vout.UsedBlockheight  }   ");
                Console.WriteLine($"    UsedTxid               = {vout.UsedTxid         }   ");
                foreach (var item in vout.AdditionalProperties)
                {
                    Console.WriteLine($"   Vout Property: Key = {item.Key}, Value = {item.Value}");
                }
                Console.WriteLine();
            }
            Console.WriteLine($"Blocktime                   = {info.Blocktime            }   ");
            Console.WriteLine($"TotalSent                   = {info.Totalsent            }   ");
            Console.WriteLine($"Fee                         = {info.Fee                  }   ");
            Console.WriteLine($"Time                        = {info.Time                 }   ");
            Console.WriteLine($"Confirmations               = {info.Confirmations        }   ");
            foreach (var item in info.AdditionalProperties)
            {
                Console.WriteLine($"Property: Key = {item.Key}, Value = {item.Value}");
            }
            Console.WriteLine();
            */
            return transaction;
        }

        public static IToken TokenMetadata(TokenTypes type, string tokenid, string txid, object obj)
        {
            client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            var transaction = TokenMetadataAsync(client, type, tokenid, txid);
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
            IToken token = new NeblioNTP1Token();

            GetTokenMetadataResponse info = new GetTokenMetadataResponse();
            if (string.IsNullOrEmpty(txid))
            {
                info = await client.GetTokenMetadataAsync(tokenid, 0);
            }
            else
            {
                info = await client.GetTokenMetadataOfUtxoAsync(tokenid, txid, 0);
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

            if (client == null)
            {
                client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            }

            var info = await client.SendTokenAsync(data);

            return info.TxHex;
        }

        public static async Task<string> BroadcastNTP1TxAsync(TransactionTypes type, BroadcastTxRequest data)
        {
            if (type != TransactionTypes.Neblio)
                return string.Empty;

            if (client == null)
            {
                client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            }

            var info = await client.BroadcastTxAsync(data);

            return info.Txid;
        }
    }
}
