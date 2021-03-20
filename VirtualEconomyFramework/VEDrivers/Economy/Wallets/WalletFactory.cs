using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Economy.Wallets
{
    public static class WalletFactory
    {
        public static IWallet GetWallet(Guid id, Guid owner, WalletTypes type, string name, bool useRPC, string baseURL, int port)
        {
            switch (type)
            {
                case WalletTypes.Bitcoin:
                    return null;
                    break;
                case WalletTypes.Neblio:
                    return new NeblioWallet(id, owner, name, useRPC, baseURL, port);
                    break;
            }

            return null;
        }
    }
}
