using Newtonsoft.Json;
using System.Collections.Concurrent;
using VEDriversLite.StorageDriver.StorageDrivers;
using VEDriversLite.StorageDriver.StorageDrivers.Dto;

namespace VEDriversLite.StorageDriver
{
    public class StorageHandler
    {
        public ConcurrentDictionary<string, IStorageDriver> StorageDrivers { get; set; } = new ConcurrentDictionary<string, IStorageDriver>();

        public async Task<(bool, string)> LoadDrivers(List<StorageDriverConfigDto> driversConfig)
        {
            try
            {
                foreach (var driver in driversConfig)
                {
                    if (!string.IsNullOrEmpty(driver.Type))
                    {
                        if (!Enum.TryParse(driver.Type, out StorageDriverType dt)) 
                            return (false, "wrong type of the driver");

                        var drv = StorageDriverFactory.GetStorageDriver(dt);
                        if (drv != null)
                        {
                            drv.ConnectionParams = driver.ConnectionParams;
                            drv.Location = (LocationType)Enum.Parse(typeof(LocationType), driver.Location);
                            drv.IsPublicGateway = driver.IsPublicGateway;
                            drv.IsLocal = driver.IsLocal;
                            drv.ID = driver.ID; 
                            drv.Name = driver.Name; 
                            if (string.IsNullOrEmpty(drv.ID))
                            {
                                drv.ID = Guid.NewGuid().ToString();
                            }

                            if (!StorageDrivers.TryGetValue(drv.ID, out var d))
                                StorageDrivers.TryAdd(drv.ID, drv);
                        }
                    }
                }

                return (true, "Drivers loaded.");
            }
            catch(Exception ex)
            {
                Console.WriteLine("Cannot load the drivers. " + ex.Message);
                return (false, "Cannot load the drivers. " + ex.Message);
            }
            return (false, "Cannot load the drivers.");
        }

        public async Task<(bool, string)> AddDriver(StorageDriverConfigDto driverConfig)
        {
            try
            {
                if (!string.IsNullOrEmpty(driverConfig.Type))
                {
                    if (!Enum.TryParse(driverConfig.Type, out StorageDriverType dt)) 
                        return (false, "wrong type of the driver");

                    var drv = StorageDriverFactory.GetStorageDriver(dt);
                    if (drv != null)
                    {
                        drv.ConnectionParams = driverConfig.ConnectionParams;
                        drv.Location = (LocationType)Enum.Parse(typeof(LocationType), driverConfig.Location);
                        drv.IsPublicGateway = driverConfig.IsPublicGateway;
                        drv.IsLocal = driverConfig.IsLocal;
                        drv.ID = driverConfig.ID;
                        drv.Name = driverConfig.Name;
                        if (string.IsNullOrEmpty(drv.ID))
                        {
                            drv.ID = Guid.NewGuid().ToString();
                        }

                        var res = await drv.TestConnection();
                        if (!res.Item1)
                            Console.WriteLine($"Cannot connect driver {drv.ID}, {res.Item2}.");

                        if (!StorageDrivers.TryGetValue(drv.ID, out var d))
                            StorageDrivers.TryAdd(drv.ID, drv);

                        return (true, "Driver loaded.");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot load the driver. " + ex.Message);
                return (false, "Cannot load the driver. " + ex.Message);
            }
            return (false, "Cannot load the driver.");
        }

        public async Task<(bool, string)> RemoveDriver(string driverId)
        {
            try
            {
                if (!string.IsNullOrEmpty(driverId))
                {
                    if (StorageDrivers.TryRemove(driverId, out var d))
                        return (true, "Removed");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot remove the driver. " + ex.Message);
                return (false, "Cannot remove the driver. " + ex.Message);
            }
            return (false, "Cannot remove the driver.");
        }

        public async Task<(bool, string)> SaveFileToIPFS(WriteStreamRequestDto dto)
        {
            try
            {
                var res = (false, string.Empty);
                var backuprecquired = dto.BackupInLocal;
                var datastreamCopy = new MemoryStream();
                //if (dto != null && dto.Data != null)
                    //await dto.Data.CopyToAsync(datastreamCopy);

                if (!string.IsNullOrEmpty(dto.StorageId))
                {
                    if (StorageDrivers.TryGetValue(dto.StorageId, out var d))
                    {
                        if (d.IsLocal) backuprecquired = false;
                        res = await d.WriteStreamAsync(dto);
                    }
                }
                else
                {
                    var sd = StorageDrivers.Values.FirstOrDefault(d => d.Type == StorageDriverType.IPFS);
                    
                    if (sd != null)
                        res = await sd.WriteStreamAsync(dto);
                }
                
                if (backuprecquired)
                {
                    var lsd = StorageDrivers.Values.FirstOrDefault(dd => dd.Type == StorageDriverType.IPFS && dd.IsLocal);
                    if (lsd != null)
                    {
                        Console.WriteLine("Backup file in Local IPFS...");
                        dto.Data = datastreamCopy;
                        var r = await lsd.WriteStreamAsync(dto);
                        Console.WriteLine($"Backup file in Local IPFS Result: {r.Item1}, {r.Item2}");
                    }
                }

                return res;
                //return (false, "Cannot save the file. Cannot find any IPFS driver.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot save the file. " + ex.Message);
                return (false, "Cannot save the file. " + ex.Message);
            }
        }

        public async Task<(bool, byte[])> GetFileFromIPFS(ReadFileRequestDto dto)
        {
            try
            {
                (bool, byte[]) res = (false, null);

                if (!string.IsNullOrEmpty(dto.StorageId))
                {
                    if (StorageDrivers.TryGetValue(dto.StorageId, out var d))
                        res = await d.GetBytesAsync(dto.Hash);
                }
                else
                {
                    var sd = StorageDrivers.Values.FirstOrDefault(d => d.Type == StorageDriverType.IPFS);
                    if (sd != null)
                        res = await sd.GetBytesAsync(dto.Hash);
                }

                return res;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot get the file. " + ex.Message);
                return (false, null);
            }
        }
    }
}