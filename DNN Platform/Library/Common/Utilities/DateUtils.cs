// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;

    using DotNetNuke.Data;
    using DotNetNuke.Services.Localization;

    /// <summary>
    /// Provides utility methods to work with Dates.
    /// </summary>
    public class DateUtils
    {
        private static DateTime _lastUpdateUtc = DateTime.MinValue;
        private static DateTime _lastUpdateLocal = DateTime.MinValue;

        private static TimeSpan _driftUtc = TimeSpan.MinValue;

        private static TimeSpan _driftLocal = TimeSpan.MinValue;

        /// <summary>
        /// Gets the database time.
        /// </summary>
        /// <returns>Date/time of the database in UTC.</returns>
        [Obsolete("Deprecated in DNN 9.1.0.  Replaced by GetDatabaseUtcTime. Scheduled removal in v11.0.0.")]
        public static DateTime GetDatabaseTime()
        {
            return GetDatabaseUtcTime();
        }

        /// <summary>
        /// Gets DateTime Offset of current DB.
        /// </summary>
        /// <returns>DateTimeOffset object.</returns>
        public static TimeZoneInfo GetDatabaseDateTimeOffset()
        {
            var dateTimeOffset = DataProvider.Instance().GetDatabaseTimeOffset();
            var offset = dateTimeOffset.Offset;
            var id = string.Format("UTC {0}", offset.ToString());
            return TimeZoneInfo.CreateCustomTimeZone(id, offset, id, id);
        }

        /// <summary>
        /// Gets the database server's time in UTC.
        /// </summary>
        /// <returns>Date/time of the database in UTC.</returns>
        public static DateTime GetDatabaseUtcTime()
        {
            try
            {
                // Also We check that drift is not the initial value and it is not out of the maximum UTC offset
                if (DateTime.UtcNow >= _lastUpdateUtc + TimeSpan.FromMinutes(15) || !(TimeSpan.FromHours(-26) <= _driftUtc && _driftUtc <= TimeSpan.FromHours(26)) || _driftUtc == TimeSpan.MinValue)
                {
                    _lastUpdateUtc = DateTime.UtcNow;
                    _driftUtc = DateTime.UtcNow - DataProvider.Instance().GetDatabaseTimeUtc();
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                _lastUpdateUtc = DateTime.UtcNow;
                _driftUtc = DateTime.UtcNow - DataProvider.Instance().GetDatabaseTimeUtc();
            }

            return DateTime.UtcNow + _driftUtc;
        }

        /// <summary>
        /// Gets the database server's local time of the DB server and not the web server's local time.
        /// </summary>
        /// <returns>Date/time of the database in UTC.</returns>
        public static DateTime GetDatabaseLocalTime()
        {
            try
            {
                // Also We check that drift is not the initial value and it is not out of the maximum UTC offset
                if (DateTime.UtcNow >= _lastUpdateLocal + TimeSpan.FromMinutes(15) || !(TimeSpan.FromHours(-26) <= _driftLocal && _driftLocal <= TimeSpan.FromHours(26)) || _driftLocal == TimeSpan.MinValue)
                {
                    _lastUpdateLocal = DateTime.Now;
                    _driftLocal = DateTime.Now - DataProvider.Instance().GetDatabaseTime();
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                _lastUpdateLocal = DateTime.Now;
                _driftLocal = DateTime.Now - DataProvider.Instance().GetDatabaseTime();
            }

            return DateTime.Now + _driftLocal;
        }

        /// <summary>
        /// Returns a string with the pretty printed amount of time since the specified date.
        /// </summary>
        /// <param name="date">DateTime in UTC.</param>
        /// <returns>String representing the required date for display.</returns>
        public static string CalculateDateForDisplay(DateTime date)
        {
            var utcTimeDifference = GetDatabaseUtcTime() - date;

            if (utcTimeDifference.TotalSeconds < 60)
            {
                return string.Format(Localization.GetString("SecondsAgo"), (int)utcTimeDifference.TotalSeconds);
            }

            if (utcTimeDifference.TotalMinutes < 60)
            {
                if (utcTimeDifference.TotalMinutes < 2)
                {
                    return string.Format(Localization.GetString("MinuteAgo"), (int)utcTimeDifference.TotalMinutes);
                }

                return string.Format(Localization.GetString("MinutesAgo"), (int)utcTimeDifference.TotalMinutes);
            }

            if (utcTimeDifference.TotalHours < 24)
            {
                if (utcTimeDifference.TotalHours < 2)
                {
                    return string.Format(Localization.GetString("HourAgo"), (int)utcTimeDifference.TotalHours);
                }

                return string.Format(Localization.GetString("HoursAgo"), (int)utcTimeDifference.TotalHours);
            }

            if (utcTimeDifference.TotalDays < 7)
            {
                if (utcTimeDifference.TotalDays < 2)
                {
                    return string.Format(Localization.GetString("DayAgo"), (int)utcTimeDifference.TotalDays);
                }

                return string.Format(Localization.GetString("DaysAgo"), (int)utcTimeDifference.TotalDays);
            }

            if (utcTimeDifference.TotalDays < 30)
            {
                if (utcTimeDifference.TotalDays < 14)
                {
                    return string.Format(Localization.GetString("WeekAgo"), (int)utcTimeDifference.TotalDays / 7);
                }

                return string.Format(Localization.GetString("WeeksAgo"), (int)utcTimeDifference.TotalDays / 7);
            }

            if (utcTimeDifference.TotalDays < 180)
            {
                if (utcTimeDifference.TotalDays < 60)
                {
                    return string.Format(Localization.GetString("MonthAgo"), (int)utcTimeDifference.TotalDays / 30);
                }

                return string.Format(Localization.GetString("MonthsAgo"), (int)utcTimeDifference.TotalDays / 30);
            }

            // anything else (this is the only time we have to personalize it to the user)
            return date.ToShortDateString();
        }
    }
}
