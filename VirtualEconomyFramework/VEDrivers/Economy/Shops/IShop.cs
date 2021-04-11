using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Tokens;

namespace VEDrivers.Economy.Shops
{
    public enum ShopTypes
    {
        NeblioTokenShop
    }
    public interface IShop
    {
        ShopTypes Type { get; set; }
        string Name { get; set; }
        string Address { get; set; }
        string Description { get; set; }
        string Link { get; set; }
        string OwnerName { get; set; }
        bool IsActive { get; set; }
        bool IsSettingFound { get; set; }
        bool IsRunning { get; set; }
        IToken SettingToken { get; set; }
        List<IShopItem> ShopItems { get; set; }

        Task<string> GetShopComponent();
        Task<string> StartShop();
        
    }
}
