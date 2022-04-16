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

        #region NeblioPaymentsHandling

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task ConnectNeblioAccount(string address)
        {
            if (VEDLDataContext.Accounts.TryGetValue(address, out var add))
            {
                ConnectedNeblioAccountAddress = address;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectNeblioAccount()
        {
            if (VEDLDataContext.Accounts.TryGetValue(ConnectedNeblioAccountAddress, out var add))
            {
                ConnectedNeblioAccountAddress = string.Empty;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
        public async Task ConnectDepositNeblioAccount(string address)
        {
            if (VEDLDataContext.Accounts.TryGetValue(address, out var add))
            {
                ConnectedDepositNeblioAccountAddress = address;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public async Task DisconnectDepositNeblioAccount()
        {
            if (VEDLDataContext.Accounts.TryGetValue(ConnectedDepositNeblioAccountAddress, out var add))
            {
                ConnectedDepositNeblioAccountAddress = string.Empty;
            }
        }

        private async Task CheckNeblioPayments()
        {
            if (VEDLDataContext.Accounts.TryGetValue(ConnectedNeblioAccountAddress, out var account))
            {
                var utxos = new NeblioAPI.Utxos[account.Utxos.Count];
                lock (_lock)
                {
                    account.Utxos.CopyTo(utxos);
                }

                var completedOrdersUtxos = new List<NeblioAPI.Utxos>();

                foreach (var u in utxos)
                {
                    if (u != null)
                    {
                        var info = await NeblioTransactionHelpers.GetTransactionInfo(u.Txid);
                        if (info != null && info.Confirmations > 1)
                        {
                            var msg = NeblioTransactionHelpers.ParseNeblioMessage(info);
                            if (msg.Item1)
                            {
                                //var split = msg.Item2.Split('-');
                                var ordkey = msg.Item2;
                                if (!string.IsNullOrEmpty(ordkey))
                                {
                                    if (Orders.TryGetValue(ordkey, out var ord))
                                    {
                                        if (ord.statusclass == OrderStatus.pending && ord.currency == "NBL")
                                        {
                                            Console.WriteLine($"Order {ord.id}, {ord.order_key} received Neblio payment in value {u.Value}.");
                                            try
                                            {
                                                if (((double)u.Value/NeblioTransactionHelpers.FromSatToMainRatio) >= Convert.ToDouble(ord.total, CultureInfo.InvariantCulture))
                                                {
                                                    Console.WriteLine($"Order {ord.id}, {ord.order_key} payment has correct amount and it is moved to processing state.");
                                                    if (AllowDispatchNFTOrders)
                                                    {
                                                        var add = await GetNeblioAddressFromOrderMetadata(ord);
                                                        if (add.Item1)
                                                        {
                                                            Console.WriteLine($"Order {ord.id}, {ord.order_key} Neblio Address in the order is correct.");
                                                            ord.statusclass = OrderStatus.processing;
                                                            ord.transaction_id = $"{u.Txid}:{u.Index}";
                                                            ord.date_paid = TimeHelpers.UnixTimestampToDateTime((double)u.Blocktime);
                                                            var o = await UpdateOrder(ord);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        ord.statusclass = OrderStatus.processing;
                                                        ord.transaction_id = $"{u.Txid}:{u.Index}";
                                                        ord.date_paid = TimeHelpers.UnixTimestampToDateTime((double)u.Blocktime);
                                                        var o = await UpdateOrder(ord);
                                                    }
                                                }
                                                else
                                                {
                                                    if (((double)u.Value / NeblioTransactionHelpers.FromSatToMainRatio) > 0.009)
                                                    {
                                                        Console.WriteLine($"Order {ord.id}, {ord.order_key} payment is not correct amount. It is sent back to the sender.");
                                                        var done = false;
                                                        var attempts = 50;
                                                        while (!done)
                                                        {
                                                            try
                                                            {
                                                                var nres = await account.SendNeblioPayment(account.Address,
                                                                                             ((double)u.Value / NeblioTransactionHelpers.FromSatToMainRatio) - 0.0002,
                                                                                              $"Order {ord.order_key} cannot be processed. Wrong sent amount.");
                                                                done = nres.Item1;
                                                                if (done)
                                                                {
                                                                    Console.WriteLine($"Order {ord.id}, {ord.order_key} incorrect received payment sent back with txid: {nres.Item2}");
                                                                    ord.meta_data.Add(new ProductMetadata() { key = "Incorrect Received Payment", value = $"Neblio-{nres.Item2}" });
                                                                    var o = await UpdateOrder(ord);
                                                                }
                                                                if (!nres.Item1) await Task.Delay(5000);
                                                            }
                                                            catch
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
                                                 !string.IsNullOrEmpty(ConnectedDepositNeblioAccountAddress))
                                        {
                                            completedOrdersUtxos.Add(u);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    await Task.Delay(50);//wait at least 50ms before next request to the api.
                }

                if (completedOrdersUtxos.Count > 5 && !string.IsNullOrEmpty(ConnectedDepositNeblioAccountAddress))
                {
                    var totalAmount = 0.0;
                    foreach (var cu in completedOrdersUtxos)
                    {
                        totalAmount += Convert.ToDouble(cu.Value, CultureInfo.InvariantCulture);
                    }
                    if (totalAmount >= 3)
                    {
                        Console.WriteLine($"Sending completed orders payments Utxos to deposit address.");
                        Console.WriteLine($"Total amount is {totalAmount} - 0.0002 Neblio for the fee.");
                        var done = false;
                        var attempts = 50;
                        while (!done)
                        {
                            try
                            {
                                var dres = await account.SendMultipleInputNeblioPayment(ConnectedDepositNeblioAccountAddress,
                                                                 totalAmount - 0.0002,
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
                            catch
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
