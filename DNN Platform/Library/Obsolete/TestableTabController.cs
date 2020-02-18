// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Services.Description;

using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Tabs.Internal
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class has been obsoleted in 7.3.0 - please use TabController instead. Scheduled removal in v10.0.0.")]
    public class TestableTabController : ServiceLocator<DotNetNuke.Entities.Tabs.Internal.ITabController, TestableTabController>, ITabController
    {
        protected override Func<DotNetNuke.Entities.Tabs.Internal.ITabController> GetFactory()
        {
            return () => new TestableTabController();
        }

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
    }
}
