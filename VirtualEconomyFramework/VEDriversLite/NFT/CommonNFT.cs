using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT.Dto;
using VEDriversLite.Security;

namespace VEDriversLite.NFT
{
    /// <summary>
    /// Common NFT class. It implements lots of functions for the parsing the basic parameter of the NFT.
    /// It contains also functions to decrypt or encrypt the property
    /// </summary>
    public abstract class CommonNFT : INFT
    {
        /// <summary>
        /// Text form of the NFT type like "NFT Image" or "NFT Post"
        /// The parsing is in the Common NFT
        /// </summary>
        public string TypeText { get; set; } = string.Empty;
        /// <summary>
        /// NFT Type by enum of NFTTypes
        /// </summary>
        public NFTTypes Type { get; set; }
        /// <summary>
        /// Name of the NFT
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Author of the NFT
        /// </summary>
        public string Author { get; set; } = string.Empty;
        /// <summary>
        /// Description of the NFT - for longer text please use the "Text" property
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Text of the NFT - prepared for the longer texts
        /// </summary>
        public string Text { get; set; } = string.Empty;
        /// <summary>
        /// Link to some webiste in the NFT
        /// </summary>
        public string Link { get; set; } = string.Empty;
        /// <summary>
        /// Link to the icon of the NFT
        /// </summary>
        public string IconLink { get; set; } = string.Empty;
        /// <summary>
        /// Link to the image in the NFT
        /// </summary>
        public string ImageLink { get; set; } = string.Empty;
        /// <summary>
        /// List of the tags separated by space
        /// </summary>
        public string Tags { get; set; } = string.Empty;
        /// <summary>
        /// Parsed tag list. It is parsed in Common NFT class
        /// </summary>
        public List<string> TagsList { get; set; } = new List<string>();
        /// <summary>
        /// NFT Utxo hash
        /// </summary>
        public string Utxo { get; set; } = string.Empty;
        /// <summary>
        /// NFT Utxo Index
        /// </summary>
        public int UtxoIndex { get; set; } = 0;
        /// <summary>
        /// Shorten hash including index number
        /// </summary>
        public string ShortHash => $"{NeblioTransactionHelpers.ShortenTxId(Utxo, false, 16)}:{UtxoIndex}";
        /// <summary>
        /// NFT Origin transaction hash - minting transaction in the case of original NFTs (Image, Music, Ticket)
        /// </summary>
        public string NFTOriginTxId { get; set; } = string.Empty;
        /// <summary>
        /// Source tx where the input for the NFT Minting was taken
        /// </summary>
        public string SourceTxId { get; set; } = string.Empty;
        /// <summary>
        /// Id of the token on what the NFT is created
        /// </summary>
        public string TokenId { get; set; } = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8"; // VENFT tokens as default
        /// <summary>
        /// Price of the NFT in the Neblio
        /// </summary>
        public double Price { get; set; } = 0.0;
        /// <summary>
        /// PriceActive is setted automatically when the price is setted up
        /// </summary>
        public bool PriceActive { get; set; } = false;
        /// <summary>
        /// Price of the NFT in the Dogecoin
        /// </summary>
        public double DogePrice { get; set; } = 0.0;
        /// <summary>
        /// DogePriceActive is setted automatically when the price is setted up
        /// </summary>
        public bool DogePriceActive { get; set; } = false;
        /// <summary>
        /// Related Doge Address to this NFT. If it is created by VENFT App it is filled automatically during the minting request
        /// </summary>
        public string DogeAddress { get; set; } = string.Empty;
        /// <summary>
        /// Set that this NFT will be sold as just in coppies minted for the buyer
        /// </summary>
        public bool SellJustCopy { get; set; }
        /// <summary>
        /// If the NFT is fully loaded this flag is set
        /// </summary>
        public bool IsLoaded { get; set; } = false;
        /// <summary>
        /// If the NFT is alredy saw in the payment this is set
        /// </summary>
        public bool IsInThePayments { get; set; } = false;
        /// <summary>
        /// Info for publishing NFT to the Dogeft
        /// </summary>
        public DogeftInfo DogeftInfo { get; set; } = new DogeftInfo();
        /// <summary>
        /// If the NFT is sold this will be filled
        /// </summary>
        public NFTSoldInfo SoldInfo { get; set; } = new NFTSoldInfo();
        /// <summary>
        /// DateTime stamp taken from the blockchain trnsaction
        /// </summary>
        public DateTime Time { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// History of this NFT
        /// </summary>
        [JsonIgnore]
        public List<INFT> History { get; set; } = new List<INFT>();
        /// <summary>
        /// The transaction info details
        /// </summary>
        [JsonIgnore]
        public GetTransactionInfoResponse TxDetails { get; set; } = new GetTransactionInfoResponse();
        [JsonIgnore]
        private System.Threading.Timer txdetailsTimer;
        /// <summary>
        /// This event is fired when the transaction info is refreshed
        /// </summary>
        public event EventHandler<GetTransactionInfoResponse> TxDataRefreshed;
        /// <summary>
        /// Parse the origin data of the NFT.
        /// It will track the NFT to its origin and use the data from the origin
        /// </summary>
        /// <param name="lastmetadata"></param>
        /// <returns></returns>
        public abstract Task ParseOriginData(IDictionary<string, string> lastmetadata);
        /// <summary>
        /// Retrive the Metadata of the actual NFT. 
        /// It will take the correct properties and translate them to the dictionary which can be add to the token transaction metdata
        /// If the NFT contains encrypted metadata with use of Shared Secret (EDCH) like NFT Message you must provide the parameters if you need to do encryption
        /// </summary>
        /// <param name="address">Address of the sender of the NFT</param>
        /// <param name="key">Private key of the sender of the NFT</param>
        /// <param name="receiver">Receiver of the NFT</param>
        /// <returns></returns>
        public abstract Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "");
        /// <summary>
        /// Fill common and specific properties of the NFT
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
        public abstract Task Fill(INFT NFT);
        /// <summary>
        /// Parse specific information related to the specific kind of the NFT. 
        /// This function must be overwritte in specific NFT class
        /// </summary>
        /// <param name="meta"></param>
        public abstract void ParseSpecific(IDictionary<string, string> meta);
        /// <summary>
        /// Return info if the transaction is spendable
        /// </summary>
        /// <returns></returns>
        public bool IsSpendable()
        {
            if (TxDetails != null)
                return (TxDetails.Confirmations > NeblioTransactionHelpers.MinimumConfirmations);
            else
                return false;
        }
        /// <summary>
        /// Load NFT history.
        /// It will load fully all history steps of this NFT
        /// </summary>
        /// <returns></returns>
        public async Task LoadHistory()
        {
            History = await NFTHelpers.LoadNFTsHistory(Utxo);
        }
        /// <summary>
        /// Fill common properties for the NFT
        /// </summary>
        /// <param name="nft"></param>
        /// <returns></returns>
        public async Task FillCommon(INFT nft)
        {
            IconLink = nft.IconLink;
            ImageLink = nft.ImageLink;
            Name = nft.Name;
            Link = nft.Link;
            Description = nft.Description;
            Author = nft.Author;
            SourceTxId = nft.SourceTxId;
            NFTOriginTxId = nft.NFTOriginTxId;
            TypeText = nft.TypeText;
            Utxo = nft.Utxo;
            TokenId = nft.TokenId;
            Price = nft.Price;
            PriceActive = nft.PriceActive;
            DogePrice = nft.DogePrice;
            DogePriceActive = nft.DogePriceActive;
            UtxoIndex = nft.UtxoIndex;
            Time = nft.Time;
            Tags = nft.Tags;
            TagsList = nft.TagsList;
            Text = nft.Text;
            DogeAddress = nft.DogeAddress;
            SellJustCopy = nft.SellJustCopy;
            DogeftInfo = nft.DogeftInfo;
            SoldInfo = nft.SoldInfo;
            TxDetails = nft.TxDetails;
            IsInThePayments = nft.IsInThePayments;
            IsLoaded = nft.IsLoaded;
        }
        /// <summary>
        /// Clear the object with SoldInfo of NFT
        /// </summary>
        /// <returns></returns>
        public async Task ClearSoldInfo()
        {
            SoldInfo = new NFTSoldInfo();
        }
        /// <summary>
        /// Clear all the prices inside of the NFT
        /// </summary>
        /// <returns></returns>
        public async Task ClearPrices()
        {
            DogePrice = 0.0;
            DogePriceActive = false;
            Price = 0.0;
            PriceActive = false;
        }
        /// <summary>
        /// Function will parse tags to the list of the tags
        /// </summary>
        public void ParseTags()
        {
            var split = Tags.Split(' ');
            TagsList.Clear();
            if (split.Length > 0)
                foreach (var s in split)
                    if (!string.IsNullOrEmpty(s))
                        TagsList.Add(s);
        }
        /// <summary>
        /// Load last data of the NFT.
        /// It means that it will take just the last data and not tracking the origin for the orign data
        /// </summary>
        /// <param name="metadata"></param>
        /// <returns></returns>
        public async Task LoadLastData(IDictionary<string, string> metadata)
        {
            if (metadata != null)
            {
                ParseCommon(metadata);
                await ParseDogeftInfo(metadata);
                ParseSoldInfo(metadata);
                ParseSpecific(metadata);
                ParsePrice(metadata);

                if (Type == NFTTypes.Music)
                {
                    if (string.IsNullOrEmpty(Link) && !string.IsNullOrEmpty(ImageLink))
                        Link = ImageLink;
                }
                IsLoaded = true;
            }
        }
        /// <summary>
        /// Parse price from the metadata of the NFT
        /// </summary>
        /// <param name="meta"></param>
        public void ParsePrice(IDictionary<string, string> meta)
        {
            if (meta.TryGetValue("Price", out var price))
            {
                price = price.Replace(',', '.');
                Price = double.Parse(price, CultureInfo.InvariantCulture);
            }

            if (Price > 0)
                PriceActive = true;
            else
                PriceActive = false;

            if (meta.TryGetValue("DogePrice", out var dprice))
            {
                dprice = dprice.Replace(',', '.');
                DogePrice = double.Parse(dprice, CultureInfo.InvariantCulture);
            }

            if (DogePrice > 0)
                DogePriceActive = true;
            else
                DogePriceActive = false;

            if (meta.TryGetValue("SellJustCopy", out var sjc))
            {
                if (bool.TryParse(sjc, out bool bsjc))
                    SellJustCopy = bsjc;
                else
                    SellJustCopy = false;
            }
            else
            {
                SellJustCopy = false;
            }
        }
        /// <summary>
        /// Parse info about the sellfrom the metadata of the NFT
        /// </summary>
        /// <param name="meta"></param>
        public void ParseSoldInfo(IDictionary<string, string> meta)
        {
            if (meta.TryGetValue("SoldInfo", out var sinf))
            {
                try
                {
                    SoldInfo = JsonConvert.DeserializeObject<NFTSoldInfo>(sinf);
                }
                catch { Console.WriteLine("Cannot parse sold info in the NFT"); }
            }
        }
        /// <summary>
        /// Parse common properties from the dictionary of metadata
        /// </summary>
        /// <param name="meta"></param>
        public void ParseCommon(IDictionary<string,string> meta)
        {
            if (meta.TryGetValue("Name", out var name))
                Name = name;
            if (meta.TryGetValue("Description", out var description))
                Description = description;
            if (meta.TryGetValue("Author", out var author))
                Author = author;
            if (meta.TryGetValue("Link", out var link))
                Link = link;
            if (meta.TryGetValue("Image", out var imagelink))
            {
                ImageLink = imagelink;
                if (!string.IsNullOrEmpty(ImageLink))
                {
                    if (ImageLink.Contains("https://gateway.ipfs.io"))
                        ImageLink = ImageLink.Replace("https://gateway.ipfs.io", "https://ipfs.infura.io");
                }
            }
            if (meta.TryGetValue("IconLink", out var iconlink))
                IconLink = iconlink;
            if (meta.TryGetValue("Type", out var type))
                TypeText = type;
            if (meta.TryGetValue("Text", out var text))
                Text = text;
            if (meta.TryGetValue("DogeAddress", out var dadd))
                DogeAddress = dadd;
            
            if (meta.TryGetValue("DogeftInfo", out var dfti))
            {
                try
                {
                    DogeftInfo = JsonConvert.DeserializeObject<DogeftInfo>(dfti);
                }
                catch { Console.WriteLine("Cannot parse dogeft info in the NFT"); }
            }
            if (meta.TryGetValue("Tags", out var tags))
            {
                tags = tags.Replace("#", string.Empty);
                tags = tags.Replace(",", string.Empty);
                tags = tags.Replace(";", string.Empty);
                Tags = tags;
                ParseTags();
            }

            if (meta.TryGetValue("SourceUtxo", out var su))
                SourceTxId = su;
            if (meta.TryGetValue("NFTOriginTxId", out var nfttxid))
                NFTOriginTxId = nfttxid;

            if (UtxoIndex == 1 && meta.TryGetValue("ReceiptFromPaymentUtxo", out var rfp))
                if (!string.IsNullOrEmpty(rfp))
                    TypeText = "NFT Receipt";
        }
        /// <summary>
        /// Parse dogeft info from the metadata
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        public async Task ParseDogeftInfo(IDictionary<string,string> meta)
        {
            if (meta.TryGetValue("DogeAddress", out var dadd))
                DogeAddress = dadd;
            if (meta.TryGetValue("DogeftInfo", out var dfti))
            {
                try
                {
                    DogeftInfo = JsonConvert.DeserializeObject<DogeftInfo>(dfti);
                }
                catch { Console.WriteLine("Cannot parse dogeft info in the NFT"); }
            }
        }
        /// <summary>
        /// Get common metadata of the NFT as dictionary
        /// </summary>
        /// <returns>Dicrionary with preapred common metadata for the NFT transaction</returns>
        public async Task<IDictionary<string,string>> GetCommonMetadata()
        {
            var metadata = new Dictionary<string, string>();

            metadata.Add("NFT", "true");
            if (string.IsNullOrEmpty(TypeText))
                throw new Exception("Cannot get NFT metadata without filled property TypeText!");
            metadata.Add("Type", TypeText);
            metadata.Add("Name", Name);
            metadata.Add("Author", Author);
            metadata.Add("Description", Description);
            metadata.Add("Image", ImageLink);
            Tags = Tags.Replace("#", string.Empty);
            Tags = Tags.Replace(",", string.Empty);
            Tags = Tags.Replace(";", string.Empty);
            if (!string.IsNullOrEmpty(Tags))
                metadata.Add("Tags", Tags);
            if (!string.IsNullOrEmpty(Text))
                metadata.Add("Text", Text);
            metadata.Add("Link", Link);
            if (Price > 0)
                metadata.Add("Price", Price.ToString("F6", CultureInfo.InvariantCulture));
            if (DogePrice > 0)
                metadata.Add("DogePrice", DogePrice.ToString(CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(DogeAddress))
                metadata.Add("DogeAddress", DogeAddress);
            if (SellJustCopy)
                metadata.Add("SellJustCopy", "true");
            if (!DogeftInfo.IsEmpty)
                metadata.Add("DogeftInfo", JsonConvert.SerializeObject(DogeftInfo));
            if (!SoldInfo.IsEmpty)
                metadata.Add("SoldInfo", JsonConvert.SerializeObject(SoldInfo));
            if (!string.IsNullOrEmpty(SourceTxId))
                metadata.Add("SourceTxId", SourceTxId);
            if (!string.IsNullOrEmpty(NFTOriginTxId))
                metadata.Add("NFTOriginTxId", NFTOriginTxId);
            return metadata;
        }

        /// <summary>
        /// This function will download the data from the IPFS then decrypt the encrypted file container with use of shared secret.
        /// Then the image is saved in ImageData as bytes.
        /// </summary>
        /// <param name="secret">NFT Owner Private Key</param>
        /// <param name="imageLink"></param>
        /// <param name="partner"></param>
        /// <param name="sharedkey"></param>
        /// <returns></returns>
        public virtual async Task<(bool, byte[])> DecryptImageData(NBitcoin.BitcoinSecret secret, string imageLink, string partner, string sharedkey = "")
        {
            if (!string.IsNullOrEmpty(imageLink) && (imageLink.Contains("https://gateway.ipfs.io/ipfs/") || imageLink.Contains("https://ipfs.infura.io/ipfs/")))
            {
                byte[] ImageData;
                var hash = imageLink.Replace("https://gateway.ipfs.io/ipfs/", string.Empty).Replace("https://ipfs.infura.io/ipfs/", string.Empty);
                try
                {
                    var bytes = await NFTHelpers.IPFSDownloadFromInfuraAsync(hash);
                    var dbytesres = await ECDSAProvider.DecryptBytesWithSharedSecret(bytes, partner, secret, sharedkey);
                    if (dbytesres.Item1)
                    {
                        ImageData = dbytesres.Item2;
                        var bl = ImageData.Length;
                        return (true, ImageData);
                    }
                    else
                        ImageData = null;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot download the file from ipfs or decrypt it. " + ex.Message);
                }
            }
            return (false, null);
        }

        /// <summary>
        /// Decrypt the specific property with use of shared secret
        /// </summary>
        /// <param name="prop">Property content</param>
        /// <param name="secret">NFT Owner Private Key</param>
        /// <param name="address"></param>
        /// <param name="partner"></param>
        /// <param name="sharedkey"></param>
        /// <returns></returns>
        public virtual async Task<string> DecryptProperty(string prop, NBitcoin.BitcoinSecret secret, string address = "", string partner = "" , string sharedkey = "")
        {
            if (!string.IsNullOrEmpty(prop))
            {
                if (Security.SecurityUtils.IsBase64String(prop))
                {
                    if (string.IsNullOrEmpty(partner) && !string.IsNullOrEmpty(address))
                        partner = address;

                    try
                    {
                        var d = await ECDSAProvider.DecryptStringWithSharedSecret(prop, partner, secret, sharedkey);
                        if (d.Item1)
                            return d.Item2;
                        else
                            return prop;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot decrypt property in NFT Message. " + ex.Message);
                        return prop;
                    }
                }
                else
                    return prop;
            }
            return string.Empty;
        }

        /// <summary>
        /// Stop the auto refreshin of the tx info data
        /// </summary>
        /// <returns></returns>
        public async Task StopRefreshingData()
        {
            if (txdetailsTimer != null)
                await txdetailsTimer.DisposeAsync();
        }
        /// <summary>
        /// Start auto refreshing of the tx info data
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public async Task StartRefreshingTxData(int interval = 5000)
        {
            if (TxDetails.Confirmations < (NeblioTransactionHelpers.MinimumConfirmations + 2))
            {
                TxDetails.Confirmations = 0;
                TxDetails.Time = 0;

                await StopRefreshingData();

                try
                {
                    TxDetails = await NeblioTransactionHelpers.GetTransactionInfo(Utxo);
                    TxDataRefreshed?.Invoke(this, TxDetails);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Cannot read tx details. " + ex.Message);
                }

                txdetailsTimer = new System.Threading.Timer(async (object stateInfo) =>
                {
                    if (!string.IsNullOrEmpty(Utxo))
                    {
                        try
                        {
                            var txi = await NeblioTransactionHelpers.GetTransactionInfo(Utxo);
                            TxDetails = txi;
                            TxDataRefreshed?.Invoke(this, TxDetails);
                            if (TxDetails.Confirmations > (NeblioTransactionHelpers.MinimumConfirmations + 2))
                                await StopRefreshingData();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("Cannot read tx details. " + ex.Message);
                        }
                    }

                }, new System.Threading.AutoResetEvent(false), interval, interval);
            }
        }
    }
}
