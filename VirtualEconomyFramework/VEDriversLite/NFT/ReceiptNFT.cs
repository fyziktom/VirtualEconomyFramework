using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{
    /// <summary>
    /// Receipt NFT
    /// Used for example for receipts for sold NFTs
    /// </summary>
    public class ReceiptNFT : CommonNFT
    {
        /// <summary>
        /// Create empty NFT class
        /// </summary>        
        public ReceiptNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Receipt;
            TypeText = "NFT Receipt";
        }
        /// <summary>
        /// original NFT hash which was sold
        /// </summary>
        public string NFTUtxoTxId { get; set; } = string.Empty;
        /// <summary>
        /// original NFT index which was sold
        /// </summary>
        public int NFTUtxoIndex { get; set; } = 0;
        /// <summary>
        /// Sender of the NFT
        /// </summary>
        public string Sender { get; set; } = string.Empty;
        /// <summary>
        /// Buyer of the NFT
        /// </summary>
        public string Buyer { get; set; } = string.Empty;
        /// <summary>
        /// Original payment transaction hash
        /// </summary>
        public string OriginalPaymentTxId { get; set; } = string.Empty;
        /// <summary>
        /// Receipt from the payment Utxo
        /// </summary>
        public string ReceiptFromPaymentUtxo { get; set; } = string.Empty;
        /// <summary>
        /// Sold price for this NFT
        /// </summary>
        public double SoldPrice { get; set; } = 0.0;
        /// <summary>
        /// Fill basic parameters
        /// </summary>
        /// <param name="NFT"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Parse specific parameters
        /// </summary>
        /// <param name="metadata"></param>
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
