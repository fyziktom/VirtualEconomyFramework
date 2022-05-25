using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common;
using VEDriversLite.Common.IoT.Dto;
using VEDriversLite.FluxAPI.InstanceControler.Instances.Dto;

namespace VEDriversLite.FluxAPI.InstanceControler.Instances
{
    public class FakeInstance : CommonInstance
    {
        public override async Task<CommonReturnTypeDto> InitInstance(IoTDataDriverSettings connectionSettings)
        {
            return new CommonReturnTypeDto() { Success = false, Value = "Cannot Connect" };
        }
        public override Task ProcessAllTasks(bool runOnBackground)
        {
            return Task.CompletedTask;
        }

        public override Task ProcessNextTask()
        {
            return Task.CompletedTask;
        }

    }
}
