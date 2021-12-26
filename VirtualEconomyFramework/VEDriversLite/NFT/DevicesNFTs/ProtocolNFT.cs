using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT.DevicesNFTs
{
    public class ProtocolNFT : CommonNFT
    {
        public ProtocolNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Protocol;
            TypeText = "NFT Protocol";
        }

        public string Version { get; set; } = string.Empty;
        public string Rules { get; set; } = string.Empty;
        public string RulesFile { get; set; } = string.Empty;

        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);
            var nft = NFT as ProtocolNFT;
            Version = nft.Version;
            Rules = nft.Rules;
            RulesFile = nft.RulesFile;

        }
        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            if (metadata.TryGetValue("Version", out var version))
                Version = version;
            if (metadata.TryGetValue("Rules", out var rules))
                Rules = rules;
            if (metadata.TryGetValue("RulesFile", out var rulesfile))
                RulesFile = rulesfile;
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
            if (!string.IsNullOrEmpty(Rules))
                metadata.Add("Rules", Rules);
            if (!string.IsNullOrEmpty(RulesFile))
                metadata.Add("RulesFile", RulesFile);

            return metadata;
        }
    }
}
