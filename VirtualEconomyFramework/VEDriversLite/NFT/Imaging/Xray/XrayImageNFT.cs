using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NFT.Imaging.Xray.Dto;

namespace VEDriversLite.NFT.Imaging.Xray
{
    /// <summary>
    /// Xray image NFT
    /// </summary>
    public class XrayImageNFT : CommonNFT
    {
        /// <summary>
        /// Create empty Xray Image NFT
        /// </summary>
        public XrayImageNFT()
        {
            Type = NFTTypes.XrayImage;
            TypeText = "NFT XrayImage";
        }
        /// <summary>
        /// Create empty Xray Image NFT
        /// <paramref name="utxo">Input utxo of this NFT</paramref>
        /// </summary>
        public XrayImageNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.XrayImage;
            TypeText = "NFT XrayImage";
        }

        /// <summary>
        /// Xray exposure parameters
        /// </summary>
        public XrayExposureParameters XrayParams { get; set; } = new XrayExposureParameters();
        /// <summary>
        /// This flag Indicates if the image stored in link are original or are already processed with some filters, etc.
        /// </summary>
        public bool IsOriginal { get; set; } = true;
        /// <summary>
        /// This flag Indicates if the image data stored on IPFS are raw. 
        /// In that case it needs the width, height and bitdepts/format to be able use/display them
        /// </summary>
        public bool IsRaw { get; set; } = false;
        /// <summary>
        /// If this is on, the result image is averaged from multiple images
        /// </summary>
        public bool IsAveraged { get; set; } = false;
        /// <summary>
        /// If the image is averaged from multiple frames this number tells how many frames was used
        /// </summary>
        public int CountOfFrames { get; set; } = 0;

        /// <summary>
        /// Data about the detector. 
        /// It is necessary to fill if the image data are raw
        /// </summary>
        public DetectorDataDto DetectorParameters { get; set; } = new DetectorDataDto();

        /// <summary>
        /// Object Position parameters
        /// </summary>
        public ObjectPositionDto ObjectPosition { get; set; } = new ObjectPositionDto();

        /// <summary>
        /// Xray device NFT Hash
        /// </summary>
        public string XrayDeviceNFTHash { get; set; } = string.Empty;
        /// <summary>
        /// Loaded Xray Device NFT 
        /// </summary>
        [JsonIgnore]
        public INFT XrayDeviceNFT { get; set; } = new XrayNFT();

        /// <summary>
        /// Fill basic parameters
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);
            var nft = NFT as XrayImageNFT;
            XrayParams = nft.XrayParams;
            IsOriginal = nft.IsOriginal;
            IsRaw = nft.IsRaw;
            IsAveraged = nft.IsAveraged;
            CountOfFrames = nft.CountOfFrames;
            DetectorParameters = nft.DetectorParameters;
            ObjectPosition = nft.ObjectPosition;
            XrayDeviceNFTHash = nft.XrayDeviceNFTHash;
            XrayDeviceNFT = nft.XrayDeviceNFT;
        }
        /// <summary>
        /// Parse specific parameters
        /// </summary>
        /// <param name="metadata"></param>
        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            if (metadata.TryGetValue("ExpPar", out var exposure))
            {
                if (!string.IsNullOrEmpty(exposure))
                {
                    try
                    {
                        XrayParams = JsonConvert.DeserializeObject<XrayExposureParameters>(exposure);
                    }
                    catch { Console.WriteLine("Cannot parse exposure parameters in the NFT"); }
                }
            }
            if (metadata.TryGetValue("ObjPos", out var objectposition))
            {
                if (!string.IsNullOrEmpty(objectposition))
                {
                    try
                    {
                        ObjectPosition = JsonConvert.DeserializeObject<ObjectPositionDto>(objectposition);
                    }
                    catch { Console.WriteLine("Cannot parse object position parameters in the NFT"); }
                }
            }
            if (metadata.TryGetValue("DetPar", out var detectoreparameters))
            {
                if (!string.IsNullOrEmpty(detectoreparameters))
                {
                    try
                    {
                        DetectorParameters = JsonConvert.DeserializeObject<DetectorDataDto>(detectoreparameters);
                    }
                    catch { Console.WriteLine("Cannot parse detector parameters in the NFT"); }
                }
            }
            if (metadata.TryGetValue("IsOrig", out var isorig))
            {
                if (bool.TryParse(isorig, out bool bisorig))
                    IsOriginal = bisorig;
                else
                    IsOriginal = false;
            }
            else
            {
                IsOriginal = false;
            }
            if (metadata.TryGetValue("IsRaw", out var israw))
            {
                if (bool.TryParse(israw, out bool bisraw))
                    IsRaw = bisraw;
                else
                    IsRaw = false;
            }
            else
            {
                IsRaw = false;
            }
            if (metadata.TryGetValue("IsAvg", out var isavg))
            {
                if (bool.TryParse(isavg, out bool bisavg))
                    IsAveraged = bisavg;
                else
                    IsAveraged = false;
            }
            else
            {
                IsAveraged = false;
            }
            if (metadata.TryGetValue("CoF", out var countofframes))
            {
                CountOfFrames = Int32.Parse(countofframes, CultureInfo.InvariantCulture);
            }
            else
            {
                CountOfFrames = 0;
            }

            if (metadata.TryGetValue("XrayDeviceNFT", out var dh))
                XrayDeviceNFTHash = dh;
        }
        /// <summary>
        /// Find and parse origin data
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
        /// Get last data of this NFT
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
        /// Get the NFT data for the NFT
        /// </summary>
        /// <param name="address">Address of the sender</param>
        /// <param name="key">Private key of the sender for encryption</param>
        /// <param name="receiver">receiver of the NFT</param>
        /// <returns></returns>
        public override async Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            // create token metadata
            var metadata = await GetCommonMetadata();

            if (!XrayParams.IsDefault())
                metadata.Add("ExpPar", JsonConvert.SerializeObject(XrayParams));
            if (!ObjectPosition.IsDefault())
                metadata.Add("ObjPos", JsonConvert.SerializeObject(ObjectPosition));
            if (!DetectorParameters.IsDefault())
                metadata.Add("DetPar", JsonConvert.SerializeObject(DetectorParameters));
            if (IsOriginal)
                metadata.Add("IsOrig", "true");
            if (IsRaw)
                metadata.Add("IsRaw", "true");
            if (IsAveraged)
                metadata.Add("IsAvg", "true");

            if (!string.IsNullOrEmpty(XrayDeviceNFTHash))
                metadata.Add("XrayDeviceNFT", XrayDeviceNFTHash);

            return metadata;
        }
        /// <summary>
        /// Load Xray device NFT
        /// </summary>
        /// <returns></returns>
        public async Task LoadXrayNFT()
        {
            if (!string.IsNullOrEmpty(XrayDeviceNFTHash))
            {
                try
                {
                    var nft = await NFTFactory.GetNFT("", XrayDeviceNFTHash, 0, 0, true, true, NFTTypes.Xray);
                    if (nft != null && !string.IsNullOrEmpty(nft.Utxo))
                        XrayDeviceNFT = nft;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Cannot load the Xray NFT: {XrayDeviceNFTHash} in XrayImageNFT: {Utxo}, exception: {ex.Message}");
                }
            }
        }

    }
}
