using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.StorageDriver.StorageDrivers
{
    public static class StorageDriverFactory
    {
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
