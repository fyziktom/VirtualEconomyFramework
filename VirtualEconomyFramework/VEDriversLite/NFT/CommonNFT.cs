using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT.Dto;

namespace VEDriversLite.NFT
{
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
        private void parseTags()
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
                catch (Exception ex) { Console.WriteLine("Cannot parse sold info in the NFT"); }
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
                ImageLink = imagelink;
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
                catch (Exception ex) { Console.WriteLine("Cannot parse dogeft info in the NFT"); }
            }
            if (meta.TryGetValue("Tags", out var tags))
            {
                tags = tags.Replace("#", string.Empty);
                tags = tags.Replace(",", string.Empty);
                tags = tags.Replace(";", string.Empty);
                Tags = tags;
                parseTags();
            }

            if (meta.TryGetValue("SourceUtxo", out var su))
                SourceTxId = su;
            if (meta.TryGetValue("NFTOriginTxId", out var nfttxid))
                NFTOriginTxId = nfttxid;
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
                catch (Exception ex) { Console.WriteLine("Cannot parse dogeft info in the NFT"); }
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
            switch (Type)
            {
                case NFTTypes.Image:
                    metadata.Add("Type", "NFT Image");
                    break;
                case NFTTypes.Post:
                    metadata.Add("Type", "NFT Post");
                    break;
                case NFTTypes.Music:
                    metadata.Add("Type", "NFT Music");
                    break;
                case NFTTypes.Message:
                    metadata.Add("Type", "NFT Message");
                    break;
                case NFTTypes.Profile:
                    metadata.Add("Type", "NFT Profile");
                    break;
                case NFTTypes.Payment:
                    metadata.Add("Type", "NFT Payment");
                    break;
                case NFTTypes.Ticket:
                    metadata.Add("Type", "NFT Ticket");
                    break;
                case NFTTypes.Event:
                    metadata.Add("Type", "NFT Event");
                    break;
                case NFTTypes.CoruzantProfile:
                    metadata.Add("Type", "NFT CoruzantProfile");
                    break;
                case NFTTypes.CoruzantArticle:
                    metadata.Add("Type", "NFT CoruzantArticle");
                    break;
                case NFTTypes.CoruzantPremiumArticle:
                    metadata.Add("Type", "NFT CoruzantPremiumArticle");
                    break;
                case NFTTypes.CoruzantPodcast:
                    metadata.Add("Type", "NFT CoruzantPodcast");
                    break;
                case NFTTypes.CoruzantPremiumPodcast:
                    metadata.Add("Type", "NFT CoruzantPremiumPodcast");
                    break;
            }
            
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
                metadata.Add("Price", Price.ToString(CultureInfo.InvariantCulture));
            if (DogePrice > 0)
                metadata.Add("DogePrice", DogePrice.ToString(CultureInfo.InvariantCulture));
            if (!string.IsNullOrEmpty(DogeAddress))
                metadata.Add("DogeAddress", DogeAddress);
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
