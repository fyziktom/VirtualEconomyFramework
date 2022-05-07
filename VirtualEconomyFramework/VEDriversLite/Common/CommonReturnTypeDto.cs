using System;
using System.Collections.Generic;
using System.Text;

namespace VEDriversLite
{
    /// <summary>
    /// This class is to declare the common method return type used mostly in helper classes. To generalize the return data we have added this class.
    /// </summary>
    public class CommonReturnTypeDto
    {
        /// <summary>
        /// This property tells whether the return type is a success or a failure
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// This property holds the actual value
        /// </summary>
        public object Value { get; set; }

        public static CommonReturnTypeDto GetNew<T>(bool success = false, T value = default(T))
        {
            return new CommonReturnTypeDto()
            {
                Success = success,
                Value = value
            };
        }
    }
}
