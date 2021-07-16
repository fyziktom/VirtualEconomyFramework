using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.WooCommerce.Dto
{
    public class Shipping
    {
        public string first_name { get; set; } = string.Empty;
        public string last_name { get; set; } = string.Empty;
        public string company { get; set; } = string.Empty;
        public string address_1 { get; set; } = string.Empty;
        public string address_2 { get; set; } = string.Empty;
        public string city { get; set; } = string.Empty;
        public string state { get; set; } = string.Empty;
        public string postcode { get; set; } = string.Empty;
        public string country { get; set; } = string.Empty;
        //public string email { get; set; } = string.Empty;
        //public string phone { get; set; } = string.Empty;
    }
    public class Billing
    {
        public string first_name { get; set; } = string.Empty;
        public string last_name { get; set; } = string.Empty;
        public string company { get; set; } = string.Empty;
        public string address_1 { get; set; } = string.Empty;
        public string address_2 { get; set; } = string.Empty;
        public string city { get; set; } = string.Empty;
        public string state { get; set; } = string.Empty;
        public string postcode { get; set; } = string.Empty;
        public string country { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string phone { get; set; } = string.Empty;
    }
    public class LineItem
    {
        public int id { get; set; } = 0;
        public string name { get; set; } = string.Empty;
        public int product_id { get; set; } = 0;
        public int variation_id { get; set; } = 0;
        public int quantity { get; set; } = 0;
        public string subtotal { get; set; } = string.Empty;
        public string subtotal_tax { get; set; } = string.Empty;
        public string total { get; set; } = string.Empty;
        public string total_tax { get; set; } = string.Empty;
        public List<ProductMetadata> meta_data { get; set; } = new List<ProductMetadata>();
        public double price { get; set; } = 0;
    }
    public enum OrderStatus
    {
        pending,
        processing,
        onhold,
        completed,
        cancelled,
        refunded,
        failed,
        trash
    }
    public class Order
    {
        public int id { get; set; } = 0;
        public int parent_id { get; set; } = 0;
        public string number { get; set; } = string.Empty;
        public string order_key { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
        public OrderStatus statusclass
        {
            get
            {
                try
                {
                    var s = Enum.Parse(typeof(OrderStatus), status);
                    return (OrderStatus)s;
                }
                catch
                {
                    if (status == "on-hold")
                        return OrderStatus.onhold;
                }
                return OrderStatus.pending;
            }
            set
            {
                var n = value.ToString();
                if (n == "onhold")
                    n = "on-hold";
                status = n;
            }
        }
        public string currency { get; set; } = string.Empty;
        public DateTime date_created { get; set; }
        public DateTime date_modified { get; set; }
        public string discount_total { get; set; } = string.Empty;
        public string discount_tax { get; set; } = string.Empty;
        public string shipping_total { get; set; } = string.Empty;
        public string shipping_tax { get; set; } = string.Empty;
        public string total { get; set; } = string.Empty;
        public string total_tax { get; set; } = string.Empty;
        public string payment_method { get; set; } = string.Empty;
        public string payment_method_title { get; set; } = string.Empty;
        public string transaction_id { get; set; } = string.Empty;
        public string date_paid { get; set; } = string.Empty;
        public string date_paid_gmt { get; set; } = string.Empty;
        public Billing billing { get; set; } = new Billing();
        public Shipping shipping { get; set; } = new Shipping();
        public List<ProductMetadata> meta_data { get; set; } = new List<ProductMetadata>();
        public List<LineItem> line_items { get; set; } = new List<LineItem>();
    }
}
