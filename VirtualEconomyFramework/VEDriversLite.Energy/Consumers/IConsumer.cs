using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Entities;

namespace VEDriversLite.Energy.Consumers
{
    /// <summary>
    /// Consumer type
    /// </summary>
    public enum ConsumerType
    {
        /// <summary>
        /// Not defined
        /// </summary>
        None,
        /// <summary>
        /// Device, for example computer
        /// </summary>
        Device,
        /// <summary>
        /// Set of the devices.
        /// For example one flat
        /// </summary>
        DevicesGroup,
        /// <summary>
        /// Network of the DeviceGroups.
        /// For example whole house with several independend flats with common power grid
        /// </summary>
        GroupNetwork
    }
    public interface IConsumer : IEntity
    {
        /// <summary>
        /// Type of the consumer
        /// </summary>
        ConsumerType ConsumerType { get; set; }

    }
}
