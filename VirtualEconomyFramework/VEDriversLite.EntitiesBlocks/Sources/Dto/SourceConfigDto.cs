using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;
using VEDriversLite.EntitiesBlocks.Consumers;
using VEDriversLite.EntitiesBlocks.Entities;

namespace VEDriversLite.EntitiesBlocks.Sources.Dto
{
    public class SourceConfigDto
    {
        public SourceType? SourceType { get; set; }
        public EntityType? Type { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ParentId { get; set; }
      
        public List<string>? Childs { get; set; }

        public ConcurrentDictionary<string, BaseBlockConfigDto>? Blocks { get; set; }

        // Battery specific parameters
        public double? Capacity { get; set; }
        public double? MaximumDischargePower { get; set; }
        public double? MaximumChargePower { get; set; }

        // PVE specific parameters
        public double? AngleOfPanels { get; set; }

        public void Load(ISource source)
        {
            if (source == null) return;

            Type = source.Type;
            SourceType = source.SourceType;

            if (source.Id != null)
                Id = source.Id;
            if (source.Name != null)
                Name = source.Name;
            if (source.Description != null)
                Description = source.Description;
            if (source.ParentId != null)
                ParentId = source.ParentId;
            if (source.Children != null)
                Childs = source.Children;
            if (source.Blocks != null)
            {
                Blocks = new ConcurrentDictionary<string, BaseBlockConfigDto>();
                foreach (var block in source.Blocks.Values)
                {
                    var blk = new BaseBlockConfigDto();
                    blk.Fill(block);
                    Blocks.TryAdd(blk.Id, blk);
                }
            }

            if (source.SourceType == Sources.SourceType.PVE)
                if ((source as PVESource)?.AngleOfPanels != null)
                AngleOfPanels = (source as PVESource)?.AngleOfPanels;
            if (source.SourceType == Sources.SourceType.PVE)
                if ((source as PVESource)?.MaximumDischargePower != null)
                    MaximumDischargePower = (source as PVESource)?.MaximumDischargePower;
            if (source.SourceType == Sources.SourceType.Battery)
                if ((source as BatteryStorage)?.MaximumChargePower != null)
                    MaximumChargePower = (source as BatteryStorage)?.MaximumChargePower;
            if (source.SourceType == Sources.SourceType.Battery)
                if ((source as BatteryStorage)?.MaximumDischargePower != null)
                    MaximumDischargePower = (source as BatteryStorage)?.MaximumDischargePower;
            if (source.SourceType == Sources.SourceType.Battery)
                if ((source as BatteryStorage)?.Capacity != null)
                    Capacity = (source as BatteryStorage)?.Capacity;
        }

        public ISource Fill()
        {
            
            var result = SourceFactory.GetSource((SourceType)SourceType, Name, ParentId, Id);
            if (result == null) return null;

            if (Description != null)
                result.Description = Description;

            if (Childs != null)
                result.Children = Childs;

            if (Blocks != null)
            {
                foreach (var block in Blocks.Values)
                {
                    var blk = block.GetBlockFromDto();
                    if (blk != null)
                        result.AddBlock(blk);
                }
            }

            if ((SourceType)SourceType == Sources.SourceType.PVE && AngleOfPanels != null)
                (result as PVESource).AngleOfPanels = (double)AngleOfPanels;
            if ((SourceType)SourceType == Sources.SourceType.PVE && MaximumDischargePower != null)
                (result as PVESource).MaximumDischargePower = (double)MaximumDischargePower;

            if ((SourceType)SourceType == Sources.SourceType.Battery && Capacity != null)
                (result as BatteryStorage).Capacity = (double)Capacity;
            if ((SourceType)SourceType == Sources.SourceType.Battery && MaximumChargePower != null)
                (result as BatteryStorage).MaximumChargePower = (double)MaximumChargePower;
            if ((SourceType)SourceType == Sources.SourceType.Battery && MaximumDischargePower != null)
                (result as BatteryStorage).MaximumDischargePower = (double)MaximumDischargePower;

            return result;
        }

    }
}
