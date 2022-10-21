using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.EntitiesBlocks.StorageCalculations.Dto
{
    public class BatteryStorageDto
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Dictionary of all battery blocks
        /// </summary>
        public ConcurrentDictionary<string, BatteryBlock> BatteryBlocks { get; set; } = new ConcurrentDictionary<string, BatteryBlock>();

        /// <summary>
        /// Template battery for this Battery Group
        /// </summary>
        public BatteryBlock CommonBattery { get; set; } = new BatteryBlock();

    }
}
