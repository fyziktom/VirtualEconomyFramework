using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Devices.Dto
{
    /// <summary>
    /// Settings of the IoT data driver. Most of the settings are for connection to the API or source of the data
    /// </summary>
    public class IoTDataDriverSettings
    {
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
        public CommonConnectionParams ConnectionParams { get; set; } = new CommonConnectionParams();
    }
}
