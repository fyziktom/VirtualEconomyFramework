using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Extensions.WooCommerce.Dto
{
    /// <summary>
    /// TODO
    /// </summary>
    public class Shipping
    {
        /// <summary>
        /// TODO
        /// </summary>
        public string first_name { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string last_name { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string company { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string address_1 { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string address_2 { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string city { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string state { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string postcode { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string country { get; set; } = string.Empty;
        //public string email { get; set; } = string.Empty;
        //public string phone { get; set; } = string.Empty;
    }
    /// <summary>
    /// TODO
    /// </summary>
    public class Billing
    {
        /// <summary>
        /// TODO
        /// </summary>
        public string first_name { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string last_name { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string company { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string address_1 { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string address_2 { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string city { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string state { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string postcode { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string country { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string email { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string phone { get; set; } = string.Empty;
    }
    /// <summary>
    /// TODO
    /// </summary>
    public class LineItem
    {
        /// <summary>
        /// TODO
        /// </summary>
        public int id { get; set; } = 0;
        /// <summary>
        /// TODO
        /// </summary>
        public string name { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public int product_id { get; set; } = 0;
        /// <summary>
        /// TODO
        /// </summary>
        public int variation_id { get; set; } = 0;
        /// <summary>
        /// TODO
        /// </summary>
        public int quantity { get; set; } = 0;
        /// <summary>
        /// TODO
        /// </summary>
        public string subtotal { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string subtotal_tax { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string total { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string total_tax { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public List<ProductMetadata> meta_data { get; set; } = new List<ProductMetadata>();
        /// <summary>
        /// TODO
        /// </summary>
        public double price { get; set; } = 0;
    }
    /// <summary>
    /// TODO
    /// </summary>
    public enum OrderStatus
    {
        /// <summary>
        /// TODO
        /// </summary>
        pending,
        /// <summary>
        /// TODO
        /// </summary>        
        processing,
        /// <summary>
        /// TODO
        /// </summary>        
        onhold,
        /// <summary>
        /// TODO
        /// </summary>
        completed,
        /// <summary>
        /// TODO
        /// </summary>        
        cancelled,
        /// <summary>
        /// TODO
        /// </summary>
        refunded,
        /// <summary>
        /// TODO
        /// </summary>
        failed,
        /// <summary>
        /// TODO
        /// </summary>        
        trash
    }
    /// <summary>
    /// TODO
    /// </summary>
    public class Order
    {
        /// <summary>
        /// TODO
        /// </summary>
        public int id { get; set; } = 0;
        /// <summary>
        /// TODO
        /// </summary>
        public int parent_id { get; set; } = 0;
        /// <summary>
        /// TODO
        /// </summary>
        public string number { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string order_key { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string status { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public OrderStatus statusclass
        {
            get
            {
                if (!string.IsNullOrEmpty(status))
                {
                    var st = status.Replace("on-hold", "onhold");
                    var s = Enum.Parse(typeof(OrderStatus), st);
                    return (OrderStatus)s;
                }
                return OrderStatus.onhold;
            }
            set
            {
                var n = value.ToString();
                if (n == "onhold")
                    n = "on-hold";
                status = n;
            }
        }
        /// <summary>
        /// TODO
        /// </summary>
        public string currency { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public DateTime? date_created { get; set; }
        /// <summary>
        /// TODO
        /// </summary>
        public DateTime? date_modified { get; set; }
        /// <summary>
        /// TODO
        /// </summary>
        public string discount_total { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string discount_tax { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string shipping_total { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string shipping_tax { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string total { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string total_tax { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string payment_method { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string payment_method_title { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string transaction_id { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public DateTime? date_paid { get; set; }
        /// <summary>
        /// TODO
        /// </summary>
        public DateTime? date_paid_gmt { get; set; }
        /// <summary>
        /// TODO
        /// </summary>
        public Billing billing { get; set; } = new Billing();
        /// <summary>
        /// TODO
        /// </summary>
        public Shipping shipping { get; set; } = new Shipping();
        /// <summary>
        /// TODO
        /// </summary>
        public List<ProductMetadata> meta_data { get; set; } = new List<ProductMetadata>();
        /// <summary>
        /// TODO
        /// </summary>
        public List<LineItem> line_items { get; set; } = new List<LineItem>();

        /// <summary>
        /// Fill the Dto
        /// </summary>
        /// <param name="order"></param>
        public void Fill(Order order)
        {
            id = order.id;
            parent_id = order.parent_id;
            number = order.number;
            order_key = order.order_key;
            status = order.status;
            statusclass = order.statusclass;
            currency = order.currency;
            date_created = order.date_created;
            date_modified = order.date_modified;
            discount_total = order.discount_total;
            discount_tax = order.discount_tax;
            shipping_total = order.shipping_total;
            shipping_tax = order.shipping_tax;
            total = order.total;
            total_tax = order.total_tax;
            payment_method = order.payment_method;
            payment_method_title = order.payment_method_title;
            transaction_id = order.transaction_id;
            total_tax = order.total_tax;
            date_paid = order.date_paid;
            date_paid_gmt = order.date_paid_gmt;
            billing = order.billing;
            shipping = order.shipping;
            meta_data = order.meta_data;
            line_items = new List<LineItem>();
            order.line_items.ForEach(item =>
            {
                var lineitem = new LineItem();
                lineitem = item;
                lineitem.meta_data = item.meta_data;
                line_items.Add(lineitem);
            });
        }
    }
}
