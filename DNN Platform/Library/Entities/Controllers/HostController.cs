// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Settings;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Web.Client;

    /// <inheritdoc cref="IHostSettingsService" />
    public partial class HostController : IHostSettingsService
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(HostController));
        private static PerformanceSettings performanceSettings = PerformanceSettings.ModerateCaching;

        /// <inheritdoc cref="IHostSettingsService.GetBoolean(string)" />
        public bool GetBoolean(string key)
        {
            return this.GetBoolean(key, Null.NullBoolean);
        }

        /// <inheritdoc cref="IHostSettingsService.GetBoolean(string,bool)" />
        public bool GetBoolean(string key, bool defaultValue)
        {
            Requires.NotNullOrEmpty("key", key);

            bool retValue = false;
            try
            {
                string settingValue = string.Empty;
                var settings = ((IHostSettingsService)this).GetSettings();
                if (settings.TryGetValue(key, out var setting))
                {
                    settingValue = setting.Value;
                }

                if (string.IsNullOrEmpty(settingValue))
                {
                    retValue = defaultValue;
                }
                else
                {
                    retValue = settingValue.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) || settingValue.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase);
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);

                // we just want to trap the error as we may not be installed so there will be no Settings
            }

            return retValue;
        }

        /// <inheritdoc cref="IHostSettingsService.GetDouble(string)" />
        public double GetDouble(string key)
        {
            return this.GetDouble(key, Null.NullDouble);
        }

        /// <inheritdoc cref="IHostSettingsService.GetDouble(string,double)" />
        public double GetDouble(string key, double defaultValue)
        {
            Requires.NotNullOrEmpty("key", key);

            var settings = ((IHostSettingsService)this).GetSettings();
            if (!settings.TryGetValue(key, out var value) || !double.TryParse(value.Value, out var retValue))
            {
                retValue = defaultValue;
            }

            return retValue;
        }

        /// <inheritdoc cref="IHostSettingsService.GetInteger(string)" />
        public int GetInteger(string key)
        {
            return this.GetInteger(key, Null.NullInteger);
        }

        /// <inheritdoc cref="IHostSettingsService.GetInteger(string,int)" />
        public int GetInteger(string key, int defaultValue)
        {
            Requires.NotNullOrEmpty("key", key);

            var settings = ((IHostSettingsService)this).GetSettings();
            if (!settings.TryGetValue(key, out var value) || !int.TryParse(value.Value, out var retValue))
            {
                retValue = defaultValue;
            }

            return retValue;
        }

        /// <inheritdoc/>
        IDictionary<string, IConfigurationSetting> IHostSettingsService.GetSettings()
        {
            return DataCache.GetCachedData<Dictionary<string, IConfigurationSetting>>(
                performanceSettings,
                new CacheItemArgs(
                    DataCache.HostSettingsCacheKey,
                    DataCache.HostSettingsCacheTimeOut,
                    DataCache.HostSettingsCachePriority),
                GetSettingsDictionaryCallBack,
                true);
        }

        /// <inheritdoc/>
        IDictionary<string, string> IHostSettingsService.GetSettingsDictionary()
        {
            return ((IHostSettingsService)this).GetSettings()
                .ToDictionary(c => c.Key, c => c.Value.Value);
        }

        /// <inheritdoc cref="IHostSettingsService.GetEncryptedString" />
        public string GetEncryptedString(string key, string passPhrase)
        {
            Requires.NotNullOrEmpty("key", key);
            Requires.NotNullOrEmpty("passPhrase", passPhrase);
            var cipherText = this.GetString(key);
            return Security.FIPSCompliant.DecryptAES(cipherText, passPhrase, HostSettings.GetHostGuid(this));
        }

        /// <inheritdoc cref="IHostSettingsService.GetString(string)" />
        public string GetString(string key)
        {
            return this.GetString(key, string.Empty);
        }

        /// <inheritdoc cref="IHostSettingsService.GetString(string,string)" />
        public string GetString(string key, string defaultValue)
        {
            Requires.NotNullOrEmpty("key", key);

            var settings = ((IHostSettingsService)this).GetSettings();
            if (!settings.TryGetValue(key, out var value) || value.Value == null)
            {
                return defaultValue;
            }

            return value.Value;
        }

        /// <inheritdoc/>
        void IHostSettingsService.Update(IDictionary<string, string> settings)
        {
            foreach (var settingKvp in settings)
            {
                this.Update(settingKvp.Key, settingKvp.Value, false);
            }

            DataCache.ClearHostCache(false);
        }

        /// <inheritdoc/>
        public void Update(IConfigurationSetting config)
        {
            this.Update(config, true);
        }

        /// <inheritdoc/>
        public void Update(IConfigurationSetting config, bool clearCache)
        {
            try
            {
                var dbProvider = DataProvider.Instance();
                var userId = UserController.Instance.GetCurrentUserInfo().UserID;
                var portalSettings = PortalController.Instance.GetCurrentSettings();
                var settings = GetSettingsFromDatabase();
                if (settings.ContainsKey(config.Key))
                {
                    if (settings.TryGetValue(config.Key, out var currentConfig) && currentConfig?.Value != config.Value)
                    {
                        dbProvider.UpdateHostSetting(config.Key, config.Value, config.IsSecure, userId);
                        EventLogController.Instance.AddLog(
                            config.Key,
                            config.Value,
                            portalSettings,
                            userId,
                            EventLogController.EventLogType.HOST_SETTING_UPDATED);
                    }
                }
                else
                {
                    dbProvider.UpdateHostSetting(config.Key, config.Value, config.IsSecure, userId);
                    EventLogController.Instance.AddLog(
                        config.Key,
                        config.Value,
                        portalSettings,
                        userId,
                        EventLogController.EventLogType.HOST_SETTING_CREATED);
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            if (clearCache)
            {
                DataCache.RemoveCache(DataCache.HostSettingsCacheKey);
            }
        }

        /// <inheritdoc cref="IHostSettingsService.Update(string,string,bool)" />
        public void Update(string key, string value, bool clearCache)
        {
            ((IHostSettingsService)this).Update(new ConfigurationSetting { Key = key, Value = value }, clearCache);
        }

        /// <inheritdoc cref="IHostSettingsService.Update(string,string)" />
        public void Update(string key, string value)
        {
            this.Update(key, value, true);
        }

        /// <inheritdoc cref="IHostSettingsService.UpdateEncryptedString" />
        public void UpdateEncryptedString(string key, string value, string passPhrase)
        {
            Requires.NotNullOrEmpty("key", key);
            Requires.PropertyNotNull("value", value);
            Requires.NotNullOrEmpty("passPhrase", passPhrase);
            var cipherText = Security.FIPSCompliant.EncryptAES(value, passPhrase, HostSettings.GetHostGuid(this));
            this.Update(key, cipherText);
        }

        /// <inheritdoc cref="IHostSettingsService.IncrementCrmVersion" />
        public void IncrementCrmVersion(bool includeOverridingPortals)
        {
            var currentVersion = HostSettings.GetCrmVersion(this);
            var newVersion = currentVersion + 1;
            this.Update(ClientResourceSettings.VersionKey, newVersion.ToString(CultureInfo.InvariantCulture), true);

            if (includeOverridingPortals)
            {
                PortalController.IncrementOverridingPortalsCrmVersion();
            }
        }

        /// <summary>Gets all settings from the database.</summary>
        /// <returns><see cref="Dictionary{TKey, TValue}"/>.</returns>
        private static Dictionary<string, IConfigurationSetting> GetSettingsFromDatabase()
        {
            var dicSettings = new Dictionary<string, IConfigurationSetting>();
            IDataReader dr = null;
            try
            {
                dr = DataProvider.Instance().GetHostSettings();
                while (dr.Read())
                {
                    var key = dr.GetString(0);
                    var config = new ConfigurationSetting
                    {
                        Key = key,
                        IsSecure = Convert.ToBoolean(dr[2]),
                        Value = dr.IsDBNull(1) ? string.Empty : dr.GetString(1),
                    };

                    dicSettings.Add(key, config);
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            if (!dicSettings.TryGetValue("PerformanceSetting", out var setting) || !Enum.TryParse(setting.Value, ignoreCase: true, out performanceSettings))
            {
                performanceSettings = PerformanceSettings.ModerateCaching;
            }

            return dicSettings;
        }

        private static object GetSettingsDictionaryCallBack(CacheItemArgs cacheItemArgs)
        {
            return GetSettingsFromDatabase();
        }
    }
}
