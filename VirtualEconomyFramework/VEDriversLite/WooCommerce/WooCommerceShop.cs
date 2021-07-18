using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NFT;
using VEDriversLite.WooCommerce.Dto;

namespace VEDriversLite.WooCommerce
{
    public partial class WooCommerceShop
    {
        public WooCommerceShop()
        {
        }
        public WooCommerceShop(string apiurl, string apikey, string secret)
        {
            WooCommerceStoreUrl = apiurl;
            WooCommerceStoreAPIKey = apikey;
            WooCommerceStoreSecret = secret;
        }
        public string WooCommerceStoreUrl { get; set; } = string.Empty;
        public string WooCommerceStoreAPIKey { get; set; } = string.Empty;
        public string WooCommerceStoreSecret { get; set; } = string.Empty;
        public bool IsRefreshingRunning { get; set; } = false;
        public bool AllowDispatchNFTOrders { get; set; } = true;
        /// <summary>
        /// If there is some Doge address in same project which should be searched for the payments triggers fill it here
        /// </summary>
        public string ConnectedDogeAccountAddress { get; set; } = string.Empty;
        /// <summary>
        /// If there is some Neblio address in same project which should be searched for the payments triggers fill it here
        /// This address is used as main address for processing NFT orders
        /// </summary>
        public string ConnectedNeblioAccountAddress { get; set; } = string.Empty;

        public ConcurrentDictionary<string, Order> Orders { get; set; } = new ConcurrentDictionary<string, Order>();
        public ConcurrentDictionary<int, Product> Products { get; set; } = new ConcurrentDictionary<int, Product>();

        private ConcurrentQueue<ReceivedPaymentForOrder> ReceivedPaymentsForOrdersToProcess = new ConcurrentQueue<ReceivedPaymentForOrder>();
        private ConcurrentQueue<NFTOrderToDispatch> NFTOrdersToDispatchList = new ConcurrentQueue<NFTOrderToDispatch>();

        /// <summary>
        /// This event is called whenever info about the shop is reloaded. It is periodic event.
        /// </summary>
        public event EventHandler Refreshed;

        public async Task ReLoadOrders()
        {
            var orders = await WooCommerceHelpers.GetAllOrders(
                WooCommerceHelpers.GetFullAPIUrl(
                    "orders", WooCommerceStoreUrl, WooCommerceStoreAPIKey, WooCommerceStoreSecret)
                );
            //Orders.Clear();
            foreach(var o in orders)
            {
                if (Orders.TryGetValue(o.order_key, out var ord))
                {
                    ord.Fill(o);
                }
                else
                {
                    if (o.statusclass == OrderStatus.onhold)
                    {
                        if (o.line_items.Count > 0)
                        {
                            var validateNeblioAddress = await GetNeblioAddressFromOrderMetadata(o);
                            if (validateNeblioAddress.Item1)
                            {
                                Console.WriteLine($"New Order received:{o.id}, {o.order_key}");
                                Console.WriteLine($"Order {o.id}, {o.order_key} contains valid Neblio Address for receiving NFTs https://explorer.nebl.io/address/{validateNeblioAddress.Item2}");
                                var validNFTItems = true;
                                o.line_items.ForEach(async (item) =>
                                {
                                    var sh = string.Empty;
                                    item.meta_data.ForEach(m =>
                                    {
                                        if (m.key == "ShortHash") sh = m.value;
                                    });
                                    if (!string.IsNullOrEmpty(sh))
                                    {
                                        if (VEDLDataContext.NFTHashs.TryGetValue(sh, out var nfthash))
                                            validNFTItems = false;
                                    }
                                    if (validNFTItems)
                                    {
                                        Console.WriteLine($"Order {o.id}, {o.order_key} contains valid NFT items. All are in the stock.");
                                        o.meta_data.Add(new ProductMetadata()
                                        {
                                            key = "Message from VENFT Server",
                                            value = "All NFTs are in the stock on the wallets."
                                        });
                                        o.statusclass = OrderStatus.pending;
                                        Orders.TryAdd(o.order_key, o);
                                    }
                                    else
                                    {
                                        Console.WriteLine($"Order {o.id}, {o.order_key} failed because NFT items not found in the stock.");
                                        o.statusclass = OrderStatus.failed;
                                        o.meta_data.Add(new ProductMetadata() { 
                                            key = "Message from VENFT Server", 
                                            value = "Cannot find the NFTs for the items in the order." 
                                        });
                                    }
                                    try
                                    {
                                        await UpdateOrder(o);
                                    }
                                    catch(Exception ex)
                                    {
                                        Console.WriteLine("Cannot update the order. " + ex.Message);
                                    }
                                });
                            }
                        }
                    }
                    else if (o.statusclass == OrderStatus.pending || o.statusclass == OrderStatus.processing)
                    {
                        ord = new Order();
                        ord.Fill(o);
                        Orders.TryAdd(ord.order_key, ord);
                    }
                }
            }
            foreach(var o in Orders.Values)
            {
                if (orders.FirstOrDefault(r => r.order_key == o.order_key) == null)
                    Orders.TryRemove(o.order_key, out var rord);
            }
        }

        public async Task<(bool,string)> GetNeblioAddressFromOrderMetadata(Order o)
        {
            var res = (false, string.Empty);
            o.meta_data.ForEach(async (m) =>
            {
                if (m.key == "_billing_virtual_economy_wallet_nebl_address")
                    res = await NeblioTransactionHelpers.ValidateNeblioAddress(m.value);
            });
            return res;
        }
        public async Task ReLoadProducts()
        {
            var products = await WooCommerceHelpers.GetAllProducts(
                WooCommerceHelpers.GetFullAPIUrl(
                    "products", WooCommerceStoreUrl, WooCommerceStoreAPIKey, WooCommerceStoreSecret)
                );

            foreach (var p in products)
            {
                if (Products.TryGetValue(p.id, out var prd))
                {
                    prd.Fill(p);
                }
                else
                {
                    prd = new Product();
                    prd.Fill(p);
                    Products.TryAdd(prd.id, prd);
                }
            }
            foreach (var o in Products.Values)
            {
                if (products.FirstOrDefault(r => r.id == o.id) == null)
                    Products.TryRemove(o.id, out var rprd);
            }
        }

        public async Task<string> StartRefreshingData(int interval = 10000)
        {
            await ReLoadProducts();
            await ReLoadOrders();

            var minorRefresh = 5;
            var firstLoad = true;
            // todo cancelation token
            _ = Task.Run(async () =>
            {
                IsRefreshingRunning = true;
                Stopwatch st = new Stopwatch();

                while (true)
                {
                    long reduceTime = 0;
                    try
                    {
                        if (firstLoad)
                        {
                            try
                            {
                                //await ReloadCategories(); //todo
                                firstLoad = false;
                            }
                            catch (Exception ex)
                            {
                                //todo
                            }
                        }
                        else
                        {
                            await ReLoadProducts();
                            await ReLoadOrders();
                        }

                        if (!string.IsNullOrEmpty(ConnectedDogeAccountAddress))
                        {
                            await CheckDogePayments();
                        }
                        if (AllowDispatchNFTOrders)
                        {
                            await CheckReceivedPaymentsToDispatch();
                            st.Start();
                            await SendOrdersToCustomer();
                            st.Stop();
                            var spenttime = st.ElapsedMilliseconds;
                            st.Reset();
                            if (spenttime < interval)
                                reduceTime = spenttime;
                            else
                                reduceTime = interval;
                        }

                        minorRefresh--;
                        if (minorRefresh < 0)
                        {
                            minorRefresh = 5;
                            //await ReloadCategories(); //todo
                        }
                        Refreshed?.Invoke(this, null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot process standard cycle. " + ex.Message);
                        //await InvokeErrorEvent(ex.Message, "Unknown Error During Refreshing Data");
                    }

                    await Task.Delay(interval - (int)reduceTime);
                }
                IsRefreshingRunning = false;
            });

            return await Task.FromResult("RUNNING");
        }

        public async Task<Order> UpdateOrderStatus(string orderkey, string status)
        {
            if (Orders.TryGetValue(orderkey, out var ord))
            {
                var stat = (OrderStatus)Enum.Parse(typeof(OrderStatus), status);
                ord.statusclass = stat;
                var order = await WooCommerceHelpers.UpdateOrder(ord,
                    WooCommerceHelpers.GetFullAPIUrl(
                        $"orders/{ord.id}", WooCommerceStoreUrl, WooCommerceStoreAPIKey, WooCommerceStoreSecret)
                    );
                if (order != null)
                    ord = order;
                return order;
            }
            else
            {
                throw new Exception("Cannot find the order.");
            }
        }
        public async Task<Order> UpdateOrderTxId(string orderkey, string txid)
        {
            if (Orders.TryGetValue(orderkey, out var ord))
            {
                ord.transaction_id = txid;
                var order = await WooCommerceHelpers.UpdateOrder(ord,
                    WooCommerceHelpers.GetFullAPIUrl(
                        $"orders/{ord.id}", WooCommerceStoreUrl, WooCommerceStoreAPIKey, WooCommerceStoreSecret)
                    );
                if (order != null)
                    ord = order;
                return order;
            }
            else
            {
                throw new Exception("Cannot find the order.");
            }
        }
        public async Task<Order> UpdateOrder(Order order)
        {
            if (Orders.TryGetValue(order.order_key, out var ord))
            {
                var o = await WooCommerceHelpers.UpdateOrder(order,
                    WooCommerceHelpers.GetFullAPIUrl(
                        $"orders/{ord.id}", WooCommerceStoreUrl, WooCommerceStoreAPIKey, WooCommerceStoreSecret)
                    );
                return order;
            }
            else
            {
                throw new Exception("Cannot find the order.");
            }
        }

        public async Task<Product> AddProduct(INFT nft, int quantity = 1, Dictionary<string,string> options = null)
        {
            var prod = await WooCommerceHelpers.AddNewProduct(nft, quantity,
                WooCommerceHelpers.GetFullAPIUrl($"products", 
                                                 WooCommerceStoreUrl, 
                                                 WooCommerceStoreAPIKey, 
                                                 WooCommerceStoreSecret),
                options);
            if (Products.TryGetValue(prod.id, out var prd))
                prd = prod;
            else
                Products.TryAdd(prod.id, prod);
            return prod;
        }

        public async Task<Product> GetProduct(int productid)
        {
            var prod = await WooCommerceHelpers.GetProduct(productid.ToString(),
                    WooCommerceHelpers.GetFullAPIUrl(
                        $"products/{productid}", WooCommerceStoreUrl, WooCommerceStoreAPIKey, WooCommerceStoreSecret)
                    );
            if (Products.TryGetValue(productid, out var prd))
                prd = prod;
            return prod;
        }


        public async Task<Order> GetOrder(int orderid)
        {
            var ord = await WooCommerceHelpers.GetOrder(orderid.ToString(),
                    WooCommerceHelpers.GetFullAPIUrl(
                        $"orders/{orderid}", WooCommerceStoreUrl, WooCommerceStoreAPIKey, WooCommerceStoreSecret)
                    );
            if (Orders.TryGetValue(ord.order_key, out var o))
                o = ord;
            return ord;
        }

        public async Task<bool> CheckIfCategoryExists(string category)
        {
            var exists = false;
            Products.Values.ToList().ForEach(p =>
            {
                p.meta_data.ForEach(m =>
                {
                    if (m.value == "Category" && m.key == category) exists = true;
                });
            });
            return exists;
        }
    }
}
