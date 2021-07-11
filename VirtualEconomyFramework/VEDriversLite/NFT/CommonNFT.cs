using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;

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
            UtxoIndex = nft.UtxoIndex;
            Time = nft.Time;
            Tags = nft.Tags;
            Text = nft.Text;
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

        public void ParsePrice(IDictionary<string, string> meta)
        {
            if (meta.TryGetValue("Price", out var price))
                Price = double.Parse(price, CultureInfo.InvariantCulture);

            if (Price > 0)
                PriceActive = true;
            else
                PriceActive = false;
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
            if (meta.TryGetValue("Tags", out var tags))
            {
                Tags = tags;
                parseTags();
            }
            //if (meta.TryGetValue("SourceUtxo", out var su))
                //NFTOriginTxId = su;
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
            if (!string.IsNullOrEmpty(Tags))
                metadata.Add("Tags", Tags);
            if (!string.IsNullOrEmpty(Text))
                metadata.Add("Text", Text);
            metadata.Add("Link", Link);
            if (Price > 0)
                metadata.Add("Price", Price.ToString(CultureInfo.InvariantCulture));

            return metadata;
        }

        public async Task StopRefreshingData()
        {
            if (txdetailsTimer != null)
                txdetailsTimer.Dispose();
        }
        
        public async Task StartRefreshingTxData()
        {
            TxDetails.Confirmations = 0;
            TxDetails.Time = 0;

            if (txdetailsTimer != null)
            {
                txdetailsTimer.Dispose();
            }

            try
            {
                TxDetails = await NeblioTransactionHelpers.GetTransactionInfo(Utxo);
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
                        TxDetails = await NeblioTransactionHelpers.GetTransactionInfo(Utxo);
                        TxDataRefreshed?.Invoke(this, TxDetails);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Cannot read tx details. " + ex.Message);
                    }
                }

            }, new System.Threading.AutoResetEvent(false), 5000, 5000);
        }
    }
}
