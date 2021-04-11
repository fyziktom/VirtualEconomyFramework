using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Database;

namespace VEDrivers.Economy.Wallets.Handlers
{
    public abstract class CommonWalletHandler : IWalletHandler
    {
        public abstract Task<bool> LoadWalletsFromDb(IDbConnectorService dbservice);

        public abstract Task RefreshWallets();

        public abstract bool ReloadAccounts();

        public abstract Task<string> UpdateWallet(Guid id, Guid ownerid, string walletName, WalletTypes type, string urlBase, int port, IDbConnectorService dbservice);
    }
}
