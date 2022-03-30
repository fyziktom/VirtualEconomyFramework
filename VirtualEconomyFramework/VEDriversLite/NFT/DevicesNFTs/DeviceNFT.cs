using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT.DevicesNFTs
{
    /// <summary>
    /// NFT for description of common device which can be for example some electronic device
    /// </summary>
    public class DeviceNFT : CommonNFT
    {
        /// <summary>
        /// Create empty device
        /// </summary>
        /// <param name="utxo"></param>
        public DeviceNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Device;
            TypeText = "NFT Device";
        }

        /// <summary>
        /// Version of the Device
        /// </summary>
        public string Version { get; set; } = string.Empty;
        /// <summary>
        /// Protocol NFT hash related to this device
        /// It should describe the communication with the device on public APIs
        /// </summary>
        public string ProtocolNFTHash { get; set; } = string.Empty;
        /// <summary>
        /// HW source NFT. Should lead to source of the hardware design
        /// </summary>
        public string HWSrcNFTHash { get; set; } = string.Empty;
        /// <summary>
        /// FW source NFT. Should lead to source of the firmware
        /// </summary>
        public string FWSrcNFTHash { get; set; } = string.Empty;
        /// <summary>
        /// SW source NFT. Should lead to source of the software
        /// </summary>
        public string SWSrcNFTHash { get; set; } = string.Empty;
        /// <summary>
        /// Mechanical source NFT. Should lead to source of the mechanical design
        /// </summary>
        public string MechSrcNFTHash { get; set; } = string.Empty;
        /// <summary>
        /// MAC address - will be moved to IoT device
        /// </summary>
        public string MAC { get; set; } = string.Empty;
        /// <summary>
        /// Unique Id of the producet/device
        /// </summary>
        public string UniqueId { get; set; } = string.Empty;

        /// <summary>
        /// Loaded protocol NFT
        /// </summary>
        [JsonIgnore]
        public INFT LoadedProtocolNFT { get; set; } = new ProtocolNFT("");
        /// <summary>
        /// Loaded HW Source NFT
        /// </summary>
        [JsonIgnore]
        public INFT HwSourceNFT { get; set; } = new HWSrcNFT("");
        /// <summary>
        /// Loaded FW Source NFT
        /// </summary>
        [JsonIgnore]
        public INFT FwSourceNFT { get; set; } = new FWSrcNFT("");
        /// <summary>
        /// Loaded SW Source NFT
        /// </summary>
        [JsonIgnore]
        public INFT SwSourceNFT { get; set; } = new SWSrcNFT("");
        /// <summary>
        /// Loaded Mechanical Source NFT
        /// </summary>
        [JsonIgnore]
        public INFT MechSourceNFT { get; set; } = new MechSrcNFT("");

        /// <summary>
        /// Fill NFT with data from template
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);
            var nft = NFT as DeviceNFT;
            Version = nft.Version;
            ProtocolNFTHash = nft.ProtocolNFTHash;
            HWSrcNFTHash = nft.HWSrcNFTHash;
            FWSrcNFTHash = nft.FWSrcNFTHash;
            SWSrcNFTHash = nft.SWSrcNFTHash;
            MechSrcNFTHash = nft.MechSrcNFTHash;
            MAC = nft.MAC;
            UniqueId = nft.UniqueId;
        }
        /// <summary>
        /// Parse specific properties
        /// </summary>
        /// <param name="metadata"></param>
        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            if (metadata.TryGetValue("Version", out var version))
                Version = version;
            if (metadata.TryGetValue("ProtocolNFT", out var prot))
                ProtocolNFTHash = prot;
            if (metadata.TryGetValue("HWSrcNFT", out var hw))
                HWSrcNFTHash = hw;
            if (metadata.TryGetValue("FWSrcNFT", out var fw))
                FWSrcNFTHash = fw;
            if (metadata.TryGetValue("SWSrcNFT", out var sw))
                SWSrcNFTHash = sw;
            if (metadata.TryGetValue("MechSrcNFT", out var mech))
                MechSrcNFTHash = mech;
            if (metadata.TryGetValue("MAC", out var mac))
                MAC = mac;
            if (metadata.TryGetValue("UniqueId", out var uid))
                UniqueId = uid;
        }

        /// <summary>
        /// Parse origin properties
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
        /// Get metadata of this NFT
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
            if (!string.IsNullOrEmpty(ProtocolNFTHash))
                metadata.Add("ProtocolNFT", ProtocolNFTHash);
            if (!string.IsNullOrEmpty(HWSrcNFTHash))
                metadata.Add("HWSrcNFT", HWSrcNFTHash);
            if (!string.IsNullOrEmpty(HWSrcNFTHash))
                metadata.Add("FWSrcNFT", FWSrcNFTHash);
            if (!string.IsNullOrEmpty(FWSrcNFTHash))
                metadata.Add("SWSrcNFT", SWSrcNFTHash);
            if (!string.IsNullOrEmpty(SWSrcNFTHash))
                metadata.Add("MechSrcNFT", MechSrcNFTHash);
            if (!string.IsNullOrEmpty(MAC))
                metadata.Add("MAC", MAC);
            if (!string.IsNullOrEmpty(UniqueId))
                metadata.Add("UniqueId", UniqueId);

            return metadata;
        }

        /// <summary>
        /// Load all source NFTs
        /// </summary>
        /// <returns></returns>
        public async Task LoadSourceNFTs()
        {
            if (!string.IsNullOrEmpty(ProtocolNFTHash))
            {
                try
                {
                    var nft = await NFTFactory.GetNFT("", ProtocolNFTHash, 0, 0, true, true, NFTTypes.Protocol);
                    if (nft != null && !string.IsNullOrEmpty(nft.Utxo))
                        LoadedProtocolNFT = nft;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cannot load the ProtocolNFT: {ProtocolNFTHash} in DeviceNFT: {Utxo}, exception: {ex.Message}");
                }
            }
            if (!string.IsNullOrEmpty(HWSrcNFTHash))
            {
                try
                {
                    var nft = await NFTFactory.GetNFT("", HWSrcNFTHash, 0, 0, true, true, NFTTypes.HWSrc);
                    if (nft != null && !string.IsNullOrEmpty(nft.Utxo))
                        HwSourceNFT = nft;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cannot load the HWSrcNFT: {HWSrcNFTHash} in DeviceNFT: {Utxo}, exception: {ex.Message}");
                }
            }
            if (!string.IsNullOrEmpty(FWSrcNFTHash))
            {
                try
                {
                    var nft = await NFTFactory.GetNFT("", FWSrcNFTHash, 0, 0, true, true, NFTTypes.FWSrc);
                    if (nft != null && !string.IsNullOrEmpty(nft.Utxo))
                        FwSourceNFT = nft;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cannot load the FWSrcNFT: {FWSrcNFTHash} in DeviceNFT: {Utxo}, exception: {ex.Message}");
                }
            }
            if (!string.IsNullOrEmpty(SWSrcNFTHash))
            {
                try
                {
                    var nft = await NFTFactory.GetNFT("", SWSrcNFTHash, 0, 0, true, true, NFTTypes.SWSrc);
                    if (nft != null && !string.IsNullOrEmpty(nft.Utxo))
                        SwSourceNFT = nft;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cannot load the SWSrcNFT: {SWSrcNFTHash} in DeviceNFT: {Utxo}, exception: {ex.Message}");
                }
            }
            if (!string.IsNullOrEmpty(MechSrcNFTHash))
            {
                try
                {
                    var nft = await NFTFactory.GetNFT("", MechSrcNFTHash, 0, 0, true, true, NFTTypes.MechSrc);
                    if (nft != null && !string.IsNullOrEmpty(nft.Utxo))
                        MechSourceNFT = nft;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cannot load the MechSrcNFT: {MechSrcNFTHash} in DeviceNFT: {Utxo}, exception: {ex.Message}");
                }
            }
        }
        
    }
}
