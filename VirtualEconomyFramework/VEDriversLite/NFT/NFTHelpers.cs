using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite.NFT
{
    public class LoadNFTOriginDataDto
    {
        public string Utxo { get; set; } = string.Empty;
        public string SourceTxId { get; set; } = string.Empty;
        public string NFTOriginTxId { get; set; } = string.Empty;
        public Dictionary<string, string> NFTMetadata { get; set; } = new Dictionary<string, string>();
    }
    public static class NFTHelpers
    {
        private static HttpClient httpClient = new HttpClient();
        private static IClient _client;
        private static string BaseURL = "https://ntp1node.nebl.io/";
        private static string TokenId = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8";
        private static string TokenSymbol = "VENFT";

        public static async Task<LoadNFTOriginDataDto> LoadNFTOriginData(string utxo)
        {
            var result = new LoadNFTOriginDataDto();
            var txid = utxo;
            while (true)
            {
                try
                {
                    var check = await CheckIfMintTx(txid);
                    if (check.Item1)
                    {
                        var meta = await CheckIfContainsNFTData(txid);
                        if (meta != null)
                        {
                            result.NFTMetadata = meta;
                            result.SourceTxId = check.Item2;
                            result.NFTOriginTxId = txid;
                            return result;
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        txid = check.Item2;
                    }

                    await Task.Delay(1);
                }
                catch(Exception ex)
                {
                    Console.WriteLine("Problem in loading of NFT origin!");
                }
            }
        }

        public static async Task<Dictionary<string,string>> CheckIfContainsNFTData(string utxo)
        {
            var meta = await NeblioTransactionHelpers.GetTransactionMetadata(TokenId, utxo);

            if (meta.TryGetValue("NFT", out var value))
            {
                if (!string.IsNullOrEmpty(value))
                {
                    if (value == "true")
                    {
                        return meta;
                    }
                }
            }

            return null;
        }

        public static async Task<(bool,string)> CheckIfMintTx(string utxo)
        {
            var info = await NeblioTransactionHelpers.GetTransactionInfo(utxo);

            if (info != null) {
                if (info.Vin != null)
                {
                    if (info.Vin.Count > 0)
                    {
                        var vin = info.Vin.ToArray()?[0];
                        if (vin.Tokens != null)
                        {
                            if (vin.Tokens.Count > 0)
                            {
                                var vintok = vin.Tokens.ToArray()?[0];
                                if (vintok != null)
                                {
                                    if (vintok.Amount > 1)
                                        return (true, vin.Txid);
                                    else if (vintok.Amount == 1)
                                        return (false, vin.Txid);
                                }
                            }
                        }
                    }
                }
            }
            
            return (false, string.Empty);
        }

        public static async Task<List<INFT>> LoadAddressNFTs(string address)
        {
            List<INFT> nfts = new List<INFT>();
            var utxos = await NeblioTransactionHelpers.GetAddressNFTsUtxos(address);
            foreach(var u in utxos)
            {
                var nft = await NFTFactory.GetNFT(NFTTypes.Image, u.Txid);
                if (nft != null)
                {
                    if (!(nfts.Any(n => n.Utxo == nft.Utxo)))
                        nfts.Add(nft);
                }
            }

            return nfts;
        }

    }
}
