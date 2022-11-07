using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Entities;

namespace VEDriversLite.EntitiesBlocks.Consumers
{
    public abstract class CommonConsumer : CommonEntity, IConsumer
    {
        public ConsumerType ConsumerType { get; set; } = ConsumerType.None;
    }
}
