using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Extensions.WooCommerce.Dto
{
    /// <summary>
    /// Woo commerce shop category dto
    /// </summary>
    public class Category
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
        public string slug { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string description { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string display { get; set; } = "default";
        /// <summary>
        /// TODO
        /// </summary>
        public int count { get; set; } = 0;

        /// <summary>
        /// Fill the Dto
        /// </summary>
        /// <param name="category"></param>
        public void Fill(Category category)
        {
            id = category.id;
            name = category.name;
            slug = category.slug;
            description = category.description;
            display = category.display;
            count = category.count;
        }
    }
}
