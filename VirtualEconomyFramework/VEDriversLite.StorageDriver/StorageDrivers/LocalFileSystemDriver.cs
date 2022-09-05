using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common;
using VEDriversLite.StorageDriver.StorageDrivers.Dto;

namespace VEDriversLite.StorageDriver.StorageDrivers
{
    internal class LocalFileSystemDriver : CommonStorageDriver
    {
        public LocalFileSystemDriver()
        {
            Type = StorageDriverType.FileSystem;
        }

        public override async Task<(bool, string)> TestConnection()
        {
            if (string.IsNullOrEmpty(ConnectionParams.FileStoragePath))
                return (false, string.Empty);

            try
            {
                var id = Guid.NewGuid().ToString();
                var path = Path.Combine(ConnectionParams.FileStoragePath, $"{id}-test.txt");

                FileHelpers.WriteTextToFile(path, $"TEST:{id}");

                if (FileHelpers.IsFileExists(path))
                {
                    Status.IsAvailable = true;
                    Status.LastPingRoundtripTime = 1;
                    
                    // delete temporary file
                    File.Delete(path);

                    return (true, $"PING:{Status.LastPingRoundtripTime}");
                }
                else
                {
                    Status.IsAvailable = false;
                    Status.LastPingRoundtripTime = 0;
                    return (false, string.Empty);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot access the local storage: {ID}, error: {ex.Message}");
                Status.IsAvailable = false;
                Status.LastPingRoundtripTime = 0;
                return (false, string.Empty);
            }
        }

        public override async Task<(bool,byte[])> GetBytesAsync(string path)
        {
            var response = new StreamResponseDto();
            var fullpath = Path.Combine(ConnectionParams.FileStoragePath, path);

            try
            {
                var bytes = await File.ReadAllBytesAsync(fullpath);
                var filename = Path.GetFileName(fullpath);

                if (bytes?.Length > 0)
                {
                    return (true, bytes);
                }
                return (false, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot load the file: {fullpath}, error: {ex.Message}");
                return (false, null);
            }
        }
        public override async Task<(bool, StreamResponseDto)> GetStreamAsync(string path)
        {
            var response = new StreamResponseDto();
            var fullpath = Path.Combine(ConnectionParams.FileStoragePath, path);

            try
            {
                var bytes = await File.ReadAllBytesAsync(fullpath);
                var filename = Path.GetFileName(fullpath);

                if (bytes?.Length > 0)
                {
                    using (var stream = new MemoryStream(bytes))
                    {
                        response.Filename = filename;
                        response.Data = stream;
                        return (true, response);
                    }
                }
                return (false, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot load the file: {fullpath}, error: {ex.Message}");
                return (false, null);
            }
        }

        public override async Task<(bool, string)> RemoveFileAsync(string path)
        {
            try
            {
                File.Delete(path);
                return (true, "REMOVED");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot remove the file: {path}, error: {ex.Message}");
                return (false, string.Empty);
            }
        }

        public override async Task<(bool, string)> WriteStreamAsync(WriteStreamRequestDto dto)
        {
            var fullpath = Path.Combine(ConnectionParams.FileStoragePath, dto.Filename);

            try
            {
                if (dto.Data != null)
                {
                    var fileStream = new FileStream(fullpath, FileMode.Create, FileAccess.Write);
                    dto.Data.CopyTo(fileStream);
                    fileStream.Dispose();

                    return (true, "SAVED");
                }
                return (false, null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Cannot load the file: {fullpath}, error: {ex.Message}");
                return (false, null);
            }
        }
    }
}
