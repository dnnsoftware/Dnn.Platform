using System;

namespace DotNetNuke.Services.Localization.Persian
{
    public class PersianCalendar : System.Globalization.PersianCalendar
    {
        public override int GetYear(DateTime time)
        {
            try { return base.GetYear(time); }
            catch { }
            return time.Year;
        }

        public override int GetMonth(DateTime time)
        {
            try { return base.GetMonth(time); }
            catch { }
            return time.Month;
        }

        public override int GetDayOfMonth(DateTime time)
        {
            try { return base.GetDayOfMonth(time); }
            catch { }
            return time.Day;
        }

        public override int GetDayOfYear(DateTime time)
        {
            try { return base.GetDayOfYear(time); }
            catch { }
            return time.DayOfYear;
        }

        public override DayOfWeek GetDayOfWeek(DateTime time)
        {
            try { return base.GetDayOfWeek(time); }
            catch { }
            return time.DayOfWeek;
        }
    }
}
