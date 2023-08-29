// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.GoogleMailAuthProvider.Components;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;

using Google.Apis.Json;
using Google.Apis.Util.Store;

/// <summary>Google credentials data store class.</summary>
public class GoogleCredentialDataStore : IDataStore
{
    private readonly IHostSettingsService hostSettingsService;
    private readonly int portalId;

    /// <summary>
    /// Initializes a new instance of the <see cref="GoogleCredentialDataStore"/> class.
    /// </summary>
    /// <param name="portalId">The portal id.</param>
    /// <param name="hostSettingsService">The host settings service.</param>
    public GoogleCredentialDataStore(int portalId, IHostSettingsService hostSettingsService)
    {
        this.portalId = portalId;
        this.hostSettingsService = hostSettingsService;
    }

    /// <inheritdoc />
    public Task ClearAsync()
    {
        var settingName = string.Format(Constants.DataStoreSettingName, this.portalId);
        if (this.portalId == Null.NullInteger)
        {
            this.hostSettingsService.Update(settingName, null, true);
        }
        else
        {
            PortalController.UpdatePortalSetting(this.portalId, settingName, null, true);
        }

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task DeleteAsync<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key MUST have a value");
        }

        var dataStore = this.LoadDataStore();
        var settingName = GenerateStoredKey(key, typeof(T));
        if (dataStore.ContainsKey(settingName))
        {
            dataStore.Remove(settingName);
        }

        this.SaveDataStore(dataStore);

        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<T> GetAsync<T>(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key MUST have a value");
        }

        var dataStore = this.LoadDataStore();
        var settingName = GenerateStoredKey(key, typeof(T));
        if (!dataStore.TryGetValue(settingName, out var settingValue))
        {
            settingValue = string.Empty;
        }

        if (string.IsNullOrWhiteSpace(settingValue))
        {
            return Task.FromResult(default(T));
        }

        try
        {
            return Task.FromResult(NewtonsoftJsonSerializer.Instance.Deserialize<T>(settingValue));
        }
        catch (Exception ex)
        {
            return Task.FromException<T>(ex);
        }
    }

    /// <inheritdoc />
    public Task StoreAsync<T>(string key, T value)
    {
        if (string.IsNullOrEmpty(key))
        {
            throw new ArgumentException("Key MUST have a value");
        }

        var dataStore = this.LoadDataStore();
        var settingName = GenerateStoredKey(key, typeof(T));
        var settingValue = NewtonsoftJsonSerializer.Instance.Serialize(value);
        dataStore[settingName] = settingValue;
        this.SaveDataStore(dataStore);

        return Task.CompletedTask;
    }

    private static string GenerateStoredKey(string key, Type t)
    {
        return $"{t.FullName}-{key}";
    }

    private IDictionary<string, string> LoadDataStore()
    {
        var settingName = string.Format(Constants.DataStoreSettingName, this.portalId);
        string settingValue;

        if (this.portalId == Null.NullInteger)
        {
            settingValue = this.hostSettingsService.GetEncryptedString(settingName, Config.GetDecryptionkey());
        }
        else
        {
            settingValue = PortalController.GetEncryptedString(settingName, this.portalId, Config.GetDecryptionkey());
        }

        if (string.IsNullOrWhiteSpace(settingValue))
        {
            return new Dictionary<string, string>();
        }

        return NewtonsoftJsonSerializer.Instance.Deserialize<IDictionary<string, string>>(settingValue);
    }

    private void SaveDataStore(IDictionary<string, string> dataStore)
    {
        var settingName = string.Format(Constants.DataStoreSettingName, this.portalId);
        var settingValue = NewtonsoftJsonSerializer.Instance.Serialize(dataStore);

        if (this.portalId == Null.NullInteger)
        {
            this.hostSettingsService.UpdateEncryptedString(settingName, settingValue, Config.GetDecryptionkey());
        }
        else
        {
            PortalController.UpdateEncryptedString(this.portalId, settingName, settingValue, Config.GetDecryptionkey());
        }
    }
}
