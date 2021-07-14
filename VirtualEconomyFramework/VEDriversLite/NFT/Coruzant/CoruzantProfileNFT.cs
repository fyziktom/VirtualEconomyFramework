using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT.Coruzant
{
    public class BuzzsproutEpisodeDto
    {
        public int id { get; set; } = 0;
        public string title { get; set; } = string.Empty;
        public string audio_url { get; set; } = string.Empty;
        public string artwork_url { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string summary { get; set; } = string.Empty;
        public string artist { get; set; } = string.Empty;
        public string guid { get; set; } = string.Empty;
        public int total_plays { get; set; } = 0;
    }
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
        public string PodcastId { get; set; } = string.Empty;
        public string Linkedin { get; set; } = string.Empty;
        public string Twitter { get; set; } = string.Empty;
        

        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);
            if (NFT.Type == NFTTypes.CoruzantProfile)
            {
                var pnft = NFT as CoruzantProfileNFT;

                Surname = pnft.Surname;
                Nickname = pnft.Nickname;
                CompanyLink = pnft.CompanyLink;
                CompanyName = pnft.CompanyName;
                PodcastLink = pnft.PodcastLink;
                PodcastId = pnft.PodcastId;
                PersonalPageLink = pnft.PersonalPageLink;
                WorkingPosition = pnft.WorkingPosition;
                Age = pnft.Age;
                Linkedin = pnft.Linkedin;
                Twitter = pnft.Twitter;
            }
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
            if (meta.TryGetValue("Linkedin", out var lkd))
                Linkedin = lkd;
            if (meta.TryGetValue("PodcastId", out var pdb))
                PodcastId = pdb;
            if (meta.TryGetValue("Twitter", out var twit))
                Twitter = twit;
        }

        public override async Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            var nftData = await NFTHelpers.LoadNFTOriginData(Utxo);
            if (nftData != null)
            {

                ParseCommon(lastmetadata);

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;

                ParseSpecific(lastmetadata);
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
                ParseOriginData(metadata);
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
            var metadata = await GetCommonMetadata();

            metadata.Add("Surname", Surname);
            metadata.Add("Nickname", Nickname);
            metadata.Add("Age", Age.ToString());
            metadata.Add("Linkedin", Linkedin);
            metadata.Add("Twitter", Twitter);
            metadata.Add("PersonalPageLink", PersonalPageLink);
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
            if (!string.IsNullOrEmpty(PodcastId))
                metadata.Add("PodcastId", PodcastId);
            
            return metadata;
        }
    }
}
