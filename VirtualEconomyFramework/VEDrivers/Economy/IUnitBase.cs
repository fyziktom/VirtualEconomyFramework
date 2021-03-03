using System;
using System.Collections.Generic;
using System.Text;

namespace VEDrivers.Economy
{
    public interface IUnitBase : IEconomyBase
    {
        // maximum supply of the coin or token
        double? MaxSupply { get; set; }
        // actual circulation supply
        double? CirculatingSuply { get; set; }
        // common transfer fee
        double? TransferFee { get; set; }
        // actual BTC price of coin or token
        double? ActualBTCPrice { get; set; }
    }
}
