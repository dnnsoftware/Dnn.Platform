// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals
{
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Settings;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Log.EventLog;

    /// <summary>
    /// Manages saving Portal Settings to the database and
    /// updating the cache. This is designed to be used
    /// with the <see cref="PortalSettingsService"/>.
    /// </summary>
    public class PortalSaveSettingsService : ISaveSettingsService
    {
        private readonly int portalId;
        private readonly string cultureCode;
        private readonly int userId;

        /// <summary>
        /// Initializes a new instance of the <see cref="PortalSaveSettingsService"/> class.
        /// </summary>
        /// <param name="portalId">The portalId to save settings.</param>
        /// <param name="cultureCode">The culture code to save settings.</param>
        public PortalSaveSettingsService(int portalId, string cultureCode)
        {
            this.portalId = portalId;
            this.cultureCode = cultureCode;
            this.userId = UserController.Instance.GetCurrentUserInfo().UserID;
        }

        /// <inheritdoc />
        public void Update(string key, string value, bool clearCache)
        {
            this.Update(key, value, false, clearCache);
        }

        /// <inheritdoc />
        public void UpdateEncrypted(string key, string value, string passPhrase, bool clearCache)
        {
            Requires.NotNullOrEmpty("key", key);
            Requires.PropertyNotNull("value", value);
            Requires.NotNullOrEmpty("passPhrase", passPhrase);

            var encryptedText = FIPSCompliant.EncryptAES(value, passPhrase, Host.Host.GUID);
            this.Update(key, encryptedText, true, clearCache);
        }

        /// <inheritdoc />
        public void UpdateEncrypted(string key, string value, bool clearCache)
        {
            this.Update(key, value, true, clearCache);
        }

        // TODO - Add xml docs
        private void Update(string key, string value, bool isSecure, bool shouldClearCache)
        {
            UpdateDatabase();
            AddEventLog();
            if (shouldClearCache)
            {
                ClearCache();
            }

            void UpdateDatabase()
            {
                DataProvider.Instance().UpdatePortalSetting(this.portalId, key, value, this.userId, this.cultureCode, isSecure);
            }

            void AddEventLog()
            {
                var propertyName = value + ((this.cultureCode == Null.NullString) ? string.Empty : " (" + this.cultureCode + ")");
                IPortalSettings portalSettings = new PortalSettings(this.portalId);

                EventLogController.Instance.AddLog(
                    propertyName,
                    value,
                    portalSettings,
                    this.userId,
                    EventLogController.EventLogType.PORTAL_SETTING_UPDATED);
            }

            void ClearCache()
            {
                DataCache.ClearPortalCache(this.portalId, false);
                DataCache.RemoveCache(DataCache.PortalDictionaryCacheKey);
            }
        }
    }
}
