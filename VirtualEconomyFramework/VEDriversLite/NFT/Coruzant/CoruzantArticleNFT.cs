using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT.Coruzant
{
    public class CoruzantArticleNFT : CommonCoruzantNFT
    {
        public CoruzantArticleNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.CoruzantArticle;
            TypeText = "NFT CoruzantPost";
            TokenId = "La9ADonmDwxsNKJGvnRWy8gmWmeo72AEeg8cK7";
        }

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

        public string AuthorProfileUtxo { get; set; } = string.Empty;
        public string FullPostLink { get; set; } = string.Empty;
        public string LastComment { get; set; } = string.Empty;
        public string LastCommentBy { get; set; } = string.Empty;


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
