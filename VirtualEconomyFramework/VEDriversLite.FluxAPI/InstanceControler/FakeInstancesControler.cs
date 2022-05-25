using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Common;
using VEDriversLite.FluxAPI.InstanceControler.Instances.Dto;

namespace VEDriversLite.FluxAPI.InstanceControler
{
    public class FakeInstancesControler : CommonInstancesControler
    {
        public override async Task<bool> Initialize(string name,
                                                   string serviceName,
                                                   string serviceId,
                                                   bool initAll = false,
                                                   int port = 0,
                                                   string username = "",
                                                   string password = "")
        {
            return true;
        }

        public override async Task<CommonReturnTypeDto> Request(TaskToRunRequestDto task)
        {
            return CommonReturnTypeDto.GetNew<TaskToRunResponseDto>();
        }
    }
}
