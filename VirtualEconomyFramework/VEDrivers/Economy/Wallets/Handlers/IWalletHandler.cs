using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Economy.Wallets.Handlers
{
    public interface IWalletHandler
    {
        Task<string> UpdateWallet(Guid id, Guid ownerid, string walletName, WalletTypes type, string urlBase, int port);
        bool LoadWalletsFromDb();
        Task RefreshWallets();
        bool ReloadAccounts();
    }
}
