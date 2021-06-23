using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{
    public class ProfileNFT : CommonNFT
    {
        public ProfileNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Profile;
            TypeText = "NFT Profile";
        }

        public int Age { get; set; } = 0;
        public string Surname { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string RelationshipStatus { get; set; } = string.Empty;

        public override async Task Fill(INFT NFT)
        {
            var pnft = NFT as ProfileNFT;
            IconLink = pnft.IconLink;
            ImageLink = pnft.ImageLink;
            Name = pnft.Name;
            Link = pnft.Link;
            Description = pnft.Description;
            Author = pnft.Author;
            SourceTxId = pnft.SourceTxId;
            NFTOriginTxId = pnft.NFTOriginTxId;
            TypeText = pnft.TypeText;
            Utxo = pnft.Utxo;
            TokenId = pnft.TokenId;
            UtxoIndex = pnft.UtxoIndex;
            Price = pnft.Price;
            PriceActive = pnft.PriceActive;
            Time = pnft.Time;
            Name = pnft.Name;
            Surname = pnft.Surname;
            Nickname = pnft.Nickname;
            RelationshipStatus = pnft.RelationshipStatus;
            Age = pnft.Age;
        }

        public override async Task ParseOriginData()
        {
            var nftData = await NFTHelpers.LoadNFTOriginData(Utxo);
            if (nftData != null)
            {
                if (nftData.NFTMetadata.TryGetValue("Name", out var name))
                    Name = name;
                if (nftData.NFTMetadata.TryGetValue("Surname", out var surname))
                    Surname = surname;
                if (nftData.NFTMetadata.TryGetValue("Nickname", out var nickname))
                    Nickname = nickname;
                if (nftData.NFTMetadata.TryGetValue("RelationshipStatus", out var relationshipStatus))
                    RelationshipStatus = relationshipStatus;
                if (nftData.NFTMetadata.TryGetValue("Description", out var description))
                    Description = description;
                if (nftData.NFTMetadata.TryGetValue("Link", out var link))
                    Link = link;
                if (nftData.NFTMetadata.TryGetValue("Image", out var imagelink))
                    ImageLink = imagelink;
                if (nftData.NFTMetadata.TryGetValue("Age", out var age))
                    Age = Convert.ToInt32(age);
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

        public async Task GetLastData()
        {
            var nftData = await NFTHelpers.LoadLastData(Utxo);
            if (nftData != null)
            {
                if (nftData.NFTMetadata.TryGetValue("Name", out var name))
                    Name = name;
                if (nftData.NFTMetadata.TryGetValue("Surname", out var surname))
                    Surname = surname;
                if (nftData.NFTMetadata.TryGetValue("Nickname", out var nickname))
                    Nickname = nickname;
                if (nftData.NFTMetadata.TryGetValue("RelationshipStatus", out var relationshipStatus))
                    RelationshipStatus = relationshipStatus;
                if (nftData.NFTMetadata.TryGetValue("Description", out var description))
                    Description = description;
                if (nftData.NFTMetadata.TryGetValue("Link", out var link))
                    Link = link;
                if (nftData.NFTMetadata.TryGetValue("Image", out var imagelink))
                    ImageLink = imagelink;
                if (nftData.NFTMetadata.TryGetValue("Age", out var age))
                    Age = Convert.ToInt32(age);
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

        public async Task LoadLastData(Dictionary<string,string> metadata)
        {
            if (metadata != null)
            {
                if (metadata.TryGetValue("Name", out var name))
                    Name = name;
                if (metadata.TryGetValue("Surname", out var surname))
                    Surname = surname;
                if (metadata.TryGetValue("Nickname", out var nickname))
                    Nickname = nickname;
                if (metadata.TryGetValue("RelationshipStatus", out var relationshipStatus))
                    RelationshipStatus = relationshipStatus;
                if (metadata.TryGetValue("Description", out var description))
                    Description = description;
                if (metadata.TryGetValue("Link", out var link))
                    Link = link;
                if (metadata.TryGetValue("Image", out var imagelink))
                    ImageLink = imagelink;
                if (metadata.TryGetValue("Age", out var age))
                    Age = Convert.ToInt32(age);
                if (metadata.TryGetValue("Type", out var type))
                    TypeText = type;
                if (metadata.TryGetValue("SourceUtxo", out var su))
                    NFTOriginTxId = su;
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
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Type", "NFT Profile");
            metadata.Add("Name", Name);
            metadata.Add("Surname", Surname);
            metadata.Add("Age", Age.ToString());
            metadata.Add("Description", Description);
            metadata.Add("Image", ImageLink);
            metadata.Add("Link", Link);
            
            return metadata;
        }
    }
}
