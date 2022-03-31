using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.NFT
{

    public class OrderNFT : CommonNFT
    {
        public OrderNFT(string utxo)
        {
            Utxo = utxo;
            Type = NFTTypes.Order;
            TypeText = "NFT Order";
        }
        public List<InvoiceItem> InvoiceItems { get; set; } = new List<InvoiceItem>();

        public string SellerProfileNFT { get; set; } = string.Empty;
        public string BuyerProfileNFT { get; set; } = string.Empty;
        public string OriginalPaymentTxId { get; set; } = string.Empty;
        public string FileLink { get; set; } = string.Empty;
        public double TotalPrice { get; set; } = 0.0;
        public string ApprovedByProfile { get; set; } = string.Empty;
        public string ApprovedNote { get; set; } = string.Empty;
        public bool Aprooved { get; set; } = false;
        public bool AlreadyPaid { get; set; } = false;

        public DateTime ExposeDate { get; set; } = DateTime.UtcNow;
        public int MaxCountOfDaysFromExposeToPayment { get; set; } = 30;
        public int MaxCountOfDaysAfterPaymentDate { get; set; } = 30;

        public override async Task Fill(INFT NFT)
        {
            await FillCommon(NFT);

            var pnft = NFT as OrderNFT;
            InvoiceItems = pnft.InvoiceItems;
            SellerProfileNFT = pnft.SellerProfileNFT;
            TotalPrice = pnft.TotalPrice;
            BuyerProfileNFT = pnft.BuyerProfileNFT;
            OriginalPaymentTxId = pnft.OriginalPaymentTxId;
            AlreadyPaid = pnft.AlreadyPaid;
            ExposeDate = pnft.ExposeDate;
            FileLink = pnft.FileLink;
            ApprovedByProfile = pnft.ApprovedByProfile;
            ApprovedNote = pnft.ApprovedNote;
            MaxCountOfDaysFromExposeToPayment = pnft.MaxCountOfDaysFromExposeToPayment;
            MaxCountOfDaysAfterPaymentDate = pnft.MaxCountOfDaysAfterPaymentDate;
            Aprooved = pnft.Aprooved;
        }

        public override void ParseSpecific(IDictionary<string, string> metadata)
        {
            if (metadata.TryGetValue("Seller", out var seller))
                SellerProfileNFT = seller;
            if (metadata.TryGetValue("Buyer", out var buyer))
                BuyerProfileNFT = buyer;
            if (metadata.TryGetValue("FileLink", out var filelink))
                FileLink = filelink;
            if (metadata.TryGetValue("ApprovedByProfile", out var appby))
                ApprovedByProfile = appby;
            if (metadata.TryGetValue("ApprovedNote", out var appnote))
                ApprovedNote = appnote;
            if (metadata.TryGetValue("ExposeDay", out var date))
            {
                try
                {
                    ExposeDate = DateTime.Parse(date);
                }
                catch
                {
                    Console.WriteLine("Cannot parse NFT Ticket Event Date");
                }
            }
            if (metadata.TryGetValue("Items", out var items))
            {
                if (!string.IsNullOrEmpty(items))
                {
                    try
                    {
                        var itms = JsonConvert.DeserializeObject<List<InvoiceItem>>(items);
                        if (itms != null)
                            InvoiceItems = itms;
                    }
                    catch
                    {
                        Console.WriteLine($"Cannot deserialize the Invoice Items of InvoiceNFT: {Utxo}:{UtxoIndex}");
                        InvoiceItems = new List<InvoiceItem>();
                    }
                }
                else
                {
                    InvoiceItems = new List<InvoiceItem>();
                }
            }
            if (metadata.TryGetValue("TotalPrice", out var totalPrice))
                if (!string.IsNullOrEmpty(totalPrice))
                    TotalPrice = Convert.ToDouble(totalPrice, CultureInfo.InvariantCulture);
            if (metadata.TryGetValue("MaxCountOfDaysFromExposeToPayment", out var maxCountOfDaysFromExposeToPayment))
                if (!string.IsNullOrEmpty(maxCountOfDaysFromExposeToPayment))
                    TotalPrice = Convert.ToInt32(maxCountOfDaysFromExposeToPayment, CultureInfo.InvariantCulture);

            if (metadata.TryGetValue("MaxCountOfDaysAfterPaymentDate", out var maxCountOfDaysAfterPaymentDate))
                if (!string.IsNullOrEmpty(maxCountOfDaysAfterPaymentDate))
                    TotalPrice = Convert.ToDouble(MaxCountOfDaysAfterPaymentDate, CultureInfo.InvariantCulture);

            if (metadata.TryGetValue("OriginalPaymentTxId", out var optxid))
                OriginalPaymentTxId = optxid;

            if (!string.IsNullOrEmpty(OriginalPaymentTxId))
                AlreadyPaid = true;

        }

        public override Task ParseOriginData(IDictionary<string, string> lastmetadata)
        {
            throw new NotImplementedException();
        }

        public override async Task<IDictionary<string, string>> GetMetadata(string address = "", string key = "", string receiver = "")
        {
            // create token metadata
            var metadata = await GetCommonMetadata();
            metadata.Add("Seller", SellerProfileNFT);
            metadata.Add("Buyer", BuyerProfileNFT);
            metadata.Add("ExposeDate", ExposeDate.ToString());
            if (!string.IsNullOrEmpty(FileLink))
                metadata.Add("FileLink", FileLink);
            if (!string.IsNullOrEmpty(ApprovedByProfile))
                metadata.Add("ApprovedByProfile", ApprovedByProfile);
            if (!string.IsNullOrEmpty(ApprovedNote))
                metadata.Add("ApprovedNote", ApprovedNote);

            if (InvoiceItems.Count > 0)
            {
                metadata.Add("Items", JsonConvert.SerializeObject(InvoiceItems));
            }
            if (!string.IsNullOrEmpty(OriginalPaymentTxId))
                metadata.Add("OriginalPaymentTxId", OriginalPaymentTxId);

            if (TotalPrice > 0.0)
                metadata.Add("TotalPrice", Convert.ToString(TotalPrice,CultureInfo.InvariantCulture));
            return metadata;
        }

        public void AddInvoiceItem(string utxo, int index, double price, int amount)
        {
            var invit = new InvoiceItem()
            {
                ItemUtxo = utxo,
                ItemUtxoIndex = index,
                ItemPrice = price,
                ItemCount = amount
            };
            InvoiceItems.Add(invit);
            TotalPrice += price * amount;
        }
    }
}
