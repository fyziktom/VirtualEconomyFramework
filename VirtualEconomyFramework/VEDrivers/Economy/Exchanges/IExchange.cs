using System;
using System.Collections.Generic;
using System.Text;

namespace VEDrivers.Economy.Exchanges
{
    interface IExchange : IEconomyBase
    {
        string APIBaseURL { get; set; }
    }
}
