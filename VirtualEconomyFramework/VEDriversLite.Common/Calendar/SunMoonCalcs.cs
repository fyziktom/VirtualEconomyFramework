#region license
/*
(c) 2011-2015, Vladimir Agafonkin, (c) 2016 VPKSoft
Based on a JavaScript library SunCalc for calculating sun/moon position and light phases.
https://github.com/mourner/suncalc
Translated to c# by VPKSoft, http://www.vpksoft.net

Many thanks for this class to VPKSoft :) I have added just function: GetSunBeamAngle
*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Common.Calendar
{
    public static class DateTimeJavaScriptExt
    {
        public static double ValueOf(this DateTime dt) // JavaScript Date.valueOf()
        {
            dt = dt.Kind == DateTimeKind.Local ? dt.ToUniversalTime() : dt;
            return (dt - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds;
        }

        public static DateTime FromJScriptValue(this DateTime dt, double ms)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(ms);
        }
    }

    public class SunMoonCalcs
    {
        private const double dayMs = 86400000;
        private const double J1970 = 2440588;
        private const double J2000 = 2451545;
        private const double PI = Math.PI;
        private const double rad = Math.PI / 180.0;
        private const double e = rad * 23.4397; // obliquity of the Earth

        public class SunTime
        {
            public double Angle { get; set; }
            public string MorningName { get; set; } = string.Empty;
            public string EveningName { get; set; } = string.Empty;
        }

        public class SunTimeRiseSet : SunTime
        {
            public DateTime RiseTime { get; set; }
            public DateTime SetTime { get; set; }
        }

        // sun times configuration (angle, morning name, evening name)
        public static List<SunTime> SunTimes { get; set; } = new List<SunTime>(new SunTime[]
        {
            new SunTime {Angle = -0.833,    MorningName = "sunrise",        EveningName = "sunset" },
            new SunTime {Angle = -0.3,      MorningName = "sunriseEnd",     EveningName = "sunsetStart" },
            new SunTime {Angle = -6,        MorningName = "dawn",           EveningName = "dusk" },
            new SunTime {Angle = -12,       MorningName = "nauticalDawn",   EveningName = "nauticalDusk" },
            new SunTime {Angle = -18,       MorningName = "nightEnd",       EveningName = "night" },
            new SunTime {Angle = 6,         MorningName = "goldenHourEnd",  EveningName = "goldenHour" }
        });

        // adds a custom time to the times config
        public static void AddTime(SunTime sunTime)
        {
            SunTimes.Add(sunTime);
        }

        public class RaDec
        {
            public double ra { get; set; } = 0;
            public double dec { get; set; } = 0;
        }

        public static double ToJulianDate(DateTime dt)
        {
            dt = dt.Kind == DateTimeKind.Local ? dt.ToUniversalTime() : dt;
            return (dt - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).TotalMilliseconds / dayMs - 0.5 + J1970;
        }

        public static DateTime FromJulianDate(double jd)
        {
            return double.IsNaN(jd) ? DateTime.MinValue : new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds((jd + 0.5 - J1970) * dayMs);
        }

        public static double JulianDays(DateTime dt)
        {
            dt = dt.Kind == DateTimeKind.Local ? dt.ToUniversalTime() : dt;
            return ToJulianDate(dt) - J2000;
        }

        public static double RightAscension(double l, double b)
        {
            return Math.Atan2(Math.Sin(l) * Math.Cos(e) - Math.Tan(b) * Math.Sin(e), Math.Cos(l));
        }

        public static double Declination(double l, double b)
        {
            return Math.Asin(Math.Sin(b) * Math.Cos(e) + Math.Cos(b) * Math.Sin(e) * Math.Sin(l));
        }

        public static double Azimuth(double H, double phi, double dec)
        {
            return Math.Atan2(Math.Sin(H), Math.Cos(H) * Math.Sin(phi) - Math.Tan(dec) * Math.Cos(phi));
        }

        public static double Altitude(double H, double phi, double dec)
        {
            return Math.Asin(Math.Sin(phi) * Math.Sin(dec) + Math.Cos(phi) * Math.Cos(dec) * Math.Cos(H));
        }

        public static double SiderealTime(double d, double lw)
        {
            return rad * (280.16 + 360.9856235 * d) - lw;
        }

        public static double AstroRefraction(double h)
        {
            if (h < 0) // the following formula works for positive altitudes only.
            {
                h = 0; // if h = -0.08901179 a div/0 would occur.
            }

            // formula 16.4 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
            // 1.02 / tan(h + 10.26 / (h + 5.10)) h in degrees, result in arc minutes -> converted to rad:
            return 0.0002967 / Math.Tan(h + 0.00312536 / (h + 0.08901179));
        }

        // general sun calculations
        public static double SolarMeanAnomaly(double d)
        {
            return rad * (357.5291 + 0.98560028 * d);
        }

        public static DateTime HoursLater(DateTime dt, double h)
        {
            return new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddMilliseconds(dt.ValueOf() + h * dayMs / 24);
        }

        public static double EclipticLongitude(double M)
        {
            double C = rad * (1.9148 * Math.Sin(M) + 0.02 * Math.Sin(2 * M) + 0.0003 * Math.Sin(3 * M)); // equation of center
            double P = rad * 102.9372; // perihelion of the Earth
            return M + C + P + PI;
        }

        public static RaDec SunCoords(double d)
        {
            double M = SolarMeanAnomaly(d);
            double L = EclipticLongitude(M);
            return new RaDec { dec = Declination(L, 0), ra = RightAscension(L, 0) };
        }

        public class MoonRaDecDist
        {
            public double ra { get; set; } = 0;
            public double dec { get; set; } = 0;
            public double dist { get; set; } = 0;
        }

        public static MoonRaDecDist MoonCoords(double d) // geocentric ecliptic coordinates of the moon
        {
            double L = rad * (218.316 + 13.176396 * d); // ecliptic longitude
            double M = rad * (134.963 + 13.064993 * d); // mean anomaly
            double F = rad * (93.272 + 13.229350 * d);  // mean distance

            double l = L + rad * 6.289 * Math.Sin(M); // longitude
            double b = rad * 5.128 * Math.Sin(F);     // latitude
            double dt = 385001 - 20905 * Math.Cos(M);  // distance to the moon in km

            return new MoonRaDecDist { ra = RightAscension(l, b), dec = Declination(l, b), dist = dt };
        }

        public class SunCalc
        {
            private const double J0 = 0.0009;

            public class AzAlt
            {
                public double Azimuth { get; set; } = 0;
                public double Altitude { get; set; } = 0;
            }

            public static AzAlt GetPosition(DateTime dt, double lat, double lng)
            {
                double lw = rad * -lng;
                double phi = rad * lat;
                double d = JulianDays(dt);
                RaDec c = SunCoords(d);
                double H = SiderealTime(d, lw) - c.ra;
                return new AzAlt { Azimuth = Azimuth(H, phi, c.dec), Altitude = Altitude(H, phi, c.dec) };
            }

            public static double GetSunBeamAngle(AzAlt pos, double panelBaseAngle, double panelAzimuth, bool returnDegrees = false, bool round = false, int decimals = 0)
            {
                var zenitsunangle = Math.PI / 2 - pos.Altitude;

                var angle = Math.PI / 2 - 
                            Math.Acos(
                                Math.Cos(zenitsunangle) * Math.Cos(panelBaseAngle) +
                                Math.Sin(zenitsunangle) * Math.Sin(panelBaseAngle) * Math.Cos(pos.Azimuth -
                                panelAzimuth)
                            );
                if (!returnDegrees)
                    return round ? Math.Round(angle,decimals) : angle;
                else
                    return round ? Math.Round(angle * (180 / Math.PI), decimals) : angle * (180 / Math.PI);
            }

            public static double JulianCycle(double d, double lw)
            {
                return Math.Round(d - J0 - lw / (2 * PI));
            }

            public static double ApproxTransit(double Ht, double lw, double n)
            {
                return J0 + (Ht + lw) / (2 * PI) + n;
            }

            public static double SolarTransitJ(double ds, double M, double L)
            {
                return J2000 + ds + 0.0053 * Math.Sin(M) - 0.0069 * Math.Sin(2.0 * L);
            }

            public static double HourAngle(double h, double phi, double d)
            {
                return Math.Acos((Math.Sin(h) - Math.Sin(phi) * Math.Sin(d)) / (Math.Cos(phi) * Math.Cos(d)));
            }

            // returns set time for the given sun altitude
            public static double GetSetJ(double h, double lw, double phi, double dec, double n, double M, double L)
            {
                double w = HourAngle(h, phi, dec);
                double a = ApproxTransit(w, lw, n);
                return SolarTransitJ(a, M, L);
            }
            // solar disc diameter 
            public static void GetTimes(DateTime dt, double lat, double lng, out DateTime rise, out DateTime set, double angle = -0.833)
            {
                double lw = rad * -lng;
                double phi = rad * lat;
                double d = JulianDays(dt);
                double n = JulianCycle(d, lw);
                double ds = ApproxTransit(0, lw, n);

                double M = SolarMeanAnomaly(ds);
                double L = EclipticLongitude(M);
                double dec = Declination(L, 0);

                double Jnoon = SolarTransitJ(ds, M, L);
                double Jset = GetSetJ(angle * rad, lw, phi, dec, n, M, L);
                double Jrise = Jnoon - (Jset - Jnoon);

                rise = double.IsNaN(Jrise) ? DateTime.MinValue : FromJulianDate(Jrise);
                set = double.IsNaN(Jset) ? DateTime.MinValue : FromJulianDate(Jset);
            }

            public static List<SunTimeRiseSet> GetTimes(DateTime dt, double lat, double lng)
            {
                List<SunTimeRiseSet> retval = new List<SunTimeRiseSet>();
                DateTime rise, set;
                foreach (SunTime st in SunTimes)
                {
                    GetTimes(dt, lat, lng, out rise, out set, st.Angle);
                    retval.Add(new SunTimeRiseSet { Angle = st.Angle, MorningName = st.MorningName, EveningName = st.EveningName, RiseTime = rise, SetTime = set });
                }
                return retval;
            }
        }

        public class MoonCalc
        {
            public class MoonAzAltDistPa
            {
                public double Azimuth { get; set; } = 0;
                public double Altitude { get; set; } = 0;
                public double Distance { get; set; } = 0;
                public double ParallacticAngle { get; set; } = 0;
            }

            public class MoonFracPhaseAngle
            {
                public double Fraction { get; set; } = 0;
                public double Phase { get; set; } = 0;
                public double Angle { get; set; } = 0;
            }

            public static MoonAzAltDistPa GetMoonPosition(DateTime dt, double lat, double lng)
            {
                double lw = rad * -lng;
                double phi = rad * lat;
                double d = JulianDays(dt);

                MoonRaDecDist c = MoonCoords(d);
                double H = SiderealTime(d, lw) - c.ra;
                double h = Altitude(H, phi, c.dec);
                // formula 14.1 of "Astronomical Algorithms" 2nd edition by Jean Meeus (Willmann-Bell, Richmond) 1998.
                double pa = Math.Atan2(Math.Sin(H), Math.Tan(phi) * Math.Cos(c.dec) - Math.Sin(c.dec) * Math.Cos(H));

                h += AstroRefraction(h); // altitude correction for refraction
                return new MoonAzAltDistPa { Azimuth = Azimuth(H, phi, c.dec), Altitude = h, Distance = c.dist, ParallacticAngle = pa };
            }

            public static MoonFracPhaseAngle GetMoonIllumination(DateTime dt)
            {
                double d = JulianDays(dt);
                RaDec s = SunCoords(d);
                MoonRaDecDist m = MoonCoords(d);
                double sdist = 149598000; // distance from Earth to Sun in km
                double phi = Math.Acos(Math.Sin(s.dec) * Math.Sin(m.dec) + Math.Cos(s.dec) * Math.Cos(m.dec) * Math.Cos(s.ra - m.ra));
                double inc = Math.Atan2(sdist * Math.Sin(phi), m.dist - sdist * Math.Cos(phi));
                double angle = Math.Atan2(Math.Cos(s.dec) * Math.Sin(s.ra - m.ra), Math.Sin(s.dec) * Math.Cos(m.dec) -
                               Math.Cos(s.dec) * Math.Sin(m.dec) * Math.Cos(s.ra - m.ra));
                return new MoonFracPhaseAngle { Fraction = (1 + Math.Cos(inc)) / 2, Phase = 0.5 + 0.5 * inc * (angle < 0 ? -1 : 1) / PI, Angle = angle };
            }

            // DateTime.Max = always up, DateTime.Min = always down
            public static void GetMoonTimes(DateTime dt, double lat, double lng, out DateTime risem, out DateTime setm, out bool? alwaysUp, out bool? alwaysDown)
            {
                dt = new DateTime(dt.Year, dt.Month, dt.Day, 0, 0, 0, DateTimeKind.Utc);

                DateTime t = dt;

                double hc = 0.133 * rad;
                double h0 = GetMoonPosition(t, lat, lng).Altitude - hc;
                double h1, h2, rise = 0, set = 0, a, b, xe, ye = 0, d, x1, x2, dx;
                int roots;

                for (double i = 1.0; i <= 24.0; i += 2.0)
                {
                    h1 = GetMoonPosition(HoursLater(t, i), lat, lng).Altitude - hc;
                    h2 = GetMoonPosition(HoursLater(t, i + 1), lat, lng).Altitude - hc;

                    a = (h0 + h2) / 2 - h1;
                    b = (h2 - h0) / 2;
                    xe = -b / (2 * a);
                    ye = (a * xe + b) * xe + h1;
                    d = b * b - 4 * a * h1;
                    roots = 0;

                    if (d >= 0)
                    {
                        dx = Math.Sqrt(d) / (Math.Abs(a) * 2);
                        x1 = xe - dx;
                        x2 = xe + dx;
                        if (Math.Abs(x1) <= 1)
                        {
                            roots++;
                        }

                        if (Math.Abs(x2) <= 1)
                        {
                            roots++;
                        }

                        if (x1 < -1)
                        {
                            x1 = x2;
                        }

                        if (roots == 1)
                        {
                            if (h0 < 0)
                            {
                                rise = i + x1;
                            }
                            else
                            {
                                set = i + x1;
                            }
                        }
                        else if (roots == 2)
                        {
                            rise = i + (ye < 0 ? x2 : x1);
                            set = i + (ye < 0 ? x1 : x2);
                        }

                        if (rise > 0 && set > 0)
                        {
                            break;
                        }

                        h0 = h2;
                    }
                }

                risem = DateTime.MinValue;
                setm = DateTime.MinValue;

                if (rise > 0)
                {
                    risem = HoursLater(t, rise);
                }

                if (set > 0)
                {
                    setm = HoursLater(t, set);
                }

                alwaysUp = null;
                alwaysDown = null;

                if (rise < 0 && set < 0)
                {
                    if (ye > 0)
                    {
                        alwaysUp = true;
                        alwaysDown = false;
                        risem = DateTime.MaxValue;
                        setm = DateTime.MaxValue;
                    }
                    else
                    {
                        alwaysDown = true;
                        alwaysUp = false;
                    }
                }
            }
        }
    }
}