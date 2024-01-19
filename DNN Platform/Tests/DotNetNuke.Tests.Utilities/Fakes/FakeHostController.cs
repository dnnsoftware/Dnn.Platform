// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities.Fakes;

using System;
using System.Collections.Generic;
using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Settings;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Controllers;

public class FakeHostController : IHostController, IHostSettingsService
{
    private readonly IReadOnlyDictionary<string, IConfigurationSetting> settings;

    public FakeHostController(IReadOnlyDictionary<string, IConfigurationSetting> settings)
    {
        this.settings = settings;
    }

    public bool GetBoolean(string key) => this.GetBoolean(key, Null.NullBoolean);

    public bool GetBoolean(string key, bool defaultValue)
    {
        if (this.settings.TryGetValue(key, out var setting))
        {
            return setting.Value.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) || setting.Value.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase);
        }

        return defaultValue;
    }

    double IHostSettingsService.GetDouble(string key)
    {
        throw new System.NotImplementedException();
    }

    double IHostSettingsService.GetDouble(string key, double defaultValue)
    {
        throw new System.NotImplementedException();
    }

    string IHostSettingsService.GetEncryptedString(string key, string passPhrase)
    {
        throw new System.NotImplementedException();
    }

    int IHostSettingsService.GetInteger(string key)
    {
        throw new System.NotImplementedException();
    }

    int IHostSettingsService.GetInteger(string key, int defaultValue)
    {
        throw new System.NotImplementedException();
    }

    IDictionary<string, IConfigurationSetting> IHostSettingsService.GetSettings()
    {
        throw new System.NotImplementedException();
    }

    IDictionary<string, string> IHostSettingsService.GetSettingsDictionary()
    {
        return this.GetSettingsDictionary();
    }

    string IHostSettingsService.GetString(string key)
    {
        throw new System.NotImplementedException();
    }

    string IHostSettingsService.GetString(string key, string defaultValue)
    {
        throw new System.NotImplementedException();
    }

    void IHostSettingsService.IncrementCrmVersion(bool includeOverridingPortals)
    {
        throw new System.NotImplementedException();
    }

    public void Update(IConfigurationSetting config)
    {
        throw new System.NotImplementedException();
    }

    public void Update(IConfigurationSetting config, bool clearCache)
    {
        throw new System.NotImplementedException();
    }

    public void Update(IDictionary<string, string> settings)
    {
        throw new System.NotImplementedException();
    }

    void IHostSettingsService.Update(string key, string value)
    {
        throw new System.NotImplementedException();
    }

    void IHostSettingsService.Update(string key, string value, bool clearCache)
    {
        throw new System.NotImplementedException();
    }

    void IHostSettingsService.UpdateEncryptedString(string key, string value, string passPhrase)
    {
        throw new System.NotImplementedException();
    }

    double IHostController.GetDouble(string key, double defaultValue)
    {
        throw new System.NotImplementedException();
    }

    double IHostController.GetDouble(string key)
    {
        throw new System.NotImplementedException();
    }

    int IHostController.GetInteger(string key)
    {
        throw new System.NotImplementedException();
    }

    int IHostController.GetInteger(string key, int defaultValue)
    {
        throw new System.NotImplementedException();
    }

    Dictionary<string, ConfigurationSetting> IHostController.GetSettings()
    {
        throw new System.NotImplementedException();
    }

    public Dictionary<string, string> GetSettingsDictionary()
    {
        throw new System.NotImplementedException();
    }

    string IHostController.GetEncryptedString(string key, string passPhrase)
    {
        throw new System.NotImplementedException();
    }

    string IHostController.GetString(string key)
    {
        throw new System.NotImplementedException();
    }

    string IHostController.GetString(string key, string defaultValue)
    {
        throw new System.NotImplementedException();
    }

    public void Update(Dictionary<string, string> settings)
    {
        throw new System.NotImplementedException();
    }

    public void Update(ConfigurationSetting config)
    {
        throw new System.NotImplementedException();
    }

    public void Update(ConfigurationSetting config, bool clearCache)
    {
        throw new System.NotImplementedException();
    }

    void IHostController.Update(string key, string value, bool clearCache)
    {
        throw new System.NotImplementedException();
    }

    void IHostController.Update(string key, string value)
    {
        throw new System.NotImplementedException();
    }

    void IHostController.UpdateEncryptedString(string key, string value, string passPhrase)
    {
        throw new System.NotImplementedException();
    }

    void IHostController.IncrementCrmVersion(bool includeOverridingPortals)
    {
        throw new System.NotImplementedException();
    }
}
