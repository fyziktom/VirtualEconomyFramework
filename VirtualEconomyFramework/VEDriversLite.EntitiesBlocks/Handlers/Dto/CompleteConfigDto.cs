using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Consumers.Dto;
using VEDriversLite.EntitiesBlocks.Sources.Dto;

namespace VEDriversLite.EntitiesBlocks.Handlers.Dto
{
    public class CompleteConfigDto
    {
        public List<ConsumerConfigDto> Consumers { get; set; } = new List<ConsumerConfigDto>();
        public List<SourceConfigDto> Sources { get; set; } = new List<SourceConfigDto>();

    }
}
