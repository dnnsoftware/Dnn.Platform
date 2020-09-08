
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    using DotNetNuke.Abstractions.Settings;
    using DotNetNuke.Common;
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

        /// <summary>
        /// Gets the current Portal Settings.
        /// </summary>
        protected IDictionary<string, string> Settings { get; }

        /// <summary>
        /// Gets the save service, used for saving the
        /// current portal settings.
        /// </summary>
        protected ISaveSettingsService SaveService { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalSettingsService"/> class.
        /// </summary>
        /// <param name="settings">The current portal settings.</param>
        /// <param name="saveService">The save settings implementation.</param>
        public PortalSettingsService(IDictionary<string, string> settings, ISaveSettingsService saveService)
        {
            this.Settings = settings;
            this.SaveService = saveService;
        }

        /// <inheritdoc />
        public bool GetBoolean(string key)
        {
            return this.GetBoolean(key, false);
        }

        /// <inheritdoc />
        public bool GetBoolean(string key, bool defaultValue)
        {
            try
            {
                if (this.Settings.TryGetValue(key, out string value) && !string.IsNullOrEmpty(value))
                {
                    return
                        value.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) ||
                        value.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }

            return defaultValue;
        }

        /// <inheritdoc />
        public double GetDouble(string key)
        {
            return this.GetDouble(key, double.MinValue);
        }

        /// <inheritdoc />
        public double GetDouble(string key, double defaultValue)
        {
            try
            {
                if (this.Settings.TryGetValue(key, out string value) && !string.IsNullOrEmpty(value))
                {
                    return Convert.ToDouble(value);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }

            return defaultValue;
        }

        /// <inheritdoc />
        public string GetEncryptedString(string key, string passPhrase)
        {
            Requires.NotNullOrEmpty(nameof(key), key);
            Requires.NotNullOrEmpty(nameof(passPhrase), passPhrase);

            var cipherText = this.GetString(key);
            return FIPSCompliant.DecryptAES(cipherText, passPhrase, Host.GUID);
        }

        /// <inheritdoc />
        public string GetEncryptedString(string key)
        {
            return this.GetEncryptedString(key, Config.GetDecryptionkey());
        }

        /// <inheritdoc />
        public int GetInteger(string key)
        {
            return this.GetInteger(key, -1);
        }

        /// <inheritdoc />
        public int GetInteger(string key, int defaultValue)
        {
            try
            {
                if (this.Settings.TryGetValue(key, out string value) && !string.IsNullOrEmpty(value))
                {
                    return Convert.ToInt32(value);
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }

            return defaultValue;
        }

        /// <inheritdoc />
        public IDictionary<string, IConfigurationSetting> GetSettings()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IDictionary<string, string> GetSettingsDictionary()
        {
            return new ReadOnlyDictionary<string, string>(this.Settings);
        }

        /// <inheritdoc />
        public string GetString(string key)
        {
            return this.GetString(key, string.Empty);
        }

        /// <inheritdoc />
        public string GetString(string key, string defaultValue)
        {
            try
            {
                if (this.Settings.TryGetValue(key, out string value) && !string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }

            return defaultValue;
        }

        // this is specific to host settings and doesn't belong here
        public void IncrementCrmVersion(bool includeOverridingPortals)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Update(IConfigurationSetting config)
        {
            this.SaveService.Update(config.Key, config.Value, false);
        }

        /// <inheritdoc />
        public void Update(IConfigurationSetting config, bool clearCache)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Update(IDictionary<string, string> settings)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Update(string key, string value)
        {
            this.SaveService.Update(key, value, false);
        }

        /// <inheritdoc />
        public void Update(string key, string value, bool clearCache)
        {
            this.SaveService.Update(key, value, clearCache);
        }

        /// <inheritdoc />
        public void UpdateEncryptedString(string key, string value, string passPhrase)
        {
            this.SaveService.UpdateEncrypted(key, value, passPhrase, false);
        }

        /// <inheritdoc />
        public void UpdateEncryptedString(string key, string value, string passPhrase, bool clearCache)
        {
            this.SaveService.UpdateEncrypted(key, value, passPhrase, clearCache);
        }

        /// <inheritdoc />
        public void UpdateEncryptedString(string key, string value)
        {
            this.SaveService.UpdateEncrypted(key, value, false);
        }

        /// <inheritdoc />
        public void UpdateEncryptedString(string key, string value, bool clearCache)
        {
            this.SaveService.UpdateEncrypted(key, value, clearCache);
        }
    }
}
