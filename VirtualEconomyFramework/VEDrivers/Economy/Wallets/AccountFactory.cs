using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Tokens;

namespace VEDrivers.Economy.Wallets
{
    public static class AccountFactory
    {
        public static IAccount GetAccount(Guid id, AccountTypes type, Guid owner, Guid walletId, string name, string address, double totalbalance)
        {
            switch (type)
            {
                case AccountTypes.Bitcoin:
                    return null;
                case AccountTypes.Neblio:
                    if (id == Guid.Empty)
                    {
                        id = Guid.NewGuid();
                    }
                    var acc = new NeblioAccount() { Id = id, OwnerId = owner, WalletId = walletId, Name = name, Address = address, TotalBalance = totalbalance };
                    return acc;
            }

            return null;
        }
    }
}
