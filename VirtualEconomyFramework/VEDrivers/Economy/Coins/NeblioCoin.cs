using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Economy.Coins
{
    public class NeblioCoin : CommonCoin
    {
        public NeblioCoin()
        {
            Name = "Neblio";
            Symbol = "NEBL";
            BaseURL = "https://ntp1.node.nebl.io/";
        }

        public override async Task<string> GetDetails()
        {
            return await Task.FromResult("OK");
        }
    }
}
