using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Economy.DTO
{

    public class NeblioQTWalletInfo
    {
        public string id { get; set; }
        public NeblioQTWalletInfoDetails result { get; set; }
    }

    public class NeblioQTWalletInfoDetails
    {
        public int walletversion { get; set; }
        public double balance { get; set; }
        public double delegated_balance { get; set; }
        public double cold_staking_balance { get; set; }
        public double unconfirmed_balance { get; set; }
        public double immature_balance { get; set; }
        public double immature_delegated_balance { get; set; }
        public double immature_cold_staking_balance { get; set; }
        public int txcount { get; set; }
        public double keypoololdest { get; set; }
        public double keypoolsize { get; set; }
    }
}
