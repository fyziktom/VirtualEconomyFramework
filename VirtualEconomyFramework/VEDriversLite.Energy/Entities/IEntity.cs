using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks;

namespace VEDriversLite.Energy.Entities
{
    public enum EntityType
    {
        None = VEDriversLite.EntitiesBlocks.Entities.EntityType.None,
        Source = VEDriversLite.EntitiesBlocks.Entities.EntityType.Source,
        Consumer = VEDriversLite.EntitiesBlocks.Entities.EntityType.Consumer,
        Both = VEDriversLite.EntitiesBlocks.Entities.EntityType.Both,
    }

    public interface IEntity : VEDriversLite.EntitiesBlocks.Entities.IEntity
    {
  
    }
}
