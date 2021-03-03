using System;
using System.Collections.Generic;
using System.Text;
using VEDrivers.Economy.Wallets;

namespace VEDrivers.Economy.Persons
{
    public class Owner : IOwner
    {
        public Owner()
        {
            Id = Guid.NewGuid();
            Wallets = new List<IWallet>();
            Accounts = new List<IAccount>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string SurName { get; set; } = string.Empty;
        public IEnumerable<IWallet> Wallets { get; set; }
        public IEnumerable<IAccount> Accounts { get; set; }
    }
}
