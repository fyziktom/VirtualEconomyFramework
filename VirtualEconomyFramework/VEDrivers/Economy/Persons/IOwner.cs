using System;
using System.Collections.Generic;
using System.Text;
using VEDrivers.Economy.Wallets;

namespace VEDrivers.Economy.Persons
{
    public interface IOwner
    {
        Guid Id { get; set; }
        string Name { get; set; }
        string SurName { get; set; }
        IEnumerable<IWallet> Wallets { get; set; }
        IEnumerable<IAccount> Accounts { get; set; }
    }
}
