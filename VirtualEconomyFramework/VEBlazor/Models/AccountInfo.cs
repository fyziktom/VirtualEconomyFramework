using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VEDriversLite.Dto;
using VEDriversLite.Security;

namespace VEFramework.VEBlazor.Models
{
    public class AccountInfo
    {
        [System.ComponentModel.DataAnnotations.Key]
        public long Id { get; set; }
        public string Address { get; set; }
        public string Key { get; set; }
        public string OpenAPIKey { get; set; }
    }

    public class SubAccountAccountInfo : AccountExportDto
    {
        [System.ComponentModel.DataAnnotations.Key]
        public long Id { get; set; }
    }
}
