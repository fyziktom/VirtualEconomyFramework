using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Devices.APIs.HARDWARIO.Dto;
using VEDriversLite.Devices.Dto;

namespace VEDriversLite.Devices.APIs.HARDWARIO
{
    /// <summary>
    /// API Client for the HARDWARIO Cloud API
    /// </summary>
    public class HARDWARIOApiClient
    {
        /// <summary>
        /// Common connection properties like Url, security, etc.
        /// </summary>
        public CommonConnectionParams CommonConnParams { get; set; } = new CommonConnectionParams();
        private static object _lock = new object();

        /// <summary>
        /// Load the connection parameters
        /// </summary>
        /// <param name="ccop"></param>
        public void SetCommonConnectionParameters(CommonConnectionParams ccop)
        {
            lock (_lock)
            {
                if (ccop != null)
                    CommonConnParams = ccop;
            }
        }

        /// <summary>
        /// Create Client for connection with the API token
        /// </summary>
        /// <param name="apitoken"></param>
        /// <returns></returns>
        public HttpClient GetClient(string apitoken = "")
        {
            HttpClient httpClient = new HttpClient();
            
            if (CommonConnParams.Secured)
            {
                if (!string.IsNullOrEmpty(apitoken))
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apitoken);
                else if (string.IsNullOrEmpty(apitoken) && !string.IsNullOrEmpty(CommonConnParams.Token))
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", CommonConnParams.Token);
                else
                    throw new Exception("Cannot request Hardwario cloud without API Token.");
            }

            return httpClient;
        }
        /// <summary>
        /// Get message from the API for specific device
        /// </summary>
        /// <param name="deviceid">Device Id</param>
        /// <param name="group">Group Id</param>
        /// <param name="apitoken">API token for communication with HARDWARIO Cloud</param>
        /// <returns></returns>

        public async Task<(bool, List<HWDto>)> GetMessages(string deviceid, string group, string apitoken = "")
        {
            try
            {
                var apiurl = $"{CommonConnParams.Url}/messages?group_id={group}&device_id={deviceid}";

                var httpClient = GetClient(apitoken);
                var res = await httpClient.GetAsync(apiurl);
                if (res.IsSuccessStatusCode)
                {
                    var resmsg = await res.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(resmsg))
                    {
                        var msgs = JsonConvert.DeserializeObject<List<HWDto>>(resmsg);
                        return (true, msgs);
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return (false, new List<HWDto>());
        }

        /// <summary>
        /// Get undread message
        /// </summary>
        /// <param name="deviceid"></param>
        /// <param name="group"></param>
        /// <param name="apitoken"></param>
        /// <returns></returns>
        public async Task<(bool, HWDto)> GetUnreadedMessage(string deviceid, string group, string apitoken = "")
        {
            try
            {
                var apiurl = $"{CommonConnParams.Url}/messages/receive?group_id={group}&device_id={deviceid}";

                var httpClient = GetClient(apitoken);
                var res = await httpClient.GetAsync(apiurl);
                if (res.IsSuccessStatusCode)
                {
                    var resmsg = await res.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(resmsg))
                    {
                        var msgs = JsonConvert.DeserializeObject<HWDto>(resmsg);
                        return (true, msgs);
                    }
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return (false, new HWDto());
        }

        private class messageConfirm
        {
            public messageConfirm(string messageid)
            {
                if(messageid != null) message_id = messageid;
            }
            public string message_id { get; set; } = string.Empty;
        }
        /// <summary>
        /// confirm readed message - TODO: some bug, api not respond correctly
        /// </summary>
        /// <param name="deviceid"></param>
        /// <param name="group"></param>
        /// <param name="messageid"></param>
        /// <param name="apitoken"></param>
        /// <returns></returns>
        public async Task<(bool, string)> ConfirmReadOfMessage(string deviceid, string group, string messageid, string apitoken = "")
        {
            try
            {
                var apiurl = $"{CommonConnParams.Url}/messages/confirm";//?group_id={group}&device_id={deviceid}";

                var httpClient = GetClient(apitoken);
                
                var content = new StringContent(
                  JsonConvert.SerializeObject(new messageConfirm(messageid)),
                  System.Text.Encoding.UTF8,
                  "application/json"
                );

                var res = await httpClient.PostAsync(apiurl, content);
                
                /*
                var requestMessage = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = new StringContent(JsonConvert.SerializeObject(new messageConfirm(messageid)), Encoding.UTF8, "application/json"),
                    RequestUri = new Uri(apiurl)
                };

                if (CommonConnParams.Secured)
                {
                    if (!string.IsNullOrEmpty(apitoken))
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apitoken);
                    else if (string.IsNullOrEmpty(apitoken) && !string.IsNullOrEmpty(CommonConnParams.Token))
                        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", CommonConnParams.Token);
                    else
                        throw new Exception("Cannot request Hardwario cloud without API Token.");
                }
                
                var res = await httpClient.SendAsync(requestMessage);
                */
                if (res.IsSuccessStatusCode)
                {
                    var resmsg = await res.Content.ReadAsStringAsync();
                    if (!string.IsNullOrEmpty(resmsg))
                        return (true, resmsg);
                }
            }
            catch (Exception ex) { Console.WriteLine(ex.Message); }

            return (false, string.Empty);
        }
    }
}
