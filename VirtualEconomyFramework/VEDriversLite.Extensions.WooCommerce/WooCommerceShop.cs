using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NFT;
using VEDriversLite.Extensions.WooCommerce.Dto;

namespace VEDriversLite.Extensions.WooCommerce
{
    public partial class WooCommerceShop
    {
        public WooCommerceShop()
        {
        }
        public WooCommerceShop(string apiurl, string apikey, string secret, string jwt, bool allowDispatchNFTOrders)
        {
            WooCommerceStoreUrl = apiurl;
            WooCommerceStoreAPIKey = apikey;
            WooCommerceStoreSecret = secret;
            WooCommerceStoreJWTToken = jwt;
            AllowDispatchNFTOrders = allowDispatchNFTOrders;
        }
        /// <summary>
        /// TODO
        /// </summary>
        public string WooCommerceStoreUrl { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string WooCommerceStoreAPIKey { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string WooCommerceStoreSecret { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public string WooCommerceStoreJWTToken { get; set; } = string.Empty;
        /// <summary>
        /// TODO
        /// </summary>
        public bool IsRefreshingRunning { get; set; } = false;
        /// <summary>
        /// TODO
        /// </summary>
        public bool AllowDispatchNFTOrders { get; set; } = true;
        /// <summary>
        /// If there is some Doge address in same project which should be searched for the payments triggers fill it here
        /// </summary>
        public string ConnectedDogeAccountAddress { get; set; } = string.Empty;
        /// <summary>
        /// If there is some Doge address in same project which should be used as deposit account for completed orders payments fill it here
        /// This helps to keep low amount of utxos on main doge account address where the new payments are searched. 
        /// </summary>
        public string ConnectedDepositDogeAccountAddress { get; set; } = string.Empty;
        /// <summary>
        /// If there is some Neblio address in same project which should be searched for the payments triggers fill it here
        /// </summary>
        public string ConnectedNeblioAccountAddress { get; set; } = string.Empty;
        /// <summary>
        /// If there is some Neblio address in same project which should be used as deposit account for completed orders payments fill it here
        /// This helps to keep low amount of utxos on main neblio account address where the new payments are searched. 
        /// </summary>
        public string ConnectedDepositNeblioAccountAddress { get; set; } = string.Empty;
        /// <summary>
        /// This address is used as main address for processing NFT orders
        /// </summary>
        public string ConnectedNFTWarehouseNeblioAccountAddress { get; set; } = string.Empty;

        /// <summary>
        /// TODO
        /// </summary>
        public ConcurrentDictionary<string, Order> Orders { get; set; } = new ConcurrentDictionary<string, Order>();
        /// <summary>
        /// TODO
        /// </summary>
        public ConcurrentDictionary<int, Product> Products { get; set; } = new ConcurrentDictionary<int, Product>();
        /// <summary>
        /// TODO
        /// </summary>
        public ConcurrentDictionary<int, Category> Categories { get; set; } = new ConcurrentDictionary<int, Category>();
        /// <summary>
        /// TODO
        /// </summary>
        public ConcurrentDictionary<string, NFTOrderToDispatch> NFTOrdersToDispatchDict { get; set; } = new ConcurrentDictionary<string, NFTOrderToDispatch>();

        /// <summary>
        /// This event is called whenever info about the shop is reloaded. It is periodic event.
        /// </summary>
        public event EventHandler Refreshed;

        private System.Timers.Timer refreshTimer = new System.Timers.Timer();

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="interval"></param>
        /// <returns></returns>
        public async Task StartRefreshingData(double interval = 5000)
        {
            await Reload();
            refreshTimer.Interval = interval;
            refreshTimer.AutoReset = false;
            refreshTimer.Elapsed += RefreshTimer_Elapsed;
            refreshTimer.Enabled = true;
            refreshTimer.Start();
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public async Task StopRefreshingData()
        {
            refreshTimer.Stop();
            refreshTimer.Enabled = false;
            refreshTimer.Elapsed += RefreshTimer_Elapsed;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            refreshTimer.Stop();
            refreshTimer.Enabled = false;
            Reload().Wait();
            refreshTimer.Enabled = true;
            refreshTimer.Start();
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public async Task Reload()
        {
            try
            {
                await ReLoadProducts();
                await ReLoadOrders();
                await ReLoadCategories();
                if (!string.IsNullOrEmpty(ConnectedDogeAccountAddress))
                {
                    await CheckDogePayments();
                }
                else if (!string.IsNullOrEmpty(ConnectedNeblioAccountAddress))
                {
                    await CheckNeblioPayments();
                }

                if (AllowDispatchNFTOrders)
                {
                    await CheckReceivedPaymentsToDispatch();
                    await SendOrdersToCustomer();
                }
                Refreshed?.Invoke(this, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot process standard cycle. " + ex.Message);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
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
                            var valid = false;
                            var add = string.Empty;
                            if (AllowDispatchNFTOrders)
                            {
                                var validateNeblioAddress = await GetNeblioAddressFromOrderMetadata(o);
                                valid = validateNeblioAddress.Item1;
                                add = validateNeblioAddress.Item2;
                            }

                            if (valid && AllowDispatchNFTOrders)
                            {
                                Console.WriteLine($"New Order received:{o.id}, {o.order_key}");
                                Console.WriteLine($"Order {o.id}, {o.order_key} contains valid Neblio Address for receiving NFTs https://explorer.nebl.io/address/{add}");
                                var validNFTItems = true;
                                o.line_items.ForEach(async (item) =>
                                {
                                    var sh = string.Empty;
                                    item.meta_data.ForEach(m =>
                                    {
                                        if (m.key == "ShortHash") sh = Convert.ToString(m.value);
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
                                        o.meta_data.Add(new ProductMetadata()
                                        {
                                            key = "Message from VENFT Server",
                                            value = "Cannot find the NFTs for the items in the order."
                                        });
                                    }
                                });
                                try
                                {
                                    await UpdateOrder(o);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Cannot update the order. " + ex.Message);
                                }
                            }
                            else if (!AllowDispatchNFTOrders)
                            {
                               Console.WriteLine($"New Order received:{o.id}, {o.order_key}");
                                try
                                {
                                    o.statusclass = OrderStatus.pending;
                                    Orders.TryAdd(o.order_key, o);
                                    await UpdateOrder(o);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Cannot update the order. " + ex.Message);
                                }
                            }
                            else if (AllowDispatchNFTOrders && !valid)
                            {
                                Console.WriteLine($"Order {o.id}, {o.order_key} does not contains valid Neblio address.");
                                o.statusclass = OrderStatus.failed;
                                o.meta_data.Add(new ProductMetadata()
                                {
                                    key = "Message from VENFT Server",
                                    value = "Does not contains valid Neblio address."
                                });
                                try
                                {
                                    await UpdateOrder(o, true);
                                }
                                catch (Exception ex)
                                {
                                    Console.WriteLine("Cannot update the order. " + ex.Message);
                                }
                            }
                        }
                    }
                    else if (o.statusclass == OrderStatus.pending || o.statusclass == OrderStatus.processing)// || o.statusclass == OrderStatus.completed)
                    {
                        Console.WriteLine($"Order {o.id}, {o.order_key} is already in the {o.status} state. Just adding to the list.");
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

        /// <summary>
        /// TODO REFACTOR THIS METHOD AND CHANGE THE REFERENCES
        /// </summary>
        /// <param name="o"></param>
        /// <returns></returns>
        public async Task<(bool,string)> GetNeblioAddressFromOrderMetadata(Order o)
        {
            string response = string.Empty;
            bool counter = false;
            var res = (counter, response);
            o.meta_data.ForEach(async (m) =>
            {
                if (m.key == "_billing_virtual_economy_wallet_nebl_address" || m.key == "_billing_neblio_address" || m.key == VEDLDataContext.WooCommerceStoreCheckoutFieldCustomerNeblioAddress)
                {
                    response = NeblioTransactionHelpers.ValidateNeblioAddress(Convert.ToString(m.value));
                    counter = true;
                }
            });
            return res;
        }
        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
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

        /// <summary>
        /// TODO
        /// </summary>
        /// <returns></returns>
        public async Task ReLoadCategories()
        {
            var categories = await WooCommerceHelpers.GetAllCategoreis(
                WooCommerceHelpers.GetFullAPIUrl(
                    "products/categories", WooCommerceStoreUrl, WooCommerceStoreAPIKey, WooCommerceStoreSecret)
                );

            foreach (var c in categories)
            {
                if (Categories.TryGetValue(c.id, out var cat))
                {
                    cat.Fill(c);
                }
                else
                {
                    cat = new Category();
                    cat.Fill(c);
                    Categories.TryAdd(cat.id, cat);
                }
            }
            foreach (var o in Categories.Values)
            {
                if (categories.FirstOrDefault(r => r.id == o.id) == null)
                    Categories.TryRemove(o.id, out var rprd);
            }
        }

        /*
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
        */

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="orderkey"></param>
        /// <param name="status"></param>
        /// <returns></returns>
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

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="orderkey"></param>
        /// <param name="txid"></param>
        /// <returns></returns>
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

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="order"></param>
        /// <param name="withoutListCheck"></param>
        /// <returns></returns>
        public async Task<Order> UpdateOrder(Order order, bool withoutListCheck = false)
        {
            if (!withoutListCheck)
            {
                if (Orders.TryGetValue(order.order_key, out var ord))
                {
                    var o = await WooCommerceHelpers.UpdateOrder(order,
                        WooCommerceHelpers.GetFullAPIUrl(
                            $"orders/{ord.id}", WooCommerceStoreUrl, WooCommerceStoreAPIKey, WooCommerceStoreSecret)
                        );
                    ord.Fill(o);
                    return ord;
                }
                else
                {
                    throw new Exception("Cannot find the order.");
                }
            }
            else
            {
                var o = await WooCommerceHelpers.UpdateOrder(order,
                        WooCommerceHelpers.GetFullAPIUrl(
                            $"orders/{order.id}", WooCommerceStoreUrl, WooCommerceStoreAPIKey, WooCommerceStoreSecret)
                        );
                order.Fill(o);
                return order;
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="nft"></param>
        /// <param name="categories"></param>
        /// <param name="quantity"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public async Task<Product> AddProduct(INFT nft, List<Category> categories, int quantity = 1, Dictionary<string,string> options = null)
        {
            try
            {
                var prod = await WooCommerceHelpers.AddNewProduct(nft, categories, quantity,
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
            catch(Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="productid"></param>
        /// <returns></returns>
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

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="orderid"></param>
        /// <returns></returns>
        public async Task<Order> GetOrder(int orderid)
        {
            var ord = await WooCommerceHelpers.GetOrder(orderid.ToString(),
                    WooCommerceHelpers.GetFullAPIUrl(
                        $"orders/{orderid}", WooCommerceStoreUrl, WooCommerceStoreAPIKey, WooCommerceStoreSecret)
                    );
            if (Orders.TryGetValue(ord.order_key, out var o))
                o.Fill(ord);
            return ord;
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
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
