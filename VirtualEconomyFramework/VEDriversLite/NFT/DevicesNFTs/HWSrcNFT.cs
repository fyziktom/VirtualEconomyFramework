﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT.DevicesNFTs
{
    /// <summary>
    /// HW Src NFT - represents the source plans of some hardware
    /// </summary>
    public class HWSrcNFT : CommonNFT
    {
        /// <summary>
        /// Constructor of the empty NFT HWSrc
        /// </summary>
        /// <param name="utxo"></param>
        public HWSrcNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.HWSrc;
            TypeText = "NFT HWSrc";
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
            var nft = NFT as HWSrcNFT;
            Version = nft.Version;
            Tool = nft.Tool;
            RepositoryType = nft.RepositoryType;
            RepositoryLink = nft.RepositoryLink;

        }
        /// <summary>
        /// Parse specific data for the NFT
        /// </summary>
        /// <param name="metadata"></param>
        public override void ParseSpecific(IDictionary<string, object> metadata)
        {
            if (metadata.TryGetValue("Version", out var version))
                Version = version as string;
            if (metadata.TryGetValue("Tool", out var tool))
                Tool = tool as string;
            if (metadata.TryGetValue("RepositoryType", out var repot))
                RepositoryType = repot as string;
            if (metadata.TryGetValue("RepositoryLink", out var repo))
                RepositoryLink = repo as string;
        }
        /// <summary>
        /// Parse origin data of the NFT
        /// </summary>
        /// <param name="lastmetadata"></param>
        /// <returns></returns>
        public override async Task ParseOriginData(IDictionary<string, object> lastmetadata)
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
        public override async Task<IDictionary<string, object>> GetMetadata(string address = "", string key = "", string receiver = "")
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
