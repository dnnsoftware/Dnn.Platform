#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Globalization;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    public class TabVersionSettings : ServiceLocator<ITabVersionSettings, TabVersionSettings>, ITabVersionSettings
    {
        #region Constants
        private const int TabVersionsMaxNumber = 5;
        private const string TabVersionQueryStringParam = "DnnTabVersion";
        private const string TabVersioningSettingKey = "TabVersioningSettingKey";
        #endregion

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
                Convert.ToBoolean(PortalController.GetPortalSetting("TabVersionsEnabled", portalId, Boolean.FalseString));
        }

        public bool IsVersioningEnabled(int portalId, int tabId)
        {
            Requires.NotNegative("portalId", portalId);
            Requires.NotNegative("tabId", tabId);

            if (!IsVersioningEnabled(portalId))
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
