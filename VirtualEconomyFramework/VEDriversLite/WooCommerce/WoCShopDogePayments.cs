using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Events;
using VEDriversLite.WooCommerce.Dto;

namespace VEDriversLite.WooCommerce
{
    public partial class WooCommerceShop
    {
        public string LastCheckedDogePaymentUtxo { get; set; } = string.Empty;


        #region DogePaymentsHandling

        public async Task ConnectDogeAccount(string address)
        {
            ConnectedDogeAccountAddress = address;
            if (VEDLDataContext.DogeAccounts.TryGetValue(address, out var doge))
            {
                doge.NewDogeUtxos -= Doge_NewDogeUtxos;
                doge.NewDogeUtxos += Doge_NewDogeUtxos;
               // await CheckDogePayments();
            }
        }

        public async Task DisconnectDogeAccount()
        {
            if (VEDLDataContext.DogeAccounts.TryGetValue(ConnectedDogeAccountAddress, out var doge))
            {
                doge.NewDogeUtxos -= Doge_NewDogeUtxos;
            }
            ConnectedDogeAccountAddress = string.Empty;
        }

        private void Doge_NewDogeUtxos(object sender, IEventInfo e)
        {
            CheckDogePayments();
        }

        private async Task CheckDogePayments()
        {
            if (VEDLDataContext.DogeAccounts.TryGetValue(ConnectedDogeAccountAddress, out var doge))
            {
                foreach (var u in doge.Utxos)
                {
                    if (u.TxId == LastCheckedDogePaymentUtxo)
                        break;
                    if (u.Confirmations > 2)
                    {
                        var info = await DogeTransactionHelpers.TransactionInfoAsync(u.TxId);
                        if (info != null && info.Transaction != null)
                        {
                            var msg = await DogeTransactionHelpers.ParseDogeMessage(info);
                            if (msg.Item1)
                            {
                                var split = msg.Item2.Split('-');
                                if (split != null && split.Length == 2 && !string.IsNullOrEmpty(split[1]) && split[1].Length > 0)
                                {
                                    var addver = await NeblioTransactionHelpers.ValidateNeblioAddress(split[0]);
                                    if (addver.Item1)
                                    {
                                        if (WooCommerceHelpers.Shop.Orders.TryGetValue(split[1], out var ord))
                                        {
                                            if (ord.statusclass == OrderStatus.onhold ||
                                                ord.statusclass == OrderStatus.pending)
                                            {
                                                try
                                                {
                                                    if (Convert.ToDouble(ord.total, CultureInfo.InvariantCulture) == Convert.ToDouble(u.Value, CultureInfo.InvariantCulture))
                                                    {
                                                        ord.statusclass = OrderStatus.processing;
                                                        ord.transaction_id = $"{u.TxId}:{u.N}";
                                                        var o = await WooCommerceHelpers.Shop.UpdateOrder(ord);
                                                        if (!string.IsNullOrEmpty(o.order_key))
                                                        {
                                                            WooCommerceHelpers.Shop.ReceivedPaymentsForOrdersToProcess.Enqueue(new ReceivedPaymentForOrder()
                                                            {
                                                                CustomerAddress = ConnectedDogeAccountAddress,
                                                                Currency = "DOGE",
                                                                OrderId = ord.id,
                                                                Amount = Convert.ToDouble(u.Value, CultureInfo.InvariantCulture),
                                                                OrderKey = ord.order_key,
                                                                PaymentId = $"{u.TxId}:{u.N}",
                                                                NeblioCustomerAddress = addver.Item2
                                                            });
                                                            LastCheckedDogePaymentUtxo = u.TxId;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var done = false;
                                                        var attempts = 50;
                                                        while (!done)
                                                        {
                                                            try
                                                            {
                                                                var dres = await doge.SendPayment(doge.Address,
                                                                                              Convert.ToDouble(u.Value, CultureInfo.InvariantCulture) - 1,
                                                                                              $"Order {ord.order_key} cannot be accepted. wrong sent amount.");
                                                                done = dres.Item1;
                                                                if (!dres.Item1) await Task.Delay(5000);
                                                            }
                                                            catch (Exception ex)
                                                            {
                                                                await Task.Delay(5000);
                                                            }
                                                            attempts--;
                                                            if (attempts < 0) break;
                                                        }
                                                    }
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine("Cannot send update of the order." + ex.Message);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        #endregion

    }
}
