using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Sources.Dto;
using VEDriversLite.EntitiesBlocks.Entities;

namespace VEDriversLite.EntitiesBlocks.Sources
{
    public static class SourceFactory
    {
        public static ISource GetSource(SourceType type, string name, string parentId, string id = null)
        {
            ISource source = null;
            switch(type)
            {
                case SourceType.PVE:
                    source = new PVESource();
                    break;
                case SourceType.Battery:
                    source = new BatteryStorage();
                    break;
                case SourceType.PowerGridSupplier:
                    source = new PowerGridSupplier();
                    break;
            }
            if (source != null)
            {
                if (id != null)
                    source.Id = id;
                source.Name = name;
                source.ParentId = parentId;
            }
            return source;
        }

        public static ISource GetSourceFromJson(string json)
        {
            SourceConfigDto loaded = null;
            try
            {
                loaded = JsonConvert.DeserializeObject<SourceConfigDto>(json);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Cannot deserialize the source from the json. Exception: " + ex.Message);
                throw new Exception("Cannot deserialize the source from the json. Exception: " + ex.Message);
            }

            ISource source = null;
            if (loaded != null)
            {
                if (loaded.Type == EntityType.Source)
                {
                    try
                    {
                        switch (loaded.SourceType)
                        {
                            case SourceType.PVE:
                                source = JsonConvert.DeserializeObject<PVESource>(json) ?? new PVESource();
                                break;
                            case SourceType.Battery:
                                source = JsonConvert.DeserializeObject<BatteryStorage>(json) ?? new BatteryStorage();
                                break;
                            case SourceType.PowerGridSupplier:
                                source = JsonConvert.DeserializeObject<PowerGridSupplier>(json) ?? new PowerGridSupplier();
                                break;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Cannot deserialize the source from the json. Exception: " + ex.Message);
                        throw new Exception("Cannot deserialize the source from the json. Exception: " + ex.Message);
                    }
                }
            }

            return source;
        }
    }
}
