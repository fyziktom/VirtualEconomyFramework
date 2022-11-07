using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Blocks;
using VEDriversLite.EntitiesBlocks.Blocks.Dto;
using VEDriversLite.EntitiesBlocks.Entities;

namespace VEDriversLite.EntitiesBlocks.Consumers.Dto
{
    public class ConsumerConfigDto
    {
        public ConsumerType? ConsumerType { get; set; }
        public EntityType? Type { get; set; }
        public string? Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ParentId { get; set; }

        public List<string>? Childs { get; set; }

        public ConcurrentDictionary<string, BaseBlockConfigDto>? Blocks { get; set; }

        public void Load(IConsumer consumer)
        {
            if (consumer == null) return;

            Type = consumer.Type;
            ConsumerType = consumer.ConsumerType;

            if (consumer.Id != null)
                Id = consumer.Id;
            if (consumer.Name != null)
                Name = consumer.Name;
            if (consumer.Description != null)
                Description = consumer.Description;
            if (consumer.ParentId != null)
                ParentId = consumer.ParentId;
            if (consumer.Children != null)
                Childs = consumer.Children;
            if (consumer.Blocks != null)
            {
                Blocks = new ConcurrentDictionary<string, BaseBlockConfigDto>();
                foreach (var block in consumer.Blocks.Values)
                {
                    var blk = new BaseBlockConfigDto();
                    blk.Fill(block);
                    Blocks.TryAdd(blk.Id, blk);
                }
            }
        }

        public IConsumer Fill()
        {
            var result = ConsumerFactory.GetConsumer((ConsumerType)ConsumerType, Name, ParentId, Id);
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

            return result;
        }

    }
}
