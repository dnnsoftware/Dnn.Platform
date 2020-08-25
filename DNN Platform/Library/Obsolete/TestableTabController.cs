// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Web.Services.Description;

    using DotNetNuke.Framework;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class has been obsoleted in 7.3.0 - please use TabController instead. Scheduled removal in v10.0.0.")]
    public class TestableTabController : ServiceLocator<DotNetNuke.Entities.Tabs.Internal.ITabController, TestableTabController>, ITabController
    {
        public void DeleteTabUrl(TabUrlInfo tabUrl, int portalId, bool clearCache)
        {
            TabController.Instance.DeleteTabUrl(tabUrl, portalId, clearCache);
        }

        public TabInfo GetTab(int tabId, int portalId)
        {
            return TabController.Instance.GetTab(tabId, portalId);
        }

        public Dictionary<string, string> GetCustomAliases(int tabId, int portalId)
        {
            return TabController.Instance.GetCustomAliases(tabId, portalId);
        }

        public List<TabAliasSkinInfo> GetAliasSkins(int tabId, int portalId)
        {
            return TabController.Instance.GetAliasSkins(tabId, portalId);
        }

        public List<TabUrlInfo> GetTabUrls(int tabId, int portalId)
        {
            return TabController.Instance.GetTabUrls(tabId, portalId);
        }

        public void SaveTabUrl(TabUrlInfo tabUrl, int portalId, bool clearCache)
        {
            TabController.Instance.SaveTabUrl(tabUrl, portalId, clearCache);
        }

        protected override Func<DotNetNuke.Entities.Tabs.Internal.ITabController> GetFactory()
        {
            return () => new TestableTabController();
        }
    }
}
