using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDrivers.Common
{
    /// <summary>
    /// This class is used for mapping in the database
    /// All of these properties are recommended to use when writing to the Db
    /// </summary>
    public class CommonDbObjectBase : ICommonDbObjectBase
    {
        /// <summary>
        /// Login of user who created the object
        /// </summary>
        public string CreatedBy { get; set; } = "DEFAULT";
        /// <summary>
        /// Login of user who modified the object properties
        /// </summary>
        public string ModifiedBy { get; set; } = "DEFAULT";
        /// <summary>
        /// Version of property
        /// </summary>
        public string Version { get; set; } = "0.1";
        /// <summary>
        /// Set when the item should not be used anymore
        /// Better than rmove it
        /// </summary>
        public bool? Deleted { get; set; }

        /// <summary>
        /// Should be automatically generated when object is changed
        /// UTC Time
        /// </summary>
        public DateTime ModifiedOn { get; set; } = DateTime.UtcNow;
        /// <summary>
        /// Should be automatically generated in the object factory
        /// UTC Time
        /// </summary>
        public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
    }
}
