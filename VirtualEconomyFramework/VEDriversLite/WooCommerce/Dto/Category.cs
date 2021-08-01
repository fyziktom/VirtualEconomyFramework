using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.WooCommerce.Dto
{
    public class Category
    {
        public int id { get; set; } = 0;
        public string name { get; set; } = string.Empty;
        public string slug { get; set; } = string.Empty;
        public string description { get; set; } = string.Empty;
        public string display { get; set; } = "default";
        public int count { get; set; } = 0;

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
