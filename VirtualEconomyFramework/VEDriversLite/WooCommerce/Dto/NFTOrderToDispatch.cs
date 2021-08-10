using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.NFT;

namespace VEDriversLite.WooCommerce.Dto
{
    public class NFTOrderToDispatch
    {
        public int OrderId { get; set; } = 0;
        public string OrderKey { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public string Currency { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string NeblioCustomerAddress { get; set; } = string.Empty;
        public bool IsUnique { get; set; } = false;
        public string ShortHash { get; set; } = string.Empty;
        public string Utxo { get; set; } = string.Empty;
        public int UtxoIndex { get; set; } = 0;
        public bool IsCategory { get; set; } = false;
        public string Category { get; set; } = string.Empty;
        public int Quantity { get; set; } = 0;
        public double TotalPriceForLineItem { get; set; } = 0.0;
        public string OwnerMainAccount { get; set; } = string.Empty;
        public string OwnerSubAccount { get; set; } = string.Empty;
    }
}
