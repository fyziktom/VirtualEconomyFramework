using System;
using System.Collections.Generic;
using System.Text;
using VEDriversLite.NFT;

namespace VEDriversLite.WooCommerce.Dto
{
    /// <summary>
    /// NFT Order which should be send to the client
    /// </summary>
    public class NFTOrderToDispatch
    {
        /// <summary>
        /// Woo Commerce Order Id
        /// </summary>
        public int OrderId { get; set; } = 0;
        /// <summary>
        /// Woo Commerce Order Key
        /// </summary>
        public string OrderKey { get; set; } = string.Empty;
        /// <summary>
        /// Payment hash or ID
        /// </summary>
        public string PaymentId { get; set; } = string.Empty;
        /// <summary>
        /// Payment currency
        /// </summary>
        public string Currency { get; set; } = string.Empty;
        /// <summary>
        /// Customer real address
        /// </summary>
        public string CustomerAddress { get; set; } = string.Empty;
        /// <summary>
        /// Customer Neblio Address where to send the NFT
        /// </summary>
        public string NeblioCustomerAddress { get; set; } = string.Empty;
        /// <summary>
        /// Is it unique NFT?
        /// </summary>
        public bool IsUnique { get; set; } = false;
        /// <summary>
        /// Shorten hash of the NFT to send
        /// </summary>
        public string ShortHash { get; set; } = string.Empty;
        /// <summary>
        /// Utxo of the NFT
        /// </summary>
        public string Utxo { get; set; } = string.Empty;
        /// <summary>
        /// Utxo index of the NFT
        /// </summary>
        public int UtxoIndex { get; set; } = 0;
        /// <summary>
        /// Is it whole category?
        /// </summary>
        public bool IsCategory { get; set; } = false;
        /// <summary>
        /// number of category
        /// </summary>
        public string Category { get; set; } = string.Empty;
        /// <summary>
        /// Quantity of the NFT for mutliple
        /// </summary>
        public int Quantity { get; set; } = 0;
        /// <summary>
        /// Total price per one line item
        /// </summary>
        public double TotalPriceForLineItem { get; set; } = 0.0;
        /// <summary>
        /// Owner main account address
        /// </summary>
        public string OwnerMainAccount { get; set; } = string.Empty;
        /// <summary>
        /// Owner sub account address
        /// </summary>
        public string OwnerSubAccount { get; set; } = string.Empty;
    }
}
