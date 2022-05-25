using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VEDriversLite.Common;
using VEDriversLite.Common.IoT.Dto;
using VEDriversLite.NFT;

namespace VEDriversLite.Devices
{
    /// <summary>
    /// Type of the IoT Data Drivers
    /// </summary>
    public enum IoTDataDriverType
    {
        /// <summary>
        /// Not specified
        /// </summary>
        Common,
        /// <summary>
        /// Hardwario IoT platform
        /// </summary>
        HARDWARIO,
        /// <summary>
        /// PLFramework platform
        /// </summary>
        PLFramework,
        /// <summary>
        /// M5Stack IoT platform
        /// </summary>
        M5Stack
    }

    /// <summary>
    /// Basic interface for IoT Data Driver
    /// </summary>
    public interface IIoTDataDriver
    {
        /// <summary>
        /// Name of the driver
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Driver type
        /// </summary>
        IoTDataDriverType Type { get; set; }
        /// <summary>
        /// Communication Schemes (Requests - classic API), (PubSub - like MQTT)
        /// </summary>
        CommunicationSchemeType ComSchemeType { get; set; }
        /// <summary>
        /// Communication type - REST API, MSSQL Database, MQTT, etc.
        /// </summary>
        IoTCommunicationType IoTComType { get; set; }
        /// <summary>
        /// Main connection parameters
        /// </summary>
        CommonConnectionParams CommonConnParams { get; set; }
        /// <summary>
        /// Cancelation token source for the cancel of automatic loading of the messages
        /// </summary>
        CancellationTokenSource CancelTokenSource { get; set; }
        /// <summary>
        /// Cancelation token for the cancel of automatic loading of the messages
        /// </summary>
        CancellationToken CancelToken { get; set; }
        /// <summary>
        /// Is the Driver running - means autorefresh of the messages
        /// </summary>
        bool IsRunning { get; set; }
        /// <summary>
        /// New message found
        /// </summary>

        event EventHandler<string> NewDataReceived;

        /// <summary>
        /// Init driver communication - means autorefresh of the messages
        /// </summary>
        /// <param name="nft"></param>
        /// <returns></returns>
        Task Init(INFT nft);
        /// <summary>
        /// Deinit driver communiation - means autorefresh of the messages
        /// </summary>
        /// <returns></returns>
        Task DeInit();
        /// <summary>
        /// Set connection parameters
        /// </summary>
        /// <param name="ccop"></param>
        /// <returns></returns>
        Task SetConnParams(CommonConnectionParams ccop);
    }
}
