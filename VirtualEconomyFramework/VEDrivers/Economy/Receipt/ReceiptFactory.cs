using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Economy.Receipt
{
    public static class ReceiptFactory
    {
        public static IReceipt GetReceipt(ReceiptTypes type, Guid walletId, Guid accountId, string accountAddr, string txId, bool  loadtxdetails = true, bool loadCurrencyDetails = true)
        {
            switch (type)
            {
                case ReceiptTypes.Bitcoin:
                    return null;
                case ReceiptTypes.Neblio:
                    var rcp = new NeblioReceipt(txId, walletId, accountId, accountAddr, loadtxdetails, loadCurrencyDetails);
                    return rcp;
            }

            return null;
        }
    }
}
