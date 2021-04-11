using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Economy.Shops
{
    public abstract class CommonShopItem : IShopItem
    {
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public double Price { get; set; } = 0.0;
        public double Lot { get; set; } = 0.0;
        public double MaxSupply { get; set; } = 0.0;
        public bool IsActive { get; set; } = false;
        public bool IsLotDynamic { get; set; } = false;
    }
}
