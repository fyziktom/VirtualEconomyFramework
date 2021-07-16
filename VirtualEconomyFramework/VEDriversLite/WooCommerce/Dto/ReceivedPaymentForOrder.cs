using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.WooCommerce.Dto
{
    public class ReceivedPaymentForOrder
    {
        public int OrderId { get; set; } = 0;
        public string OrderKey { get; set; } = string.Empty;
        public string PaymentId { get; set; } = string.Empty;
        public double Amount { get; set; } = 0.0;
        public string Currency { get; set; } = string.Empty;
        public string CustomerAddress { get; set; } = string.Empty;
        public string NeblioCustomerAddress { get; set; } = string.Empty;
    }
}
