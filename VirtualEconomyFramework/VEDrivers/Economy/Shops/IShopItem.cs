using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Economy.Shops
{
    public interface IShopItem
    {
        string Name { get; set; }
        string Description { get; set; }
        double Price { get; set; }
        double Lot { get; set; }
        double MaxSupply { get; set; }
        bool IsActive { get; set; }
        bool IsLotDynamic { get; set; }
    }
}
