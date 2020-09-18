// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    using System;
    using System.Globalization;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Framework;

    public class TabVersionSettings : ServiceLocator<ITabVersionSettings, TabVersionSettings>, ITabVersionSettings
    {
        private const int TabVersionsMaxNumber = 5;
        private const string TabVersionQueryStringParam = "DnnTabVersion";
        private const string TabVersioningSettingKey = "TabVersioningSettingKey";

        public int GetMaxNumberOfVersions(int portalId)
        {
            Requires.NotNegative("portalId", portalId);
            return portalId == Null.NullInteger ? TabVersionsMaxNumber : PortalController.GetPortalSettingAsInteger("TabVersionsMaxNumber", portalId, TabVersionsMaxNumber);
        }

        public void SetMaxNumberOfVersions(int portalId, int maxNumberOfVersions)
        {
            Requires.NotNegative("portalId", portalId);
            PortalController.UpdatePortalSetting(portalId, "TabVersionsMaxNumber", maxNumberOfVersions.ToString(CultureInfo.InvariantCulture));
        }

        public void SetEnabledVersioningForPortal(int portalId, bool enabled)
        {
            Requires.NotNegative("portalId", portalId);
            PortalController.UpdatePortalSetting(portalId, "TabVersionsEnabled", enabled.ToString(CultureInfo.InvariantCulture));
        }

        public void SetEnabledVersioningForTab(int tabId, bool enabled)
        {
            Requires.NotNegative("tabId", tabId);

            TabController.Instance.UpdateTabSetting(tabId, TabVersioningSettingKey, enabled.ToString(CultureInfo.InvariantCulture));
        }

        public bool IsVersioningEnabled(int portalId)
        {
            Requires.NotNegative("portalId", portalId);
            return
                Convert.ToBoolean(PortalController.GetPortalSetting("TabVersionsEnabled", portalId, bool.FalseString));
        }

        public bool IsVersioningEnabled(int portalId, int tabId)
        {
            Requires.NotNegative("portalId", portalId);
            Requires.NotNegative("tabId", tabId);

            if (!this.IsVersioningEnabled(portalId))
            {
                return false;
            }

            var tabInfo = TabController.Instance.GetTab(tabId, portalId);
            var isHostOrAdminPage = TabController.Instance.IsHostOrAdminPage(tabInfo);
            if (isHostOrAdminPage)
            {
                return false;
            }

            var settings = TabController.Instance.GetTabSettings(tabId);
            var isVersioningEnableForTab = settings[TabVersioningSettingKey] == null
                || Convert.ToBoolean(settings[TabVersioningSettingKey]);

            return isVersioningEnableForTab;
        }

        public string GetTabVersionQueryStringParameter(int portalId)
        {
            Requires.NotNegative("portalId", portalId);
            return PortalController.GetPortalSetting("TabVersionQueryStringParameter", portalId, TabVersionQueryStringParam);
        }

        protected override Func<ITabVersionSettings> GetFactory()
        {
            return () => new TabVersionSettings();
        }
    }
}
