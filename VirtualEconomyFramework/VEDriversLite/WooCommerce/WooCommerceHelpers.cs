using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NFT;
using VEDriversLite.WooCommerce.Dto;

namespace VEDriversLite.WooCommerce
{
    public static class WooCommerceHelpers
    {
        public static bool IsInitialized { get; set; } = false;
        private static HttpClient httpClient = new HttpClient();
        public static WooCommerceShop Shop { get; set; } = new WooCommerceShop();
        private static void RequestFilter(HttpWebRequest request)
        {
            request.UserAgent = "VENFT App";
        }
        public static async Task<bool> InitStoreApiConnection(string apiurl, string apikey, string secret)
        {
            try
            {
                if (!string.IsNullOrEmpty(apiurl) && !string.IsNullOrEmpty(apikey) && !string.IsNullOrEmpty(secret))
                {
                    VEDLDataContext.WooCommerceStoreUrl = apiurl;
                    VEDLDataContext.WooCommerceStoreAPIKey = apikey;
                    VEDLDataContext.WooCommerceStoreSecret = secret;

                    var res = await httpClient.GetAsync(GetFullAPIUrl(""));
                    var resmsg = await res.Content.ReadAsStringAsync();
                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        //Console.WriteLine(resmsg);
                        IsInitialized = true;
                        Shop = new WooCommerceShop(apiurl, apikey, secret);
                        await Shop.StartRefreshingData();
                        return true;
                    }
                    else
                    {
                        Console.WriteLine(resmsg);
                        IsInitialized = false;
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot init WooCommerce store API connection. " + ex.Message);
                return false;
            }
        }

        public static string GetFullAPIUrl(string command, string apiurl = "", string apikey = "", string secret = "")
        {
            if (!string.IsNullOrEmpty(apiurl) && !string.IsNullOrEmpty(apikey) && !string.IsNullOrEmpty(secret))
                return apiurl + $"{command}?consumer_key={apikey}&consumer_secret={secret}";
            else
                return VEDLDataContext.WooCommerceStoreUrl + $"products?consumer_key={VEDLDataContext.WooCommerceStoreAPIKey}&consumer_secret={VEDLDataContext.WooCommerceStoreSecret}";
        }

        public static async Task<Product> AddNewProduct(INFT nft, int quantity = 1, string apiurl = "", Dictionary<string,string> options = null)
        {
            if (IsInitialized && string.IsNullOrEmpty(apiurl))
                apiurl = GetFullAPIUrl("products");

            Product p = null;
            var link = nft.ImageLink; 

            if (nft.Type == NFTTypes.Music)
            {
                link = nft.Link;
            }

            var price = "0.0";
            if (nft.DogePriceActive)
                price = Convert.ToString(nft.DogePrice, CultureInfo.InvariantCulture).Replace(",", ".");
            else if (nft.PriceActive)
                price = Convert.ToString(nft.Price, CultureInfo.InvariantCulture).Replace(",", ".");
            
            p = new Product()
            {
                name = nft.Name,
                description = nft.Text,
                regular_price = price,
                //images = new List<ImageObject>() { new ImageObject() { src = link } },
                status = "publish",
                downloadable = true,
                stock_quantity = quantity,
                stock_status_enum = StockStatus.instock,
                _virtual = true,
                type = "simple",
                short_description = nft.Description,
                meta_data = new List<ProductMetadata>() {
                        new ProductMetadata() { key = "Utxo", value =  nft.Utxo },
                        new ProductMetadata() { key = "Utxoindex", value =  nft.UtxoIndex.ToString() },
                        new ProductMetadata() { key = "ShortHash", value =  nft.ShortHash }
                        },
                downloads = new List<DownloadsObject>() {
                        new DownloadsObject() {
                            name = "NFT on IPFS",
                            file = link,
                            external_url = $"https://nft.ve-nft.com/?txid={nft.Utxo}",
                            categories = new List<CategoryOfDownloads>() {
                                new CategoryOfDownloads() { id = 3 }
                            }
                        }
                    }
            };

            if (options != null)
            {
                foreach(var o in options)
                {
                    p.meta_data.Add(new ProductMetadata() { key = o.Key, value = o.Value });
                }
            }

            var pr = JsonConvert.SerializeObject(p);
            pr = pr.Replace("_virtual", "virtual");
            var content = new StringContent(pr, Encoding.UTF8, "application/json");
            try
            {
                var res = await httpClient.PostAsync(apiurl, content);
                var resmsg = await res.Content.ReadAsStringAsync();
                //Console.WriteLine(resmsg);
                var prd = JsonConvert.DeserializeObject<Product>(resmsg);
                return prd;
            }
            catch(Exception ex) { Console.WriteLine(ex.Message); }

            return new Product();
        }

        public static async Task<Order> UpdateOrder(Order order, string apiurl = "")
        {
            if (IsInitialized && string.IsNullOrEmpty(apiurl))
                apiurl = GetFullAPIUrl($"orders/{order.id}");

            var ord = JsonConvert.SerializeObject(order);
            var content = new StringContent(ord, Encoding.UTF8, "application/json");
            try
            {
                var res = await httpClient.PutAsync(apiurl, content);
                var resmsg = await res.Content.ReadAsStringAsync();
                //Console.WriteLine(resmsg);
                var ordr = JsonConvert.DeserializeObject<Order>(resmsg);
                return ordr;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return new Order();
        }

        public static async Task<Product> GetProduct(string id, string apiurl = "")
        {
            try
            {
                if (IsInitialized && string.IsNullOrEmpty(apiurl))
                    apiurl = GetFullAPIUrl($"products/{id}");

                var res = await httpClient.GetAsync(apiurl);
                var resmsg = await res.Content.ReadAsStringAsync();
                //Console.WriteLine(resmsg);
                var prd = JsonConvert.DeserializeObject<Product>(resmsg);
                if (resmsg.Contains("\"virtual\": true") || resmsg.Contains("\"virtual\":true") || resmsg.Contains("\'virtual\': true") || resmsg.Contains("\'virtual\':true"))
                    prd._virtual = true;
                return prd;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return new Product();
        }

        public static async Task<Order> GetOrder(string id, string apiurl = "")
        {
            try
            {
                if (IsInitialized && string.IsNullOrEmpty(apiurl))
                    apiurl = GetFullAPIUrl($"orders/{id}");

                var res = await httpClient.GetAsync(apiurl);
                var resmsg = await res.Content.ReadAsStringAsync();
                //Console.WriteLine(resmsg);
                var ord = JsonConvert.DeserializeObject<Order>(resmsg);
                return ord;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return new Order();
        }

        public static async Task<List<Product>> GetAllProducts(string apiurl = "")
        {
            try
            {
                if (IsInitialized && string.IsNullOrEmpty(apiurl))
                    apiurl = GetFullAPIUrl("products");
                apiurl += "&per_page=100";
                var res = await httpClient.GetAsync(apiurl);
                var resmsg = await res.Content.ReadAsStringAsync();
                //Console.WriteLine(resmsg);
                var prds = JsonConvert.DeserializeObject<List<Product>>(resmsg);
                return prds;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return new List<Product>();
        }

        public static async Task<List<Order>> GetAllOrders(string apiurl = "")
        {
            try
            {
                if (IsInitialized && string.IsNullOrEmpty(apiurl))
                    apiurl = GetFullAPIUrl("orders");
                apiurl += "&per_page=100";
                var res = await httpClient.GetAsync(apiurl);
                var resmsg = await res.Content.ReadAsStringAsync();
                //Console.WriteLine(resmsg);
                var ords = JsonConvert.DeserializeObject<List<Order>>(resmsg);
                return ords;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return new List<Order>();
        }

    }
}
