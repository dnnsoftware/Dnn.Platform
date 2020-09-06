
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Abstractions.Settings;
    using DotNetNuke.Instrumentation;

    public class PortalSettingsService : ISettingsService
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalSettingsService));

        int portalId = -1;
        string cultureCode = string.Empty;
        IDictionary<string, string> settings;

        public PortalSettingsService(int portalId, string cultureCode, IDictionary<string, string> settings)
        {
            this.portalId = portalId;
            this.cultureCode = cultureCode;
            this.settings = settings;
        }

        /// <inheritdoc />
        public bool GetBoolean(string key)
        {
            try
            {
                if (this.settings.TryGetValue(key, out string value))
                {
                    if (string.IsNullOrEmpty(value))
                    {
                        return false;
                    }
                    else
                    {
                        return
                            value.StartsWith("Y", StringComparison.InvariantCultureIgnoreCase) ||
                            value.Equals("TRUE", StringComparison.InvariantCultureIgnoreCase);
                    }
                }
            }
            catch (Exception exception)
            {
                Logger.Error(exception);
            }

            return false;
        }

        /// <inheritdoc />
        public bool GetBoolean(string key, bool defaultValue)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public double GetDouble(string key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public double GetDouble(string key, double defaultValue)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public string GetEncryptedString(string key, string passPhrase)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public string GetEncryptedString(string key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public int GetInteger(string key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public int GetInteger(string key, int defaultValue)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IDictionary<string, IConfigurationSetting> GetSettings()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public IDictionary<string, string> GetSettingsDictionary()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public string GetString(string key)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public string GetString(string key, string defaultValue)
        {
            throw new NotImplementedException();
        }

        // this is specific to host settings and doesn't belong here
        public void IncrementCrmVersion(bool includeOverridingPortals)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Update(IConfigurationSetting config)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void Update(string key, string value, bool clearCache)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void UpdateEncryptedString(string key, string value, string passPhrase)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public void UpdateEncryptedString(string key, string value)
        {
            throw new NotImplementedException();
        }

        private void Update(string key, string value, bool clearCache, bool isSecure)
        {
            throw new NotImplementedException();
        }
    }
}
