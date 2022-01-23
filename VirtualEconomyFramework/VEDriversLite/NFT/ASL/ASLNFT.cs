using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT.ASL
{
    public class ASLNFT : CommonNFT
    {
        public ASLNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.ASL;
            TypeText = "NFT ASL";
        }

        public bool IsRunning { get; set; } = false;
        public bool IsAccepted { get; set; } = false;
        public string Version { get; set; } = string.Empty;
        public string Tool { get; set; } = string.Empty;
        public string Host { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string FirstProcessingAddress { get; set; } = string.Empty;
        public string RepositoryType { get; set; } = string.Empty;
        public string RepositoryLink { get; set; } = string.Empty;
        public string PlanTxId { get; set; } = string.Empty;

        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);
            var nft = NFT as ASLNFT;
            IsRunning = nft.IsRunning;
            IsAccepted = nft.IsAccepted;
            FirstProcessingAddress = nft.FirstProcessingAddress;
            Host = nft.Host;
            Code = nft.Code;
            Version = nft.Version;
            Tool = nft.Tool;
            RepositoryType = nft.RepositoryType;
            RepositoryLink = nft.RepositoryLink;
            PlanTxId = nft.PlanTxId;
        }
        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            if (metadata.TryGetValue("PlanTxId", out var plan))
                PlanTxId = plan;
            if (metadata.TryGetValue("Version", out var version))
                Version = version;
            if (metadata.TryGetValue("Tool", out var tool))
                Tool = tool;
            if (metadata.TryGetValue("RepositoryType", out var repot))
                RepositoryType = repot;
            if (metadata.TryGetValue("RepositoryLink", out var repo))
                RepositoryLink = repo;
            if (metadata.TryGetValue("Host", out var host))
                Host = host;
            if (metadata.TryGetValue("Code", out var code))
                Code = code;
            if (metadata.TryGetValue("ProcAddr", out var procaddr))
                FirstProcessingAddress = procaddr;
            if (metadata.TryGetValue("IsAccepted", out var acpt))
            {
                if (bool.TryParse(acpt, out bool bacpt))
                    IsAccepted = bacpt;
                else
                    IsAccepted = false;
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
            if (!string.IsNullOrEmpty(RepositoryType))
                metadata.Add("RepositoryType", RepositoryType);
            if (!string.IsNullOrEmpty(RepositoryLink))
                metadata.Add("RepositoryLink", RepositoryLink);
            if (!string.IsNullOrEmpty(Host))
                metadata.Add("Host", Host);
            if (!string.IsNullOrEmpty(Code))
                metadata.Add("Code", Code);
            if (!string.IsNullOrEmpty(FirstProcessingAddress))
                metadata.Add("ProcAddr", FirstProcessingAddress);
            if (!string.IsNullOrEmpty(PlanTxId))
                metadata.Add("PlanTxId", PlanTxId);
            if (IsAccepted)
                metadata.Add("IsAccepted", "true");

            return metadata;
        }
    }
}
