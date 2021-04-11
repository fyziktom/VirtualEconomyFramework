using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Database;

namespace VEDrivers.Economy.Wallets.Handlers
{
    public interface IWalletHandler
    {
        Task<string> UpdateWallet(Guid id, Guid ownerid, string walletName, WalletTypes type, string urlBase, int port, IDbConnectorService dbservice);
        Task<bool> LoadWalletsFromDb(IDbConnectorService dbservice);
        Task RefreshWallets();
        bool ReloadAccounts();
    }
}
