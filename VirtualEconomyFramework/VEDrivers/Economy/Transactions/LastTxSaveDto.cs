using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Economy.Transactions
{
    public class LastTxSaveDto
    {
        public string LastProcessedTxId { get; set; } = string.Empty;
        public string LastConfirmedTxId { get; set; } = string.Empty;
    }
}
