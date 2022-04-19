using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NFT.Imaging.Xray;
using VEDriversLite.NFT.Imaging.Xray.Dto;

namespace VEDriversLite.NFT.Imaging.Xray
{
    /// <summary>
    /// Xray device NFT
    /// </summary>
    public class XrayNFT : CommonNFT
    {
        /// <summary>
        /// Create empty NFT
        /// </summary>
        public XrayNFT()
        {
            Type = NFTTypes.Xray;
            TypeText = "NFT Xray";
        }
        /// <summary>
        /// Create empty NFT with preload hash
        /// </summary>
        /// <param name="utxo"></param>
        public XrayNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Xray;
            TypeText = "NFT Xray";
        }

        /// <summary>
        /// Commercial Xray Device Product Type
        /// Leave empty if unknown or custom
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;
        /// <summary>
        /// Commercial Detector Produt Type
        /// Leave empty if unknown or custom.
        /// </summary>
        public string DetectorName { get; set; } = string.Empty;
        /// <summary>
        /// Commercial X Ray source Product Type
        /// Leave empty if unknown or custom.
        /// </summary>
        public string SourceName { get; set; } = string.Empty;
        /// <summary>
        /// Commercial Positioner Product Type
        /// This can be also robot type if the robot is used for manipulation
        /// Leave empty if unknown or custom.
        /// </summary>
        public string PositionerName { get; set; } = string.Empty;
        /// <summary>
        /// Data about the detector. 
        /// It is necessary to fill if the standart type of the detector is not provided.
        /// </summary>
        public DetectorDataDto DetectorParameters { get; set; } = new DetectorDataDto();
        /// <summary>
        /// Xray source parameters.
        /// It is recommended to fill them
        /// </summary>
        public SourceParametersDto SourceParameters { get; set; } = new SourceParametersDto();

        /// <summary>
        /// Positioner parameters. Informations about the axes
        /// </summary>
        public PositionerParametersDto PositionerParameters { get; set; } = new PositionerParametersDto();

        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);
            var nft = NFT as XrayNFT;
            DeviceName = nft.DeviceName;
            DetectorName = nft.DetectorName;
            SourceName = nft.SourceName;
            PositionerName = nft.PositionerName;
            DetectorParameters = nft.DetectorParameters;
            SourceParameters = nft.SourceParameters;
            PositionerParameters = nft.PositionerParameters;
        }
        /// <summary>
        /// Parse specific parameters
        /// </summary>
        /// <param name="metadata"></param>
        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            if (metadata.TryGetValue("DetPar", out var detparam))
            {
                if (!string.IsNullOrEmpty(detparam))
                {
                    try
                    {
                        DetectorParameters = JsonConvert.DeserializeObject<DetectorDataDto>(detparam);
                    }
                    catch { Console.WriteLine("Cannot parse detector parameters in the NFT"); }
                }
            }
            if (metadata.TryGetValue("SrcPar", out var srcparam))
            {
                if (!string.IsNullOrEmpty(srcparam))
                {
                    try
                    {
                        SourceParameters = JsonConvert.DeserializeObject<SourceParametersDto>(srcparam);
                    }
                    catch { Console.WriteLine("Cannot parse source parameters in the NFT"); }
                }
            }
            if (metadata.TryGetValue("PosPar", out var posparam))
            {
                if (!string.IsNullOrEmpty(posparam))
                {
                    try
                    {
                        PositionerParameters = JsonConvert.DeserializeObject<PositionerParametersDto>(posparam);
                    }
                    catch { Console.WriteLine("Cannot parse detector parameters in the NFT"); }
                }
            }

            if (metadata.TryGetValue("DeviceName", out var dn))
                DeviceName = dn;
            if (metadata.TryGetValue("DetectorName", out var detn))
                DetectorName = detn;            
            if (metadata.TryGetValue("SourceName", out var sn))
                SourceName = sn;
            if (metadata.TryGetValue("PositionerName", out var posn))
                PositionerName = posn;
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

            if (!string.IsNullOrEmpty(DeviceName))
                metadata.Add("DeviceName", DeviceName);
            if (!string.IsNullOrEmpty(DetectorName))
                metadata.Add("DetectorName", DetectorName);
            if (!string.IsNullOrEmpty(SourceName))
                metadata.Add("SourceName", SourceName);
            if (!string.IsNullOrEmpty(PositionerName))
                metadata.Add("PositionerName", PositionerName);

            if (!SourceParameters.IsDefault())
                metadata.Add("SrcPos", JsonConvert.SerializeObject(SourceParameters));
            if (!DetectorParameters.IsDefault())
                metadata.Add("DetPar", JsonConvert.SerializeObject(DetectorParameters));
            if (!PositionerParameters.IsDefault())
                metadata.Add("PosPar", JsonConvert.SerializeObject(PositionerParameters));

            return metadata;
        }        
    }
}
