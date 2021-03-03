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
    public interface ICommonDbObjectBase
    {
        /// <summary>
        /// Login of user who created the object
        /// </summary>
        string CreatedBy { get; set; }
        /// <summary>
        /// Login of user who modified the object properties
        /// </summary>
        string ModifiedBy { get; set; }
        /// <summary>
        /// Version of property
        /// </summary>
        string Version { get; set; }
        /// <summary>
        /// Set when the item should not be used anymore
        /// Better than rmove it
        /// </summary>
        bool? Deleted { get; set; }

        /// <summary>
        /// Should be automatically generated when object is changed
        /// UTC Time
        /// </summary>
        DateTime ModifiedOn { get; set; }
        /// <summary>
        /// Should be automatically generated in the object factory
        /// UTC Time
        /// </summary>
        DateTime CreatedOn { get; set; }
    }
}
