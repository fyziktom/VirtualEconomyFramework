using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite;
using VEDriversLite.NeblioAPI;
using VEDriversLite.NFT;
using VEDriversLite.StorageDriver.StorageDrivers;
using VEDriversLite.StorageDriver.StorageDrivers.Dto;
using static System.Windows.Forms.LinkLabel;
using VEDriversLite.StorageDriver.StorageDrivers.Dto.IPFS;
using Newtonsoft.Json;
using VEDriversLite.StorageDriver.Helpers;
using System.Diagnostics;

namespace TestVEDriversLite
{
    public static class StorageDriverTests
    {
        [TestEntry]
        public static void Storage_AddDriver(string param)
        {
            Storage_AddDriverAsync(param);
        }
        public static async Task Storage_AddDriverAsync(string param)
        {
            VEDriversLite.StorageDriver.Helpers.IPFSHelpers.GatewayURL = "https://ve-framework.com/ipfs/";

            var res = await VEDLDataContext.Storage.AddDriver(new StorageDriverConfigDto()
            {
                Type = "IPFS",
                Name = "LocalIPFS",
                Location = "Local",
                ID = "LocalIPFS",
                IsPublicGateway = false,
                IsLocal = true,
                ConnectionParams = new StorageDriverConnectionParams()
                {
                    APIUrl = "http://127.0.0.1/",
                    APIPort = 5001,
                    Secured = false,
                    GatewayURL = "http://127.0.0.1/ipfs/",
                    GatewayPort = 8080,
                }
            });

            if (res.Item1)
                Console.WriteLine("Driver LocalIPFS added. " + res.Item2);
            else
                Console.WriteLine("Driver LocalIPFS cannot be added." + res.Item2);


            res = await VEDLDataContext.Storage.AddDriver(new StorageDriverConfigDto()
            {
                Type = "IPFS",
                Name = "BDP",
                Location = "Cloud",
                ID = "BDP",
                IsPublicGateway = true,
                IsLocal = false,
                ConnectionParams = new StorageDriverConnectionParams()
                {
                    APIUrl = "https://ve-framework.com/",
                    APIPort = 443,
                    Secured = false,
                    GatewayURL = "https://ve-framework.com/ipfs/",
                    GatewayPort = 443,
                }
            });

            if (res.Item1)
                Console.WriteLine("Driver BDP added. " + res.Item2);
            else
                Console.WriteLine("Driver BDP cannot be added." + res.Item2);

            res = await VEDLDataContext.Storage.AddDriver(new StorageDriverConfigDto()
            {
                Type = "IPFS",
                Name = "Infura",
                Location = "Cloud",
                ID = "Infura",
                IsPublicGateway = true,
                IsLocal = false,
                ConnectionParams = new StorageDriverConnectionParams()
                {
                    APIUrl = "https://ipfs.infura.io/",
                    APIPort = 5001,
                    Secured = true,
                    Username = "1urI71lwIaCjNo4b2kyL8LQ5Rlf",
                    Password = "ce9c8fb81ab177c713841cecc3f9af51",
                    GatewayURL = "https://bdp.infura-ipfs.io/ipfs/",
                    GatewayPort = 443,
                }
            });

            if (res.Item1)
                Console.WriteLine("Driver Infura added. " + res.Item2);
            else
                Console.WriteLine("Driver Infura cannot be added." + res.Item2);


            Console.WriteLine("All Added and Initialized.");
        }

        [TestEntry]
        public static void Storage_FileUpload(string param)
        {
            Storage_FileUploadAsync(param);
        }
        public static async Task Storage_FileUploadAsync(string param)
        {

            var filebytes = File.ReadAllBytes(param);

            var link = string.Empty;
            try
            {
                using (Stream stream = new MemoryStream(filebytes))
                {
                    var result = await VEDLDataContext.Storage.SaveFileToIPFS(new WriteStreamRequestDto()
                    {
                        Data = stream,
                        Filename = param,
                        DriverType = StorageDriverType.IPFS,
                        StorageId = "BDP",
                        BackupInLocal = false,
                    });

                    if (result.Item1)
                        Console.WriteLine("Image Link: " + result.Item2);
                    else
                        Console.WriteLine("Cannot upload. " + result.Item2);

                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during uploading the image to the IPFS." + ex.Message);
            }

        }

        [TestEntry]
        public static void Storage_APIUploadToIPFS(string param)
        {
            Storage_APIUploadToIPFSAsync(param);
        }
        public static async Task Storage_APIUploadToIPFSAsync(string param)
        {
            if (VEDLDataContext.Storage.StorageDrivers.TryGetValue("LocalIPFS", out var driver))
            {
                IPFSAPIAddResponseDto parsedResponse = null;
                var filebytes = File.ReadAllBytes(param);
                using (Stream stream = new MemoryStream(filebytes))
                {
                    var dto = new WriteStreamRequestDto()
                    {
                        Data = stream,
                        Filename = param,
                        DriverType = StorageDriverType.IPFS,
                        StorageId = "Infura",
                        BackupInLocal = true,
                    };

                    using var client = new HttpClient();
                    if (driver.ConnectionParams.Secured)
                    {
                        var byteArray = Encoding.ASCII.GetBytes(driver.ConnectionParams.Username + ":" + driver.ConnectionParams.Password);
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                        client.DefaultRequestHeaders.Add("mode", "no-cors");
                        client.DefaultRequestHeaders.Add("Origin", "https://ve-nft.com");
                    }
                    using (var content = new MultipartFormDataContent())
                    {
                        content.Add(new StreamContent(dto.Data)
                        {
                            Headers =
                            {
                                ContentLength = dto.Data.Length,
                                ContentType = new MediaTypeHeaderValue("multipart/form-data")
                            }
                        }, "file", dto.Filename ?? string.Empty);

                        var response = await client.PostAsync(driver.ConnectionParams.APIUrl.Trim('/') + $":{driver.ConnectionParams.APIPort}/api/v0/add", content);
                        var data = await response.Content.ReadAsStringAsync();
                        if (!string.IsNullOrEmpty(data))
                            parsedResponse = JsonConvert.DeserializeObject<IPFSAPIAddResponseDto>(data);
 
                    }
                }
            }
        }

        [TestEntry]
        public static void Storage_APIDownloadFromIPFS(string param)
        {
            Storage_APIDownloadFromIPFSAsync(param);
        }
        public static async Task Storage_APIDownloadFromIPFSAsync(string param)
        {
            if (VEDLDataContext.Storage.StorageDrivers.TryGetValue("Infura", out var driver))
            {
                //IPFSAPIAddResponseDto parsedResponse = null;
                byte[] bytes = null;
                using var client = new HttpClient();
                if (driver.ConnectionParams.Secured)
                {
                    var byteArray = Encoding.ASCII.GetBytes(driver.ConnectionParams.Username + ":" + driver.ConnectionParams.Password);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    client.DefaultRequestHeaders.Add("mode", "no-cors");
                    client.DefaultRequestHeaders.Add("Origin", "https://ve-nft.com");
                }
                //var content = new FormUrlEncodedContent(parameters);
                var response = await client.PostAsync(driver.ConnectionParams.APIUrl.Trim('/') + $":{driver.ConnectionParams.APIPort}/api/v0/cat?arg={param}", null);
                var stream = await response.Content.ReadAsStreamAsync();
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    bytes = ms.ToArray();
                    Console.WriteLine($"First file size: {bytes.Length}");
                }
                //if (!string.IsNullOrEmpty(data))
                //parsedResponse = JsonConvert.DeserializeObject<IPFSAPIAddResponseDto>(data);

            }

            if (VEDLDataContext.Storage.StorageDrivers.TryGetValue("LocalIPFS", out driver))
            {
                //IPFSAPIAddResponseDto parsedResponse = null;
                byte[] bytes = null;
                using var client = new HttpClient();
                if (driver.ConnectionParams.Secured)
                {
                    var byteArray = Encoding.ASCII.GetBytes(driver.ConnectionParams.Username + ":" + driver.ConnectionParams.Password);
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                    client.DefaultRequestHeaders.Add("mode", "no-cors");
                    client.DefaultRequestHeaders.Add("Origin", "https://ve-nft.com");
                }
                //var content = new FormUrlEncodedContent(parameters);
                var response = await client.PostAsync(driver.ConnectionParams.APIUrl.Trim('/') + $":{driver.ConnectionParams.APIPort}/api/v0/cat?arg={param}", null);
                var stream = await response.Content.ReadAsStreamAsync();
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    bytes = ms.ToArray();

                    Console.WriteLine($"Second file size: {bytes.Length}");
                }
                //if (!string.IsNullOrEmpty(data))
                //parsedResponse = JsonConvert.DeserializeObject<IPFSAPIAddResponseDto>(data);

            }
        }

        [TestEntry]
        public static void Storage_APIPinAddIPFS(string param)
        {
            Storage_APIPinAddIPFSAsync(param);
        }
        public static async Task Storage_APIPinAddIPFSAsync(string param)
        {
            
            try
            {
                if (VEDLDataContext.Storage.StorageDrivers.TryGetValue("Infura", out var driver))
                {
                    IPFSAPIPinAddResponseDto parsedResponse = null;
                    using var client = new HttpClient();
                    if (driver.ConnectionParams.Secured)
                    {
                        var byteArray = Encoding.ASCII.GetBytes(driver.ConnectionParams.Username + ":" + driver.ConnectionParams.Password);
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                        client.DefaultRequestHeaders.Add("mode", "no-cors");
                        client.DefaultRequestHeaders.Add("Origin", "https://ve-nft.com");
                    }
                    var response = await client.PostAsync(driver.ConnectionParams.APIUrl.Trim('/') + $":{driver.ConnectionParams.APIPort}/api/v0/pin/add?arg=/ipfs/{param}", null);
                    var data = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(data))
                        parsedResponse = JsonConvert.DeserializeObject<IPFSAPIPinAddResponseDto>(data);
                    if (parsedResponse != null && parsedResponse.Pins != null)
                        if (parsedResponse.Pins.Contains(param))
                            Console.WriteLine( $"CID {param} Pinned");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot pin the file on IPFS. " + ex.Message);
            }
        }

        [TestEntry]
        public static void Storage_APIPinLsIPFS(string param)
        {
            Storage_APIPinLsIPFSAsync(param);
        }
        public static async Task Storage_APIPinLsIPFSAsync(string param)
        {

            try
            {
                if (VEDLDataContext.Storage.StorageDrivers.TryGetValue("LocalIPFS", out var driver))
                {
                    IPFSAPIPinAddResponseDto parsedResponse = null;
                    using var client = new HttpClient();
                    if (driver.ConnectionParams.Secured)
                    {
                        var byteArray = Encoding.ASCII.GetBytes(driver.ConnectionParams.Username + ":" + driver.ConnectionParams.Password);
                        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
                        client.DefaultRequestHeaders.Add("mode", "no-cors");
                        client.DefaultRequestHeaders.Add("Origin", "https://ve-nft.com");
                    }
                    var response = await client.PostAsync(driver.ConnectionParams.APIUrl.Trim('/') + $":{driver.ConnectionParams.APIPort}/api/v0/pin/ls?arg=QmYvtp4vTsqqghpNzaCWF5z3NdXCuf91jQGS6Ko91tubJP", null);
                    var data = await response.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(data))
                        parsedResponse = JsonConvert.DeserializeObject<IPFSAPIPinAddResponseDto>(data);
                    if (parsedResponse != null && parsedResponse.Pins != null)
                        if (parsedResponse.Pins.Contains(param))
                            Console.WriteLine($"CID {param} Pinned");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot pin the file on IPFS. " + ex.Message);
            }
        }

        [TestEntry]
        public static void Storage_GetHashFromLink(string param)
        {
            Storage_GetHashFromLinkAsync(param);
        }
        public static async Task Storage_GetHashFromLinkAsync(string param)
        {
            try
            {
                var hash = IPFSHelpers.GetHashFromIPFSLink(param);
                Console.WriteLine($"Link: {param}");
                Console.WriteLine($"Hash: {hash}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during getting hash from the link." + ex.Message);
            }

        }

        [TestEntry]
        public static void Storage_GetFromIPFS(string param)
        {
            Storage_GetFromIPFSAsync(param);
        }
        public static async Task Storage_GetFromIPFSAsync(string param)
        {
            try
            {

                Stopwatch sw = new Stopwatch();
                //if (VEDLDataContext.Storage.StorageDrivers.TryGetValue("BDP", out var driver))
                foreach (var driver in VEDLDataContext.Storage.StorageDrivers.Values)
                {
                    sw.Reset();
                    sw.Start();
                    var res = await driver.GetBytesAsync(param);
                    sw.Stop();
                    if (res.Item1)
                    {
                        Console.WriteLine("----------------------");
                        Console.WriteLine($"Driver {driver.Name}");
                        Console.WriteLine($"Request Time: {sw.ElapsedMilliseconds} ms");
                        Console.WriteLine($"File Hash: {param}");
                        Console.WriteLine($"File Size: {res.Item2.Length}");
                        Console.WriteLine("----------------------");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during downloading the image to the IPFS." + ex.Message);
            }

        }

    }
}
