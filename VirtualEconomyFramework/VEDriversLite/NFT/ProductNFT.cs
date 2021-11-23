using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{

    public class ProductNFT : CommonNFT
    {
        public ProductNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Product;
            TypeText = "NFT Product";
        }

        public string ProducerProfileNFT { get; set; } = string.Empty;
        public string Datasheet { get; set; } = string.Empty;
        /// <summary>
        /// Price in USD per one Unit
        /// </summary>
        public double UnitPrice { get; set; } = 0.0;

        public override async Task Fill(INFT NFT)
        {
            IconLink = NFT.IconLink;
            ImageLink = NFT.ImageLink;
            Name = NFT.Name;
            Link = NFT.Link;
            Description = NFT.Description;
            Author = NFT.Author;
            SourceTxId = NFT.SourceTxId;
            NFTOriginTxId = NFT.NFTOriginTxId;
            Time = NFT.Time;
            Utxo = NFT.Utxo;
            TokenId = NFT.TokenId;
            UtxoIndex = NFT.UtxoIndex;
            Price = NFT.Price;
            PriceActive = NFT.PriceActive;

            var pnft = NFT as ProductNFT;
            ProducerProfileNFT = pnft.ProducerProfileNFT;
            Datasheet = pnft.Datasheet;
            UnitPrice = pnft.UnitPrice;
        }

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

        public override Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            throw new NotImplementedException();
        }

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
