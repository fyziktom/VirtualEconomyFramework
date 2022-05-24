using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Economy.Tokens;

namespace VEDrivers.Economy.Transactions
{
    public static class TransactionFactory
    {
        public static ITransaction GetTransaction(TransactionTypes type, string txid, string address, string walletName, bool justDto = true)
        {
            switch (type)
            {
                case TransactionTypes.Neblio:
                    return new NeblioTransaction(txid, address, walletName, justDto);
            }

            return null;
        }
    }
}
