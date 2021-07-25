using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NFT;
using VEDriversLite.WooCommerce.Dto;

namespace VEDriversLite.WooCommerce
{
    public partial class WooCommerceShop
    {
        private async Task CheckReceivedPaymentsToDispatch()
        {
            var processingOrders = Orders.Values.Where(o => o.statusclass == OrderStatus.processing).ToList();
            foreach (var order in processingOrders)
            {
                order.line_items.ForEach(async (item) =>
                {
                    var sh = string.Empty;
                    var cat = string.Empty;
                    if (Products.TryGetValue(item.product_id, out var prod))
                    {
                        prod.meta_data.ForEach(m =>
                        {
                            if (m.key == "ShortHash") sh = m.value;
                            if (m.key == "Category") cat = m.value;
                        });
                        if (VEDLDataContext.NFTHashs.TryGetValue(sh, out var nfth))
                        {
                            var validateNeblioAddress = await GetNeblioAddressFromOrderMetadata(order);
                            if (validateNeblioAddress.Item1)
                            {
                                Console.WriteLine($"Order {order.id}, {order.order_key} processing...moving item id {item.product_id}({sh}) to dispatch list.");
                                if (!string.IsNullOrEmpty(cat))
                                {
                                    NFTOrdersToDispatchList.Enqueue(new NFTOrderToDispatch()
                                    {
                                        NeblioCustomerAddress = validateNeblioAddress.Item2, // todo take from order
                                        Category = cat,
                                        Quantity = item.quantity,
                                        IsCategory = true,
                                        IsUnique = false,
                                        ShortHash = sh,
                                        OrderId = order.id,
                                        OrderKey = order.order_key,
                                        PaymentId = order.transaction_id,
                                        Utxo = nfth.TxId,
                                        UtxoIndex = nfth.Index,
                                        OwnerMainAccount = nfth.MainAddress,
                                        OwnerSubAccount = nfth.SubAccountAddress
                                    });
                                }
                                else if (!string.IsNullOrEmpty(sh))
                                {
                                    NFTOrdersToDispatchList.Enqueue(new NFTOrderToDispatch()
                                    {
                                        NeblioCustomerAddress = validateNeblioAddress.Item2, // todo take from order
                                        Category = string.Empty,
                                        IsCategory = false,
                                        IsUnique = true,
                                        ShortHash = sh,
                                        OrderId = order.id,
                                        OrderKey = order.order_key,
                                        PaymentId = order.transaction_id,
                                        Utxo = nfth.TxId,
                                        UtxoIndex = nfth.Index,
                                        OwnerMainAccount = nfth.MainAddress,
                                        OwnerSubAccount = nfth.SubAccountAddress
                                    });
                                }
                            }
                        }
                    }
                });
            }
        }

        private async Task SendOrdersToCustomer()
        {
            while (!NFTOrdersToDispatchList.IsEmpty)
            {
                if (NFTOrdersToDispatchList.TryDequeue(out var dto))
                {
                    if (Orders.TryGetValue(dto.OrderKey, out var order))
                    {
                        if (order.statusclass == OrderStatus.processing)
                        {
                            if (!VEDLDataContext.Accounts.TryGetValue(dto.OwnerMainAccount, out var account))
                            {
                                Console.WriteLine("Cannot process the order. NFT Owner Neblio Address is not imported to Accounts list.");
                                return;
                            }
                            var nft = await NFTFactory.GetNFT("", dto.Utxo, dto.UtxoIndex, 0, true);
                            if (nft == null)
                            {
                                Console.WriteLine($"Cannot process the order {dto.OrderKey} - {dto.OrderId}. Cannot load the NFT.");
                                return;
                            }
                            var done = false;
                            (bool, string) res = (false, string.Empty);
                            if (dto.IsCategory)
                            {
                                var check = false;
                                order.meta_data.ForEach(m =>
                                {
                                    if (m.key == dto.Category)
                                        check = true;
                                });
                                if (!check)
                                {
                                    Console.WriteLine($"Order {order.id}, {order.order_key}. Sending NFT {dto.ShortHash} to Address https://explorer.nebl.io/address/{dto.NeblioCustomerAddress}");

                                    var attempts = 20;
                                    while (!done)
                                    {
                                        try
                                        {
                                            if (string.IsNullOrEmpty(dto.OwnerSubAccount))
                                                res = await account.MintMultiNFTLargeAmount(nft, dto.Quantity, dto.NeblioCustomerAddress);
                                            else
                                                res = await account.MultimintNFTLargeOnSubAccount(dto.OwnerSubAccount, nft, dto.Quantity, dto.NeblioCustomerAddress);
                                            done = res.Item1;
                                            if (!res.Item1)
                                                await Task.Delay(5000);
                                            else
                                            {
                                                order.meta_data.Add(new ProductMetadata() { key = dto.Category, value = res.Item2 });
                                                Console.WriteLine($"Order {order.id}, {order.order_key}. NFT {dto.ShortHash} sent in tx: https://explorer.nebl.io/tx/{res.Item2}");
                                            }
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
                            else if (dto.IsUnique)
                            {
                                var check = false;
                                order.meta_data.ForEach(m =>
                                {
                                    if (m.key == dto.ShortHash)
                                        check = true;
                                });
                                if (!check)
                                {
                                    Console.WriteLine($"Order {order.id}, {order.order_key}. Sending NFT {dto.ShortHash} to Address https://explorer.nebl.io/address/{dto.NeblioCustomerAddress}");

                                    var attempts = 20;
                                    while (!done)
                                    {
                                        try
                                        {
                                            if (string.IsNullOrEmpty(dto.OwnerSubAccount))
                                                res = await account.SendNFT(dto.NeblioCustomerAddress, nft);
                                            else
                                                res = await account.SendNFTFromSubAccount(dto.OwnerSubAccount, dto.NeblioCustomerAddress, nft);
                                            done = res.Item1;
                                            if (!res.Item1)
                                                await Task.Delay(5000);
                                            else
                                            {
                                                order.meta_data.Add(new ProductMetadata() { key = dto.ShortHash, value = res.Item2 });
                                                Console.WriteLine($"Order {order.id}, {order.order_key}. NFT {dto.ShortHash} sent in tx: https://explorer.nebl.io/tx/{res.Item2}");
                                            }
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
                            var complete = 0;
                            order.line_items.ForEach(item =>
                            {
                                var sh = string.Empty;
                                var cat = string.Empty;
                                if (Products.TryGetValue(item.product_id, out var product))
                                {
                                    product.meta_data.ForEach(m =>
                                    {
                                        if (m.key == "ShortHash") sh = m.value;
                                        if (m.key == "Category") cat = m.value;
                                    });
                                    if (!string.IsNullOrEmpty(cat))
                                    {
                                        order.meta_data.ForEach(m =>
                                        {
                                            if (m.key.Contains(cat)) complete++;
                                        });
                                    }
                                    else if (string.IsNullOrEmpty(cat) && !string.IsNullOrEmpty(sh))
                                    {
                                        order.meta_data.ForEach(m =>
                                        {
                                            if (m.key.Contains(sh)) complete++;
                                        });
                                    }
                                }
                            });
                            if (complete == order.line_items.Count)
                            {
                                Console.WriteLine($"Order {order.id}, {order.order_key}. Is complete. All items was sent.");
                                order.statusclass = OrderStatus.completed;
                            }
                            var o = await UpdateOrder(order);
                            order.Fill(o);
                        }
                    }
                }
            }
        }
    }
}
