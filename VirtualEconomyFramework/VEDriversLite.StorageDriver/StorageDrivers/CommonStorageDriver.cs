using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using VEDriversLite.StorageDriver.StorageDrivers.Dto;

namespace VEDriversLite.StorageDriver.StorageDrivers
{
    /// <summary>
    /// Implementation of the common part for the Storage Drivers
    /// </summary>
    public abstract class CommonStorageDriver : IStorageDriver
    {
        /// <summary>
        /// Type of the driver
        /// </summary>
        public StorageDriverType Type { get; set; } = StorageDriverType.None;
        /// <summary>
        /// Location of the driver, describes if it is Local or in internet
        /// </summary>
        public LocationType Location { get; set; } = LocationType.Local;
        /// <summary>
        /// Unique ID of the driver
        /// </summary>
        public string ID { get; set; } = string.Empty;
        /// <summary>
        /// Name of the driver
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Connection parameters such as URL, IP, etc.
        /// </summary>
        public StorageDriverConnectionParams ConnectionParams { get; set; } = new StorageDriverConnectionParams();
        /// <summary>
        /// If the Driver for the Local instance of the storage, this is set
        /// </summary>
        public bool IsLocal { get; set; }
        /// <summary>
        /// The information about setup of the public gateway parameters
        /// </summary>
        public bool IsPublicGateway { get; set; }
        /// <summary>
        /// Status of the connection
        /// </summary>
        public ConnectionStatus Status { get; set; } = new ConnectionStatus();
        /// <summary>
        /// Write stream of the data to the storage
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        public abstract Task<(bool, string)> WriteStreamAsync(WriteStreamRequestDto dto);
        /// <summary>
        /// Get bytes from the storage based on the path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public abstract Task<(bool, byte[])> GetBytesAsync(string path);
        /// <summary>
        /// Get stream of the data from the storage based on the path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public abstract Task<(bool, StreamResponseDto)> GetStreamAsync(string path);
        /// <summary>
        /// Remove file on the storage based on the path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public abstract Task<(bool, string)> RemoveFileAsync(string path);
        /// <summary>
        /// Test connection to the storage
        /// </summary>
        /// <returns></returns>
        public abstract Task<(bool, string)> TestConnection();
        
    }
}
