// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components
{
    using System;

    using Dnn.Modules.BulkInstall.Components.DataAccess.DataControllers;
    using Dnn.Modules.BulkInstall.Components.DataAccess.Models;
    using Dnn.Modules.BulkInstall.Components.Exceptions;

    using DotNetNuke.Common.Utilities;

    /// <summary>The manager for <see cref="Setting"/>.</summary>
    /// <param name="dataController">The data controller.</param>
    public sealed class SettingManager(SettingDataController dataController)
    {
        private const string SettingCacheKey = "Cantarus:PolyDeploy:Setting_";

        private readonly SettingDataController dataController = dataController;

        /// <summary>Gets a setting.</summary>
        /// <param name="group">The group.</param>
        /// <param name="key">The key.</param>
        /// <returns>The setting.</returns>
        /// <exception cref="SettingNotFoundException">The setting was not found.</exception>
        public Setting GetSetting(string group, string key)
        {
            Setting setting;

            // Attempt to retrieve from cache.
            string cacheKey = BuildCacheKey(group, key);

            setting = DataCache.GetCache<Setting>(cacheKey);

            // Was in cache?
            if (setting == null)
            {
                // Not in cache, go to database.
                setting = this.dataController.GetSetting(group, key);

                // Was in db?
                if (setting != null)
                {
                    // Cache it for 15 minutes.
                    DataCache.SetCache(cacheKey, setting, TimeSpan.FromMinutes(15));
                }
                else
                {
                    throw SettingNotFoundException.Create(group, key);
                }
            }

            return setting;
        }

        /// <summary>Creates or updates a setting.</summary>
        /// <param name="group">The setting group.</param>
        /// <param name="key">The setting key.</param>
        /// <param name="value">The setting value.</param>
        public void SetSetting(string group, string key, string value)
        {
            // Retrieve setting.
            Setting setting = this.dataController.GetSetting(group, key);

            // Does it already exist?
            if (setting == null)
            {
                // No, create it.
                setting = new Setting()
                {
                    Group = group,
                    Key = key,
                    Value = value,
                };

                this.dataController.Create(setting);
            }
            else
            {
                // Yes, Update it.
                setting.Value = value;

                this.dataController.Update(setting);
            }

            // Clear cache.
            DataCache.RemoveCache(BuildCacheKey(group, key));
        }

        private static string BuildCacheKey(string group, string key)
        {
            return $"{SettingCacheKey}{group}_{key}";
        }
    }
}
