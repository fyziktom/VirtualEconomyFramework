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
        private static object _lock { get; set; } = new object();

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
            //CheckDogePayments();
        }

        private async Task CheckDogePayments()
        {
            if (VEDLDataContext.DogeAccounts.TryGetValue(ConnectedDogeAccountAddress, out var doge))
            {
                var utxos = new DogeAPI.Utxo[doge.Utxos.Count];
                lock (_lock)
                {
                    doge.Utxos.CopyTo(utxos);
                }

                var completedOrdersUtxos = new List<DogeAPI.Utxo>();

                foreach (var u in utxos)
                {
                    if (u != null)
                    {
                        if (u.Confirmations > 1)
                        {
                            var info = await DogeTransactionHelpers.TransactionInfoAsync(u.TxId, true);
                            if (info != null && info.Transaction != null)
                            {
                                var msg = await DogeTransactionHelpers.ParseDogeMessage(info);
                                if (msg.Item1)
                                {
                                    //var split = msg.Item2.Split('-');
                                    var ordkey = msg.Item2;
                                    if (!string.IsNullOrEmpty(ordkey))
                                    {
                                        if (Orders.TryGetValue(ordkey, out var ord))
                                        {
                                            if (ord.statusclass == OrderStatus.pending && ord.currency == "DGC") // && ord.payment_method == "cod")
                                            {
                                                Console.WriteLine($"Order {ord.id}, {ord.order_key} received DOGE payment in value {u.Value}.");
                                                try
                                                {
                                                    if (Convert.ToDouble(u.Value, CultureInfo.InvariantCulture) >= Convert.ToDouble(ord.total, CultureInfo.InvariantCulture))
                                                    {
                                                        Console.WriteLine($"Order {ord.id}, {ord.order_key} payment has correct amount and it is moved to processing state.");
                                                        if (AllowDispatchNFTOrders)
                                                        {
                                                            var add = await GetNeblioAddressFromOrderMetadata(ord);
                                                            if (add.Item1)
                                                            {
                                                                Console.WriteLine($"Order {ord.id}, {ord.order_key} received Neblio Address in DOGE Payment message matchs with Address in the order.");
                                                                ord.statusclass = OrderStatus.processing;
                                                                ord.transaction_id = $"{u.TxId}:{u.N}";
                                                                ord.date_paid = TimeHelpers.UnixTimestampToDateTime(u.Time * 1000);
                                                                var o = await UpdateOrder(ord);
                                                            }
                                                        }
                                                        else
                                                        {
                                                            ord.statusclass = OrderStatus.processing;
                                                            ord.transaction_id = $"{u.TxId}:{u.N}";
                                                            ord.date_paid = TimeHelpers.UnixTimestampToDateTime(u.Time * 1000);
                                                            var o = await UpdateOrder(ord);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        if (Convert.ToDouble(u.Value, CultureInfo.InvariantCulture) > 2)
                                                        {
                                                            Console.WriteLine($"Order {ord.id}, {ord.order_key} payment is not correct amount. It is sent back to the sender.");
                                                            var done = false;
                                                            var attempts = 50;
                                                            while (!done)
                                                            {
                                                                try
                                                                {
                                                                    var dres = await doge.SendPayment(doge.Address,
                                                                                                  Convert.ToDouble(u.Value, CultureInfo.InvariantCulture) - 1,
                                                                                                  $"Order {ord.order_key} cannot be processed. Wrong sent amount.");
                                                                    done = dres.Item1;
                                                                    if (done)
                                                                    {
                                                                        Console.WriteLine($"Order {ord.id}, {ord.order_key} incorrect received payment sent back with txid: {dres.Item2}");
                                                                        ord.meta_data.Add(new ProductMetadata() { key = "Incorrect Received Payment", value = $"DOGE-{dres.Item2}" });
                                                                        var o = await UpdateOrder(ord);
                                                                    }
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
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine("Cannot send update of the order." + ex.Message);
                                                }
                                            }
                                            else if (ord.statusclass == OrderStatus.completed && 
                                                     completedOrdersUtxos.Count <= 10 &&
                                                     !string.IsNullOrEmpty(ConnectedDepositDogeAccountAddress))
                                            {
                                                completedOrdersUtxos.Add(u);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }

                    await Task.Delay(500);//wait at least 500ms before next request to the api. otherwise chain.so api can revoke the request
                }

                if (completedOrdersUtxos.Count > 5 && !string.IsNullOrEmpty(ConnectedDepositDogeAccountAddress))
                {
                    var totalAmount = 0.0;
                    foreach (var cu in completedOrdersUtxos)
                    {
                        totalAmount += Convert.ToDouble(cu.Value, CultureInfo.InvariantCulture);
                    }
                    if (totalAmount >= 3)
                    {
                        Console.WriteLine($"Sending completed orders payments Utxos to deposit address.");
                        Console.WriteLine($"Total amount is {totalAmount} - 2 DOGE for the fee.");
                        var done = false;
                        var attempts = 50;
                        while (!done)
                        {
                            try
                            {
                                var dres = await doge.SendMultipleInputPayment(ConnectedDepositDogeAccountAddress,
                                                                 totalAmount - 2,
                                                                 completedOrdersUtxos,
                                                                 $"Transfer of completed orders payment.");
                                done = dres.Item1;
                                if (done)
                                {
                                    completedOrdersUtxos.Clear();
                                    Console.WriteLine($"Paymenents for completed orders transfered with txid: {dres.Item2}");
                                }
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
            }
        }

        #endregion

    }
}
