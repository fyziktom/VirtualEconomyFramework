using Ipfs.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Bookmarks;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite.NFT
{
    public enum LoadingImageStages
    {
        NotStarted,
        Loading,
        Loaded
    }
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
        public static string TokenId = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8";
        private static string TokenSymbol = "VENFT";
        public static readonly IpfsClient ipfs = new IpfsClient("https://ipfs.infura.io:5001");

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

        public static async Task<LoadNFTOriginDataDto> LoadLastData(string utxo)
        {
            var result = new LoadNFTOriginDataDto();
            var txid = utxo;
            try
            {
                var meta = await NeblioTransactionHelpers.GetTransactionMetadata(TokenId, utxo);
                if (meta.TryGetValue("NFT", out var value))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        if (value == "true")
                        {
                            result.NFTMetadata = meta;

                            if (meta.TryGetValue("SourceUtxo", out var sourceTxId))
                                result.NFTOriginTxId = sourceTxId;

                            return result;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Problem in loading of NFT origin!");
            }

            return null;
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

            foreach (var u in utxos)
            {
                if (u.Tokens != null)
                {
                    if (u.Tokens.Count > 0)
                    {
                        foreach(var t in u.Tokens)
                        {
                            if (t.TokenId == TokenId)
                            {
                                if (t.Amount == 1)
                                {
                                    var nft = await NFTFactory.GetNFT(TokenId, u.Txid);

                                    if (nft != null)
                                    {
                                        if (!(nfts.Any(n => n.Utxo == nft.Utxo)))
                                            nfts.Add(nft);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return nfts;
        }

        public static async Task<List<INFT>> LoadNFTsHistory(string utxo)
        {
            try
            {
                List<INFT> nfts = new List<INFT>();
                bool end = false;
                var txid = utxo;

                while (!end)
                {
                    end = true;

                    GetTransactionInfoResponse txinfo = null;
                    List<Vin> vins = new List<Vin>();
                    try
                    {
                        txinfo = await NeblioTransactionHelpers.GetTransactionInfo(txid);
                        vins = txinfo.Vin.ToList();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot load transaction info." + ex.Message);
                    }

                    if (vins != null)
                    {
                        foreach (var vin in vins)
                        {
                            if (vin.Tokens != null)
                            {
                                var toks = vin.Tokens.ToList();
                                if (toks != null)
                                {
                                    if (toks.Count > 0)
                                    {
                                        if (toks[0] != null)
                                        {
                                            if (toks[0].Amount > 0) // this is still nft so load the state in the moment of history
                                            {
                                                try
                                                {
                                                    var nft = await NFTFactory.GetNFT(TokenId, txinfo.Txid);

                                                    if (nft != null)
                                                    {
                                                        if (!(nfts.Any(n => n.Utxo == nft.Utxo)))
                                                        {
                                                            nfts.Add(nft);
                                                        }
                                                        // go to previous tx
                                                        txid = vin.Txid;
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine("Error while loading NFT history step" + ex.Message);
                                                }

                                                if (toks[0].Amount > 1)
                                                {
                                                    end = true;
                                                }
                                                else if (toks[0].Amount == 1)
                                                {
                                                    end = false;
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                         
                return nfts;
            }
            catch(Exception ex)
            {
                var msg = ex.Message;
                Console.WriteLine("Exception during loading NFT history: " + msg);
                return new List<INFT>();
            }
        }

        public static async Task<List<ActiveTab>> GetTabs(string tabs)
        {
            return JsonConvert.DeserializeObject<List<ActiveTab>>(tabs);
        }

        public static async Task<string> SerializeTabs(List<ActiveTab> tabs)
        {
            return JsonConvert.SerializeObject(tabs);
        }

        public static async Task<string> MintImageNFT(NeblioAccount account, ImageNFT newNFT)
        {
            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Name", newNFT.Name);
            metadata.Add("Author", newNFT.Author);
            metadata.Add("Description", newNFT.Description);
            metadata.Add("Image", newNFT.ImageLink);
            metadata.Add("Link", newNFT.Link);
            metadata.Add("Type", "NFT Image");

            // fill input data for sending tx
            var dto = new MintNFTData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = TokenId, // id of token
                Metadata = metadata,
                Password = "", // put here your password
                SenderAddress = account.Address
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.MintNFTTokenAsync(dto, account);
                if (rtxid != null)
                {
                    return rtxid;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<string> MintProfileNFT(NeblioAccount account, ProfileNFT profile)
        {
            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Name", profile.Name);
            metadata.Add("Surname", profile.Surname);
            metadata.Add("Age", profile.Age.ToString());
            metadata.Add("Description", profile.Description);
            metadata.Add("Image", profile.ImageLink);
            metadata.Add("Link", profile.Link);
            metadata.Add("Type", "NFT Profile");

            // fill input data for sending tx
            var dto = new MintNFTData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = TokenId, // id of token
                Metadata = metadata,
                Password = "", // put here your password
                SenderAddress = account.Address
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.MintNFTTokenAsync(dto, account);
                if (rtxid != null)
                {
                    return rtxid;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<string> MintPostNFT(NeblioAccount account, PostNFT newNFT)
        {
            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Name", newNFT.Name);
            metadata.Add("Author", newNFT.Author);
            metadata.Add("Description", newNFT.Description);
            metadata.Add("Image", newNFT.ImageLink);
            metadata.Add("Link", newNFT.Link);
            metadata.Add("Type", "NFT Post");

            // fill input data for sending tx
            var dto = new MintNFTData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = TokenId, // id of token
                Metadata = metadata,
                Password = "", // put here your password
                SenderAddress = account.Address
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.MintNFTTokenAsync(dto, account);
                if (rtxid != null)
                {
                    return rtxid;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<string> ChangeProfileNFT(NeblioAccount account, ProfileNFT profile)
        {
            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Name", profile.Name);
            metadata.Add("Surname", profile.Surname);
            metadata.Add("Nickname", profile.Nickname);
            metadata.Add("Age", profile.Age.ToString());
            metadata.Add("Description", profile.Description);
            metadata.Add("Image", profile.ImageLink);
            metadata.Add("Link", profile.Link);
            metadata.Add("Type", "NFT Profile");

            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = TokenId, // id of token
                Metadata = metadata,
                Amount = 1,
                sendUtxo = new List<string>() { profile.Utxo },
                Password = "", // put here your password
                SenderAddress = account.Address,
                ReceiverAddress = account.Address
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendNFTTokenAsync(dto, account, fee: 20000);
                if (rtxid != null)
                {
                    return rtxid;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<string> ChangePostNFT(NeblioAccount account, PostNFT postnft, string utxo)
        {
            if (string.IsNullOrEmpty(utxo))
                throw new Exception("Wrong token txid input.");

            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Name", postnft.Name);
            metadata.Add("Author", postnft.Author);
            metadata.Add("Description", postnft.Description);
            metadata.Add("Image", postnft.ImageLink);
            metadata.Add("Link", postnft.Link);
            metadata.Add("Type", "NFT Post");
            if (postnft.PriceActive)
                metadata.Add("Price", postnft.Price.ToString(CultureInfo.InvariantCulture));

            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = TokenId, // id of token
                Metadata = metadata,
                Amount = 1,
                sendUtxo = new List<string>() { utxo },
                Password = "", // put here your password
                SenderAddress = account.Address,
                ReceiverAddress = account.Address
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendNFTTokenAsync(dto, account, fee: 20000);
                if (rtxid != null)
                {
                    return rtxid;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<string> SendOrderedNFT(NeblioAccount account, PaymentNFT payment)
        {
            var NFT = account.NFTs.Where(n => n.Utxo == payment.NFTUtxoTxId).FirstOrDefault();
            if (NFT == null)
                throw new Exception("Cannot find NFT in the address NFT list.");

            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            switch (NFT.Type)
            {
                case NFTTypes.Image:
                    metadata.Add("Type", "NFT Image");
                    break;
                case NFTTypes.Post:
                    metadata.Add("Type", "NFT Post");
                    break;
            }

            if (NFT.Type == NFTTypes.Post)
            {
                metadata.Add("Name", NFT.Name);
                metadata.Add("Author", NFT.Author);
                metadata.Add("Description", NFT.Description);
                metadata.Add("Image", NFT.ImageLink);
                metadata.Add("Link", NFT.Link);
            }

            metadata.Add("SoldPrice", payment.Price.ToString(CultureInfo.InvariantCulture));

            metadata.Add("SourceUtxo", NFT.NFTOriginTxId);

            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Amount = 1,
                Id = TokenId, // id of token
                Symbol = "VENFT", // symbol of token
                Metadata = metadata,
                Password = "", // put here your password,
                sendUtxo = new List<string>() { payment.NFTUtxoTxId, payment.Utxo },
                SenderAddress = account.Address,
                ReceiverAddress = payment.Sender
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendMultiTokenAPIAsync(dto, account);
                if (!string.IsNullOrEmpty(rtxid))
                {
                    return rtxid;
                }
                else
                {
                    throw new Exception("Sending Multi Token Transaction was not successfull.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<string> SendNFTPayment(NeblioAccount account, string receiver, INFT image, string utxo, double price)
        {
            if (string.IsNullOrEmpty(utxo))
                throw new Exception("Wrong token txid input.");

            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Sender", account.Address);
            metadata.Add("NFTUtxoTxId", image.Utxo);
            metadata.Add("Image", image.ImageLink);
            metadata.Add("Price", price.ToString(CultureInfo.InvariantCulture));
            metadata.Add("Type", "NFT Payment");

            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = TokenId, // id of token
                Metadata = metadata,
                Amount = 1,
                sendUtxo = new List<string>() { utxo },
                Password = "", // put here your password
                SenderAddress = account.Address,
                ReceiverAddress = receiver
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendNTP1TokenWithPaymentAPIAsync(dto, account, price, 20000);
                if (rtxid != null)
                {
                    return rtxid;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<string> SendNFT(NeblioAccount account, INFT NFT, bool priceWrite, double price = 0.0002)
        {
            if (price < 0.0002)
                throw new Exception("Price cannot be lower than 0.0002 NEBL.");

            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");

            switch (NFT.Type)
            {
                case NFTTypes.Image:
                    metadata.Add("Type", "NFT Image");
                    break;
                case NFTTypes.Post:
                    metadata.Add("Type", "NFT Post");
                    break;
            }

            if (NFT.Type == NFTTypes.Post)
            {
                metadata.Add("Name", NFT.Name);
                metadata.Add("Author", NFT.Author);
                metadata.Add("Description", NFT.Description);
                metadata.Add("Image", NFT.ImageLink);
                metadata.Add("Link", NFT.Link);
            }

            if (priceWrite)
                metadata.Add("Price", price.ToString(CultureInfo.InvariantCulture));

            metadata.Add("SourceUtxo", NFT.NFTOriginTxId);

            var utxo = NFT.Utxo;

            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = TokenId, // id of token
                Metadata = metadata,
                Amount = 1,
                sendUtxo = new List<string>() { utxo },
                Password = "", // put here your password
                SenderAddress = account.Address,
                ReceiverAddress = account.Address
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendNFTTokenAsync(dto, account, fee: 20000);
                if (rtxid != null)
                {
                    return rtxid;
                }
                else
                {
                    return string.Empty;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public static async Task<ProfileNFT> FindProfileNFT(NeblioAccount account)
        {
            if (account != null)
            {
                if (account.NFTs != null)
                {
                    foreach(var n in account.NFTs)
                    {
                        if (n.Type == NFTTypes.Profile)
                        {
                            return (ProfileNFT)n;
                        }
                    }
                }
            }

            return new ProfileNFT("");
        }

        public static async Task<ProfileNFT> FindProfileNFT(List<INFT> nfts)
        {
            if (nfts != null)
            {
                foreach (var n in nfts)
                {
                    if (n.Type == NFTTypes.Profile)
                    {
                        return (ProfileNFT)n;
                    }
                }
            }

            return new ProfileNFT("");
        }
    }
}
