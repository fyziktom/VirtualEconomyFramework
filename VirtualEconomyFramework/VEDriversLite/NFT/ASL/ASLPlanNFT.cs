using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT.ASL
{
    public class ASLPlanNFT : CommonNFT
    {
        public ASLPlanNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.ASLPlan;
            TypeText = "NFT ASLPlan";
        }

        public bool IsRunning { get; set; } = false;
        public string Version { get; set; } = string.Empty;
        public string Tool { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public string CodeLanguage { get; set; } = string.Empty;
        public string ProcessingAddress { get; set; } = string.Empty;
        public string HostContainer { get; set; } = string.Empty;
        public int MaxNumOfSamples { get; set; } = 10;
        public int MaxNumOfItems { get; set; } = 100;

        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);
            var nft = NFT as ASLPlanNFT;
            IsRunning = nft.IsRunning;
            ProcessingAddress = nft.ProcessingAddress;
            Host = nft.Host;
            HostContainer = nft.HostContainer;
            CodeLanguage = nft.CodeLanguage;
            Version = nft.Version;
            Tool = nft.Tool;
            MaxNumOfItems = nft.MaxNumOfItems;
            MaxNumOfSamples = nft.MaxNumOfSamples;
        }
        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            if (metadata.TryGetValue("ProcAddr", out var procaddr))
                ProcessingAddress = procaddr;
            if (metadata.TryGetValue("Version", out var version))
                Version = version;
            if (metadata.TryGetValue("Tool", out var tool))
                Tool = tool;
            if (metadata.TryGetValue("HostContainer", out var hostc))
                HostContainer = hostc;
            if (metadata.TryGetValue("Host", out var host))
                Host = host;
            if (metadata.TryGetValue("CodeL", out var code))
                CodeLanguage = code;
            if (metadata.TryGetValue("MaxNI", out var maxni))
            {
                if (int.TryParse(maxni, out int imaxni))
                    MaxNumOfItems = imaxni;
                else
                    MaxNumOfItems = 100;
            }
            if (metadata.TryGetValue("MaxNS", out var maxns))
            {
                if (int.TryParse(maxns, out int imaxns))
                    MaxNumOfSamples = imaxns;
                else
                    MaxNumOfSamples = 10;
            }
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
            if (!string.IsNullOrEmpty(ProcessingAddress))
                metadata.Add("ProcAddr", ProcessingAddress);
            if (!string.IsNullOrEmpty(HostContainer))
                metadata.Add("HostC", HostContainer);
            if (!string.IsNullOrEmpty(Host))
                metadata.Add("Host", Host);
            if (!string.IsNullOrEmpty(CodeLanguage))
                metadata.Add("CodeL", CodeLanguage);
            metadata.Add("MaxNI", MaxNumOfItems.ToString());
            metadata.Add("MaxNS", MaxNumOfSamples.ToString());

            return metadata;
        }
    }
}
