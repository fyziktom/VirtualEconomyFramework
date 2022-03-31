using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT.Coruzant
{
    /// <summary>
    /// Dto to serialize information from buzzsprout api
    /// </summary>
    public class BuzzsproutEpisodeDto
    {
        /// <summary>
        /// ID of the episode
        /// </summary>
        public int id { get; set; } = 0;
        /// <summary>
        /// Title of the episode
        /// </summary>
        public string title { get; set; } = string.Empty;
        /// <summary>
        /// Audio url of the episode
        /// </summary>
        public string audio_url { get; set; } = string.Empty;
        /// <summary>
        /// Image url of the episode
        /// </summary>
        public string artwork_url { get; set; } = string.Empty;
        /// <summary>
        /// Description of the episode
        /// </summary>
        public string description { get; set; } = string.Empty;
        /// <summary>
        /// Short summary of the episode
        /// </summary>
        public string summary { get; set; } = string.Empty;
        /// <summary>
        /// Artists
        /// </summary>
        public string artist { get; set; } = string.Empty;
        /// <summary>
        /// GUID of the episode
        /// </summary>
        public string guid { get; set; } = string.Empty;
        /// <summary>
        /// Total played
        /// </summary>
        public int total_plays { get; set; } = 0;
    }
    /// <summary>
    /// Coruzant profile NFT
    /// </summary>
    public class CoruzantProfileNFT : CommonCoruzantNFT
    {
        /// <summary>
        /// Construct empty Coruzant profile
        /// </summary>
        /// <param name="utxo"></param>
        public CoruzantProfileNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.CoruzantProfile;
            TypeText = "NFT CoruzantProfile";
            TokenId = "La9ADonmDwxsNKJGvnRWy8gmWmeo72AEeg8cK7";
        }

        /// <summary>
        /// Age of the person
        /// </summary>
        public int Age { get; set; } = 0;
        /// <summary>
        /// Person Surname
        /// </summary>
        public string Surname { get; set; } = string.Empty;
        /// <summary>
        /// Person Nickname
        /// </summary>
        public string Nickname { get; set; } = string.Empty;
        /// <summary>
        /// Personal page url link
        /// </summary>
        public string PersonalPageLink { get; set; } = string.Empty;
        /// <summary>
        /// Name of the company where person is working
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;
        /// <summary>
        /// Link to the company website
        /// </summary>
        public string CompanyLink { get; set; } = string.Empty;
        /// <summary>
        /// Working position of the person
        /// </summary>
        public string WorkingPosition { get; set; } = string.Empty;
        /// <summary>
        /// Podcast Id with this person
        /// </summary>
        public string PodcastId { get; set; } = string.Empty;
        /// <summary>
        /// LinkedIn link of the person
        /// </summary>
        public string Linkedin { get; set; } = string.Empty;
        /// <summary>
        /// Twitter link of the person
        /// </summary>
        public string Twitter { get; set; } = string.Empty;
        
        /// <summary>
        /// Fill the data from template
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Parse specific parameters
        /// </summary>
        /// <param name="metadata"></param>
        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            if (metadata.TryGetValue("Surname", out var surname))
                Surname = surname;
            if (metadata.TryGetValue("Nickname", out var nickname))
                Nickname = nickname;
            if (metadata.TryGetValue("WorkingPosition", out var workpos))
                WorkingPosition = workpos;
            if (metadata.TryGetValue("PodcastLink", out var pdl))
                PodcastLink = pdl;
            if (metadata.TryGetValue("PersonalPageLink", out var ppl))
                PersonalPageLink = ppl;
            if (metadata.TryGetValue("CompanyName", out var comn))
                CompanyName = comn;
            if (metadata.TryGetValue("CompanyLink", out var coml))
                CompanyLink = coml;
            if (metadata.TryGetValue("Age", out var age))
                Age = Convert.ToInt32(age);
            if (metadata.TryGetValue("Linkedin", out var lkd))
                Linkedin = lkd;
            if (metadata.TryGetValue("PodcastId", out var pdb))
                PodcastId = pdb;
            if (metadata.TryGetValue("Twitter", out var twit))
                Twitter = twit;
        }

        /// <summary>
        /// Parse origin data of the NFT
        /// </summary>
        /// <param name="lastmetadata"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get last data for this NFT
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// Get Metadata for this NFT
        /// </summary>
        /// <param name="address"></param>
        /// <param name="key"></param>
        /// <param name="receiver"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
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
