using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.WooCommerce.Dto
{
    /// <summary>
    /// Received payment for order in woocommerce
    /// </summary>
    public class ReceivedPaymentForOrder
    {
        /// <summary>
        /// TODO
        /// </summary>
        public int OrderId { get; set; } = 0;
        /// <summary>
        /// TODO
        /// </summary>
        public string OrderKey { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string PaymentId { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public double Amount { get; set; } = 0.0;
        /// <summary>
        /// TODO
        /// </summary>
        public string Currency { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string CustomerAddress { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string NeblioCustomerAddress { get; set; } = string.Empty;
    }
}
