using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.EntitiesBlocks.Handlers;

namespace VEFramework.VEBlazor.EntitiesBlocks.Services
{
    public class AppData
    {
        public BaseEntitiesHandler EntitiesHandler { get; set; } = new BaseEntitiesHandler();
        public string RootItemId { get; set; } = string.Empty;
        public string RootItemName { get; set; } = string.Empty;
        public string StoredConfig { get; set; } = string.Empty;

    }
}
