using Cantarus.Modules.PolyDeploy.Components.DataAccess.DataControllers;
using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using Cantarus.Modules.PolyDeploy.Components.Exceptions;
using DotNetNuke.Common.Utilities;
using System;

namespace Cantarus.Modules.PolyDeploy.Components
{
    internal class SettingManager
    {
        private const string SettingCacheKey = "Cantarus:PolyDeploy:Setting_";

        private static SettingDataController SettingDC = new SettingDataController();

        public static Setting GetSetting(string group, string key)
        {
            Setting setting;

            // Attempt to retrieve from cache.
            string cacheKey = BuildCacheKey(group, key);

            setting = DataCache.GetCache<Setting>(cacheKey);

            // Was in cache?
            if (setting == null)
            {
                // Not in cache, go to database.
                setting = SettingDC.GetSetting(group, key);

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

        public static void SetSetting(string group, string key, string value)
        {
            // Retrieve setting.
            Setting setting = SettingDC.GetSetting(group, key);

            // Does it already exist?
            if (setting == null)
            {
                // No, create it.
                setting = new Setting()
                {
                    Group = group,
                    Key = key,
                    Value = value
                };

                SettingDC.Create(setting);
            }
            else
            {
                // Yes, Update it.
                setting.Value = value;

                SettingDC.Update(setting);
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
