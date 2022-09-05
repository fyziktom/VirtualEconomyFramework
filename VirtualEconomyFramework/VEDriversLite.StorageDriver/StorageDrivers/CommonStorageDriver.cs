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
    public abstract class CommonStorageDriver : IStorageDriver
    {
        public StorageDriverType Type { get; set; } = StorageDriverType.None;
        public LocationType Location { get; set; } = LocationType.Local;
        public string ID { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public StorageDriverConnectionParams ConnectionParams { get; set; } = new StorageDriverConnectionParams();
        public bool IsLocal { get; set; }
        public bool IsPublicGateway { get; set; }
        public ConnectionStatus Status { get; set; } = new ConnectionStatus();

        public abstract Task<(bool, string)> WriteStreamAsync(WriteStreamRequestDto dto);

        public abstract Task<(bool, byte[])> GetBytesAsync(string path);

        public abstract Task<(bool, StreamResponseDto)> GetStreamAsync(string path);

        public abstract Task<(bool, string)> RemoveFileAsync(string path);

        public abstract Task<(bool, string)> TestConnection();
        
    }
}
