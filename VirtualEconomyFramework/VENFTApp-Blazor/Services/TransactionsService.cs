using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VEDriversLite;

public class TransactionsService
{
    public async Task<List<string>> LoadTransactions(string address, NeblioAccount account, bool subAccount)
    {
        if (!subAccount)
        {
            var inf = await NeblioTransactionHelpers.AddressInfoAsync(account.Address);
            return inf.Transactions?.Reverse().ToList() ?? new List<string>();
        }
        else
        {
            if (account.SubAccounts.TryGetValue(address, out _))
            {
                var inf = await NeblioTransactionHelpers.AddressInfoAsync(address);
                return inf.Transactions?.Reverse().ToList() ?? new List<string>();
            }
        }
        return new List<string>();
    }

    public async Task<TxDetails> LoadTxDetails(string txid, NeblioAccount account)
    {
        var tinfo = await NeblioTransactionHelpers.GetTransactionInfo(txid);
        string sender = await NeblioTransactionHelpers.GetTransactionSender(txid, tinfo);
        bool fromAnotherAccount = true;
        bool fromSubAccount = true;

        if (sender == account.Address)
        {
            sender = "Main Account";
            fromAnotherAccount = false;
            fromSubAccount = false;
        }
        else if (account.SubAccounts.TryGetValue(sender, out var sacc))
        {
            if (!string.IsNullOrEmpty(sacc.Name))
                sender = sacc.Name;
            else
                sender = sacc.BookmarkFromAccount.Name;
            fromAnotherAccount = false;
            fromSubAccount = true;
        }

        string rec = await NeblioTransactionHelpers.GetTransactionReceiver(txid, tinfo);
        string receiver = string.Empty;
        var recbkm = account.IsInTheBookmarks(rec);

        if (rec == account.Address)
            receiver = "Main Account";
        else if (recbkm.Item1)
            receiver = recbkm.Item2.Name;

        if (string.IsNullOrEmpty(receiver))
            receiver = NeblioTransactionHelpers.ShortenAddress(rec);

        var time = TimeHelpers.UnixTimestampToDateTime((double)tinfo.Blocktime);

        return new TxDetails()
        {
            FromAnotherAccount = fromAnotherAccount,
            FromSubAccount = fromSubAccount,
            Info = tinfo,
            Receiver = receiver,
            Sender = sender,
            Time = time,
        };
    }
}

