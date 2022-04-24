using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Dto;
using VEDriversLite.Events;
using VEDriversLite.Extensions.WooCommerce.Dto;

namespace VEDriversLite.Extensions.WooCommerce
{
    public partial class WooCommerceShop
    {
        /// <summary>
        /// TODO
        /// </summary>
        public string LastCheckedDogePaymentUtxo { get; set; } = string.Empty;
        private static object _lock { get; set; } = new object();

        #region DogePaymentsHandling

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="address"></param>
        /// <returns></returns>
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

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
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

                foreach (var u in utxos)
                {
                    if (u != null && u.Confirmations > 1)
                    {
                            var info = await DogeTransactionHelpers.TransactionInfoAsync(u.TxId, true);
                        if (info != null && info.Transaction != null)
                        {
                            var msg = DogeTransactionHelpers.ParseDogeMessage(info);
                            if (msg.Success)
                            {
                                var ordkey = msg.Value.ToString();
                                if (!string.IsNullOrEmpty(ordkey) && Orders.TryGetValue(ordkey, out var ord))
                                {
                                    if (ord.statusclass == OrderStatus.pending && ord.currency == "DGC") // && ord.payment_method == "cod")
                                    {
                                        Console.WriteLine($"Order {ord.id}, {ord.order_key} received DOGE payment in value {u.Value}.");
                                        try
                                        {
                                            if (Convert.ToDouble(u.Value, CultureInfo.InvariantCulture) >= Convert.ToDouble(ord.total, CultureInfo.InvariantCulture))
                                            {
                                                try
                                                {
                                                    Console.WriteLine($"Order {ord.id}, {ord.order_key} payment has correct amount and it is moved to processing state.");
                                                    if (AllowDispatchNFTOrders)
                                                    {
                                                        var add = await GetNeblioAddressFromOrderMetadata(ord);
                                                        if (add.Item1)
                                                        {
                                                            Console.WriteLine($"Order {ord.id}, {ord.order_key} Order Contains correct Neblio address. NFT Order will be dispatched.");
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

                                                    // send deposit payment
                                                    var resdeposit = await SendDepositPayment(u, ord);
                                                    Console.WriteLine(resdeposit.Item2);
                                                }
                                                catch (Exception ex)
                                                {
                                                    Console.WriteLine("Cannot set order to processing state.");
                                                }
                                            }
                                            else
                                            {
                                                var respayback = await SendDogePaymentBack(u, ord);
                                                Console.WriteLine(respayback.Item2);
                                            }
                                        }
                                        catch (Exception ex)
                                        {
                                            Console.WriteLine("Cannot send update of the order." + ex.Message);
                                        }
                                    }
                                    else if (ord.statusclass == OrderStatus.completed &&
                                             !string.IsNullOrEmpty(ConnectedDepositDogeAccountAddress))
                                    {
                                        // send deposit payment
                                        var resdeposit = await SendDepositPayment(u, ord);
                                        Console.WriteLine(resdeposit.Item2);
                                    }
                                }
                            }
                        }
                    }

                    await Task.Delay(500);//wait at least 500ms before next request to the api. otherwise chain.so api can revoke the request
                }
            }
        }

        #endregion

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="utxo"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<(bool,string)> SendDogePaymentBack(DogeAPI.Utxo utxo, Order order)
        {
            try
            {
                if (VEDLDataContext.DogeAccounts.TryGetValue(ConnectedDogeAccountAddress, out var doge))
                {
                    if (Convert.ToDouble(utxo.Value, CultureInfo.InvariantCulture) > 2)
                    {
                        Console.WriteLine($"Order {order.id}, {order.order_key} payment is not correct amount. It is sent back to the sender.");
                        var done = false;
                        var attempts = 50;
                        while (!done)
                        {
                            try
                            {
                                var dres = await doge.SendPayment(doge.Address,
                                                              Convert.ToDouble(utxo.Value, CultureInfo.InvariantCulture) - 1,
                                                              $"Order {order.order_key} cannot be processed. Wrong sent amount.");
                                done = dres.Item1;
                                if (done)
                                {
                                    Console.WriteLine($"Order {order.id}, {order.order_key} incorrect received payment sent back with txid: {dres.Item2}");
                                    order.meta_data.Add(new ProductMetadata() { key = "Incorrect Received Payment", value = $"DOGE-{dres.Item2}" });
                                    var o = await UpdateOrder(order);
                                }
                                if (!dres.Item1)
                                {
                                    Console.WriteLine("Cannot send wrong amountDoge payment back to the customer. Trying again in 5 seconds. " + dres.Item2);
                                    await Task.Delay(5000);
                                }
                                else
                                {
                                    return (true, dres.Item2);
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine("Cannot send wrong amountDoge payment back to the customer. Trying again in 5 seconds. " + ex.Message);
                                await Task.Delay(5000);
                            }
                            attempts--;
                            if (attempts < 0) break;
                        }
                    }
                }
                return (false, string.Empty);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Cannot send wrong amount payment back to the customer. Utxo: {utxo.TxId}, Order: {order.order_key}. " + ex.Message);
                return (false, ex.Message);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="utxo"></param>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<(bool, string)> SendDepositPayment(DogeAPI.Utxo utxo, Order order)
        {
            try
            {
                if (VEDLDataContext.DogeAccounts.TryGetValue(ConnectedDogeAccountAddress, out var doge))
                {
                    (bool, string) dres = (false, string.Empty);
                    if (VEDLDataContext.WooCommerceStoreSendDogeToAuthor)
                    {
                        var receiversAmounts = await ParseAuthorsAmountFromOrder(order);

                        var rest = 0.0;
                        var totalToAuthors = 0.0;
                        foreach (var r in receiversAmounts)
                            totalToAuthors += r.Value;
                        rest = Convert.ToDouble(utxo.Value, CultureInfo.InvariantCulture) - totalToAuthors - 1;

                        // if something rest send to the main deposit account
                        if (receiversAmounts.TryGetValue(ConnectedDepositDogeAccountAddress, out var value))
                        {
                            receiversAmounts.Remove(ConnectedDepositDogeAccountAddress);
                            rest += value;
                        }
                        receiversAmounts.Add(ConnectedDepositDogeAccountAddress, rest);
                        // send the utxo to the deposit account
                        dres = await doge.SendMultipleOutputPayment(receiversAmounts,
                                                      message: $"Order {order.order_key} moved to processing state.", utxos: new List<DogeAPI.Utxo> { utxo });
                    }
                    else
                    {
                        // send the utxo to the deposit account
                        dres = await doge.SendPayment(doge.Address,
                                                      Convert.ToDouble(utxo.Value, CultureInfo.InvariantCulture) - 1,
                                                      $"Order {order.order_key} moved to processing state.", utxo: utxo.TxId, N: utxo.N);
                    }
                    if (dres.Item1)
                    {
                        order.meta_data.Add(new ProductMetadata() { key = "Doge deposit and author reward payment txid.", value = dres.Item2 });
                        var o = await UpdateOrder(order);
                        Console.WriteLine($"Doge payment resend to the deposit account. Txid: {dres.Item2}");
                    }
                    else
                    {
                        Console.WriteLine($"Doge payment cannot be send to the deposit account. Message: {dres.Item2}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Doge payment cannot be send to the deposit account. Message: {ex.Message}");
                return (false, ex.Message);
            }
            return (false, string.Empty);
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="order"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, double>> ParseAuthorsAmountFromOrder(Order order)
        {
            if (order == null)
                return new Dictionary<string, double>();

            Dictionary<string, double> receiversAmounts = new Dictionary<string, double>();
            order.line_items.ForEach(async (item) =>
            {
                var authoraddress = string.Empty;
                var sh = string.Empty;
                var adogeaddFromOrder = string.Empty;
                if (WooCommerceHelpers.Shop.Products.TryGetValue(item.product_id, out var product))
                {
                    product.meta_data.ForEach(m =>
                    {
                        if (m.key == "ShortHash") sh = Convert.ToString(m.value);
                        if (m.key == "AuthorDogeAddress") adogeaddFromOrder = Convert.ToString(m.value);
                    });
                    if (!string.IsNullOrEmpty(sh))
                    {
                        if (VEDLDataContext.NFTHashs.TryGetValue(sh, out var nfthash))
                        {
                            if (VEDLDataContext.DepositSchemes.Count > 0)
                            {
                                var schemeName = nfthash.DogeftInfo.RewardSchemeName;
                                if (string.IsNullOrEmpty(schemeName))
                                    schemeName = VEDLDataContext.DepositSchemes.Values.FirstOrDefault().Name;

                                if (VEDLDataContext.DepositSchemes.TryGetValue(nfthash.DogeftInfo.RewardSchemeName, out var sch))
                                {
                                    foreach (var add in sch.DepositAddresses.Values)
                                    {
                                        if (add.TakeAddressFromNFT)
                                        {
                                            // author reward. The address will be taken from the nft data
                                            var amnt = Convert.ToDouble(item.total, CultureInfo.InvariantCulture) * (add.Percentage / 100);
                                            amnt = Math.Round(amnt);
                                            if (!string.IsNullOrEmpty(adogeaddFromOrder))
                                                authoraddress = adogeaddFromOrder;
                                            else
                                                authoraddress = nfthash.DogeftInfo.AuthorDogeAddress;

                                            if (receiversAmounts.TryGetValue(authoraddress, out var value))
                                            {
                                                receiversAmounts.Remove(authoraddress);
                                                amnt += value;
                                            }
                                            receiversAmounts.Add(authoraddress, amnt);
                                        }
                                        else
                                        {
                                            // address is prefilled in the scheme
                                            // case of dogeft account, dogepalooza, etc.
                                            var amnt = Convert.ToDouble(item.total, CultureInfo.InvariantCulture) * (add.Percentage / 100);
                                            amnt = Math.Round(amnt);
                                            if (!string.IsNullOrEmpty(add.Address))
                                                authoraddress = ConnectedDepositDogeAccountAddress;
                                            else
                                                authoraddress = add.Address;

                                            if (receiversAmounts.TryGetValue(authoraddress, out var value))
                                            {
                                                receiversAmounts.Remove(authoraddress);
                                                amnt += value;
                                            }
                                            receiversAmounts.Add(authoraddress, amnt);
                                        }
                                    }
                                }
                            }
                            else
                            {
                                Console.WriteLine("Cannot process deposit split transaction. There are no Deposit Scheme in the appsetting.json config.");
                            }
                        }
                    }
                }
            });

            return receiversAmounts;
        }
    }
}
