using Ipfs.Http;
using NBitcoin;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Bookmarks;
using VEDriversLite.Builder;
using VEDriversLite.Events;
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT.Dto;
using VEDriversLite.Security;

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

        public static event EventHandler<IEventInfo> NewEventInfo;

        public static void InitHandlers()
        {
            NeblioTransactionHelpers.NewEventInfo += NeblioTransactionHelpers_NewEventInfo;
        }
        public static void DeInitHandlers()
        {
            NeblioTransactionHelpers.NewEventInfo -= NeblioTransactionHelpers_NewEventInfo;
        }

        private static void NeblioTransactionHelpers_NewEventInfo(object sender, IEventInfo e)
        {
            NewEventInfo?.Invoke(null, e);
        }

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

        public static async Task<INFT> FindProfileOfAddress(string address, ICollection<Utxos> utxos = null)
        {
            if (utxos == null)
                utxos = await NeblioTransactionHelpers.GetAddressNFTsUtxos(address);
            INFT profile = null;

            foreach (var u in utxos)
                if (u.Tokens != null && u.Tokens.Count > 0)
                    foreach (var t in u.Tokens)
                        if (t.TokenId == TokenId && t.Amount == 1)
                        {
                            var nft = await NFTFactory.GetNFT(TokenId, u.Txid);
                            if (nft != null)
                                if (nft.Type == NFTTypes.Profile && profile == null)
                                    profile = nft;
                        }
             
            return profile;
        }

        public static async Task<(INFT, List<INFT>)> LoadAddressNFTsWithProfile(string address)
        {
            List<INFT> nfts = new List<INFT>();
            var utxos = await NeblioTransactionHelpers.GetAddressNFTsUtxos(address);
            INFT profile = null;

            foreach (var u in utxos)
            {
                if (u.Tokens != null)
                {
                    if (u.Tokens.Count > 0)
                    {
                        foreach (var t in u.Tokens)
                        {
                            if (t.TokenId == TokenId)
                            {
                                if (t.Amount == 1)
                                {
                                    var nft = await NFTFactory.GetNFT(TokenId, u.Txid);

                                    if (nft != null)
                                    {
                                        if (nft.Type == NFTTypes.Profile && profile == null)
                                            profile = nft;

                                        if (!(nfts.Any(n => n.Utxo == nft.Utxo)))
                                            nfts.Add(nft);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            return (profile, nfts);
        }
        public static async Task<List<INFT>> LoadAddressNFTs(string address)
        {
            List<INFT> nfts = new List<INFT>();
            var utxos = await NeblioTransactionHelpers.GetAddressNFTsUtxos(address);

            foreach (var u in utxos)
            {
                if (u.Tokens != null && u.Tokens.Count > 0)
                {
                    foreach (var t in u.Tokens)
                    {
                        if (t.TokenId == TokenId && t.Amount == 1)
                        {
                            var nft = await NFTFactory.GetNFT(TokenId, u.Txid);
                            if (nft != null)
                                //if (!(nfts.Any(n => n.Utxo == nft.Utxo))) // todo TEST in cases with first minting on address
                                    nfts.Add(nft);
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

                                if (toks != null && toks.Count > 0 && toks[0] != null && toks[0].Amount > 0)
                                {
                                    try
                                    {
                                        var nft = await NFTFactory.GetNFT(TokenId, txinfo.Txid, true);

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
                                        end = true;
                                    else if (toks[0].Amount == 1)
                                        end = false;
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

        public static async Task<string> MintMultiImageNFT(string address, int coppies, EncryptionKey ekey, INFT NFT, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            if (string.IsNullOrEmpty(NFT.ImageLink))
                throw new Exception("Cannot create NFT Image without image link.");

            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Name", NFT.Name);
            metadata.Add("Author", NFT.Author);
            metadata.Add("Description", NFT.Description);
            metadata.Add("Image", NFT.ImageLink);
            metadata.Add("Link", NFT.Link);
            if (NFT.Price > 0)
                metadata.Add("Price", NFT.Price.ToString(CultureInfo.InvariantCulture));
            metadata.Add("Type", "NFT Image");

            // fill input data for sending tx
            var dto = new MintNFTData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = TokenId, // id of token
                Metadata = metadata,
                SenderAddress = address
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.MintMultiNFTTokenAsync(dto, coppies, ekey, nutxos, tutxos);
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

        public static async Task<string> MintImageNFT(string address, EncryptionKey ekey, INFT NFT, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            if (string.IsNullOrEmpty(NFT.ImageLink))
                throw new Exception("Cannot create NFT Image without image link.");

            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Name", NFT.Name);
            metadata.Add("Author", NFT.Author);
            metadata.Add("Description", NFT.Description);
            metadata.Add("Image", NFT.ImageLink);
            metadata.Add("Link", NFT.Link);
            if (NFT.Price > 0)
                metadata.Add("Price", NFT.Price.ToString(CultureInfo.InvariantCulture));
            metadata.Add("Type", "NFT Image");

            // fill input data for sending tx
            var dto = new MintNFTData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = TokenId, // id of token
                Metadata = metadata,
                SenderAddress = address
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.MintNFTTokenAsync(dto, ekey, nutxos, tutxos);
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

        public static async Task<string> MintProfileNFT(string address, EncryptionKey ekey, INFT nft, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            var profile = nft as ProfileNFT;

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
                SenderAddress = address
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.MintNFTTokenAsync(dto, ekey, nutxos, tutxos);
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

        public static async Task<string> MintPostNFT(string address, EncryptionKey ekey, INFT NFT, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Name", NFT.Name);
            metadata.Add("Author", NFT.Author);
            metadata.Add("Description", NFT.Description);
            metadata.Add("Image", NFT.ImageLink);
            metadata.Add("Link", NFT.Link);
            if (NFT.Price > 0)
                metadata.Add("Price", NFT.Price.ToString(CultureInfo.InvariantCulture));
            metadata.Add("Type", "NFT Post");

            // fill input data for sending tx
            var dto = new MintNFTData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = TokenId, // id of token
                Metadata = metadata,
                SenderAddress = address
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.MintNFTTokenAsync(dto, ekey, nutxos, tutxos);
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

        public static async Task<string> MintMusicNFT(string address, EncryptionKey ekey, INFT NFT, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Name", NFT.Name);
            metadata.Add("Author", NFT.Author);
            metadata.Add("Description", NFT.Description);
            metadata.Add("Image", NFT.ImageLink);
            metadata.Add("Link", NFT.Link);
            if (NFT.Price > 0)
                metadata.Add("Price", NFT.Price.ToString(CultureInfo.InvariantCulture));
            metadata.Add("Type", "NFT Music");

            // fill input data for sending tx
            var dto = new MintNFTData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = TokenId, // id of token
                Metadata = metadata,
                SenderAddress = address
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.MintNFTTokenAsync(dto, ekey, nutxos, tutxos);
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

        public static async Task<string> MintMultiPostNFT(string address, int coppies, EncryptionKey ekey, INFT NFT, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Name", NFT.Name);
            metadata.Add("Author", NFT.Author);
            metadata.Add("Description", NFT.Description);
            metadata.Add("Image", NFT.ImageLink);
            metadata.Add("Link", NFT.Link);
            if (NFT.Price > 0)
                metadata.Add("Price", NFT.Price.ToString(CultureInfo.InvariantCulture));
            metadata.Add("Type", "NFT Post");

            // fill input data for sending tx
            var dto = new MintNFTData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = TokenId, // id of token
                Metadata = metadata,
                SenderAddress = address
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.MintMultiNFTTokenAsync(dto, coppies, ekey, nutxos, tutxos);
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

        public static async Task<string> MintMultiMusicNFT(string address, int coppies, EncryptionKey ekey, INFT NFT, ICollection<Utxos> nutxos, ICollection<Utxos> tutxos)
        {
            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Name", NFT.Name);
            metadata.Add("Author", NFT.Author);
            metadata.Add("Description", NFT.Description);
            metadata.Add("Image", NFT.ImageLink);
            metadata.Add("Link", NFT.Link);
            if (NFT.Price > 0)
                metadata.Add("Price", NFT.Price.ToString(CultureInfo.InvariantCulture));
            metadata.Add("Type", "NFT Music");

            // fill input data for sending tx
            var dto = new MintNFTData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = TokenId, // id of token
                Metadata = metadata,
                SenderAddress = address
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.MintMultiNFTTokenAsync(dto, coppies, ekey, nutxos, tutxos);
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

        public static async Task<string> ChangeProfileNFT(string address, EncryptionKey ekey, INFT nft, ICollection<Utxos> nutxos)
        {
            var profile = nft as ProfileNFT;
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
                SenderAddress = address,
                ReceiverAddress = address
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendNFTTokenAsync(dto, ekey, nutxos);
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

        public static async Task<string> ChangePostNFT(string address, EncryptionKey ekey, INFT nft, ICollection<Utxos> nutxos)
        {
            var postnft = nft as PostNFT;

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
                sendUtxo = new List<string>() { postnft.Utxo },
                SenderAddress = address,
                ReceiverAddress = address
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendNFTTokenAsync(dto, ekey, nutxos);
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

        public static async Task<string> SendOrderedNFT(string address, EncryptionKey ekey, PaymentNFT payment, INFT NFT, ICollection<Utxos> nutxos)
        {
            if (NFT == null)
                throw new Exception("Cannot find NFT in the address NFT list.");

            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Type", NFT.TypeText);
            
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
                Metadata = metadata,
                sendUtxo = new List<string>() { payment.NFTUtxoTxId, payment.Utxo },
                SenderAddress = address,
                ReceiverAddress = payment.Sender
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendMultiTokenAPIAsync(dto, ekey, nutxos);
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

        public static async Task<string> SendNFTPayment(string address, EncryptionKey ekey, string receiver, INFT nft, ICollection<Utxos> nutxos)
        {
            if (string.IsNullOrEmpty(nft.Utxo))
                throw new Exception("Wrong token txid input.");

            if (!nft.PriceActive)
                throw new Exception("NFT is not for sale.");

            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Sender", address);
            metadata.Add("NFTUtxoTxId", nft.Utxo);
            metadata.Add("Image", nft.ImageLink);
            metadata.Add("Price", nft.Price.ToString(CultureInfo.InvariantCulture));
            metadata.Add("Type", "NFT Payment");

            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = TokenId, // id of token
                Metadata = metadata,
                Amount = 1,
                sendUtxo = new List<string>() { nft.Utxo },
                SenderAddress = address,
                ReceiverAddress = receiver
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendNTP1TokenWithPaymentAPIAsync(dto, ekey, nft.Price, nutxos);
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

        public static async Task<string> SendNFT(string address, string receiver, EncryptionKey ekey, INFT NFT, bool priceWrite, ICollection<Utxos> nutxos, double price = 0.0002)
        {
            if (price < 0.0002)
                throw new Exception("Price cannot be lower than 0.0002 NEBL.");

            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Type", NFT.TypeText);

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

            // fill input data for sending tx
            var dto = new SendTokenTxData() // please check SendTokenTxData for another properties such as specify source UTXOs
            {
                Id = TokenId, // id of token
                Metadata = metadata,
                Amount = 1,
                sendUtxo = new List<string>() { NFT.Utxo },
                SenderAddress = address,
                ReceiverAddress = receiver
            };

            try
            {
                // send tx
                var rtxid = await NeblioTransactionHelpers.SendNFTTokenAsync(dto, ekey, nutxos);
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

        public static async Task<ProfileNFT> FindProfileNFT(ICollection<INFT> nfts)
        {
            if (nfts != null)
                foreach (var n in nfts)
                    if (n.Type == NFTTypes.Profile)
                        return (ProfileNFT)n;
            return new ProfileNFT("");
        }

        public static async Task<(bool, PubKey)> GetPubKeyFromProfileNFTTx(string address)
        {
            var profile = await FindProfileOfAddress(address);
            //var profile = await FindProfileNFT(nfts);
            if (profile == null)
                return (false, null);
            var txhex = await NeblioTransactionHelpers.GetTxHex(profile.Utxo);
            if (string.IsNullOrEmpty(txhex))
                return (false, null);
            var tx = Transaction.Parse(txhex, NeblioTransactionHelpers.Network);
            if (tx == null)
                return (false, null);
            var pubkey = tx.Inputs[0].ScriptSig.GetAllPubKeys();
            if (pubkey == null || pubkey.Count() == 0)
                return (false, null);
            else
                return (true, pubkey[0]);
        }

        public static async Task<(bool, PubKey)> GetPubKeyFromLastFoundTx(string address)
        {
            var tx = await NeblioTransactionHelpers.GetLastSentTransaction(address);
            //var profile = await FindProfileNFT(nfts);
            if (tx == null)
                return (false, null);
            var pubkey = tx.Inputs[0].ScriptSig.GetAllPubKeys();
            if (pubkey == null || pubkey.Count() == 0)
                return (false, null);
            else
                return (true, pubkey[0]);
        }

        public static async Task<(bool, GetNFTOwnerDto)> GetNFTWithOwner(string txid)
        {
            var nft = await NFTFactory.GetNFT(TokenId, txid, true);
            if (nft != null && txid == nft.Utxo)
            {
                var txhex = await NeblioTransactionHelpers.GetTxHex(txid);
                var tx = NBitcoin.Transaction.Parse(txhex, NeblioTransactionHelpers.Network);

                if (tx != null && tx.Outputs.Count > 0 && tx.Inputs.Count > 0)
                {
                    var outp = tx.Outputs[0];
                    var inpt = tx.Inputs[0];
                    if (outp != null && inpt != null)
                    {
                        var scr = outp.ScriptPubKey;
                        var add = scr.GetDestinationAddress(NeblioTransactionHelpers.Network);
                        var utxos = await NeblioTransactionHelpers.GetAddressUtxosObjects(add.ToString());
                        var addi = inpt.ScriptSig.GetSignerAddress(NeblioTransactionHelpers.Network);
                        if (utxos.FirstOrDefault(u => (u.Txid == txid && u.Value == 10000 && u.Tokens.Count > 0 && u.Tokens.FirstOrDefault()?.Amount == 1)) != null)
                        {
                            return (true, new GetNFTOwnerDto()
                            {
                                NFT = nft,
                                TxId = txid,
                                Owner = add.ToString(),
                                Sender = addi.ToString()
                            });
                        }
                        else
                        {
                            return (false, new GetNFTOwnerDto()
                            {
                                NFT = nft,
                                TxId = txid,
                                Owner = add.ToString(),
                                Sender = addi.ToString(),
                            });
                        }

                    }
                }
            }
            return (false, null);
        }
    }
}
