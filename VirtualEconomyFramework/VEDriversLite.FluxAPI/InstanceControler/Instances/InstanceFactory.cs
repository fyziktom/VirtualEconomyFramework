using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.FluxAPI.InstanceControler.Instances
{
    public static class InstanceFactory
    {
        public static IInstance GetInstance(InstanceType type)
        {
            switch (type)
            {
                case InstanceType.APIService:
                    return new APIServiceInstance();
                case InstanceType.Fake:
                    return new FakeInstance();
                default:
                    return null;
            }
        }
    }
}
