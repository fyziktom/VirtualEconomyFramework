using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{
    public class ReceiptNFT : CommonNFT
    {
        public ReceiptNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Receipt;
            TypeText = "NFT Receipt";
        }

        public string NFTUtxoTxId { get; set; } = string.Empty;
        public int NFTUtxoIndex { get; set; } = 0;
        public string Sender { get; set; } = string.Empty;
        public string Buyer { get; set; } = string.Empty;
        public string OriginalPaymentTxId { get; set; } = string.Empty;
        public string ReceiptFromPaymentUtxo { get; set; } = string.Empty;
        public double SoldPrice { get; set; } = 0.0;

        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);

            var pnft = NFT as ReceiptNFT;
            NFTUtxoTxId = pnft.NFTUtxoTxId;
            NFTUtxoIndex = pnft.NFTUtxoIndex;
            Sender = pnft.Sender;
            SoldPrice = pnft.SoldPrice;
            Buyer = pnft.Buyer;
            OriginalPaymentTxId = pnft.OriginalPaymentTxId;
            ReceiptFromPaymentUtxo = pnft.ReceiptFromPaymentUtxo;
        }

        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            if (metadata.TryGetValue("NFTUtxoTxId", out var nfttxid))
            {
                NFTUtxoTxId = nfttxid;
                NFTOriginTxId = nfttxid;
            }
            if (metadata.TryGetValue("Sender", out var sender))
                Sender = sender;
            if (metadata.TryGetValue("NFTUtxoIndex", out var index))
                if (!string.IsNullOrEmpty(index))
                    NFTUtxoIndex = Convert.ToInt32(index);
            if (metadata.TryGetValue("SoldPrice", out var soldprice))
            {
                if (!string.IsNullOrEmpty(soldprice))
                {
                    var prc = soldprice.Replace(',', '.');
                    SoldPrice = Convert.ToDouble(prc, CultureInfo.InvariantCulture);
                }
            }

            if (metadata.TryGetValue("ReceiptFromPaymentUtxo", out var rfp))
                ReceiptFromPaymentUtxo = rfp;
            if (metadata.TryGetValue("OriginalPaymentTxId", out var optxid))
                OriginalPaymentTxId = optxid;

            Buyer = NeblioTransactionHelpers.GetTransactionReceiver(Utxo, TxDetails).GetAwaiter().GetResult();
        }

        public override Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            throw new NotImplementedException();
        }

        public override async Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            // create token metadata
            var metadata = await GetCommonMetadata();
            metadata.Add("Sender", Sender);
            metadata.Add("NFTUtxoTxId", NFTUtxoTxId);
            metadata.Add("NFTUtxoIndex", NFTUtxoIndex.ToString());
            if (!string.IsNullOrEmpty(OriginalPaymentTxId))
                metadata.Add("OriginalPaymentTxId", OriginalPaymentTxId);
            if (!string.IsNullOrEmpty(ReceiptFromPaymentUtxo))
                metadata.Add("ReceiptFromPaymentUtxo", ReceiptFromPaymentUtxo);
            if (SoldPrice > 0.0)
                metadata.Add("SoldPrice", Convert.ToString(SoldPrice,CultureInfo.InvariantCulture));
            return metadata;
        }
    }
}
