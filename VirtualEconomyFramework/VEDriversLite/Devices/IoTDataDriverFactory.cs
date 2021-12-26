using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Devices
{
    public static class IoTDataDriverFactory
    {
        public static async Task<IIoTDataDriver> GetIoTDataDriver(IoTDataDriverType type)
        {
            IIoTDataDriver driver = null;

            switch (type)
            {
                case IoTDataDriverType.HARDWARIO:
                    driver = new HARDWARIOIoTDataDriver();
                    break;
            }

            return driver;
        }
    }
}
