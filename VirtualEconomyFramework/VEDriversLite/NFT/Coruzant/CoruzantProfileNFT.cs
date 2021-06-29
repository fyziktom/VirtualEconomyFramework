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
            TokenId = "La9ADonmDwxsNKJGvnRWy8gmWmeo72AEeg8cK7";
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
            await FillCommon(NFT);

            Surname = pnft.Surname;
            Nickname = pnft.Nickname;
            CompanyLink = pnft.CompanyLink;
            CompanyName = pnft.CompanyName;
            PodcastLink = pnft.PodcastLink;
            PersonalPageLink = pnft.PersonalPageLink;
            WorkingPosition = pnft.WorkingPosition;
            Age = pnft.Age;
        }

        private void ParseSpecific(IDictionary<string, string> meta)
        {
            if (meta.TryGetValue("Surname", out var surname))
                Surname = surname;
            if (meta.TryGetValue("Nickname", out var nickname))
                Nickname = nickname;
            if (meta.TryGetValue("WorkingPosition", out var workpos))
                WorkingPosition = workpos;
            if (meta.TryGetValue("PodcastLink", out var pdl))
                PodcastLink = pdl;
            if (meta.TryGetValue("PersonalPageLink", out var ppl))
                PersonalPageLink = ppl;
            if (meta.TryGetValue("CompanyName", out var comn))
                CompanyName = comn;
            if (meta.TryGetValue("CompanyLink", out var coml))
                CompanyLink = coml;
            if (meta.TryGetValue("Age", out var age))
                Age = Convert.ToInt32(age);
        }

        public override async Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            var nftData = await NFTHelpers.LoadNFTOriginData(Utxo);
            if (nftData != null)
            {

                ParseCommon(nftData.NFTMetadata);

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;

                ParseSpecific(nftData.NFTMetadata);
            }
        }

        public async Task GetLastData()
        {
            var nftData = await NFTHelpers.LoadLastData(Utxo);
            if (nftData != null)
            {
                ParseCommon(nftData.NFTMetadata);

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;

                ParseSpecific(nftData.NFTMetadata);
            }
        }

        public async Task LoadLastData(Dictionary<string,string> metadata)
        {
            if (metadata != null)
            {
                ParseCommon(metadata);
                ParseSpecific(metadata);
            }
        }

        public override async Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            if (string.IsNullOrEmpty(ImageLink))
                throw new Exception("Cannot create NFT CoruzantProfile without image link.");
            if (string.IsNullOrEmpty(Name) || string.IsNullOrEmpty(Surname))
                throw new Exception("Cannot create NFT CoruzantProfile without Name or Surname");
            if (string.IsNullOrEmpty(Author))
                throw new Exception("Cannot create NFT CoruzantProfile without author.");
            if (string.IsNullOrEmpty(CompanyName))
                throw new Exception("Cannot create NFT CoruzantProfile without company name.");
            if (string.IsNullOrEmpty(PersonalPageLink))
                throw new Exception("Cannot create NFT CoruzantProfile without Personal Page Link.");

            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Type", "NFT CoruzantProfile");

            metadata.Add("Name", Name);
            metadata.Add("Surname", Surname);
            metadata.Add("Nickname", Nickname);
            metadata.Add("Age", Age.ToString());
            metadata.Add("Author", Author);
            metadata.Add("Description", Description);
            metadata.Add("Image", ImageLink);
            metadata.Add("Link", Link);
            metadata.Add("PersonalPageLink", PersonalPageLink);
            if (!string.IsNullOrEmpty(Tags))
                metadata.Add("Tags", Tags);
            if (!string.IsNullOrEmpty(IconLink))
                metadata.Add("IconLink", IconLink);
            if (!string.IsNullOrEmpty(PodcastLink))
                metadata.Add("PodcastLink", PodcastLink);
            if (!string.IsNullOrEmpty(CompanyName))
                metadata.Add("CompanyName", CompanyName);
            if (!string.IsNullOrEmpty(CompanyLink))
                metadata.Add("CompanyLink", CompanyLink);
            if (!string.IsNullOrEmpty(WorkingPosition))
                metadata.Add("WorkingPosition", WorkingPosition);

            return metadata;
        }
    }
}
