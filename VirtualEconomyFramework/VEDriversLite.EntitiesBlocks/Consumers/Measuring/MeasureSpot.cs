using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common.IoT.Dto;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;

namespace VEDriversLite.EntitiesBlocks.Consumers.Measuring
{
    /// <summary>
    /// https://www.promotic.eu/en/pmdoc/Subsystems/Comm/PmDrivers/IEC62056_OBIS.htm
    /// </summary>
    public enum OBIS
    {
        /// <summary>
        /// Positive active energy (A+) total [kWh]
        /// </summary>
        code180,
        /// <summary>
        /// Positive active energy (A+) in tariff T1 [kWh]
        /// </summary>
        code181,
        /// <summary>
        /// Positive active energy (A+) in tariff T2 [kWh]
        /// </summary>
        code182,
        /// <summary>
        /// Positive active energy (A+) in tariff T3 [kWh]
        /// </summary>
        code183,
        /// <summary>
        /// Positive active energy (A+) in tariff T4 [kWh]
        /// </summary>
        code184,
    }

    public class MeasureSpot
    {
        /// <summary>
        /// Name of the Measure Spot
        /// </summary>
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Id of measure spot
        /// </summary>
        public string Id { get; set; } = string.Empty;
        /// <summary>
        /// Id of parent of this spot. Usually some spots network or entity
        /// </summary>
        public string ParentId { get; set; } = string.Empty;
        /// <summary>
        /// Connection parameters for SmartMeter or remote simulator
        /// </summary>
        public CommonConnectionParams ConnectionParams { get; set; } = new CommonConnectionParams();

        /// <summary>
        /// Measured profiles
        /// </summary>
        public ConcurrentDictionary<OBIS, DataProfile> Profiles { get; set; } = new ConcurrentDictionary<OBIS, DataProfile>();

        /// <summary>
        /// Subscribe to data channel through driver for DLMS, MQTT, OPCUA, etc.
        /// Not implemented yet.
        /// </summary>
        /// <returns></returns>
        public bool SubscribeDataChannel()
        {
            // todo subscribtion to DLMS/MQTT/OPCUA, etc.
            return false;
        }
    }
}
