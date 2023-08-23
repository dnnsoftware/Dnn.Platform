// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.GoogleMailAuthProvider.Components
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Web;
    using Google.Apis.Json;
    using Google.Apis.Util.Store;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Google credentials data store class.
    /// </summary>
    public class GoogleCredentialDataStore : IDataStore
    {
        private static readonly Task CompletedTask = Task.FromResult(0);
        private readonly IServiceProvider serviceProvider = GetServiceProvider();

        private int portalId;

        /// <summary>
        /// Initializes a new instance of the <see cref="GoogleCredentialDataStore"/> class.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        public GoogleCredentialDataStore(int portalId)
        {
            this.portalId = portalId;
        }

        /// <summary>
        /// Clear all the credentials.
        /// </summary>
        /// <returns>Task.</returns>
        public Task ClearAsync()
        {
            var settingName = string.Format(Constants.DataStoreSettingName, this.portalId);
            if (this.portalId == Null.NullInteger)
            {
                this.GetService<IHostSettingsService>().Update(settingName, null, true);
            }
            else
            {
                PortalController.UpdatePortalSetting(this.portalId, settingName, null, true);
            }

            return Task.CompletedTask;
        }

        /// <summary>
        /// Delete the credentials.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="key">The credential key.</param>
        /// <returns>the credential.</returns>
        public Task DeleteAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            var dataStore = this.LoadDataStore();
            var settingName = this.GenerateStoredKey(key, typeof(T));
            if (dataStore.ContainsKey(settingName))
            {
                dataStore.Remove(settingName);
            }

            this.SaveDataStore(dataStore);

            return CompletedTask;
        }

        /// <summary>
        /// Get the credential.
        /// </summary>
        /// <typeparam name="T">The data type.</typeparam>
        /// <param name="key">The credential key.</param>
        /// <returns>The credential.</returns>
        public Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            var dataStore = this.LoadDataStore();
            var settingName = this.GenerateStoredKey(key, typeof(T));
            var settingValue = string.Empty;

            if (dataStore.ContainsKey(settingName))
            {
                settingValue = dataStore[settingName];
            }

            TaskCompletionSource<T> tcs = new TaskCompletionSource<T>();
            if (!string.IsNullOrWhiteSpace(settingValue))
            {
                try
                {
                    tcs.SetResult(NewtonsoftJsonSerializer.Instance.Deserialize<T>(settingValue));
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            }
            else
            {
                tcs.SetResult(default(T));
            }

            return tcs.Task;
        }

        /// <summary>
        /// Stores the given value for the given key.
        /// </summary>
        /// <typeparam name="T">The type to store in the data store.</typeparam>
        /// <param name="key">The key.</param>
        /// <param name="value">The value to store in the data store.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public Task StoreAsync<T>(string key, T value)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("Key MUST have a value");
            }

            var dataStore = this.LoadDataStore();
            var settingName = this.GenerateStoredKey(key, typeof(T));
            var settingValue = NewtonsoftJsonSerializer.Instance.Serialize(value);
            dataStore[settingName] = settingValue;
            this.SaveDataStore(dataStore);

            return CompletedTask;
        }

        private static IServiceProvider GetServiceProvider()
        {
            return HttpContextSource.Current?.GetScope()?.ServiceProvider ??
                DependencyInjectionInitialize.BuildServiceProvider();
        }

        private T GetService<T>()
            where T : class
        {
            return (T)this.serviceProvider.GetService(typeof(T));
        }

        private IDictionary<string, string> LoadDataStore()
        {
            var settingName = string.Format(Constants.DataStoreSettingName, this.portalId);
            var settingValue = string.Empty;

            if (this.portalId == Null.NullInteger)
            {
                settingValue = this.GetService<IHostSettingsService>().GetEncryptedString(settingName, Config.GetDecryptionkey());
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
                this.GetService<IHostSettingsService>().UpdateEncryptedString(settingName, settingValue, Config.GetDecryptionkey());
            }
            else
            {
                PortalController.UpdateEncryptedString(this.portalId, settingName, settingValue, Config.GetDecryptionkey());
            }
        }

        private string GenerateStoredKey(string key, Type t)
        {
            return string.Format("{0}-{1}", t.FullName, key);
        }
    }
}
