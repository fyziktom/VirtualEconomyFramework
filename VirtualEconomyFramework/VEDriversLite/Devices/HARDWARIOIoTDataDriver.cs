using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Devices.Dto;
using VEDriversLite.NFT;

namespace VEDriversLite.Devices
{
    public class HARDWARIOIoTDataDriver : CommonIoTDataDriver
    {
        public override event EventHandler<string> NewDataReceived;

        private readonly Random _random = new Random();

        public override async Task DeInit()
        {
            if (CancelToken != null)
                CancelTokenSource.Cancel();            
        }

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

                    var res = await RequestNewMessageFromAPI();
                    if (res.Item1 && !string.IsNullOrEmpty(res.Item2))
                    {
                        NewDataReceived?.Invoke(this, res.Item2);
                    }
                    
                    await Task.Delay(CommonConnParams.CommonRefreshInterval);
                }

            }, CancelToken);

            IsRunning = false;
        }

        public override async Task SetConnParams(CommonConnectionParams ccop)
        {
            if(ccop != null)
                CommonConnParams = ccop;
        }

        private async Task<(bool,string)> RequestNewMessageFromAPI()
        {
            var temp = _random.Next(20, 30);
            var tempf = _random.Next(10, 99);
            return (true,"{ \"temperature\":" + $"{temp}.{tempf}" + " }");
        }
    }
}
