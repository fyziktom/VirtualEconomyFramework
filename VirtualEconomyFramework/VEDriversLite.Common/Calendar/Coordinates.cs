using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Common.Calendar
{
    /// <summary>
    /// Coordinates on the planet
    /// </summary>
    public class Coordinates
    {
        public Coordinates() { }
        public Coordinates(double lat, double lng)
        {
            Latitude = lat;
            Longitude = lng;
        }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}
