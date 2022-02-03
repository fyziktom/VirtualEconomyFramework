using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Accounts
{
    public static class AccountFactory
    {
        public static async Task<IAccount> GetAccount(AccountType type)
        {
            IAccount account = null;
            switch (type)
            {
                case AccountType.Neblio:
                    account = new NeblioAccount();
                    break;
            }

            return account;
        }
    }
}
