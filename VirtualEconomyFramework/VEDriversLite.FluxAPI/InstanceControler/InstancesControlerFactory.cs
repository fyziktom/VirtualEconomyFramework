using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.FluxAPI.InstanceControler
{
    public static class InstancesControlerFactory
    {
        public static IInstancesControler GetInstanceControler(InstancesControlerType type)
        {
            switch (type)
            {
                case InstancesControlerType.Basic:
                    return new InstancesControler();
                case InstancesControlerType.Fake:
                    return new FakeInstancesControler();
                default:
                    return null;
            }
        }
    }
}
