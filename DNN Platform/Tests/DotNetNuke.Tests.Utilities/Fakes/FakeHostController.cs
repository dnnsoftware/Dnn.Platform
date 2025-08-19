// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Tests.Utilities.Fakes;

using System;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Abstractions.Settings;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities;
using DotNetNuke.Entities.Controllers;

public class FakeHostController(IReadOnlyDictionary<string, IConfigurationSetting> settings)
    : IHostController, IHostSettingsService
{
    public bool GetBoolean(string key) => this.GetBoolean(key, Null.NullBoolean);

    public bool GetBoolean(string key, bool defaultValue)
    {
        if (settings.TryGetValue(key, out var setting))
        {
            return setting.Value.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) || setting.Value.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase);
        }

        return defaultValue;
    }

    public double GetDouble(string key, double defaultValue)
        => double.TryParse(this.GetString(key), out var value) ? value : defaultValue;

    public double GetDouble(string key)
        => this.GetDouble(key, Null.NullDouble);

    public int GetInteger(string key)
        => this.GetInteger(key, Null.NullInteger);

    public int GetInteger(string key, int defaultValue)
        => int.TryParse(this.GetString(key), out var value) ? value : defaultValue;

    public string GetString(string key)
        => this.GetString(key, string.Empty);

    public string GetString(string key, string defaultValue)
        => settings.TryGetValue(key, out var setting) ? setting.Value : defaultValue;

    public Dictionary<string, string> GetSettingsDictionary()
        => settings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value.Value);

    public IDictionary<string, IConfigurationSetting> GetSettings()
        => settings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

    Dictionary<string, ConfigurationSetting> IHostController.GetSettings()
        => settings.ToDictionary(kvp => kvp.Key, kvp => kvp.Value as ConfigurationSetting);

    IDictionary<string, string> IHostSettingsService.GetSettingsDictionary()
        => this.GetSettingsDictionary();

    public void Update(IConfigurationSetting config) => throw new NotImplementedException();

    public void Update(IConfigurationSetting config, bool clearCache) => throw new NotImplementedException();

    public void Update(IDictionary<string, string> settings) => throw new NotImplementedException();

    public string GetEncryptedString(string key, string passPhrase) => throw new NotImplementedException();

    public void Update(Dictionary<string, string> settings) => throw new NotImplementedException();

    public void Update(ConfigurationSetting config) => throw new NotImplementedException();

    public void Update(ConfigurationSetting config, bool clearCache) => throw new NotImplementedException();

    public void Update(string key, string value, bool clearCache) => throw new NotImplementedException();

    public void Update(string key, string value) => throw new NotImplementedException();

    public void UpdateEncryptedString(string key, string value, string passPhrase) => throw new NotImplementedException();

    public void IncrementCrmVersion(bool includeOverridingPortals) => throw new NotImplementedException();
}
