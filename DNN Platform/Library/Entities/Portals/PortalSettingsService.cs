// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Settings;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;

    /// <summary>
    /// Provides an implementation of the <see cref="ISettingsService"/>
    /// used for updating the Portal Settings. This is instantiated by the
    /// <see cref="PortalSettingsManager"/>.
    /// </summary>
    public class PortalSettingsService : ISettingsService
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalSettingsService));

        protected Func<IDictionary<string, string>> LoadSettingsCallback { get; }

        /// <summary>
        /// Gets the current Portal Settings.
        /// </summary>
        protected IDictionary<string, string> Settings { get; private set; }

        /// <summary>
        /// Gets the save service, used for saving the
        /// current portal settings.
        /// </summary>
        protected ISaveSettingsService SaveService { get; }

        /// <summary>
        /// Gets the delete service, used for deleting
        /// a portal setting.
        /// </summary>
        protected IDeleteSettingsService DeleteService { get; }

        /// <inheritdoc />
        public string this[string key]
        {
            get => this.Get(key);
            set => this.Update(key, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalSettingsService"/> class.
        /// </summary>
        /// <param name="settings">The current portal settings.</param>
        /// <param name="saveService">The save settings implementation.</param>
        /// <param name="deleteService">The delete settings implementation</param>
        public PortalSettingsService(Func<IDictionary<string, string>> loadSettingsCallback, ISaveSettingsService saveService, IDeleteSettingsService deleteService)
        {
            this.LoadSettingsCallback = loadSettingsCallback;
            this.SaveService = saveService;
        }

        /// <inheritdoc />
        public T Get<T>(string key)
            where T : IConvertible
        {
            try
            {
                // DNN uses special logic for boolean. It accepts Y and TRUE. We should make sure that is convertible
                return (T)Convert.ChangeType(this[key], typeof(T));
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }

            return default(T);
        }

        /// <inheritdoc />
        public T GetEncrypted<T>(string key)
            where T : IConvertible
        {
            return this.GetEncrypted<T>(key, Config.GetDecryptionkey());
        }

        /// <inheritdoc />
        public T GetEncrypted<T>(string key, string passPhrase)
            where T : IConvertible
        {
            try
            {
                var decryptedText = FIPSCompliant.DecryptAES(this[key], passPhrase, Host.GUID);
                return (T)Convert.ChangeType(decryptedText, typeof(T));
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }

            return default(T);
        }

        /// <inheritdoc />
        public string GetEncrypted(string key) =>
            this.GetEncrypted<string>(key);

        /// <inheritdoc />
        public string GetEncrypted(string key, string passPhrase) =>
            this.GetEncrypted<string>(key, passPhrase);

        /// <inheritdoc />
        public void Delete(string key) =>
            this.DeleteService.Delete(key);

        public void DeleteAll() =>
            this.DeleteService.DeleteAll();

        public void DeleteAll(bool clearCache) =>
            this.DeleteService.DeleteAll(clearCache);

        /// <inheritdoc />
        public void Delete(string key, bool clearCache) => 
            this.DeleteService.Delete(key, clearCache);

        /// <inheritdoc />
        public void Update(string key, string value) => 
            this.SaveService.Update(key, value, false);

        /// <inheritdoc />
        public void UpdateEncrypted(string key, string value) => 
            this.SaveService.UpdateEncrypted(key, value, false);

        /// <inheritdoc />
        public void UpdateEncrypted(string key, string value, string passPhrase) =>
            this.SaveService.UpdateEncrypted(key, value, passPhrase, false);

        /// <inheritdoc />
        public void Update(string key, string value, bool clearCache) =>
            this.SaveService.Update(key, value, clearCache);

        /// <inheritdoc />
        public void UpdateEncrypted(string key, string value, bool clearCache) =>
            this.SaveService.UpdateEncrypted(key, value, clearCache);

        /// <inheritdoc />
        public void UpdateEncrypted(string key, string value, string passPhrase, bool clearCache) =>
            this.SaveService.UpdateEncrypted(key, value, passPhrase, clearCache);

        protected virtual string Get(string key)
        {
            if (this.Settings == null)
            {
                this.Settings = this.LoadSettingsCallback();
            }

            try
            {
                if (this.Settings.TryGetValue(key, out string value))
                {
                    return value;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }

            return default(string);
        }
    }
}
