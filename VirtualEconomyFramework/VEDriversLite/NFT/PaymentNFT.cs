using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{
    public class PaymentNFT : CommonNFT
    {
        public PaymentNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Payment;
            TypeText = "NFT Payment";
        }

        public string NFTUtxoTxId { get; set; } = string.Empty;
        public int NFTUtxoIndex { get; set; } = 0;
        public string Sender { get; set; } = string.Empty;
        public bool Matched { get; set; } = false;

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

            var pnft = NFT as PaymentNFT;
            NFTUtxoTxId = pnft.NFTUtxoTxId;
            NFTUtxoIndex = pnft.NFTUtxoIndex;
            Sender = pnft.Sender;
        }

        public async Task LoadLastData(Dictionary<string, string> metadata)
        {
            if (metadata != null)
            {
                if (metadata.TryGetValue("Name", out var name))
                    Name = name;
                if (metadata.TryGetValue("NFTUtxoTxId", out var nfttxid))
                {
                    NFTUtxoTxId = nfttxid;
                    NFTOriginTxId = nfttxid;
                }
                if (metadata.TryGetValue("Sender", out var sender))
                    Sender = sender;
                if (metadata.TryGetValue("Image", out var imagelink))
                    ImageLink = imagelink;
                if (metadata.TryGetValue("Type", out var type))
                    TypeText = type;
                if (metadata.TryGetValue("NFTUtxoIndex", out var index))
                    if (!string.IsNullOrEmpty(index))
                        NFTUtxoIndex = Convert.ToInt32(index);
                if (metadata.TryGetValue("Price", out var price))
                {
                    if (!string.IsNullOrEmpty(price))
                    {
                        price = price.Replace(',', '.');
                        Price = double.Parse(price, CultureInfo.InvariantCulture);
                        PriceActive = true;
                    }
                    else
                    {
                        PriceActive = false;
                    }
                }
                else
                {
                    PriceActive = false;
                }
            }
        }

        public override Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            throw new NotImplementedException();
        }

        public override async Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            // create token metadata
            var metadata = new Dictionary<string, string>();
            metadata.Add("NFT", "true");
            metadata.Add("Type", "NFT Payment");
            metadata.Add("Sender", Sender);
            metadata.Add("NFTUtxoTxId", NFTUtxoTxId);
            metadata.Add("NFTUtxoIndex", NFTUtxoIndex.ToString());
            metadata.Add("Image", ImageLink);
            metadata.Add("Link", Link);
            metadata.Add("Price", Price.ToString(CultureInfo.InvariantCulture));

            return metadata;
        }
    }
}
