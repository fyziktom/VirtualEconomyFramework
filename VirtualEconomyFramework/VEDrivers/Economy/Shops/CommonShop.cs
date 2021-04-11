using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Tokens;

namespace VEDrivers.Economy.Shops
{
    public abstract class CommonShop : IShop
    {
        public ShopTypes Type { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Link { get; set; } = string.Empty;
        public string OwnerName { get; set; } = string.Empty;
        public bool IsActive { get; set; } = false;
        public bool IsSettingFound { get; set; }
        public bool IsRunning { get; set; }
        public IToken SettingToken { get; set; } = null;
        public List<IShopItem> ShopItems { get; set; } = new List<IShopItem>();

        public abstract Task<string> GetShopComponent();

        public abstract Task<string> StartShop();
        
    }
}
