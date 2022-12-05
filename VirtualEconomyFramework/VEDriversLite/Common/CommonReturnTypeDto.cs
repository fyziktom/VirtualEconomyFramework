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
        /// <summary>
        /// Get empty object with specified type of the object in result
        /// </summary>
        /// <typeparam name="T">Define type of return object</typeparam>
        /// <param name="success">indicate success of some process which returned this object</param>
        /// <param name="value">not specified type. in case of no parameters it returns default state of T</param>
        /// <returns></returns>
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
