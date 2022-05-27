using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT.DevicesNFTs
{
    /// <summary>
    /// SW Src NFT - represents the source plans/code of some software
    /// </summary>
    public class SWSrcNFT : CommonNFT
    {
        /// <summary>
        /// Constructor of the empty NFT SWSrc
        /// </summary>
        /// <param name="utxo"></param>
        public SWSrcNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.SWSrc;
            TypeText = "NFT SWSrc";
        }

        /// <summary>
        /// Version of the source
        /// </summary>
        public string Version { get; set; } = string.Empty;
        /// <summary>
        /// Tool to open or edit the source
        /// </summary>
        public string Tool { get; set; } = string.Empty;
        /// <summary>
        /// Type of the repository
        /// </summary>
        public string RepositoryType { get; set; } = string.Empty;
        /// <summary>
        /// Link to repository or its hash
        /// </summary>
        public string RepositoryLink { get; set; } = string.Empty;
        /// <summary>
        /// Fill the data of the NFT
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);
            var nft = NFT as SWSrcNFT;
            Version = nft.Version;
            Tool = nft.Tool;
            RepositoryType = nft.RepositoryType;
            RepositoryLink = nft.RepositoryLink;
        }
        /// <summary>
        /// Parse specific data for the NFT
        /// </summary>
        /// <param name="metadata"></param>        
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
                ParsePrice(nftData.NFTMetadata);
                ParseSpecific(nftData.NFTMetadata);

                SourceTxId = nftData.SourceTxId;
                NFTOriginTxId = nftData.NFTOriginTxId;
            }
            IsLoaded = true;
        }

        /// <summary>
        /// Get NFT metadata.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="key"></param>
        /// <param name="receiver"></param>
        /// <returns></returns>
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
