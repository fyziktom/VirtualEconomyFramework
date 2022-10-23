using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.EntitiesBlocks.Financial
{
    public class Discont
    {
        /// <summary>
        /// discont of the money value in the percentage per year. For example 3%/y input is 3 (not 0.03!)
        /// </summary>
        public double DiscontInPercentagePerYear { get; set; } = 0.0;

        /// <summary>
        /// Get Disconted value based on the start - end interval and input value.
        /// Function uses the input value for Discont stored in the instance of the class
        /// </summary>
        /// <param name="input"></param>
        /// <param name="start"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public double GetDiscontedValue(double input, DateTime start, DateTime end)
        {
            var years = (end - start).TotalDays / (start.AddYears(1) - start).TotalDays;
            return input * (1 / (Math.Pow((1 + (DiscontInPercentagePerYear / 100)), years)) );
        }
    }
}
