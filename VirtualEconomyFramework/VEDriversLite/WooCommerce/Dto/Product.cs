using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.WooCommerce.Dto
{
    public class CategoryOfDownloads
    {
        public int id { get; set; } = 0;
    }
    public class DownloadsObject
    {
        public string name { get; set; } = string.Empty;
        public string file { get; set; } = string.Empty;
        public string external_url { get; set; } = string.Empty;
        public List<CategoryOfDownloads> categories { get; set; } = new List<CategoryOfDownloads>();
    }
    public class ImageObject
    {
        public int id { get; set; } = 0;
        public string src { get; set; } = string.Empty;
    }
    public class ProductMetadata
    {
        public string key { get; set; } = string.Empty;
        public object value { get; set; }
    }
    public enum StockStatus
    {
        instock,
        outofstock,
        onbackorder
    }
    public class Product
    {
        public int id { get; set; } = 0;
        public string name { get; set; } = string.Empty;
        public string permalink { get; set; } = string.Empty;
        public string slug { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string regular_price { get; set; } = string.Empty;
        public string status { get; set; } = "publish";
        public bool _virtual { get; set; } = true;
        public bool enable_html_description { get; set; } = false;
        public bool downloadable { get; set; } = true;
        public string type { get; set; } = "simple";
        public int? stock_quantity { get; set; } = 1;
        public bool manage_stock { get; set; } = false;
        public string stock_status { get; set; } = "instock";
        public StockStatus stock_status_enum
        {
            get => (StockStatus)Enum.Parse(typeof(StockStatus), stock_status);
            set => stock_status = value.ToString();
        }
        public string short_description { get; set; } = string.Empty;
        public List<DownloadsObject> downloads { get; set; } = new List<DownloadsObject>();
        public List<ImageObject> images { get; set; } = new List<ImageObject>();
        public List<ProductMetadata> meta_data { get; set; } = new List<ProductMetadata>();
        public List<Category> categories { get; set; } = new List<Category>();
        public void Fill(Product product)
        {
            id = product.id;
            name = product.name;
            slug = product.slug;
            permalink = product.permalink;
            status = product.status;
            description = product.description;
            regular_price = product.regular_price;
            _virtual = product._virtual;
            enable_html_description = product.enable_html_description;
            downloadable = product.downloadable;
            type = product.type;
            stock_quantity = product.stock_quantity;
            manage_stock = product.manage_stock;
            stock_status = product.stock_status;
            stock_status_enum = product.stock_status_enum;
            short_description = product.short_description;
            downloads = product.downloads;
            images = product.images;
            meta_data = product.meta_data;
            categories = product.categories;
        }
    }
}
