using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Common.IoT.Dto
{
    /// <summary>
    /// Scheme of the communication
    /// </summary>
    public enum CommunicationSchemeType
    {
        /// <summary>
        /// Request to API
        /// </summary>
        Requests,
        /// <summary>
        /// Publisher Subscriber model - for example MQTT
        /// </summary>
        PubSub
    }
    /// <summary>
    /// Communication type
    /// </summary>
    public enum IoTCommunicationType
    {
        /// <summary>
        /// API access
        /// </summary>
        API,
        /// <summary>
        /// File storage
        /// </summary>
        File,
        /// <summary>
        /// Database Microsoft SQL
        /// </summary>
        DbMSSQL,
        /// <summary>
        /// Database PostgreSQL
        /// </summary>
        DbPostgreSQL,
        /// <summary>
        /// Database SQLite
        /// </summary>
        DbSQLite,
        /// <summary>
        /// MQTT protocol
        /// </summary>
        MQTT = 100,
        /// <summary>
        /// OPC UA protocol
        /// </summary>
        OPCUA = 101
    }
    
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
