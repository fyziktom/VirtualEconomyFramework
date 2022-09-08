using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.StorageDriver.StorageDrivers
{
    /// <summary>
    /// Factory to create Storage Driver based on the type
    /// </summary>
    public static class StorageDriverFactory
    {
        /// <summary>
        /// Get Storage Driver based on the type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static IStorageDriver GetStorageDriver(StorageDriverType type)
        {
            IStorageDriver driver = null;
            switch (type)
            {
                case StorageDriverType.FileSystem:
                    driver = new LocalFileSystemDriver();
                    break;
                case StorageDriverType.IPFS:
                    driver = new IPFSStorageDriver();
                    break;
            }
            return driver;
        }
    }
}
