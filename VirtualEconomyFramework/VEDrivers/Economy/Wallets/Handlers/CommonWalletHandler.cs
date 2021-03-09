using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Economy.Wallets.Handlers
{
    public abstract class CommonWalletHandler : IWalletHandler
    {
        public abstract bool LoadWalletsFromDb();

        public abstract Task RefreshWallets();

        public abstract bool ReloadAccounts();

        public abstract Task<string> UpdateWallet(Guid id, Guid ownerid, string walletName, WalletTypes type, string urlBase, int port);
    }
}
