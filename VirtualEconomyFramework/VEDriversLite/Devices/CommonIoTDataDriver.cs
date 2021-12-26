using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VEDriversLite.Devices.Dto;
using VEDriversLite.NFT;

namespace VEDriversLite.Devices
{
    public abstract class CommonIoTDataDriver : IIoTDataDriver
    {
        public string Name { get; set; } = string.Empty;
        public IoTDataDriverType Type { get; set; } = IoTDataDriverType.HARDWARIO;
        public CommunicationSchemeType ComSchemeType { get; set; } = CommunicationSchemeType.Requests;
        public IoTCommunicationType IoTComType { get; set; } = IoTCommunicationType.API;
        public CommonConnectionParams CommonConnParams { get; set; } = new CommonConnectionParams();
        public CancellationTokenSource CancelTokenSource { get; set; } = new CancellationTokenSource();
        public CancellationToken CancelToken { get; set; }
        public bool IsRunning { get; set; } = false;

        public abstract event EventHandler<string> NewDataReceived;

        public abstract Task Init(INFT nft);
        public abstract Task DeInit();
        public abstract Task SetConnParams(CommonConnectionParams ccop);
    }
}
