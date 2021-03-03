using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Economy.Tokens
{
    public class NeblioNTP1Token : CommonToken
    {
        public NeblioNTP1Token()
        {

        }

        public override async Task<string> GetDetails()
        {
            return await Task.FromResult("OK");
        }
    }
}
