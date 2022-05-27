using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT.DevicesNFTs
{
    /// <summary>
    /// Protocol NFT - represents description of the communication or translaction of the informations
    /// </summary>
    public class ProtocolNFT : CommonNFT
    {
        /// <summary>
        /// Constructor of the empty NFT Protocol
        /// </summary>
        /// <param name="utxo"></param>
        public ProtocolNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Protocol;
            TypeText = "NFT Protocol";
        }
        /// <summary>
        /// Version of the protocol
        /// </summary>
        public string Version { get; set; } = string.Empty;
        /// <summary>
        /// Description of the rules of communication
        /// </summary>
        public string Rules { get; set; } = string.Empty;
        /// <summary>
        /// Description of the protocol in file (hash or link)
        /// </summary>
        public string RulesFile { get; set; } = string.Empty;
        /// <summary>
        /// Fill the data of the NFT
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);
            var nft = NFT as ProtocolNFT;
            Version = nft.Version;
            Rules = nft.Rules;
            RulesFile = nft.RulesFile;

        }
        /// <summary>
        /// Parse specific data for the NFT
        /// </summary>
        /// <param name="metadata"></param>
        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            if (metadata.TryGetValue("Version", out var version))
                Version = version;
            if (metadata.TryGetValue("Rules", out var rules))
                Rules = rules;
            if (metadata.TryGetValue("RulesFile", out var rulesfile))
                RulesFile = rulesfile;
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
            if (!string.IsNullOrEmpty(Rules))
                metadata.Add("Rules", Rules);
            if (!string.IsNullOrEmpty(RulesFile))
                metadata.Add("RulesFile", RulesFile);

            return metadata;
        }
    }
}
