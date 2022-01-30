using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Accounts.NFTModules
{
    public static class NFTModuleFactory
    {
        public static async Task<INFTModule> GetNFTModule(NFTModuleType type, string address, string id = "")
        {
            INFTModule module = null;

            switch (type)
            {
                case NFTModuleType.VENFT:
                    module = new VENFTModule();
                    break;
                case NFTModuleType.Shop:
                    module = new ShopNFTModule();
                    break;
                case NFTModuleType.Messages:
                    module = new MessageNFTModule();
                    break;
            }
            if (module != null)
                await module.Init(address, id);

            return module;
        }
    }
}
