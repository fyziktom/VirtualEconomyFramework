using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.StorageDriver.StorageDrivers.Dto;

namespace VEDriversLite.StorageDriver.StorageDrivers
{
    public enum StorageDriverType
    {
        None,
        FileSystem,
        IPFS,
        FTP,
        RESTAPI,
        SSH
    }
    public enum LocationType
    {
        Local,
        LocalNetwork,
        Cloud
    }
    public interface IStorageDriver
    {
        StorageDriverType Type { get; set; }
        LocationType Location { get; set; }
        string ID { get; set; }
        string Name { get; set; }

        StorageDriverConnectionParams ConnectionParams { get; set; }
        ConnectionStatus Status { get; set; }
        bool IsLocal { get; set; }
        bool IsPublicGateway { get; set; }

        Task<(bool, string)> WriteStreamAsync(WriteStreamRequestDto dto);

        Task<(bool, byte[])> GetBytesAsync(string path);
        Task<(bool, StreamResponseDto)> GetStreamAsync(string path);
        Task<(bool, string)> RemoveFileAsync(string path);
        Task<(bool, string)> TestConnection();
    }
}
