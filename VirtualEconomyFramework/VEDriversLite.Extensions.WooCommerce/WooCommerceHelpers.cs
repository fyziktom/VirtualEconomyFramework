using Ipfs.Http;
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
using VEDriversLite.Common;
using VEDriversLite.NFT;
using VEDriversLite.Extensions.WooCommerce.Dto;
using WordPressPCL;

namespace VEDriversLite.Extensions.WooCommerce
{
    /// <summary>
    /// Woo Commerce API integration helper class
    /// </summary>
    public static class WooCommerceHelpers
    {
        /// <summary>
        /// Is helper class initialized
        /// </summary>
        public static bool IsInitialized { get; set; } = false;
        private static HttpClient httpClient = new HttpClient();
        /// <summary>
        /// TODO
        /// </summary>
        public static WooCommerceShop Shop { get; set; } = new WooCommerceShop();
        /// <summary>
        /// TODO
        /// </summary>

        public static readonly IpfsClient ipfs = new IpfsClient("https://ipfs.infura.io:5001");
        /// <summary>
        /// TODO
        /// </summary>
        public static WordPressClient wpClient { get; set; }
        private static void RequestFilter(HttpWebRequest request)
        {
            request.UserAgent = "VENFT App";
        }

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="imageLink"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static async Task<(bool,string)> UploadIFPSImageToWP(string imageLink, string name)
        {
            try
            {
                HttpClient client = new HttpClient();
                var msg = await client.GetAsync(imageLink);
                await using (Stream stream = await msg.Content.ReadAsStreamAsync())
                {
                    var type = msg.Content.Headers.ContentType.ToString();
                    if (type.Contains("mp3") || type.Contains("mp4") || type.Contains("avi"))
                        return (false, "Cannot use mp3,mp4, or avi as the product image.");
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

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="imageLink"></param>
        /// <param name="filename"></param>
        /// <returns></returns>
        public static async Task<(bool, string)> UploadIFPSImageToWPByAPI(string imageLink, string filename)
        {
            try
            {
                //var fileContentType = "multipart/form-data";
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

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="apiurl"></param>
        /// <param name="wplogin"></param>
        /// <param name="wppass"></param>
        /// <returns></returns>
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

        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="apiurl"></param>
        /// <param name="jwt"></param>
        /// <returns></returns>
        public static async Task<(bool, string)> InitWPAPI(string apiurl, string jwt)
        {
            try
            {
                wpClient = new WordPressClient(apiurl.Replace("wc/v3/", string.Empty));
                wpClient.AuthMethod = WordPressPCL.Models.AuthMethod.JWT;
                wpClient.SetJWToken(jwt);
                var isvalid = await wpClient.IsValidJWToken();
                Console.WriteLine(jwt);
                return (true, jwt);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot get JWT token. " + ex.Message);
                return (false, "Cannot get JWT Token. " + ex.Message);
            }
        }
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="apiurl"></param>
        /// <param name="apikey"></param>
        /// <param name="secret"></param>
        /// <param name="jwt"></param>
        /// <param name="withRefreshing"></param>
        /// <returns></returns>
        public static async Task<bool> InitStoreApiConnection(string apiurl, string apikey, string secret, string jwt, bool withRefreshing = false)
        {
            try
            {
                //if (string.IsNullOrEmpty(jwt)) return false;
                //throw new Exception("Please obtain JWT token first. It is important for upload of images to the eshop. ");
                if (!string.IsNullOrEmpty(apiurl) && !string.IsNullOrEmpty(apikey) && !string.IsNullOrEmpty(secret))
                {
                    VEDLDataContext.WooCommerceStoreUrl = apiurl;
                    VEDLDataContext.WooCommerceStoreAPIKey = apikey;
                    VEDLDataContext.WooCommerceStoreSecret = secret;
                    VEDLDataContext.WooCommerceStoreJWTToken = jwt;

                    httpClient.Timeout = new TimeSpan(0, 0, 0, 0, 500);
                    var res = await httpClient.GetAsync(GetFullAPIUrl(""));
                    if (res != null)
                    {
                        var resmsg = await res.Content.ReadAsStringAsync();
                        if (res.StatusCode == HttpStatusCode.OK)
                        {
                            if (!string.IsNullOrEmpty(jwt))
                            {
                                var apir = await InitWPAPI(apiurl, jwt);
                                if (!apir.Item1)
                                {
                                    Console.WriteLine("Saved JWT Token is not correct.");
                                    return false;
                                }
                            }
                            //Console.WriteLine(resmsg);
                            IsInitialized = true;

                            Shop = new WooCommerceShop(apiurl, apikey, secret, VEDLDataContext.WooCommerceStoreJWTToken, VEDLDataContext.AllowDispatchNFTOrders);
                            if (withRefreshing) await Shop.StartRefreshingData();
                            return true;
                        }
                        else
                        {
                            Console.WriteLine(resmsg);
                        }
                    }
                }
                IsInitialized = false;
                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot init WooCommerce store API connection. " + ex.Message);
                return false;
            }
        }
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="command"></param>
        /// <param name="apiurl"></param>
        /// <param name="apikey"></param>
        /// <param name="secret"></param>
        /// <returns></returns>
        public static string GetFullAPIUrl(string command, string apiurl = "", string apikey = "", string secret = "")
        {
            if (!string.IsNullOrEmpty(apiurl) && !string.IsNullOrEmpty(apikey) && !string.IsNullOrEmpty(secret))
                return apiurl + $"{command}?consumer_key={apikey}&consumer_secret={secret}";
            else
                return VEDLDataContext.WooCommerceStoreUrl + $"{command}?consumer_key={VEDLDataContext.WooCommerceStoreAPIKey}&consumer_secret={VEDLDataContext.WooCommerceStoreSecret}";
        }
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="nft"></param>
        /// <param name="categories"></param>
        /// <param name="quantity"></param>
        /// <param name="apiurl"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static async Task<Product> AddNewProduct(INFT nft, List<Category> categories, int quantity = 1, string apiurl = "", Dictionary<string,string> options = null)
        {
            try
            {
                if (IsInitialized && string.IsNullOrEmpty(apiurl))
                    apiurl = GetFullAPIUrl("products");

                if (wpClient == null) throw new Exception("Please init the connection and obtain JWT Token first.");
                if (string.IsNullOrEmpty(nft.ImageLink)) throw new Exception("Image link cannot be empty.");
                if (categories == null || categories.Count == 0) throw new Exception("Please provide at least one category.");
                if (System.Text.RegularExpressions.Regex.Match(nft.Name, RegexMatchPaterns.EmojiPattern).Success)
                    throw new Exception("You cannot use Emojis in the name if you want to publish it into the shop.");
                if (System.Text.RegularExpressions.Regex.Match(nft.Description, RegexMatchPaterns.EmojiPattern).Success)
                    throw new Exception("You cannot use Emojis in the description if you want to publish it into the shop.");

                Product p = null;
                var link = nft.ImageLink;
                var productlink = nft.ImageLink;
                var desc = nft.Description;

                
                if (nft.Type == NFTTypes.Music ||
                    (nft.Type == NFTTypes.Ticket && (nft as TicketNFT).MusicInLink) ||
                    (nft.Type == NFTTypes.Event && (nft as EventNFT).MusicInLink))
                {
                    productlink = nft.Link;
                    //desc += " - This NFT contains music which is available after your order is finished.";
                }

                var price = "0.0";
                if (nft.DogePriceActive)
                    price = Convert.ToString(nft.DogePrice, CultureInfo.InvariantCulture).Replace(",", ".");
                else if (nft.PriceActive)
                    price = Convert.ToString(nft.Price, CultureInfo.InvariantCulture).Replace(",", ".");

                var resi = await UploadIFPSImageToWP(link, nft.Name.Replace(" ", "_")); // todo add illegal chars check/cleanup
                var imagelink = string.Empty;
                if (resi.Item1) imagelink = resi.Item2;

                var authorDogeAddress = string.Empty;
                if (!string.IsNullOrEmpty(nft.DogeftInfo.AuthorDogeAddress))
                    authorDogeAddress = nft.DogeftInfo.AuthorDogeAddress;
                var metadata = await nft.GetMetadata();
                if (metadata.ContainsKey("SoldInfo"))
                    metadata.Remove("SoldInfo");
                if (metadata.ContainsKey("DogeftInfo"))
                    metadata.Remove("DogeftInfo");
                p = new Product()
                {
                    name = nft.Name,
                    description = nft.Text,
                    regular_price = price,
                    images = new List<ImageObject>() { new ImageObject() { src = imagelink } },
                    status = "publish",
                    categories = categories,
                    downloadable = true,
                    stock_quantity = quantity,
                    stock_status_enum = StockStatus.instock,
                    _virtual = true,
                    type = "simple",
                    short_description = desc,
                    meta_data = new List<ProductMetadata>() {
                        new ProductMetadata() { key = "Author", value =  nft.Author },
                        new ProductMetadata() { key = "Utxo", value =  nft.Utxo },
                        new ProductMetadata() { key = "Utxoindex", value =  nft.UtxoIndex.ToString() },
                        new ProductMetadata() { key = "ShortHash", value =  nft.ShortHash },
                        new ProductMetadata() { key = "AuthorDogeAddress", value = authorDogeAddress },
                        new ProductMetadata() { key = "NFTData", value = JsonConvert.SerializeObject(metadata) }
                        },
                    downloads = new List<DownloadsObject>() {
                        new DownloadsObject() {
                            name = "NFT on IPFS",
                            file = productlink,
                            external_url = $"https://nft.ve-nft.com/?txid={nft.Utxo}",
                            categories = new List<CategoryOfDownloads>() {
                                new CategoryOfDownloads() { id = 3 }
                            }
                        }
                    }
                };

                if (options != null)
                {
                    foreach (var o in options)
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
                catch (Exception ex) 
                {
                    ////log.Error("Cannot upload WooCommerce product. " + ex.Message);
                    Console.WriteLine(ex.Message); 
                }

                return new Product();
            }
            catch
            {
                ////log.Error("Cannot add new WooCommerce Product. " + ex.Message);
                return new Product();
            }
        }
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="order"></param>
        /// <param name="apiurl"></param>
        /// <returns></returns>
        public static async Task<Order> UpdateOrder(Order order, string apiurl = "")
        {
            if (IsInitialized && string.IsNullOrEmpty(apiurl))
                apiurl = GetFullAPIUrl($"orders/{order.id}");
            if (order.currency != "DGC") order.currency = "DGC";

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
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="id"></param>
        /// <param name="apiurl"></param>
        /// <returns></returns>
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
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="id"></param>
        /// <param name="apiurl"></param>
        /// <returns></returns>
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
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="apiurl"></param>
        /// <returns></returns>
        public static async Task<List<Product>> GetAllProducts(string apiurl = "")
        {
            try
            {
                var prods = new List<Product>();
                var loaded = false;
                var page = 1;
                var attempts = 1000;
                while (!loaded)
                {
                    if (IsInitialized && string.IsNullOrEmpty(apiurl))
                        apiurl = GetFullAPIUrl("products");
                    apiurl += "&per_page=20";
                    apiurl += $"&page={page}";
                    try
                    {
                        var res = await httpClient.GetAsync(apiurl);
                        var resmsg = await res.Content.ReadAsStringAsync();
                        //Console.WriteLine(resmsg);

                        var prds = JsonConvert.DeserializeObject<List<Product>>(resmsg);
                        if (prds != null && prds.Count > 0)
                            foreach (var p in prds) prods.Add(p);
                        else
                            loaded = true;
                        page++;
                        attempts--;
                        if (attempts < 0) loaded = true;
                    }
                    catch
                    {
                        loaded = true;
                    }
                }
                return prods;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return new List<Product>();
        }
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="apiurl"></param>
        /// <returns></returns>
        public static async Task<List<Order>> GetAllOrders(string apiurl = "")
        {
            try
            {
                var orders = new List<Order>();
                var loaded = false;
                var page = 1;
                var attempts = 1000;
                while (!loaded)
                {
                    try
                    {
                        if (IsInitialized && string.IsNullOrEmpty(apiurl))
                            apiurl = GetFullAPIUrl("orders");
                        apiurl += "&per_page=20";
                        apiurl += $"&page={page}";
                        var res = await httpClient.GetAsync(apiurl);
                        var resmsg = await res.Content.ReadAsStringAsync();
                        //Console.WriteLine(resmsg);
                        var ords = JsonConvert.DeserializeObject<List<Order>>(resmsg);
                        if (ords != null && ords.Count > 0)
                            foreach (var o in ords) orders.Add(o);
                        else
                            loaded = true;
                        page++;
                        attempts--;
                        if (attempts < 0) loaded = true;
                    }
                    catch
                    {
                        loaded = true;
                    }
                }
                return orders;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return new List<Order>();
        }
        /// <summary>
        /// TODO
        /// </summary>
        /// <param name="apiurl"></param>
        /// <returns></returns>
        public static async Task<List<Category>> GetAllCategoreis(string apiurl = "")
        {
            try
            {
                var categories = new List<Category>();
                var loaded = false;
                var page = 1;
                var attempts = 1000;
                while (!loaded)
                {
                    try
                    {
                        if (IsInitialized && string.IsNullOrEmpty(apiurl))
                            apiurl = GetFullAPIUrl("products/categories");
                        apiurl += "&per_page=20";
                        apiurl += $"&page={page}";
                        var res = await httpClient.GetAsync(apiurl);
                        var resmsg = await res.Content.ReadAsStringAsync();
                        //Console.WriteLine(resmsg);
                        var cats = JsonConvert.DeserializeObject<List<Category>>(resmsg);
                        if (cats != null && cats.Count > 0)
                            foreach (var c in cats) categories.Add(c);
                        else
                            loaded = true;
                        page++;
                        attempts--;
                        if (attempts < 0) loaded = true;
                    }
                    catch
                    {
                        loaded = true;
                    }
                }
                return categories;
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return new List<Category>();
        }

    }
}
