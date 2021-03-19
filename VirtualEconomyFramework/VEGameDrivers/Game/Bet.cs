using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy;

namespace VEGameDrivers.Game
{
    public class Bet
    {
        public double Amount { get; set; } = 0.0;
        public bool IsTokenBet { get; set; } = false;
        public string TokenSymbol { get; set; } = string.Empty;
        public ICryptocurrency Cryptocurrency { get; set; } 
    }
}
