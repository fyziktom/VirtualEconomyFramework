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
                case TokenTypes.NTP1:
                    var tok = new NeblioNTP1Token("") { Name = name, Symbol = symbol, ActualBalance = totalbalance }; // todo get Id by symbol
                    return tok;
            }

            return null;
        }

        public static IToken GetTokenById(TokenTypes type, string name, string tokenId, double totalbalance)
        {
            switch (type)
            {
                case TokenTypes.Common:
                    return null;
                case TokenTypes.NTP1:
                    var tok = new NeblioNTP1Token(tokenId) { Name = name, Id = tokenId, ActualBalance = totalbalance };
                    return tok;
            }

            return null;
        }
    }
}
