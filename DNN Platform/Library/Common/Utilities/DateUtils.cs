using System;

using DotNetNuke.Services.Localization;

namespace DotNetNuke.Common.Utilities
{
    /// <summary>
    /// Provides utility methods to work with Dates
    /// </summary>
    public class DateUtils
    {

        private static DateTime _lastUpdate = DateTime.MinValue;

        private static TimeSpan _drift = TimeSpan.MinValue;

        private static DateTime GetDatabaseTime()
        {
            if (DateTime.UtcNow >= _lastUpdate + TimeSpan.FromMinutes(5))
            {
                _lastUpdate = DateTime.UtcNow;
                _drift = DateTime.UtcNow - Services.SystemDateTime.SystemDateTime.GetCurrentTimeUtc();
            }

            return DateTime.UtcNow + _drift;
        }

        /// <summary>
        /// Returns a string with the pretty printed amount of time since the specified date.
        /// </summary>
        /// <param name="date">DateTime in Utc</param>
        public static string CalculateDateForDisplay(DateTime date)
        {
            var utcTimeDifference = GetDatabaseTime() - date;

            if (utcTimeDifference.TotalSeconds < 60)
            {
                return String.Format(Localization.GetString("SecondsAgo"), (int) utcTimeDifference.TotalSeconds);
            }

            if (utcTimeDifference.TotalMinutes < 60)
            {
                if (utcTimeDifference.TotalMinutes < 2)
                {
                    return String.Format(Localization.GetString("MinuteAgo"), (int) utcTimeDifference.TotalMinutes);
                }

                return String.Format(Localization.GetString("MinutesAgo"), (int)utcTimeDifference.TotalMinutes);
            }

            if (utcTimeDifference.TotalHours < 24)
            {
                if (utcTimeDifference.TotalHours < 2)
                {
                    return String.Format(Localization.GetString("HourAgo"), (int)utcTimeDifference.TotalHours);
                }

                return String.Format(Localization.GetString("HoursAgo"), (int)utcTimeDifference.TotalHours);
            }

            if (utcTimeDifference.TotalDays < 7)
            {
                if (utcTimeDifference.TotalDays < 2)
                {
                    return String.Format(Localization.GetString("DayAgo"), (int)utcTimeDifference.TotalDays);
                }

                return String.Format(Localization.GetString("DaysAgo"), (int)utcTimeDifference.TotalDays);
            }

            if (utcTimeDifference.TotalDays < 30)
            {
                if (utcTimeDifference.TotalDays < 14)
                {
                    return String.Format(Localization.GetString("WeekAgo"), (int)utcTimeDifference.TotalDays / 7);
                }

                return String.Format(Localization.GetString("WeeksAgo"), (int)utcTimeDifference.TotalDays / 7);
            }

            if (utcTimeDifference.TotalDays < 180)
            {
                if (utcTimeDifference.TotalDays < 60)
                {
                    return String.Format(Localization.GetString("MonthAgo"), (int)utcTimeDifference.TotalDays / 30);
                }

                return String.Format(Localization.GetString("MonthsAgo"), (int)utcTimeDifference.TotalDays / 30);
            }

            // anything else (this is the only time we have to personalize it to the user)
            return date.ToShortDateString();
        }
    }
}
