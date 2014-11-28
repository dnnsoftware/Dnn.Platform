using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace DotNetNuke.Services.Localization
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
    public static class Persian
    {
        public static CultureInfo GetPersianCultureInfo()
        {
            CultureInfo PersianCultureInfo = new CultureInfo("fa-IR");

            SetPersianDateTimeFormatInfo(PersianCultureInfo.DateTimeFormat);
            SetNumberFormatInfo(PersianCultureInfo.NumberFormat);

            System.Globalization.Calendar cal = new DotNetNuke.Services.Localization.PersianCalendar();

            FieldInfo fieldInfo = PersianCultureInfo.GetType().GetField("calendar", BindingFlags.NonPublic | BindingFlags.Instance);
            if (fieldInfo != null)
                fieldInfo.SetValue(PersianCultureInfo, cal);

            FieldInfo info = PersianCultureInfo.DateTimeFormat.GetType().GetField("calendar", BindingFlags.NonPublic | BindingFlags.Instance);
            if (info != null)
                info.SetValue(PersianCultureInfo.DateTimeFormat, cal);

            return PersianCultureInfo;
        }

        public static void SetPersianDateTimeFormatInfo(DateTimeFormatInfo PersianDateTimeFormatInfo)
        {
            PersianDateTimeFormatInfo.MonthNames = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
            PersianDateTimeFormatInfo.MonthGenitiveNames = PersianDateTimeFormatInfo.MonthNames;
            PersianDateTimeFormatInfo.AbbreviatedMonthNames = PersianDateTimeFormatInfo.MonthNames;
            PersianDateTimeFormatInfo.AbbreviatedMonthGenitiveNames = PersianDateTimeFormatInfo.MonthNames;

            PersianDateTimeFormatInfo.DayNames = new string[] { "یکشنبه", "دوشنبه", "ﺳﻪشنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه" };
            PersianDateTimeFormatInfo.AbbreviatedDayNames = new string[] { "ی", "د", "س", "چ", "پ", "ج", "ش" };
            PersianDateTimeFormatInfo.ShortestDayNames = PersianDateTimeFormatInfo.AbbreviatedDayNames;
            PersianDateTimeFormatInfo.FirstDayOfWeek = DayOfWeek.Saturday;

            PersianDateTimeFormatInfo.AMDesignator = "ق.ظ";
            PersianDateTimeFormatInfo.PMDesignator = "ب.ظ";

            PersianDateTimeFormatInfo.DateSeparator = "/";
            PersianDateTimeFormatInfo.TimeSeparator = ":";

            PersianDateTimeFormatInfo.FullDateTimePattern = "tt hh:mm:ss yyyy mmmm dd dddd";
            PersianDateTimeFormatInfo.YearMonthPattern = "yyyy, MMMM";
            PersianDateTimeFormatInfo.MonthDayPattern = "dd MMMM";

            PersianDateTimeFormatInfo.LongDatePattern = "dddd, dd MMMM,yyyy";
            PersianDateTimeFormatInfo.ShortDatePattern = "yyyy/MM/dd";

            PersianDateTimeFormatInfo.LongTimePattern = "hh:mm:ss tt";
            PersianDateTimeFormatInfo.ShortTimePattern = "hh:mm tt";
        }

        public static void SetNumberFormatInfo(NumberFormatInfo PersianNumberFormatInfo)
        {
            PersianNumberFormatInfo.NumberDecimalSeparator = "/";
            PersianNumberFormatInfo.DigitSubstitution = DigitShapes.NativeNational;
            PersianNumberFormatInfo.NumberNegativePattern = 0;
        }
    }
}
