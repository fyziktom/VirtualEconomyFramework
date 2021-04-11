using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Tokens;

namespace VEDrivers.Economy.Shops
{
    public class TokenShopItem : CommonShopItem
    {
        public TokenShopItem(string name, string description, string tokenId, TokenTypes tokenType, double price = 1.0, double lot = 1)
        {
            Name = name;
            Description = description;
            Price = price;
            Lot = lot;
            Token = TokenFactory.GetTokenById(tokenType, name, tokenId, 0.0);
        }

        public IToken Token { get; set; }

    }
}
