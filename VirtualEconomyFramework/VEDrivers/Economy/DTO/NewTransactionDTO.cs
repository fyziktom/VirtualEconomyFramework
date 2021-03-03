using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Transactions;

namespace VEDrivers.Economy.DTO
{
    public class NewTransactionDTO
    {
        public string WalletName { get; set; } = string.Empty;
        public TransactionTypes Type { get; set; }
        public ITransaction TransactionDetails { get; set; }
        public Guid OwnerId { get; set; }
        public string AccountAddress { get; set; } = string.Empty;
        public string TxId { get; set; } = string.Empty;

    }
}
