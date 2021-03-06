﻿using Ipfs.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.NFT;
using VEDriversLite.WooCommerce.Dto;
using WordPressPCL;

namespace VEDriversLite.WooCommerce
{
    public static class WooCommerceHelpers
    {
        public static bool IsInitialized { get; set; } = false;
        private static HttpClient httpClient = new HttpClient();
        public static WooCommerceShop Shop { get; set; } = new WooCommerceShop();

        public static readonly IpfsClient ipfs = new IpfsClient("https://ipfs.infura.io:5001");
        public static WordPressClient wpClient { get; set; }
        private static void RequestFilter(HttpWebRequest request)
        {
            request.UserAgent = "VENFT App";
        }

        public static async Task<(bool,string)> UploadIFPSImageToWP(string imageLink, string name)
        {
            try
            {
                HttpClient client = new HttpClient();
                var msg = await client.GetAsync(imageLink);
                await using (Stream stream = await msg.Content.ReadAsStreamAsync())
                {
                    var type = msg.Content.Headers.ContentType.ToString();
                    var typesplit = type.Split('/');
                    if (typesplit.Length > 1 && !string.IsNullOrEmpty(typesplit[1]))
                        name += "." + typesplit[1];
                    var media = await wpClient.Media.Create(stream, name);
                    if (media != null)
                    {
                        return (true, media.SourceUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot upload the image {imageLink} to WP. " + ex.Message);
            }
            return (false, string.Empty);
        }

        public static async Task<(bool, string)> UploadIFPSImageToWPByAPI(string imageLink, string filename)
        {
            try
            {
                var fileContentType = "multipart/form-data";
                var url = VEDLDataContext.WooCommerceStoreUrl.Replace("wc/v3","wp/v2") + "media";
                HttpClient client = new HttpClient();
                var msg = await client.GetAsync(imageLink);
                if (msg.IsSuccessStatusCode)
                {
                    await using (Stream stream = await msg.Content.ReadAsStreamAsync())
                    {
                        using (var content = new MultipartFormDataContent())
                        {
                            content.Add(new StreamContent(stream)
                            {
                                Headers =
                                {
                                    ContentLength = stream.Length,
                                    ContentType = msg.Content.Headers.ContentType
                                }
                            }, "file", filename);
                            content.Add(new StringContent($"title={Path.GetFileName(filename)}"));
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", VEDLDataContext.WooCommerceStoreJWTToken);
                            
                            var resp = await client.PostAsync(url, content);
                            var m = await resp.Content.ReadAsStringAsync();
                            if (m != null)
                            {
                                return (true, m);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot upload the image {imageLink} to WP. " + ex.Message);
            }
            return (false, string.Empty);
        }

        public static async Task<(bool,string)> GetJWTToken(string apiurl, string wplogin, string wppass)
        {
            try
            {
                wpClient = new WordPressClient(apiurl.Replace("wc/v3/", string.Empty));
                wpClient.AuthMethod = WordPressPCL.Models.AuthMethod.JWT;
                await wpClient.RequestJWToken(wplogin, wppass);
                var isvalid = await wpClient.IsValidJWToken();
                var token = wpClient.GetToken();
                VEDLDataContext.WooCommerceStoreJWTToken = token;

                Console.WriteLine(token);
                return (true,token);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot get JWT token. " + ex.Message);
                return (false, "Cannot get JWT Token. " + ex.Message);
            }
        }

        public static async Task<bool> InitStoreApiConnection(string apiurl, string apikey, string secret, string jwt, bool withRefreshing = false)
        {
            try
            {
                if (string.IsNullOrEmpty(jwt)) return false;
                    //throw new Exception("Please obtain JWT token first. It is important for upload of images to the eshop. ");
                if (!string.IsNullOrEmpty(apiurl) && !string.IsNullOrEmpty(apikey) && !string.IsNullOrEmpty(secret))
                {
                    VEDLDataContext.WooCommerceStoreUrl = apiurl;
                    VEDLDataContext.WooCommerceStoreAPIKey = apikey;
                    VEDLDataContext.WooCommerceStoreSecret = secret;
                    VEDLDataContext.WooCommerceStoreJWTToken = jwt;

                    var res = await httpClient.GetAsync(GetFullAPIUrl(""));
                    var resmsg = await res.Content.ReadAsStringAsync();
                    if (res.StatusCode == HttpStatusCode.OK)
                    {
                        //Console.WriteLine(resmsg);
                        IsInitialized = true;

                        Shop = new WooCommerceShop(apiurl, apikey, secret, VEDLDataContext.WooCommerceStoreJWTToken);
                        if (withRefreshing) await Shop.StartRefreshingData();
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
                return VEDLDataContext.WooCommerceStoreUrl + $"{command}?consumer_key={VEDLDataContext.WooCommerceStoreAPIKey}&consumer_secret={VEDLDataContext.WooCommerceStoreSecret}";
        }

        public static async Task<Product> AddNewProduct(INFT nft, int quantity = 1, string apiurl = "", Dictionary<string,string> options = null)
        {
            if (IsInitialized && string.IsNullOrEmpty(apiurl))
                apiurl = GetFullAPIUrl("products");

            if (wpClient == null) throw new Exception("Please init the connection and obtain JWT Token first.");
            
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

            var resi = await UploadIFPSImageToWP(nft.ImageLink, nft.Name.Replace(" ", "_"));
            var imagelink = string.Empty;
            if (resi.Item1) imagelink = resi.Item2;

            p = new Product()
            {
                name = nft.Name,
                description = nft.Text,
                regular_price = price,
                images = new List<ImageObject>() { new ImageObject() { src = imagelink } },
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
