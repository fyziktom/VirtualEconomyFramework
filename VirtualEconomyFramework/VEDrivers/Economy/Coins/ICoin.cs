using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Transactions;

namespace VEDrivers.Economy.Coins
{
    public interface ICoin : IUnitBase, IEconomyBase
    {
        IEnumerable<ITransaction> Transactions { get; set; }

        Task<string> GetDetails();
    }
}
