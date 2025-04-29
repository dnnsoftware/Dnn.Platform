// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Controllers;

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
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Web.Client;

/// <inheritdoc/>
public partial class HostController : IHostSettingsService
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(HostController));

    /// <summary>Initializes a new instance of the <see cref="HostController"/> class.</summary>
    public HostController()
    {
    }

    /// <inheritdoc/>
    public bool GetBoolean(string key)
    {
        return this.GetBoolean(key, Null.NullBoolean);
    }

    /// <inheritdoc/>
    public bool GetBoolean(string key, bool defaultValue)
    {
        Requires.NotNullOrEmpty("key", key);

        bool retValue = false;
        try
        {
            string setting = string.Empty;
            if (this.GetSettings().ContainsKey(key))
            {
                setting = this.GetSettings()[key].Value;
            }

            if (string.IsNullOrEmpty(setting))
            {
                retValue = defaultValue;
            }
            else
            {
                retValue = setting.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) || setting.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase);
            }
        }
        catch (Exception exc)
        {
            Logger.Error(exc);

            // we just want to trap the error as we may not be installed so there will be no Settings
        }

        return retValue;
    }

    /// <inheritdoc/>
    public double GetDouble(string key)
    {
        return this.GetDouble(key, Null.NullDouble);
    }

    /// <inheritdoc/>
    public double GetDouble(string key, double defaultValue)
    {
        Requires.NotNullOrEmpty("key", key);

        double retValue;

        if (!this.GetSettings().ContainsKey(key) || !double.TryParse(this.GetSettings()[key].Value, out retValue))
        {
            retValue = defaultValue;
        }

        return retValue;
    }

    /// <inheritdoc/>
    public int GetInteger(string key)
    {
        return this.GetInteger(key, Null.NullInteger);
    }

    /// <inheritdoc/>
    public int GetInteger(string key, int defaultValue)
    {
        Requires.NotNullOrEmpty("key", key);

        int retValue;

        if (!this.GetSettings().ContainsKey(key) || !int.TryParse(this.GetSettings()[key].Value, out retValue))
        {
            retValue = defaultValue;
        }

        return retValue;
    }

    /// <inheritdoc/>
    IDictionary<string, IConfigurationSetting> IHostSettingsService.GetSettings()
    {
        return CBO.GetCachedObject<Dictionary<string, IConfigurationSetting>>(
            new CacheItemArgs(
                DataCache.HostSettingsCacheKey,
                DataCache.HostSettingsCacheTimeOut,
                DataCache.HostSettingsCachePriority),
            this.GetSettingsDictionaryCallBack,
            true);
    }

    /// <inheritdoc/>
    IDictionary<string, string> IHostSettingsService.GetSettingsDictionary()
    {
        return ((IHostSettingsService)this).GetSettings()
            .ToDictionary(c => c.Key, c => c.Value.Value);
    }

    /// <inheritdoc/>
    public string GetEncryptedString(string key, string passPhrase)
    {
        Requires.NotNullOrEmpty("key", key);
        Requires.NotNullOrEmpty("passPhrase", passPhrase);
        var cipherText = this.GetString(key);
        return Security.FIPSCompliant.DecryptAES(cipherText, passPhrase, Entities.Host.Host.GUID);
    }

    /// <inheritdoc/>
    public string GetString(string key)
    {
        return this.GetString(key, string.Empty);
    }

    /// <inheritdoc/>
    public string GetString(string key, string defaultValue)
    {
        Requires.NotNullOrEmpty("key", key);

        if (!this.GetSettings().ContainsKey(key) || this.GetSettings()[key].Value == null)
        {
            return defaultValue;
        }

        return this.GetSettings()[key].Value;
    }

    /// <inheritdoc/>
    void IHostSettingsService.Update(IDictionary<string, string> settings)
    {
        foreach (KeyValuePair<string, string> settingKvp in settings)
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
            var settings = this.GetSettingsFromDatabase();
            if (settings.ContainsKey(config.Key))
            {
                IConfigurationSetting currentconfig;
                settings.TryGetValue(config.Key, out currentconfig);
                if (currentconfig != null && currentconfig.Value != config.Value)
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

    /// <inheritdoc/>
    public void Update(string key, string value, bool clearCache)
    {
        this.Update(new ConfigurationSetting { Key = key, Value = value }, clearCache);
    }

    /// <inheritdoc/>
    public void Update(string key, string value)
    {
        this.Update(key, value, true);
    }

    /// <inheritdoc/>
    public void UpdateEncryptedString(string key, string value, string passPhrase)
    {
        Requires.NotNullOrEmpty("key", key);
        Requires.PropertyNotNull("value", value);
        Requires.NotNullOrEmpty("passPhrase", passPhrase);
        var cipherText = Security.FIPSCompliant.EncryptAES(value, passPhrase, Entities.Host.Host.GUID);
        this.Update(key, cipherText);
    }

    /// <inheritdoc/>
    public void IncrementCrmVersion(bool includeOverridingPortals)
    {
        var currentVersion = Host.Host.CrmVersion;
        var newVersion = currentVersion + 1;
        this.Update(ClientResourceSettings.VersionKey, newVersion.ToString(CultureInfo.InvariantCulture), true);

        if (includeOverridingPortals)
        {
            PortalController.IncrementOverridingPortalsCrmVersion();
        }
    }

    /// <summary>Gets all settings from the databse.</summary>
    /// <returns><see cref="Dictionary{TKey, TValue}"/>.</returns>
    private Dictionary<string, IConfigurationSetting> GetSettingsFromDatabase()
    {
        var dicSettings = new Dictionary<string, IConfigurationSetting>();
        IDataReader dr = null;
        try
        {
            dr = DataProvider.Instance().GetHostSettings();
            while (dr.Read())
            {
                string key = dr.GetString(0);
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

        return dicSettings;
    }

    private object GetSettingsDictionaryCallBack(CacheItemArgs cacheItemArgs)
    {
        return this.GetSettingsFromDatabase();
    }
}
