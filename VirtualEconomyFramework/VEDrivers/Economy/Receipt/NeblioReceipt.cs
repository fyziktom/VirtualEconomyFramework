using HtmlAgilityPack;
using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDrivers.Common;
using VEDrivers.Economy.Transactions;

namespace VEDrivers.Economy.Receipt
{
    public class NeblioReceipt : CommonReceipt
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public NeblioReceipt(string txid, Guid walletId, Guid accountId, string accountAddr, bool loadTxDetails = true, bool loadCryptocurrencyDetails = true)
        {
            CurrencyType = CryptocurrencyTypes.Neblio;
            WalletId = walletId;
            AccountId = accountId;
            AccountAddress = accountAddr;
            TxId = txid;

            if (EconomyMainContext.Wallets.TryGetValue(walletId.ToString(), out var wallet))
                WalletName = wallet.Name;
            if (EconomyMainContext.Accounts.TryGetValue(accountAddr, out var acc))
                AccountName = acc.Name;

            if (loadTxDetails)
            {
                var tx = GetTxDetails().GetAwaiter().GetResult();
                if (tx == null)
                {
                    log.Error("Cannot load Neblio tx details for receipt!");
                    Console.WriteLine("Cannot load Neblio tx details for receipt!");
                }
            }

            if (loadCryptocurrencyDetails)
            {
                var cr = GetCurrencyDetails().GetAwaiter().GetResult();
                if (cr == null)
                {
                    log.Error("Cannot load Neblio currency details for receipt!");
                    Console.WriteLine("Cannot load Neblio currency details for receipt!");
                }
            }
        }
        public override Task<ICryptocurrency> GetCurrencyDetails()
        {
            ICryptocurrency cr = new NeblioCryptocurrency(false);

            if (cr != null)
            {
                CurrencyDetails = cr;
                return Task.FromResult(cr);
            }
            else
            {
                return null;
            }
        }
        public override async Task<ITransaction> GetTxDetails()
        {
            if (!string.IsNullOrEmpty(TxId))
            {
                var tx = await NeblioTransactionHelpers.TransactionInfoAsync(null, TransactionTypes.Neblio, TxId);
                
                if (tx != null)
                {
                    Amount = tx.Amount;
                    TxDetails = tx;
                    return tx;
                }
                else
                {
                    return null;
                }
            }
            else
            {
                log.Error("Cannot load Neblio tx details for receipt, TxId is empty!");
                Console.WriteLine("Cannot load Neblio tx details for receipt, TxId is empty!");
                return null;
            }
        }

        public override Task<string> GetReceiptOutput()
        {
            var doc = new HtmlDocument();
            doc.Load(Path.Join(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), @"\Economy\Receipt\ReceiptTemplate\NeblioReceipt.html"));

            var output = doc.ParsedText
                .Replace("<span id=\"walletName\">Not loaded</span>", $"<span id=\"walletName\">{WalletName}</span>")
                .Replace("<span id=\"accountAddress\">Not loaded</span>", $"<span id=\"accountAddress\">{AccountName} - {AccountAddress}</span>")
                .Replace("<a id=\"txId\" href=\"https://explorer.nebl.io/tx/\" target=\"_blank\">Not loaded</a>", $"<a id=\"txId\" href=\"https://explorer.nebl.io/tx/{TxId}\" target=\"_blank\">{TxId}</a>")
                .Replace("<span id=\"amount\">Not loaded</span>", $"<span id=\"amount\">{Amount}</span>");

            /* does not work :( 
             * it can find the node, but writing back to doc is about removing and readding
            var hNode = doc.DocumentNode
                .SelectNodes("//span[@id='txId']").FirstOrDefault();
            if (hNode != null)
            {
                var t = hNode.FirstChild;
                if (t != null)
                    t.InnerHtml = TxId;
                hNode.RemoveAllChildren();
                hNode.PrependChild(t);
            }*/

            return Task.FromResult(output);
        }
    }
}
