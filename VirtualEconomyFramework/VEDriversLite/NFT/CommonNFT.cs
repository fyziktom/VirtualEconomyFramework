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
        public string TypeText { get; set; } = string.Empty;
        public NFTTypes Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string IconLink { get; set; } = string.Empty;
        public string ImageLink { get; set; } = string.Empty;
        public string Tags { get; set; } = string.Empty;
        public List<string> TagsList { get; set; } = new List<string>();
        public string Utxo { get; set; } = string.Empty;
        public string TokenId { get; set; } = "La58e9EeXUMx41uyfqk6kgVWAQq9yBs44nuQW8"; // VENFT tokens as default
        public string SourceTxId { get; set; } = string.Empty;
        public int UtxoIndex { get; set; } = 0;
        public string NFTOriginTxId { get; set; } = string.Empty;
        public double Price { get; set; } = 0.0;
        public bool PriceActive { get; set; } = false;
        public double DogePrice { get; set; } = 0.0;
        public bool DogePriceActive { get; set; } = false;
        public string DogeAddress { get; set; } = string.Empty;
        public bool IsLoaded { get; set; } = false;
        public bool IsInThePayments { get; set; } = false;
        public DogeftInfo DogeftInfo { get; set; } = new DogeftInfo();
        public NFTSoldInfo SoldInfo { get; set; } = new NFTSoldInfo();
        public string ShortHash => $"{NeblioTransactionHelpers.ShortenTxId(Utxo, false, 16)}:{UtxoIndex}";
        public DateTime Time { get; set; } = DateTime.UtcNow;
        [JsonIgnore]
        public List<INFT> History { get; set; } = new List<INFT>();

        [JsonIgnore]
        public GetTransactionInfoResponse TxDetails { get; set; } = new GetTransactionInfoResponse();
        [JsonIgnore]
        private System.Threading.Timer txdetailsTimer;

        public event EventHandler<GetTransactionInfoResponse> TxDataRefreshed;

        public abstract Task ParseOriginData(IDictionary<string, string> lastmetadata);

        public abstract Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "");

        public abstract Task Fill(INFT NFT);

        public bool IsSpendable()
        {
            if (TxDetails != null)
                return (TxDetails.Confirmations > NeblioTransactionHelpers.MinimumConfirmations);
            else
                return false;
        }
        public async Task LoadHistory()
        {
            History = await NFTHelpers.LoadNFTsHistory(Utxo);
        }

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
        }
        public async Task ClearSoldInfo()
        {
            SoldInfo = new NFTSoldInfo();
        }
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

        public abstract void ParseSpecific(IDictionary<string, string> meta);

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

        public async Task StopRefreshingData()
        {
            if (txdetailsTimer != null)
                await txdetailsTimer.DisposeAsync();
        }
        
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
