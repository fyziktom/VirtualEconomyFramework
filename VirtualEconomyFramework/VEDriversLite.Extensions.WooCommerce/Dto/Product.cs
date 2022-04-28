using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Extensions.WooCommerce.Dto
{
    /// <summary>
    /// TODO
    /// </summary>
    public class CategoryOfDownloads
    {
        /// <summary>
        /// TODO
        /// </summary>
        public int id { get; set; } = 0;
    }
    /// <summary>
    /// TODO
    /// </summary>
    public class DownloadsObject
    {
        /// <summary>
        /// TODO
        /// </summary>
        public string name { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string file { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string external_url { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public List<CategoryOfDownloads> categories { get; set; } = new List<CategoryOfDownloads>();
    }
    /// <summary>
    /// TODO
    /// </summary>
    public class ImageObject
    {
        /// <summary>
        /// TODO
        /// </summary>
        public int id { get; set; } = 0;
        /// <summary>
        /// TODO
        /// </summary>
        public string src { get; set; } = string.Empty;
    }
    /// <summary>
    /// TODO
    /// </summary>
    public class ProductMetadata
    {
        /// <summary>
        /// TODO
        /// </summary>
        public string key { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public object value { get; set; }
    }
    /// <summary>
    /// TODO
    /// </summary>
    public enum StockStatus
    {
        /// <summary>
        /// TODO
        /// </summary>
        instock,
        /// <summary>
        /// TODO
        /// </summary>
        outofstock,
        /// <summary>
        /// TODO
        /// </summary>
        onbackorder
    }
    /// <summary>
    /// TODO
    /// </summary>
    public class Product
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
        public string permalink { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string slug { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string description { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string regular_price { get; set; } = "0.0";
        /// <summary>
        /// TODO
        /// </summary>
        public string status { get; set; } = "publish";
        /// <summary>
        /// TODO
        /// </summary>
        public bool _virtual { get; set; } = true;
        /// <summary>
        /// TODO
        /// </summary>
        public bool enable_html_description { get; set; } = false;
        /// <summary>
        /// TODO
        /// </summary>
        public bool downloadable { get; set; } = true;
        /// <summary>
        /// TODO
        /// </summary>
        public string type { get; set; } = "simple";
        /// <summary>
        /// TODO
        /// </summary>
        public int? stock_quantity { get; set; } = 1;
        /// <summary>
        /// TODO
        /// </summary>
        public bool manage_stock { get; set; } = true;
        /// <summary>
        /// TODO
        /// </summary>
        public string stock_status { get; set; } = "instock";
        /// <summary>
        /// TODO
        /// </summary>
        public StockStatus stock_status_enum
        {
            get => (StockStatus)Enum.Parse(typeof(StockStatus), stock_status);
            set => stock_status = value.ToString();
        }
        /// <summary>
        /// TODO
        /// </summary>
        public string short_description { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public List<DownloadsObject> downloads { get; set; } = new List<DownloadsObject>();
        /// <summary>
        /// TODO
        /// </summary>
        public List<ImageObject> images { get; set; } = new List<ImageObject>();
        /// <summary>
        /// TODO
        /// </summary>
        public List<ProductMetadata> meta_data { get; set; } = new List<ProductMetadata>();
        /// <summary>
        /// TODO
        /// </summary>
        public List<Category> categories { get; set; } = new List<Category>();
        /// <summary>
        /// Fill the dto
        /// </summary>
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
