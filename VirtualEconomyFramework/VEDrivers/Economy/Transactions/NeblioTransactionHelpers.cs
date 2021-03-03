using Neblio.RestApi;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Economy.DTO;
using VEDrivers.Economy.Tokens;

namespace VEDrivers.Economy.Transactions
{
    public static class NeblioTransactionHelpers
    {
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
            var info = await client.GetTransactionInfoAsync(txid);
            
            ITransaction transaction = TransactionFactory.GetTranaction(type, txid);

            transaction.From.Add(info.Vin.First().PreviousOutput.Addresses.FirstOrDefault());
            var tokenin = info.Vin?.First().Tokens?.ToList()?.FirstOrDefault();

            var tokeninfo = await TokenMetadataAsync(client, TokenTypes.NTP1, tokenin.TokenId);

            transaction.Confirmations = Convert.ToInt32((double)info.Confirmations);

            if (tokenin != null)
            {
                transaction.VinTokens.Add(new NeblioNTP1Token()
                {
                    ActualBalance = tokenin.Amount,
                    Id = tokenin.TokenId,
                    MaxSupply = tokeninfo.MaxSupply,
                    Symbol = tokeninfo.Symbol,
                    Name = tokeninfo.Name,
                    IssuerName = tokeninfo.IssuerName
                });
            }

            var addr = "";
            var tokenout = info.Vout?.ToList()[0]?.Tokens?.ToList()?.FirstOrDefault();
            if (tokenout == null)
            {
                tokenout = info.Vout?.ToList()[1]?.Tokens?.ToList()?.FirstOrDefault();
                if (tokenout != null)
                    addr = info.Vout?.ToList()[1]?.ScriptPubKey?.Addresses?.ToList().FirstOrDefault();
                else
                    return null;

                transaction.Direction = TransactionDirection.Outcoming;
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
                    IssuerName = tokeninfo.IssuerName
                });
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

        public static IToken TokenMetadata(TokenTypes type, string tokenid, object obj)
        {
            client = (IClient)new Client(httpClient) { BaseUrl = NeblioCrypto.BaseURL };
            var transaction = TokenMetadataAsync(client, type, tokenid);
            return transaction.GetAwaiter().GetResult();
        }

        private class tokenUrlCarrier
        {
            public string name { get; set; } = string.Empty;
            public string url { get; set; } = string.Empty;
            public string mimeType { get; set; } = string.Empty;
        }

        public static async Task<IToken> TokenMetadataAsync(IClient client, TokenTypes type, string tokenid)
        {
            IToken token = new NeblioNTP1Token();

            var info = await client.GetTokenMetadataAsync(tokenid, 0);
            token.MaxSupply = info.InitialIssuanceAmount;
            token.Symbol = info.MetadataOfIssuance.Data.TokenName;
            token.Name = info.MetadataOfIssuance.Data.Description;
            token.IssuerName = info.MetadataOfIssuance.Data.Issuer;
            token.Id = tokenid;
            var tus = JsonConvert.DeserializeObject<List<tokenUrlCarrier>>(JsonConvert.SerializeObject(info.MetadataOfIssuance.Data.Urls));

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
    }
}
