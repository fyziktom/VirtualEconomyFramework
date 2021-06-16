using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.NFT.Coruzant.Dto
{
    public class CoruzantContentAddressDto
    {
        public string Address { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string ImageLink { get; set; } = string.Empty;
        public string ProfileLink { get; set; } = string.Empty;
        public List<string> Tags { get; set; } = new List<string>();

    }
}
