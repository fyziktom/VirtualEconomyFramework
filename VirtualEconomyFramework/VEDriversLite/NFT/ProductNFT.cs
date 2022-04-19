using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{
    /// <summary>
    /// Product NFT, used as base for orders or invoices
    /// </summary>
    public class ProductNFT : CommonNFT
    {
        /// <summary>
        /// Create empty NFT class
        /// </summary>
        public ProductNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Product;
            TypeText = "NFT Product";
        }
        /// <summary>
        /// Hash of the NFT profile of the producer of this product
        /// </summary>
        public string ProducerProfileNFT { get; set; } = string.Empty;
        /// <summary>
        /// Datasheet link or hash
        /// </summary>
        public string Datasheet { get; set; } = string.Empty;
        /// <summary>
        /// Price in USD per one Unit
        /// </summary>
        public double UnitPrice { get; set; } = 0.0;
        /// <summary>
        /// Fill basic parameters
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);

            var pnft = NFT as ProductNFT;
            ProducerProfileNFT = pnft.ProducerProfileNFT;
            Datasheet = pnft.Datasheet;
            UnitPrice = pnft.UnitPrice;
        }
        /// <summary>
        /// Parse specific parameters
        /// </summary>
        /// <param name="metadata"></param>
        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            if (metadata.TryGetValue("Producer", out var producer))
                ProducerProfileNFT = producer;
            if (metadata.TryGetValue("Datasheet", out var datasheet))
                Datasheet = datasheet;

            if (metadata.TryGetValue("UnitPrice", out var unitPrice))
                if (!string.IsNullOrEmpty(unitPrice))
                    UnitPrice = Convert.ToDouble(unitPrice, CultureInfo.InvariantCulture);

        }
        /// <summary>
        /// Find and parse origin data
        /// </summary>
        /// <param name="lastmetadata"></param>
        /// <returns></returns>
        public override Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            return Task.CompletedTask;
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

            if (!string.IsNullOrEmpty(ProducerProfileNFT))
                metadata.Add("Producer", ProducerProfileNFT);
            if (!string.IsNullOrEmpty(Datasheet))
                metadata.Add("Datasheet", Datasheet);

            if (UnitPrice > 0.0)
                metadata.Add("UnitPrice", Convert.ToString(UnitPrice,CultureInfo.InvariantCulture));
            return metadata;
        }
    }
}
