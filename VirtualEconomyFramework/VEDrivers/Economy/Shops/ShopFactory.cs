using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Economy.Shops
{
    public static class ShopFactory
    {
        public static IShop GetShop(ShopTypes type, string address, string tokenId)
        {
            switch (type)
            {
                case ShopTypes.NeblioTokenShop:
                    var shop = new NeblioTokenShop(address, tokenId);
                    return shop;
            }

            return null;
        }
    }
}
