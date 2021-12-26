using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite.Devices.Dto
{
    public class IoTDataDriverSettings
    {
        public CommunicationSchemeType ComSchemeType { get; set; } = CommunicationSchemeType.Requests;
        public IoTCommunicationType IoTComType { get; set; } = IoTCommunicationType.API;
        public CommonConnectionParams ConnectionParams { get; set; } = new CommonConnectionParams();
    }
}
