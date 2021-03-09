﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Tokens;

namespace VEDrivers.Economy.Wallets.Handlers
{
    public interface IAccountHandler
    {
        Task<string> UpdateAccount(string accountAddress, Guid walletId, AccountTypes type, string name, bool justInDb = true);
        IDictionary<string, IToken> FindTokenByMetadata(string account, string key, string value = "");
    }
}
