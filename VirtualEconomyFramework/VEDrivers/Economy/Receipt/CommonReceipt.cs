using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Transactions;

namespace VEDrivers.Economy.Receipt
{
    public abstract class CommonReceipt : IReceipt
    {
        public string TxId { get; set; } = string.Empty;
        public Guid WalletId { get; set; } = Guid.Empty;
        public string WalletName { get; set; } = string.Empty;
        public Guid AccountId { get; set; } = Guid.Empty;
        public string AccountAddress { get; set; } = string.Empty;

        public string AccountName { get; set; } = string.Empty;
        public double Amount { get; set; } = 0.0;

        public CryptocurrencyTypes CurrencyType { get; set; } = CryptocurrencyTypes.Neblio;
        public ITransaction TxDetails { get; set; }
        public ICryptocurrency CurrencyDetails { get; set; }

        public abstract Task<ICryptocurrency> GetCurrencyDetails();
        public abstract Task<ITransaction> GetTxDetails();
        public abstract Task<string> GetReceiptOutput();
    }
}
