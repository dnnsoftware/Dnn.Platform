// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals
{
    using System;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Abstractions.Settings;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Log.EventLog;

    /// <summary>
    /// Provides Delete APIs for deleting Portal Settings.
    /// </summary>
    internal class PortalDeleteSettingsService : IDeleteSettingsService
    {
        private readonly int portalId;
        private readonly string cultureCode;
        private readonly int userId;
        private readonly Func<PortalSettings> getPortalSettingsCallback;


        /// <summary>
        /// Initializes a new instance of the <see cref="PortalDeleteSettingsService"/> class.
        /// </summary>
        /// <param name="portalId">The portalId to save settings.</param>
        /// <param name="cultureCode">The culture code to save settings.</param>
        public PortalDeleteSettingsService(int portalId, string cultureCode)
        {
            this.portalId = portalId;
            this.cultureCode = cultureCode;
            this.userId = UserController.Instance.GetCurrentUserInfo().UserID;
            this.getPortalSettingsCallback = getPortalSettingsCallback;
        }

        /// <inheritdoc />
        public void Delete(string key)
        {
            DataProvider.Instance().DeletePortalSetting(this.portalId, key, this.cultureCode.ToLowerInvariant());
            EventLogController.Instance.AddLog(
                "SettingName",
                key + ((this.cultureCode == Null.NullString) ? string.Empty : " (" + this.cultureCode + ")"),
                this.GetPortalSettings(),
                this.userId, 
                EventLogController.EventLogType.PORTAL_SETTING_DELETED);
            DataCache.ClearHostCache(true);
        }

        private IPortalSettings GetPortalSettings()
        {
            var portal = PortalController.Instance.GetPortal(this.portalId);
            return new PortalSettings
            {
                PortalId = this.portalId,
                PortalName = portal.PortalName,
            };
        }
    }
}
