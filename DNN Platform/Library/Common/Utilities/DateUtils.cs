// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Common.Utilities
{
    using System;
    using System.Globalization;

    using DotNetNuke.Data;
    using DotNetNuke.Internal.SourceGenerators;
    using DotNetNuke.Services.Localization;

    /// <summary>Provides utility methods to work with Dates.</summary>
    public partial class DateUtils
    {
        private static DateTime lastUpdateUtc = DateTime.MinValue;
        private static DateTime lastUpdateLocal = DateTime.MinValue;

        private static TimeSpan driftUtc = TimeSpan.MinValue;

        private static TimeSpan driftLocal = TimeSpan.MinValue;

        /// <summary>Gets the database time.</summary>
        /// <returns>Date/time of the database in UTC.</returns>
        [DnnDeprecated(9, 1, 0, "Replaced by GetDatabaseUtcTime")]
        public static partial DateTime GetDatabaseTime()
        {
            return GetDatabaseUtcTime();
        }

        /// <summary>Gets DateTime Offset of current DB.</summary>
        /// <returns>DateTimeOffset object.</returns>
        public static TimeZoneInfo GetDatabaseDateTimeOffset()
        {
            var dateTimeOffset = DataProvider.Instance().GetDatabaseTimeOffset();
            var offset = dateTimeOffset.Offset;
            var id = $"UTC {offset}";
            return TimeZoneInfo.CreateCustomTimeZone(id, offset, id, id);
        }

        /// <summary>Gets the database server's time in UTC.</summary>
        /// <returns>Date/time of the database in UTC.</returns>
        public static DateTime GetDatabaseUtcTime()
        {
            try
            {
                // Also We check that drift is not the initial value, and it is not out of the maximum UTC offset
                if (DateTime.UtcNow >= lastUpdateUtc + TimeSpan.FromMinutes(15) || !(TimeSpan.FromHours(-26) <= driftUtc && driftUtc <= TimeSpan.FromHours(26)) || driftUtc == TimeSpan.MinValue)
                {
                    lastUpdateUtc = DateTime.UtcNow;
                    driftUtc = DateTime.UtcNow - DataProvider.Instance().GetDatabaseTimeUtc();
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                lastUpdateUtc = DateTime.UtcNow;
                driftUtc = DateTime.UtcNow - DataProvider.Instance().GetDatabaseTimeUtc();
            }

            return DateTime.UtcNow + driftUtc;
        }

        /// <summary>Gets the database server's local time of the DB server and not the web server's local time.</summary>
        /// <returns>Date/time of the database in UTC.</returns>
        public static DateTime GetDatabaseLocalTime()
        {
            try
            {
                // Also We check that drift is not the initial value and it is not out of the maximum UTC offset
                if (DateTime.UtcNow >= lastUpdateLocal + TimeSpan.FromMinutes(15) || !(TimeSpan.FromHours(-26) <= driftLocal && driftLocal <= TimeSpan.FromHours(26)) || driftLocal == TimeSpan.MinValue)
                {
                    lastUpdateLocal = DateTime.Now;
                    driftLocal = DateTime.Now - DataProvider.Instance().GetDatabaseTime();
                }
            }
            catch (ArgumentOutOfRangeException)
            {
                lastUpdateLocal = DateTime.Now;
                driftLocal = DateTime.Now - DataProvider.Instance().GetDatabaseTime();
            }

            return DateTime.Now + driftLocal;
        }

        /// <summary>Returns a string with the pretty printed amount of time since the specified date.</summary>
        /// <param name="date">DateTime in UTC.</param>
        /// <returns>String representing the required date for display.</returns>
        public static string CalculateDateForDisplay(DateTime date)
        {
            var utcTimeDifference = GetDatabaseUtcTime() - date;

            if (utcTimeDifference.TotalSeconds < 60)
            {
                return string.Format(CultureInfo.CurrentCulture, Localization.GetString("SecondsAgo"), (int)utcTimeDifference.TotalSeconds);
            }

            if (utcTimeDifference.TotalMinutes < 60)
            {
                if (utcTimeDifference.TotalMinutes < 2)
                {
                    return string.Format(CultureInfo.CurrentCulture, Localization.GetString("MinuteAgo"), (int)utcTimeDifference.TotalMinutes);
                }

                return string.Format(CultureInfo.CurrentCulture, Localization.GetString("MinutesAgo"), (int)utcTimeDifference.TotalMinutes);
            }

            if (utcTimeDifference.TotalHours < 24)
            {
                if (utcTimeDifference.TotalHours < 2)
                {
                    return string.Format(CultureInfo.CurrentCulture, Localization.GetString("HourAgo"), (int)utcTimeDifference.TotalHours);
                }

                return string.Format(CultureInfo.CurrentCulture, Localization.GetString("HoursAgo"), (int)utcTimeDifference.TotalHours);
            }

            if (utcTimeDifference.TotalDays < 7)
            {
                if (utcTimeDifference.TotalDays < 2)
                {
                    return string.Format(CultureInfo.CurrentCulture, Localization.GetString("DayAgo"), (int)utcTimeDifference.TotalDays);
                }

                return string.Format(CultureInfo.CurrentCulture, Localization.GetString("DaysAgo"), (int)utcTimeDifference.TotalDays);
            }

            if (utcTimeDifference.TotalDays < 30)
            {
                if (utcTimeDifference.TotalDays < 14)
                {
                    return string.Format(CultureInfo.CurrentCulture, Localization.GetString("WeekAgo"), (int)utcTimeDifference.TotalDays / 7);
                }

                return string.Format(CultureInfo.CurrentCulture, Localization.GetString("WeeksAgo"), (int)utcTimeDifference.TotalDays / 7);
            }

            if (utcTimeDifference.TotalDays < 180)
            {
                if (utcTimeDifference.TotalDays < 60)
                {
                    return string.Format(CultureInfo.CurrentCulture, Localization.GetString("MonthAgo"), (int)utcTimeDifference.TotalDays / 30);
                }

                return string.Format(CultureInfo.CurrentCulture, Localization.GetString("MonthsAgo"), (int)utcTimeDifference.TotalDays / 30);
            }

            // anything else (this is the only time we have to personalize it to the user)
            return date.ToShortDateString();
        }
    }
}
