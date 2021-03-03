using System;
using System.Collections.Generic;
using System.Text;

namespace VEDrivers.Economy
{
    public interface IEconomyBase
    {
        string Name { get; set; }
        string Symbol { get; set; }
        string BaseURL { get; set; }
    }
}
