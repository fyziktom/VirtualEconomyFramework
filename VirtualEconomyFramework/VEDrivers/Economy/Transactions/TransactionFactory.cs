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
        public static ITransaction GetTranaction(TransactionTypes type, string txid)
        {
            switch (type)
            {
                case TransactionTypes.Neblio:
                    return new NeblioTransaction(txid);
                    break;
            }

            return null;
        }
    }
}
