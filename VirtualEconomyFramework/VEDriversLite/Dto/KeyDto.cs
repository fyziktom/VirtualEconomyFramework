using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite
{
    /// <summary>
    /// Key export import Dto
    /// </summary>
    public class KeyDto
    {
        /// <summary>
        /// Address
        /// </summary>
        public string Address { get; set; } = string.Empty;
        /// <summary>
        /// Private key - usually exported in encrypted form
        /// </summary>
        public string Key { get; set; } = string.Empty;
    }
}
