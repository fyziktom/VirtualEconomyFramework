using System;
using System.Collections.Generic;
using System.Text;

namespace VEDrivers.Economy
{
    public interface IBalance
    {
        double? TotalBalance { get; set; }
        double? TotalSpendableBalance { get; set; }
        double? TotalUnconfirmedBalance { get; set; }
    }
}
