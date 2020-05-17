﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DNN.Integration.Test.Framework.Helpers
{
    public static class HostSettingsHelper
    {
        public static string GetHostSettingValue(string hostSettingName)
        {
            var query = string.Format("SELECT TOP(1) SettingValue FROM {{objectQualifier}}HostSettings WHERE SettingName='" + hostSettingName +"';");
            return DatabaseHelper.ExecuteScalar<string>(query);
        }

        /// <summary>
        /// Adds a new entry to HostSettings table only if it does not exist and optionally clears the site cache or recycles the app.
        /// </summary>
        /// <param name="settingName">The name to add.</param>
        /// <param name="settingValue">The value associated with the name.</param>
        /// <param name="clearCache">Whether to clear the cache for the changes to take effect or not (only when record is added).</param>
        /// <param name="recycleAppIfAdded">Whether to recycle the application or no (only when record is added).
        /// If recycling done, no clear cache will be issued.</param>
        /// <returns>True if the entry is added; false otherwise.</returns>
        public static bool AddSettingIfMissing(string settingName, string settingValue, bool clearCache = true,
            bool recycleAppIfAdded = false)
        {
            var currentValue = GetHostSettingValue(settingName);
            if (!string.IsNullOrEmpty(currentValue))
                return false;

            var query = string.Format(
                @"INSERT INTO {{objectQualifier}}HostSettings (SettingName, SettingValue, SettingIsSecure, CreatedOnDate) " +
                @"VALUES(N'{0}', N'{1}', '0', {{ts: '{2:yyyy-MM-dd HH:mm.ss.fff}'}})",
                settingName.Replace("'", "''"), settingValue.Replace("'", "''"), DateTime.UtcNow);

            DatabaseHelper.ExecuteNonQuery(query);

            if (recycleAppIfAdded)
            {
                WebApiTestHelper.RecycleApplication();
            }
            else if (clearCache)
            {
                WebApiTestHelper.ClearHostCache();
            }

            return true;
        }
    }
}
