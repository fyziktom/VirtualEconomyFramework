using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Common
{
    public static class MathHelpers
    {
        /// <summary>
        /// https://stackoverflow.com/questions/4140719/calculate-median-in-c-sharp
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="items"></param>
        /// <returns></returns>
        public static T Median<T>(this IEnumerable<T> items)
        {
            if (items != null && items.Count() > 0)
            {
                var i = (int)Math.Ceiling((double)(items.Count() - 1) / 2);
                if (i >= 0)
                {
                    var values = items.ToList();
                    values.Sort();
                    return values[i];
                }

            }
            return default(T);
        }


        public static double DegreeToRadians(double degree)
        {
            return degree * (Math.PI / 180);
        }

        public static double RadiansToDegrees(double radians)
        {
            return radians * (180 / Math.PI);
        }
    }
}
