using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Entities;

namespace VEDriversLite.Energy.Consumers
{
    public class GroupNetwork : CommonConsumer
    {
        public GroupNetwork()
        {
            Type = EntityType.Consumer;
            ConsumerType = ConsumerType.GroupNetwork;
        }
    }
}
