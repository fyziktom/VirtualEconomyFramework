using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT.Coruzant
{
    /// <summary>
    /// Coruzant specific NFT for the article
    /// </summary>
    public class CoruzantArticleNFT : CommonCoruzantNFT
    {
        /// <summary>
        /// Construct the NFT Article
        /// </summary>
        /// <param name="utxo"></param>
        public CoruzantArticleNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.CoruzantArticle;
            TypeText = "NFT CoruzantArticle";
            TokenId = "La9ADonmDwxsNKJGvnRWy8gmWmeo72AEeg8cK7";
        }

        /// <summary>
        /// Fill the NFT with data
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);

            if (NFT.Type == NFTTypes.CoruzantArticle)
            {
                var nft = NFT as CoruzantArticleNFT;
                LastComment = nft.LastComment;
                LastCommentBy = nft.LastCommentBy;
                FullPostLink = nft.FullPostLink;
                PodcastLink = nft.PodcastLink;
                AuthorProfileUtxo = nft.AuthorProfileUtxo;
            }
        }

        /// <summary>
        /// Reference to the Coruzant NFT Profile
        /// </summary>
        public string AuthorProfileUtxo { get; set; } = string.Empty;
        /// <summary>
        /// Full link to previous post
        /// </summary>
        public string FullPostLink { get; set; } = string.Empty;
        /// <summary>
        /// Last added comment to the article
        /// </summary>
        public string LastComment { get; set; } = string.Empty;
        /// <summary>
        /// Last comment was added by address or NFT Profile hash
        /// </summary>
        public string LastCommentBy { get; set; } = string.Empty;


        /// <summary>
        /// Parse specific properties
        /// </summary>
        /// <param name="metadata"></param>
        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            if (metadata.TryGetValue("AuthorProfileUtxo", out var pu))
                AuthorProfileUtxo = pu;
            if (metadata.TryGetValue("LastComment", out var lc))
                LastComment = lc;
            if (metadata.TryGetValue("LastCommentBy", out var lcb))
                LastCommentBy = lcb;
            if (metadata.TryGetValue("FullPostLink", out var fpl))
                FullPostLink = fpl;
            if (metadata.TryGetValue("PodcastLink", out var pdl))
                PodcastLink = pdl;
        }

        /// <summary>
        /// Parse origin data
        /// </summary>
        /// <param name="lastmetadata"></param>
        /// <returns></returns>
        public override async Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            var nftData = await NFTHelpers.LoadNFTOriginData(Utxo);
            if (nftData != null)
            {
                ParseCommon(lastmetadata);

                ParsePrice(nftData.NFTMetadata);

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;
            }
            ParseSpecific(lastmetadata);
        }

        /// <summary>
        /// Load last data for this NFT
        /// </summary>
        /// <returns></returns>
        public async Task GetLastData()
        {
            var nftData = await NFTHelpers.LoadLastData(Utxo);
            if (nftData != null)
            {
                ParseCommon(nftData.NFTMetadata);

                ParsePrice(nftData.NFTMetadata);

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;
            }
            ParseSpecific(nftData.NFTMetadata);
        }

        /// <summary>
        /// Get Metadata of this NFT
        /// </summary>
        /// <param name="address"></param>
        /// <param name="key"></param>
        /// <param name="receiver"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public override async Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            if (string.IsNullOrEmpty(ImageLink))
                throw new Exception("Cannot create NFT CoruzantPost without image link.");
            if (string.IsNullOrEmpty(Name))
                throw new Exception("Cannot create NFT CoruzantPost without Name");
            if (string.IsNullOrEmpty(Description))
                throw new Exception("Cannot create NFT CoruzantPost without Description.");
            if (string.IsNullOrEmpty(Author))
                throw new Exception("Cannot create NFT CoruzantPost without author.");
            if (string.IsNullOrEmpty(AuthorProfileUtxo))
                throw new Exception("Cannot create NFT CoruzantPost without Author Profile Utxo.");
            if (LastComment.Length > 250)
                throw new Exception("Cannot create NFT CoruzantPost. Comment must be shorter than 250 characters.");
            if (Description.Length > 250)
                throw new Exception("Cannot create NFT CoruzantPost. Description must be shorter than 250 characters.");

            var metadata = await GetCommonMetadata();
            if (!string.IsNullOrEmpty(FullPostLink))
                metadata.Add("FullPostLink", FullPostLink);
            if (!string.IsNullOrEmpty(PodcastLink))
                metadata.Add("PodcastLink", PodcastLink);
            if (!string.IsNullOrEmpty(AuthorProfileUtxo))
                metadata.Add("AuthorProfileUtxo", AuthorProfileUtxo);
            if (!string.IsNullOrEmpty(LastCommentBy))
                metadata.Add("LastCommentBy", LastCommentBy);
            if (!string.IsNullOrEmpty(LastComment))
                metadata.Add("LastComment", LastComment);

            return metadata;
        }
    }
}
