using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VEDriversLite.Common.Calendar
{
    [Flags]
    public enum Week
    {
        Monday = 1,
        Tuesday = 2,
        Wednesday = 4,
        Thursday = 8,
        Friday = 16,
        Saturday = 32,
        Sunday = 64,

        WorkDays = Monday | Tuesday | Wednesday | Thursday | Friday,
        Weekedn = Saturday | Sunday,
        WholeWeek = WorkDays | Weekedn
    }

    public static class WeekHelpers
    {
        public static bool IsDateTimeAllowed(this DateTime dt, Week scheduledDays)
        {
            switch (scheduledDays)
            {
                case Week.WorkDays:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Tuesday ||
                        dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Thursday ||
                        dt.DayOfWeek == DayOfWeek.Friday)
                        return true;
                    break;
                case Week.Weekedn:
                    if (dt.DayOfWeek == DayOfWeek.Saturday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.WholeWeek:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Tuesday ||
                        dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Thursday ||
                        dt.DayOfWeek == DayOfWeek.Friday ||
                        dt.DayOfWeek == DayOfWeek.Saturday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.Monday:
                    if (dt.DayOfWeek == DayOfWeek.Monday)
                        return true;
                    break;
                case Week.Tuesday:
                    if (dt.DayOfWeek == DayOfWeek.Tuesday)
                        return true;
                    break;
                case Week.Wednesday:
                    if (dt.DayOfWeek == DayOfWeek.Wednesday)
                        return true;
                    break;
                case Week.Thursday:
                    if (dt.DayOfWeek == DayOfWeek.Thursday)
                        return true;
                    break;
                case Week.Friday:
                    if (dt.DayOfWeek == DayOfWeek.Friday)
                        return true;
                    break;
                case Week.Saturday:
                    if (dt.DayOfWeek == DayOfWeek.Saturday)
                        return true;
                    break;
                case Week.Sunday:
                    if (dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;

                case Week.Monday | Week.Tuesday:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Tuesday)
                        return true;
                    break;
                case Week.Monday | Week.Tuesday | Week.Wednesday:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Tuesday ||
                        dt.DayOfWeek == DayOfWeek.Wednesday)
                        return true;
                    break;
                case Week.Monday | Week.Tuesday | Week.Wednesday | Week.Thursday:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Tuesday ||
                        dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Thursday)
                        return true;
                    break;
                case Week.Monday | Week.Tuesday | Week.Wednesday | Week.Thursday | Week.Friday | Week.Saturday:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Tuesday ||
                        dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Thursday ||
                        dt.DayOfWeek == DayOfWeek.Thursday ||
                        dt.DayOfWeek == DayOfWeek.Friday ||
                        dt.DayOfWeek == DayOfWeek.Saturday)
                        return true;
                    break;
                case Week.Tuesday | Week.Wednesday:
                    if (dt.DayOfWeek == DayOfWeek.Tuesday ||
                        dt.DayOfWeek == DayOfWeek.Wednesday)
                        return true;
                    break;
                case Week.Tuesday | Week.Wednesday | Week.Thursday:
                    if (dt.DayOfWeek == DayOfWeek.Tuesday ||
                        dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Thursday)
                        return true;
                    break;
                case Week.Tuesday | Week.Wednesday | Week.Thursday | Week.Friday:
                    if (dt.DayOfWeek == DayOfWeek.Tuesday ||
                        dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Thursday ||
                        dt.DayOfWeek == DayOfWeek.Friday)
                        return true;
                    break;
                case Week.Tuesday | Week.Wednesday | Week.Thursday | Week.Friday | Week.Saturday:
                    if (dt.DayOfWeek == DayOfWeek.Tuesday ||
                        dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Thursday ||
                        dt.DayOfWeek == DayOfWeek.Friday ||
                        dt.DayOfWeek == DayOfWeek.Saturday)
                        return true;
                    break;
                case Week.Tuesday | Week.Wednesday | Week.Thursday | Week.Friday | Week.Saturday | Week.Sunday:
                    if (dt.DayOfWeek == DayOfWeek.Tuesday ||
                        dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Thursday ||
                        dt.DayOfWeek == DayOfWeek.Friday ||
                        dt.DayOfWeek == DayOfWeek.Saturday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.Wednesday | Week.Thursday:
                    if (dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Thursday)
                        return true;
                    break;
                case Week.Wednesday | Week.Thursday | Week.Friday:
                    if (dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Thursday ||
                        dt.DayOfWeek == DayOfWeek.Friday)
                        return true;
                    break;
                case Week.Wednesday | Week.Thursday | Week.Friday | Week.Saturday:
                    if (dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Thursday ||
                        dt.DayOfWeek == DayOfWeek.Friday ||
                        dt.DayOfWeek == DayOfWeek.Saturday)
                        return true;
                    break;
                case Week.Wednesday | Week.Thursday | Week.Friday | Week.Saturday | Week.Sunday:
                    if (dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Thursday ||
                        dt.DayOfWeek == DayOfWeek.Friday ||
                        dt.DayOfWeek == DayOfWeek.Saturday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.Friday | Week.Saturday:
                    if (dt.DayOfWeek == DayOfWeek.Friday ||
                        dt.DayOfWeek == DayOfWeek.Saturday)
                        return true;
                    break;
                case Week.Friday | Week.Saturday | Week.Sunday:
                    if (dt.DayOfWeek == DayOfWeek.Friday ||
                        dt.DayOfWeek == DayOfWeek.Saturday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.Monday | Week.Tuesday | Week.Friday:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Tuesday ||
                        dt.DayOfWeek == DayOfWeek.Friday)
                        return true;
                    break;
                case Week.Monday | Week.Wednesday | Week.Friday:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Friday)
                        return true;
                    break;
                case Week.Monday | Week.Thursday | Week.Friday:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Thursday ||
                        dt.DayOfWeek == DayOfWeek.Friday)
                        return true;
                    break;
                case Week.Monday | Week.Wednesday | Week.Saturday:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Saturday)
                        return true;
                    break;
                case Week.Monday | Week.Thursday | Week.Saturday:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Thursday ||
                        dt.DayOfWeek == DayOfWeek.Saturday)
                        return true;
                    break;
                case Week.Monday | Week.Wednesday | Week.Sunday:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.Monday | Week.Thursday | Week.Sunday:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Thursday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.Monday | Week.Tuesday | Week.Friday | Week.Saturday:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Tuesday ||
                        dt.DayOfWeek == DayOfWeek.Friday ||
                        dt.DayOfWeek == DayOfWeek.Saturday)
                        return true;
                    break;
                case Week.Monday | Week.Tuesday | Week.Friday | Week.Saturday | Week.Sunday:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Tuesday ||
                        dt.DayOfWeek == DayOfWeek.Friday ||
                        dt.DayOfWeek == DayOfWeek.Saturday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.Monday | Week.Wednesday | Week.Friday | Week.Saturday | Week.Sunday:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Friday ||
                        dt.DayOfWeek == DayOfWeek.Saturday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.Monday | Week.Thursday | Week.Friday | Week.Saturday | Week.Sunday:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Thursday ||
                        dt.DayOfWeek == DayOfWeek.Friday ||
                        dt.DayOfWeek == DayOfWeek.Saturday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.Tuesday | Week.Wednesday | Week.Friday | Week.Saturday | Week.Sunday:
                    if (dt.DayOfWeek == DayOfWeek.Tuesday ||
                        dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Friday ||
                        dt.DayOfWeek == DayOfWeek.Saturday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.Tuesday | Week.Thursday | Week.Friday | Week.Saturday | Week.Sunday:
                    if (dt.DayOfWeek == DayOfWeek.Tuesday ||
                        dt.DayOfWeek == DayOfWeek.Thursday ||
                        dt.DayOfWeek == DayOfWeek.Friday ||
                        dt.DayOfWeek == DayOfWeek.Saturday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.Thursday | Week.Friday | Week.Saturday:
                    if (dt.DayOfWeek == DayOfWeek.Thursday ||
                        dt.DayOfWeek == DayOfWeek.Friday ||
                        dt.DayOfWeek == DayOfWeek.Saturday)
                        return true;
                    break;
                case Week.Monday | Week.Tuesday | Week.Weekedn:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Tuesday ||
                        dt.DayOfWeek == DayOfWeek.Saturday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.Monday | Week.Weekedn:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Saturday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.Tuesday | Week.Weekedn:
                    if (dt.DayOfWeek == DayOfWeek.Tuesday ||
                        dt.DayOfWeek == DayOfWeek.Saturday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.Wednesday | Week.Weekedn:
                    if (dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Saturday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.Thursday | Week.Weekedn:
                    if (dt.DayOfWeek == DayOfWeek.Thursday ||
                        dt.DayOfWeek == DayOfWeek.Saturday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.Monday | Week.Wednesday | Week.Weekedn:
                    if (dt.DayOfWeek == DayOfWeek.Monday ||
                        dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Saturday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.Tuesday | Week.Wednesday | Week.Weekedn:
                    if (dt.DayOfWeek == DayOfWeek.Tuesday ||
                        dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Saturday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
                case Week.Wednesday | Week.Thursday | Week.Weekedn:
                    if (dt.DayOfWeek == DayOfWeek.Wednesday ||
                        dt.DayOfWeek == DayOfWeek.Thursday ||
                        dt.DayOfWeek == DayOfWeek.Saturday ||
                        dt.DayOfWeek == DayOfWeek.Sunday)
                        return true;
                    break;
            }

            return false;
        }
    }
}
