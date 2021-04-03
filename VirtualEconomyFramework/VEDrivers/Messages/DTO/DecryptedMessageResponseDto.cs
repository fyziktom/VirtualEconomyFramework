using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Messages.DTO
{
    public class DecryptedMessageResponseDto
    {
        public string PrevMsg { get; set; } = string.Empty;
        public string NewMsg { get; set; } = string.Empty;
    }
}
