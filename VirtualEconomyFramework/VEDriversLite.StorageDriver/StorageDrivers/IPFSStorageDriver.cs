using Ipfs.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.StorageDriver.StorageDrivers.Dto;
using VEDriversLite.StorageDriver.StorageDrivers.Dto.IPFS;
using static Google.Protobuf.WellKnownTypes.Field;

namespace VEDriversLite.StorageDriver.StorageDrivers
{
    internal class IPFSStorageDriver : CommonStorageDriver
    {
        public IPFSStorageDriver()
        {
            Type = StorageDriverType.IPFS;
        }

        public override async Task<(bool, string)> TestConnection()
        {
            if (string.IsNullOrEmpty(ConnectionParams.IP) &&
                string.IsNullOrEmpty(ConnectionParams.GatewayURL) &&
                string.IsNullOrEmpty(ConnectionParams.APIUrl))
                return (false, string.Empty);

            try
            {
                Ping ping = new Ping();

                string ip = string.Empty;
                if (!string.IsNullOrEmpty(ConnectionParams.IP))
                {
                    ip = ConnectionParams.IP;
                    if (ip.Contains(":")) ip = ip.Split(':')[0];
                }
                else if (string.IsNullOrEmpty(ConnectionParams.IP) && !string.IsNullOrEmpty(ConnectionParams.APIUrl))
                {
                    var addr = ConnectionParams.APIUrl.Trim('/').Replace("https://", "www.").Replace("http://", "www.");
                    
                    var ipadd = Dns.GetHostAddresses(addr).FirstOrDefault();
                    if (ipadd != null)
                        ip = ipadd.ToString();
                }
                else if (string.IsNullOrEmpty(ConnectionParams.IP) && string.IsNullOrEmpty(ConnectionParams.APIUrl) && !string.IsNullOrEmpty(ConnectionParams.GatewayURL))
                {
                    var addr = ConnectionParams.GatewayURL.Trim('/').Replace("https://", "www.").Replace("http://", "www.");

                    var ipadd = Dns.GetHostAddresses(ConnectionParams.GatewayURL).FirstOrDefault();
                    if (ipadd != null)
                        ip = ipadd.ToString();
                }

                var result = await ping.SendPingAsync(ip, 1000);
                if (result.Status == IPStatus.Success)
                {
                    Status.IsAvailable = true;
                    Status.LastPingRoundtripTime = result.RoundtripTime;

                    return (true, $"PING:{result.RoundtripTime}");
                }
                else
                {
                    var client = new HttpClient();
                    if (ConnectionParams.Secured)
                    {
                        var byteArray = Encoding.ASCII.GetBytes(ConnectionParams.Username + ":" + ConnectionParams.Password);
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    }
                    var sw = new Stopwatch();
                    sw.Start();
                    var resp = await client.GetAsync(ConnectionParams.APIUrl);
                    sw.Stop();
                    if (resp.StatusCode == HttpStatusCode.OK || resp.StatusCode == HttpStatusCode.NotFound || resp.StatusCode == HttpStatusCode.Unauthorized)
                    {
                        Status.IsAvailable = true;
                        Status.LastPingRoundtripTime = sw.ElapsedMilliseconds;

                        return (true, $"HTTP Request:{sw.ElapsedMilliseconds}");
                    }

                    Status.IsAvailable = false;
                    Status.LastPingRoundtripTime = 0;
                    return (true, $"Warning. Driver {ID}.Cannot verify the connection with standard test.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot ping the storage: {ID}, error: {ex.Message}");
                Status.IsAvailable = false;
                Status.LastPingRoundtripTime = 0;
                return (false, string.Empty);
            }
        }

        /// <summary>
        /// Create IPFS client with authentication
        /// </summary>
        /// <param name="IpfsHostUrl"></param>
        /// <param name="IpfsHostUserName"></param>
        /// <param name="IpfsHostPassword"></param>
        /// <returns></returns>
        private IpfsClient CreateIpfsClient(string IpfsHostUrl, string IpfsHostUserName, string IpfsHostPassword, bool isSecured)
        {

            var c = new IpfsClient(IpfsHostUrl);
            
            var httpClientInfo = typeof(IpfsClient).GetField("api", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            var apiObj = httpClientInfo.GetValue(null);
            if (apiObj == null)
            {

                MethodInfo createMethod = typeof(IpfsClient).GetMethod("Api", BindingFlags.NonPublic | BindingFlags.Instance);
                var o = createMethod.Invoke(c, new Object[0]);
                var client = o as HttpClient;

                if (isSecured)
                {
                    var byteArray = Encoding.ASCII.GetBytes(IpfsHostUserName + ":" + IpfsHostPassword);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                }
                //client.DefaultRequestHeaders.Add("mode", "no-cors");
                client.DefaultRequestHeaders.Add("Origin", "https://ve-nft.com");
            }

            return c;
        }

        private HttpClient CreateHttpClient(string IpfsHostUserName, string IpfsHostPassword, bool isSecured)
        {
            var client = new HttpClient();
            if (isSecured)
            {
                var byteArray = Encoding.ASCII.GetBytes(ConnectionParams.Username + ":" + ConnectionParams.Password);
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
            client.DefaultRequestHeaders.Add("Accept", "*/*");
            //client.DefaultRequestHeaders.Add("mode", "no-cors");
            //client.DefaultRequestHeaders.Add("Origin", "https://ve-nft.com");
            return client;
        }


        /// <summary>
        /// Download file from IPFS
        /// </summary>
        /// <param name="path">CID/Hash of the file on IPFS</param>
        /// <returns></returns>
        public override async Task<(bool, byte[])> GetBytesAsync(string path)
        {
            try
            {
                //using var client = CreateHttpClient(ConnectionParams.Username, ConnectionParams.Password, ConnectionParams.Secured);
                using (var client = new HttpClient())
                {
                    if (ConnectionParams.Secured)
                    {
                        var byteArray = Encoding.ASCII.GetBytes(ConnectionParams.Username + ":" + ConnectionParams.Password);
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                        client.DefaultRequestHeaders.Add("Origin", "https://ve-nft.com");
                    }
                    //client.DefaultRequestHeaders.Add("mode", "no-cors");
                    //
                    var response = await client.GetAsync(ConnectionParams.GatewayURL.Replace("/ipfs/","").Trim('/') + $":{ConnectionParams.GatewayPort}/ipfs/{path}");
                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            return (true, ms.ToArray());
                        }
                    }
                    else
                    {
                        return (false, null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot read the file from IPFS. " + ex.Message);
            }
            return (false, null);
        }

        public override async Task<(bool, StreamResponseDto)> GetStreamAsync(string path)
        {
            try
            {
                using var client = CreateHttpClient(ConnectionParams.Username, ConnectionParams.Password, ConnectionParams.Secured);
                {
                    var response = await client.PostAsync(ConnectionParams.APIUrl.Trim('/') + $":{ConnectionParams.APIPort}/api/v0/cat?arg={path}", null);
                    if (response.IsSuccessStatusCode)
                    {
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        using (var ms = new MemoryStream())
                        {
                            stream.CopyTo(ms);
                            return (true, new StreamResponseDto() { Filename = path, Data = ms });
                        }
                    }
                    else
                    {
                        return (false, null);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot read the file from IPFS. " + ex.Message);
            }
            return (false, null);
        }

        public override async Task<(bool, string)> RemoveFileAsync(string path)
        {
            if (string.IsNullOrEmpty(ConnectionParams.APIUrl))
                return (false, "Error. Cannot remove without set up IPFS API URL.");
            var ipfs = CreateIpfsClient(ConnectionParams.APIUrl.Trim('/') + $":{ConnectionParams.APIPort}/", ConnectionParams.Username, ConnectionParams.Password, ConnectionParams.Secured);
            ipfs.UserAgent = "VEFramework";
            try
            {
                var cancelSource = new System.Threading.CancellationTokenSource();
                var token = cancelSource.Token;
                var _ = await ipfs.Pin.RemoveAsync(path);
                return (true, $"CID {path} Pinned");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot remove the file from IPFS. " + ex.Message);
            }
            return (false, $"Cannot remove CID {path}");
        }

        public override async Task<(bool, string)> WriteStreamAsync(WriteStreamRequestDto dto)
        {
            if (dto.Data == null)
                return (false, "Error. Provided null data stream.");
            if (string.IsNullOrEmpty(ConnectionParams.APIUrl))
                return (false, "Error. Cannot upload without set up IPFS API URL.");
            if (string.IsNullOrEmpty(ConnectionParams.GatewayURL))
                return (false, "Error. Cannot upload without set up IPFS Gateway.");
            try
            {
                if (dto.Data.Length <= 0)
                    return (false, "Error. Stream is empty.");
                var link = string.Empty;

                //var ipfs = CreateIpfsClient(ConnectionParams.APIUrl.Trim('/') + $":{ConnectionParams.APIPort}/", ConnectionParams.Username, ConnectionParams.Password, ConnectionParams.Secured);
                //ipfs.UserAgent = ID;
                //var reslink = await ipfs.FileSystem.AddAsync(dto.Data, dto.Filename);

                IPFSAPIAddResponseDto parsedResponse = null;
                var inputDataLength = dto.Data.Length;

                using var client = CreateHttpClient(ConnectionParams.Username, ConnectionParams.Password, ConnectionParams.Secured);
                using (var content = new MultipartFormDataContent())
                {
                    content.Add(new StreamContent(dto.Data)
                    {
                        Headers =
                            {
                                ContentLength = dto.Data.Length,
                                ContentType = new MediaTypeHeaderValue("application/octet-stream")//("multipart/form-data")
                            }
                    }, "file", dto.Filename ?? string.Empty);

                    var response = await client.PostAsync(ConnectionParams.APIUrl.Trim('/') + $":{ConnectionParams.APIPort}/api/v0/add", content);
                    var data = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(data))
                        parsedResponse = JsonConvert.DeserializeObject<IPFSAPIAddResponseDto>(data);

                }

                if (parsedResponse != null)
                {
                    //var hash = reslink.ToLink().Id.ToString();
                    var hash = parsedResponse.Hash;
                    link = ConnectionParams.GatewayURL + hash;

                    var loaded = false;
                    var attempts = 50;
                    while (attempts > 0 && !loaded)
                    {
                        try
                        {
                            var respb = await GetBytesAsync(hash);
                            if (respb.Item1)
                            {
                                var resp = new MemoryStream(respb.Item2);
                                if (resp != null && resp.Length >= (inputDataLength * 0.8))
                                    loaded = true;
                                else
                                    await Task.Delay(1000);
                            }
                            else
                            {
                                Console.WriteLine("File still not available...");
                                await Task.Delay(1000);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("File still not available: " + ex.Message);
                            await Task.Delay(1000);
                        }
                        attempts--;
                    }
                }
                return (true, link);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during uploading the image to the IPFS." + ex.Message);
                return (false, $"Error. Cannot upload file. {ex.Message}");
            }
        }

        /// <summary>
        /// Pin file to IPFS Infura with use of credentials
        /// </summary>
        /// <param name="hash"></param>
        /// <returns></returns>
        public async Task<(bool,string)> PinToIPFSAsync(string hash)
        {
            if (string.IsNullOrEmpty(ConnectionParams.APIUrl))
                return (false, "Error. Cannot pin without set up IPFS API URL.");

            try
            {
                IPFSAPIPinAddResponseDto parsedResponse = null;
                using var client = CreateHttpClient(ConnectionParams.Username, ConnectionParams.Password, ConnectionParams.Secured);
                {
                    var response = await client.PostAsync(ConnectionParams.APIUrl.Trim('/') + $":{ConnectionParams.APIPort}/api/v0/pin/add?arg=/ipfs/{hash}", null);
                    var data = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(data))
                        parsedResponse = JsonConvert.DeserializeObject<IPFSAPIPinAddResponseDto>(data);
                    if (parsedResponse != null && parsedResponse.Pins != null)
                        if (parsedResponse.Pins.Contains(hash))
                            return (true, $"CID {hash} Pinned");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot pin the file on IPFS. " + ex.Message);
            }
            return (false, $"Cannot pin CID {hash}");
        }
    }
}
