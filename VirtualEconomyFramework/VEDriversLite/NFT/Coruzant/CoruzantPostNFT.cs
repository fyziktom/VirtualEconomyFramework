using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT.Coruzant
{
    public class CoruzantPostNFT : CommonCoruzantNFT
    {
        public CoruzantPostNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.CoruzantPost;
            TypeText = "NFT CoruzantPost";
        }

        public override async Task Fill(INFT NFT)
        {
            IconLink = NFT.IconLink;
            ImageLink = NFT.ImageLink;
            Name = NFT.Name;
            Link = NFT.Link;
            Description = NFT.Description;
            Author = NFT.Author;
            SourceTxId = NFT.SourceTxId;
            NFTOriginTxId = NFT.NFTOriginTxId;
            TypeText = NFT.TypeText;
            Utxo = NFT.Utxo;
            TokenId = NFT.TokenId;
            Time = NFT.Time;
            UtxoIndex = NFT.UtxoIndex;
            Price = NFT.Price;
            PriceActive = NFT.PriceActive;
            LastComment = (NFT as CoruzantPostNFT).LastComment;
            LastCommentBy = (NFT as CoruzantPostNFT).LastCommentBy;
            FullPostLink = (NFT as CoruzantPostNFT).FullPostLink;
            AuthorProfileUtxo = (NFT as CoruzantPostNFT).AuthorProfileUtxo;
        }

        public string AuthorProfileUtxo { get; set; } = string.Empty;
        public string FullPostLink { get; set; } = string.Empty;
        public string LastComment { get; set; } = string.Empty;
        public string LastCommentBy { get; set; } = string.Empty;
        public List<string> TagsList { get; set; } = new List<string>();

        private void parseTags()
        {
            var split = Tags.Split(' ');
            TagsList.Clear();
            if (split.Length > 0)
                foreach (var s in split)
                    if (!string.IsNullOrEmpty(s))
                        TagsList.Add(s);
        }

        public override async Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            var nftData = await NFTHelpers.LoadNFTOriginData(Utxo);
            if (nftData != null)
            {
                if (nftData.NFTMetadata.TryGetValue("Name", out var name))
                    Name = name;
                if (nftData.NFTMetadata.TryGetValue("Description", out var description))
                    Description = description;
                if (nftData.NFTMetadata.TryGetValue("Author", out var author))
                    Author = author;
                if (nftData.NFTMetadata.TryGetValue("Link", out var link))
                    Link = link;
                if (nftData.NFTMetadata.TryGetValue("Tags", out var tags))
                {
                    Tags = tags;
                    parseTags();
                }
                if (nftData.NFTMetadata.TryGetValue("Image", out var imagelink))
                    ImageLink = imagelink;
                if (nftData.NFTMetadata.TryGetValue("AuthorProfileUtxo", out var pu))
                    AuthorProfileUtxo = pu;
                if (nftData.NFTMetadata.TryGetValue("LastComment", out var lc))
                    LastComment = lc;
                if (nftData.NFTMetadata.TryGetValue("LastCommentBy", out var lcb))
                    LastCommentBy = lcb;
                if (nftData.NFTMetadata.TryGetValue("FullPostLink", out var fpl))
                    FullPostLink = fpl;
                if (nftData.NFTMetadata.TryGetValue("Type", out var type))
                    TypeText = type;
                if (lastmetadata.TryGetValue("Price", out var price))
                {
                    if (!string.IsNullOrEmpty(price))
                    {
                        price = price.Replace(',', '.');
                        Price = double.Parse(price, CultureInfo.InvariantCulture);
                        PriceActive = true;
                    }
                    else
                    {
                        PriceActive = false;
                    }
                }
                else
                {
                    PriceActive = false;
                }

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;
            }
        }

        public async Task GetLastData()
        {
            var nftData = await NFTHelpers.LoadLastData(Utxo);
            if (nftData != null)
            {
                if (nftData.NFTMetadata.TryGetValue("Name", out var name))
                    Name = name;
                if (nftData.NFTMetadata.TryGetValue("Description", out var description))
                    Description = description;
                if (nftData.NFTMetadata.TryGetValue("Author", out var author))
                    Author = author;
                if (nftData.NFTMetadata.TryGetValue("Link", out var link))
                    Link = link;
                if (nftData.NFTMetadata.TryGetValue("Tags", out var tags))
                {
                    Tags = tags;
                    parseTags();
                }
                if (nftData.NFTMetadata.TryGetValue("Image", out var imagelink))
                    ImageLink = imagelink;
                if (nftData.NFTMetadata.TryGetValue("AuthorProfileUtxo", out var pu))
                    AuthorProfileUtxo = pu;
                if (nftData.NFTMetadata.TryGetValue("LastComment", out var lc))
                    LastComment = lc;
                if (nftData.NFTMetadata.TryGetValue("LastCommentBy", out var lcb))
                    LastCommentBy = lcb;
                if (nftData.NFTMetadata.TryGetValue("FullPostLink", out var fpl))
                    FullPostLink = fpl;
                if (nftData.NFTMetadata.TryGetValue("Type", out var type))
                    TypeText = type;
                if (nftData.NFTMetadata.TryGetValue("Price", out var price))
                {
                    if (!string.IsNullOrEmpty(price))
                    {
                        price = price.Replace(',', '.');
                        Price = double.Parse(price, CultureInfo.InvariantCulture);
                        PriceActive = true;
                    }
                    else
                    {
                        PriceActive = false;
                    }
                }
                else
                {
                    PriceActive = false;
                }

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;
            }
        }

        public async Task LoadLastData(Dictionary<string, string> metadata)
        {
            if (metadata != null)
            {
                if (metadata.TryGetValue("Name", out var name))
                    Name = name;
                if (metadata.TryGetValue("Description", out var description))
                    Description = description;
                if (metadata.TryGetValue("Author", out var author))
                    Author = author;
                if (metadata.TryGetValue("Link", out var link))
                    Link = link;
                if (metadata.TryGetValue("Tags", out var tags))
                {
                    Tags = tags;
                    parseTags();
                }
                if (metadata.TryGetValue("Image", out var imagelink))
                    ImageLink = imagelink;
                if (metadata.TryGetValue("AuthorProfileUtxo", out var pu))
                    AuthorProfileUtxo = pu;
                if (metadata.TryGetValue("LastComment", out var lc))
                    LastComment = lc;
                if (metadata.TryGetValue("LastCommentBy", out var lcb))
                    LastCommentBy = lcb;
                if (metadata.TryGetValue("FullPostLink", out var fpl))
                    FullPostLink = fpl;
                if (metadata.TryGetValue("Type", out var type))
                    TypeText = type;
                if (metadata.TryGetValue("SourceUtxo", out var su))
                {
                    SourceTxId = Utxo;
                    NFTOriginTxId = su;
                }
                else
                {
                    SourceTxId = Utxo;
                    NFTOriginTxId = Utxo;
                }
                if (metadata.TryGetValue("Price", out var price))
                {
                    if (!string.IsNullOrEmpty(price))
                    {
                        price = price.Replace(',', '.');
                        Price = double.Parse(price, CultureInfo.InvariantCulture);
                        PriceActive = true;
                    }
                    else
                    {
                        PriceActive = false;
                    }
                }
                else
                {
                    PriceActive = false;
                }

            }
        }

        public override async Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            return null;
        }
    }
}
