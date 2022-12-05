using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.StorageDriver.StorageDrivers.Dto;

namespace VEDriversLite.StorageDriver.StorageDrivers
{
    /// <summary>
    /// Storage drivers types
    /// Actual implemented is IPFS and FileSystem
    /// </summary>
    public enum StorageDriverType
    {
        /// <summary>
        /// None - no driver type selected
        /// </summary>
        None,
        /// <summary>
        /// Classic file system
        /// </summary>
        FileSystem,
        /// <summary>
        /// Inter Planetary File System
        /// </summary>
        IPFS,
        /// <summary>
        /// File Transfer Protocol storage
        /// </summary>
        FTP,
        /// <summary>
        /// HTTP API with upload/download of files
        /// </summary>
        RESTAPI,
        /// <summary>
        /// SSH common connection to remote system
        /// </summary>
        SSH
    }
    /// <summary>
    /// Location type
    /// </summary>
    public enum LocationType
    {
        /// <summary>
        /// Local storage on same device as app
        /// </summary>
        Local,
        /// <summary>
        /// Local network, not in public internet
        /// </summary>
        LocalNetwork,
        /// <summary>
        /// Storage is in the public internet network
        /// </summary>
        Cloud
    }
    /// <summary>
    /// Interface for StorageDrivers
    /// </summary>
    public interface IStorageDriver
    {
        /// <summary>
        /// Type of the driver
        /// </summary>
        StorageDriverType Type { get; set; }
        /// <summary>
        /// Location of the driver, describes if it is Local or in internet
        /// </summary>
        LocationType Location { get; set; }
        /// <summary>
        /// Unique ID of the driver
        /// </summary>
        string ID { get; set; }
        /// <summary>
        /// Name of the driver
        /// </summary>
        string Name { get; set; }
        /// <summary>
        /// Connection parameters such as URL, IP, etc.
        /// </summary>
        StorageDriverConnectionParams ConnectionParams { get; set; }
        /// <summary>
        /// Status of the connection
        /// </summary>
        ConnectionStatus Status { get; set; }
        /// <summary>
        /// If the Driver for the Local instance of the storage, this is set
        /// </summary>
        bool IsLocal { get; set; }
        /// <summary>
        /// The information about setup of the public gateway parameters
        /// </summary>
        bool IsPublicGateway { get; set; }
        /// <summary>
        /// Write stream of the data to the storage
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        Task<(bool, string)> WriteStreamAsync(WriteStreamRequestDto dto);
        /// <summary>
        /// Get bytes from the storage based on the path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<(bool, byte[])> GetBytesAsync(string path);
        /// <summary>
        /// Get stream of the data from the storage based on the path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<(bool, StreamResponseDto)> GetStreamAsync(string path);
        /// <summary>
        /// Remove file on the storage based on the path
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        Task<(bool, string)> RemoveFileAsync(string path);
        /// <summary>
        /// Test connection to the storage
        /// </summary>
        /// <returns></returns>
        Task<(bool, string)> TestConnection();
    }
}
