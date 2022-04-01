using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VEDriversLite.Devices.Dto;
using VEDriversLite.NFT;

namespace VEDriversLite.Devices
{
    /// <summary>
    /// Common base for the IoTDataDrivers
    /// </summary>
    public abstract class CommonIoTDataDriver : IIoTDataDriver
    {
        /// <summary>
        /// Name of the driver
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Driver type
        /// </summary>
        public IoTDataDriverType Type { get; set; } = IoTDataDriverType.HARDWARIO;
        /// <summary>
        /// Communication Schemes (Requests - classic API), (PubSub - like MQTT)
        /// </summary>
        public CommunicationSchemeType ComSchemeType { get; set; } = CommunicationSchemeType.Requests;
        /// <summary>
        /// Communication type - REST API, MSSQL Database, MQTT, etc.
        /// </summary>
        public IoTCommunicationType IoTComType { get; set; } = IoTCommunicationType.API;
        /// <summary>
        /// Main connection parameters
        /// </summary>
        public CommonConnectionParams CommonConnParams { get; set; } = new CommonConnectionParams();
        /// <summary>
        /// Cancelation token source for the cancel of automatic loading of the messages
        /// </summary>
        public CancellationTokenSource CancelTokenSource { get; set; } = new CancellationTokenSource();
        /// <summary>
        /// Cancelation token for the cancel of automatic loading of the messages
        /// </summary>
        public CancellationToken CancelToken { get; set; }
        /// <summary>
        /// Is the Driver running - means autorefresh of the messages
        /// </summary>
        public bool IsRunning { get; set; } = false;
        /// <summary>
        /// New message found
        /// </summary>

        public abstract event EventHandler<string> NewDataReceived;

        /// <summary>
        /// Init driver communication - means autorefresh of the messages
        /// </summary>
        /// <param name="nft"></param>
        /// <returns></returns>
        public abstract Task Init(INFT nft);
        /// <summary>
        /// Deinit driver communiation - means autorefresh of the messages
        /// </summary>
        /// <returns></returns>
        public abstract Task DeInit();
        /// <summary>
        /// Set connection parameters
        /// </summary>
        /// <param name="ccop"></param>
        /// <returns></returns>
        public abstract Task SetConnParams(CommonConnectionParams ccop);
    }
}
