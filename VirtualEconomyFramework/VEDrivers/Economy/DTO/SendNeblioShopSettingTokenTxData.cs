using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Shops;

namespace VEDrivers.Economy.DTO
{
    /// <summary>
    /// Data carrier for sending token
    /// </summary>
    public class SendNeblioShopSettingTokenTxData : SendTokenTxData
    {
        public SendNeblioShopSettingTokenTxData()
        {
            Metadata = new Dictionary<string, string>();
        }

        public string ShopName { get; set; } = string.Empty;
        public string ShopDescription { get; set; } = string.Empty;
        public string TokenId { get; set; } = string.Empty;
        public List<TokenShopItem> ShopItems { get; set; } = new List<TokenShopItem>();
    }
}
