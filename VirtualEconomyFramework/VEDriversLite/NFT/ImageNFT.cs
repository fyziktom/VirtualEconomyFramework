using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{
    public class ImageNFT : CommonNFT
    {
        public ImageNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Image;
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
            Utxo = NFT.Utxo;
            TokenId = NFT.TokenId;
            Price = NFT.Price;
            TypeText = NFT.TypeText;
            Time = NFT.Time;
            UtxoIndex = NFT.UtxoIndex;
            if (Price > 0)
                PriceActive = true;
            else
                PriceActive = false;
        }

        public override async Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            var nftData = await NFTHelpers.LoadNFTOriginData(Utxo);
            if (nftData != null)
            {
                if (nftData.NFTMetadata.TryGetValue("Name", out var name))
                    Name = name;
                if (nftData.NFTMetadata.TryGetValue("Author", out var author))
                    Author = author;
                if (nftData.NFTMetadata.TryGetValue("Description", out var description))
                    Description = description;
                if (nftData.NFTMetadata.TryGetValue("Link", out var link))
                    Link = link;
                if (nftData.NFTMetadata.TryGetValue("Tags", out var tags))
                    Tags = tags;
                if (nftData.NFTMetadata.TryGetValue("Image", out var imagelink))
                    ImageLink = imagelink;
                if (nftData.NFTMetadata.TryGetValue("IconLink", out var iconlink))
                    IconLink = iconlink;
                if (nftData.NFTMetadata.TryGetValue("Type", out var type))
                    TypeText = type;
                
                if (lastmetadata.TryGetValue("Price", out var price))
                        Price = double.Parse(price, CultureInfo.InvariantCulture);
                
                if (Price > 0)
                    PriceActive = true;
                else
                    PriceActive = false;
                
                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;
            }
        }

        public override async Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            if (string.IsNullOrEmpty(ImageLink))
                throw new Exception("Cannot create NFT Image without image link.");

            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Type", "NFT Image");
            metadata.Add("Name", Name);
            metadata.Add("Author", Author);
            metadata.Add("Description", Description);
            metadata.Add("Image", ImageLink);
            metadata.Add("Link", Link);
            if (Price > 0)
                metadata.Add("Price", Price.ToString(CultureInfo.InvariantCulture));

            return metadata;
        }
    }
}
