using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Devices.APIs.HARDWARIO;
using VEDriversLite.Devices.APIs.HARDWARIO.Dto;
using VEDriversLite.Devices.Dto;
using VEDriversLite.NFT;

namespace VEDriversLite.Devices
{
    /// <summary>
    /// Data driver to obtain data from the HARDWARIO Cloud API
    /// Can be used primarly for the CHESTER devices
    /// </summary>
    public class HARDWARIOIoTDataDriver : CommonIoTDataDriver
    {
        /// <summary>
        /// New data on the API
        /// </summary>
        public override event EventHandler<string> NewDataReceived;
        /// <summary>
        /// Main client
        /// </summary>

        public HARDWARIOApiClient HWApiClient { get; set; } = new HARDWARIOApiClient();


        private readonly Random _random = new Random();

        private string lastmessageid = string.Empty;

        /// <summary>
        /// Deinit driver communiation - means autorefresh of the messages
        /// </summary>
        /// <returns></returns>
        public override async Task DeInit()
        {
            if (CancelToken != null)
                CancelTokenSource.Cancel();
        }

        /// <summary>
        /// Init driver communication - means autorefresh of the messages
        /// </summary>
        /// <param name="nft"></param>
        /// <returns></returns>
        public override async Task Init(INFT nft)
        {
            CancelToken = CancelTokenSource.Token;

            _ = Task.Run(async () =>
            {
                IsRunning = true;

                var quit = false;
                while (!quit)
                {
                    if (CancelToken.IsCancellationRequested)
                        quit = true;

                    try
                    {
                        var res = await RequestNewMessageFromAPI();
                        if (res.Item1 && !string.IsNullOrEmpty(res.Item2.id))
                        {
                            if (res.Item2.id != lastmessageid)
                            {
                                NewDataReceived?.Invoke(this, JsonConvert.SerializeObject(res.Item2));
                                lastmessageid = res.Item2.id;
                            }
                        }
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine("Cannot obtain new message from the HARDWARIO Cloud API. " + ex.Message);
                    }
                    await Task.Delay(CommonConnParams.CommonRefreshInterval);
                }

            }, CancelToken);

            IsRunning = false;
        }

        /// <summary>
        /// Set connection parameters
        /// </summary>
        /// <param name="ccop"></param>
        /// <returns></returns>
        public override async Task SetConnParams(CommonConnectionParams ccop)
        {
            if (ccop != null)
                CommonConnParams = ccop;

            HWApiClient?.SetCommonConnectionParameters(ccop);
        }

        private async Task<(bool, HWDto)> RequestNewMessageFromAPI()
        {
            /* test data
            var temp = _random.Next(20, 30);
            var tempf = _random.Next(10, 99);
            return (true, "{ \"temperature\":" + $"{temp}.{tempf}" + " }");
            */

            try
            {
                var msgs = await HWApiClient?.GetMessages(CommonConnParams.DeviceId, CommonConnParams.GroupId);
                if (msgs.Item1 && msgs.Item2.Count > 0)
                    return (true, msgs.Item2[0]);
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot get the message from the Hardwario API: " + ex.Message);
            }

            return (false, new HWDto());
        }

    }
}
