using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Economy.Tokens
{
    public static class TokenFactory
    {
        public static IToken GetToken(TokenTypes type, string name, string symbol, double totalbalance)
        {
            switch (type)
            {
                case TokenTypes.Common:
                    return null;
                    break;
                case TokenTypes.NTP1:
                    var acc = new NeblioNTP1Token() { Name = name, Symbol = symbol, ActualBalance = totalbalance };
                    return acc;
            }

            return null;
        }
    }
}
