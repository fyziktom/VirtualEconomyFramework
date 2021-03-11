using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Transactions;

namespace VEDrivers.Economy.Receipt
{
    public enum ReceiptTypes
    {
        Bitcoin,
        Neblio,
        ReddCoin
    }

    public interface IReceipt
    {
        string TxId { get; set; }
        Guid WalletId { get; set; }
        string WalletName { get; set; }
        Guid AccountId { get; set; }
        string AccountAddress { get; set; }
        string AccountName { get; set; }
        double Amount { get; set; }
        CryptocurrencyTypes CurrencyType { get; set; }
        ITransaction TxDetails { get; set; }
        ICryptocurrency CurrencyDetails { get; set; }

        Task<ITransaction> GetTxDetails();
        Task<ICryptocurrency> GetCurrencyDetails();
        Task<string> GetReceiptOutput();
    }
}
