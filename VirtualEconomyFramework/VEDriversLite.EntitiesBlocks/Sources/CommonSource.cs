using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Entities;

namespace VEDriversLite.EntitiesBlocks.Sources
{
    public abstract class CommonSource : CommonEntity, ISource
    {
        public SourceType SourceType { get; set; } = SourceType.None;

    }
}
