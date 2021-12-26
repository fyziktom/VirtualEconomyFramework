using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VEDriversLite.Devices.Dto;
using VEDriversLite.NFT;

namespace VEDriversLite.Devices
{
    public enum IoTDataDriverType
    {
        Common,
        HARDWARIO,
        PLFramework,
        M5Stack
    }
    public enum CommunicationSchemeType
    {
        Requests,
        PubSub
    }
    public enum IoTCommunicationType
    {
        API,
        File,
        DbMSSQL,
        DbPostgreSQL,
        DbSQLite,
        MQTT = 100,
        OPCUA = 101
    }

    public interface IIoTDataDriver
    {
        string Name { get; set; }
        IoTDataDriverType Type { get; set; }
        CommunicationSchemeType ComSchemeType { get; set; }
        IoTCommunicationType IoTComType { get; set; }
        CommonConnectionParams CommonConnParams { get; set; }
        CancellationTokenSource CancelTokenSource { get; set; }
        CancellationToken CancelToken { get; set; }
        bool IsRunning { get; set; }

        event EventHandler<string> NewDataReceived;

        Task SetConnParams(CommonConnectionParams ccop);
        Task Init(INFT nft);
        Task DeInit();
    }
}
