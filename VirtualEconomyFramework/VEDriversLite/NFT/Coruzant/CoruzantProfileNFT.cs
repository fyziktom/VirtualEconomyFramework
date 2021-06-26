using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT.Coruzant
{
    public class CoruzantProfileNFT : CommonCoruzantNFT
    {
        public CoruzantProfileNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.CoruzantProfile;
            TypeText = "NFT CoruzantProfile";
        }

        public int Age { get; set; } = 0;
        public string Surname { get; set; } = string.Empty;
        public string Nickname { get; set; } = string.Empty;
        public string PersonalPageLink { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string CompanyLink { get; set; } = string.Empty;
        public string WorkingPosition { get; set; } = string.Empty;
        

        public override async Task Fill(INFT NFT)
        {
            var pnft = NFT as CoruzantProfileNFT;
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
            CompanyLink = pnft.CompanyLink;
            CompanyName = pnft.CompanyName;
            PersonalPageLink = pnft.PersonalPageLink;
            WorkingPosition = pnft.WorkingPosition;
            Age = pnft.Age;
        }

        public override async Task ParseOriginData(IDictionary<string, string> lastmetadata)
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
                if (nftData.NFTMetadata.TryGetValue("WorkingPosition", out var workpos))
                    WorkingPosition = workpos;
                if (nftData.NFTMetadata.TryGetValue("PersonalPageLink", out var ppl))
                    PersonalPageLink = ppl;
                if (nftData.NFTMetadata.TryGetValue("CompanyName", out var comn))
                    CompanyName = comn;
                if (nftData.NFTMetadata.TryGetValue("CompanyLink", out var coml))
                    CompanyLink = coml;
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
                if (nftData.NFTMetadata.TryGetValue("WorkingPosition", out var workpos))
                    WorkingPosition = workpos;
                if (nftData.NFTMetadata.TryGetValue("PersonalPageLink", out var ppl))
                    PersonalPageLink = ppl;
                if (nftData.NFTMetadata.TryGetValue("CompanyName", out var comn))
                    CompanyName = comn;
                if (nftData.NFTMetadata.TryGetValue("CompanyLink", out var coml))
                    CompanyLink = coml;
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
                if (metadata.TryGetValue("WorkingPosition", out var workpos))
                    WorkingPosition = workpos;
                if (metadata.TryGetValue("PersonalPageLink", out var ppl))
                    PersonalPageLink = ppl;
                if (metadata.TryGetValue("CompanyName", out var comn))
                    CompanyName = comn;
                if (metadata.TryGetValue("CompanyLink", out var coml))
                    CompanyLink = coml;
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

            }
        }

        public override async Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            return null;
        }
    }
}
