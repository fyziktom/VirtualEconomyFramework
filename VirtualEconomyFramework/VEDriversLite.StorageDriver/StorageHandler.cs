using Google.Protobuf.WellKnownTypes;
using Newtonsoft.Json;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using VEDriversLite.StorageDriver.StorageDrivers;
using VEDriversLite.StorageDriver.StorageDrivers.Dto;

namespace VEDriversLite.StorageDriver
{
    /// <summary>
    /// Main Handler for storage drivers
    /// Storage drivers can be used to access IPFS, File system and other storage technologies
    /// </summary>
    public class StorageHandler
    {
        /// <summary>
        /// Dictionary of all loaded drivers
        /// </summary>
        public ConcurrentDictionary<string, IStorageDriver> StorageDrivers { get; set; } = new ConcurrentDictionary<string, IStorageDriver>();

        /// <summary>
        /// Function will deserialize the list of the Drivers Configs Dtos and load all the drivers.
        /// It can be use to load the drivers settings from the file
        /// </summary>
        /// <param name="driversConfigJson"></param>
        /// <returns></returns>
        public async Task<(bool, string)> LoadDriversFromJson(string driversConfigJson)
        {
            if (string.IsNullOrEmpty(driversConfigJson))
                return (false, "Input drivers config cannot be empty or null.");

            try
            {
                var driversConfig = JsonConvert.DeserializeObject<List<StorageDriverConfigDto>>(driversConfigJson);
                if (driversConfig != null)
                    return await LoadDrivers(driversConfig); 
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Cannot load storage drivers. " + ex.Message);
                return (false, "Cannot load storage drivers. " + ex.Message);
            }
            return (false, "Canont load storage drivers.");
        }

        /// <summary>
        /// Function will load the drivers based on the list of the configs
        /// </summary>
        /// <param name="driversConfig"></param>
        /// <returns></returns>
        public async Task<(bool, string)> LoadDrivers(List<StorageDriverConfigDto> driversConfig)
        {
            try
            {
                foreach (var driver in driversConfig)
                {
                    if (!string.IsNullOrEmpty(driver.Type))
                    {
                        if (!System.Enum.TryParse(driver.Type, out StorageDriverType dt)) 
                            return (false, "wrong type of the driver");

                        var drv = StorageDriverFactory.GetStorageDriver(dt);
                        if (drv != null)
                        {
                            drv.ConnectionParams = driver.ConnectionParams;
                            drv.Location = (LocationType)System.Enum.Parse(typeof(LocationType), driver.Location);
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

        /// <summary>
        /// Load one driver 
        /// </summary>
        /// <param name="driverConfig"></param>
        /// <returns></returns>
        public async Task<(bool, string)> AddDriver(StorageDriverConfigDto driverConfig)
        {
            try
            {
                if (!string.IsNullOrEmpty(driverConfig.Type))
                {
                    if (!System.Enum.TryParse(driverConfig.Type, out StorageDriverType dt)) 
                        return (false, "wrong type of the driver");

                    var drv = StorageDriverFactory.GetStorageDriver(dt);
                    if (drv != null)
                    {
                        drv.ConnectionParams = driverConfig.ConnectionParams;
                        drv.Location = (LocationType)System.Enum.Parse(typeof(LocationType), driverConfig.Location);
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

        /// <summary>
        /// Remove the drivers base on ID
        /// </summary>
        /// <param name="driverId"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get list of available drivers
        /// You can filter drivers based on the type.
        /// If input types are empty it will return all available drivers
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<StorageDriverConfigDto> GetListOfDrivers(List<StorageDriverType> types = null)
        {
            List<StorageDriverConfigDto> result = new List<StorageDriverConfigDto>();
            if (StorageDrivers == null) return null;
            foreach (var driver in StorageDrivers.Values)
            {
                var add = false;
                if (types != null)
                {
                    if (types.Contains(driver.Type))
                        add = true;
                }
                if (add)
                {
                    result.Add(new StorageDriverConfigDto()
                    {
                        ConnectionParams = driver.ConnectionParams,
                        ID = driver.ID,
                        IsLocal = driver.IsLocal,
                        IsPublicGateway = driver.IsPublicGateway,
                        Location = System.Enum.GetName(typeof(LocationType), driver.Location) ?? "Local",
                        Name = driver.Name,
                        Type = System.Enum.GetName(typeof(StorageDriverType), driver.Type) ?? "None",
                    });
                }
            }
            return result;
        }

        /// <summary>
        /// Save file to the IPFS storage
        /// Function can use specific driver if the StorageId is provided. 
        /// Otherwise it iwill find first available IPFS driver and try to use it.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Get file to the IPFS storage
        /// Function can use specific driver if the StorageId is provided. 
        /// Otherwise it iwill find first available IPFS driver and try to use it.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
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