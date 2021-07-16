using System;
using System.Collections.Concurrent;
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
            foreach(var o in orders)
            {
                if (Orders.TryGetValue(o.order_key, out var ord))
                {
                    ord = o;
                }
                else
                {
                    Orders.TryAdd(o.order_key, o);
                }
            }
            foreach(var o in Orders.Values)
            {
                if (orders.FirstOrDefault(r => r.order_key == o.order_key) == null)
                    Orders.TryRemove(o.order_key, out var rord);
            }
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
                    prd = p;
                }
                else
                {
                    Products.TryAdd(p.id, p);
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

                while (true)
                {
                    try
                    {
                        if (!firstLoad)
                        {
                            try
                            {
                                //await ReloadCategories(); //todo
                                firstLoad = true;
                            }
                            catch (Exception ex)
                            {
                                //todo
                            }
                        }
                        await ReLoadProducts();
                        await ReLoadOrders();
                        if (!string.IsNullOrEmpty(ConnectedDogeAccountAddress))
                        {
                            await CheckDogePayments();
                            await CheckReceivedPaymentsToDispatch();
                            await SendOrdersToCustomer();
                        }

                        minorRefresh--;
                        if (minorRefresh < 0)
                        {
                            //await ReloadCategories(); //todo
                        }
                        Refreshed?.Invoke(this, null);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot process standard cycle. " + ex.Message);
                        //await InvokeErrorEvent(ex.Message, "Unknown Error During Refreshing Data");
                    }

                    await Task.Delay(interval);
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
