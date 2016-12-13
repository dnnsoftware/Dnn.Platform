#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

#region Usings
using System;

using DotNetNuke.Data;
using DotNetNuke.Services.Localization;

#endregion

namespace DotNetNuke.Common.Utilities
{
    /// <summary>
    /// Provides utility methods to work with Dates
    /// </summary>
    public class DateUtils
    {

        private static DateTime _lastUpdate = DateTime.MinValue;

        private static TimeSpan _drift = TimeSpan.MinValue;

        /// <summary>
        /// Gets the database time.
        /// </summary>
        /// <returns>Date/time of the database in UTC</returns>
        public static DateTime GetDatabaseTime()
        {
            DateTime result;
            try
            {
                // Also We check that drift is not the initial value and it is not out of the maximum UTC offset
                if (DateTime.UtcNow >= _lastUpdate + TimeSpan.FromMinutes(5) || !(TimeSpan.FromHours(-26) <= _drift && _drift <= TimeSpan.FromHours(26)) || _drift == TimeSpan.MinValue)
                {
                    _lastUpdate = DateTime.UtcNow;
                    _drift = DateTime.UtcNow - DataProvider.Instance().GetDatabaseTimeUtc();
                }

                result = DateTime.UtcNow + _drift;
            }
            catch (ArgumentOutOfRangeException)
            {
                _lastUpdate = DateTime.UtcNow;
                _drift = DateTime.UtcNow - DataProvider.Instance().GetDatabaseTimeUtc();
                result = DateTime.UtcNow + _drift;
            }

            return result;
        }

        /// <summary>
        /// Returns a string with the pretty printed amount of time since the specified date.
        /// </summary>
        /// <param name="date">DateTime in UTC</param>
        /// <returns>String representing the required date for display</returns>
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
