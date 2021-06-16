using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NeblioAPI;

namespace VEDriversLite.NFT.Coruzant
{
    public abstract class CommonCoruzantNFT : CommonNFT
    {
        public new string TokenId { get; set; } = "La9ADonmDwxsNKJGvnRWy8gmWmeo72AEeg8cK7"; // Coruzant tokens as default
    }
}
