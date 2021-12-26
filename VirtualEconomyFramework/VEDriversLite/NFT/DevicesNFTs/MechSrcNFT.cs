using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT.DevicesNFTs
{
    public class MechSrcNFT : CommonNFT
    {
        public MechSrcNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.MechSrc;
            TypeText = "NFT MechSrc";
        }

        public string Version { get; set; } = string.Empty;
        public string Tool { get; set; } = string.Empty;
        public string RepositoryType { get; set; } = string.Empty;
        public string RepositoryLink { get; set; } = string.Empty;

        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);
            var nft = NFT as MechSrcNFT;
            Version = nft.Version;
            Tool = nft.Tool;
            RepositoryType = nft.RepositoryType;
            RepositoryLink = nft.RepositoryLink;

        }
        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            if (metadata.TryGetValue("Version", out var version))
                Version = version;
            if (metadata.TryGetValue("Tool", out var tool))
                Tool = tool;
            if (metadata.TryGetValue("RepositoryType", out var repot))
                RepositoryType = repot;
            if (metadata.TryGetValue("RepositoryLink", out var repo))
                RepositoryLink = repo;
        }

        public override async Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            var nftData = await NFTHelpers.LoadNFTOriginData(Utxo);
            if (nftData != null)
            {
                ParseCommon(nftData.NFTMetadata);
                ParsePrice(lastmetadata);
                await ParseDogeftInfo(lastmetadata);
                ParseSoldInfo(lastmetadata);
                ParseSpecific(nftData.NFTMetadata);

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;
                IsLoaded = true;
            }
        }

        public async Task GetLastData()
        {
            var nftData = await NFTHelpers.LoadLastData(Utxo);
            if (nftData != null)
            {
                ParseCommon(nftData.NFTMetadata);
                ParsePrice(nftData.NFTMetadata);
                ParseSpecific(nftData.NFTMetadata);

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;
            }
            IsLoaded = true;
        }


        public override async Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            // create token metadata
            var metadata = await GetCommonMetadata();

            if (!string.IsNullOrEmpty(Version))
                metadata.Add("Version", Version);
            if (!string.IsNullOrEmpty(Tool))
                metadata.Add("Tool", Tool);
            if (!string.IsNullOrEmpty(RepositoryType))
                metadata.Add("RepositoryType", RepositoryType);
            if (!string.IsNullOrEmpty(RepositoryLink))
                metadata.Add("RepositoryLink", RepositoryLink);

            return metadata;
        }
    }
}
