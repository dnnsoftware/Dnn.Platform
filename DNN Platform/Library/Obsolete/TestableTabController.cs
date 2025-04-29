// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs.Internal;

using System;
using System.Collections.Generic;
using System.ComponentModel;

using DotNetNuke.Framework;
using DotNetNuke.Internal.SourceGenerators;

[EditorBrowsable(EditorBrowsableState.Never)]
[DnnDeprecated(7, 3, 0, "Please use TabController instead", RemovalVersion = 10)]
public partial class TestableTabController : ServiceLocator<DotNetNuke.Entities.Tabs.Internal.ITabController, TestableTabController>, ITabController
{
    /// <inheritdoc/>
    public void DeleteTabUrl(TabUrlInfo tabUrl, int portalId, bool clearCache)
    {
        TabController.Instance.DeleteTabUrl(tabUrl, portalId, clearCache);
    }

    /// <inheritdoc/>
    public TabInfo GetTab(int tabId, int portalId)
    {
        return TabController.Instance.GetTab(tabId, portalId);
    }

    /// <inheritdoc/>
    public Dictionary<string, string> GetCustomAliases(int tabId, int portalId)
    {
        return TabController.Instance.GetCustomAliases(tabId, portalId);
    }

    /// <inheritdoc/>
    public List<TabAliasSkinInfo> GetAliasSkins(int tabId, int portalId)
    {
        return TabController.Instance.GetAliasSkins(tabId, portalId);
    }

    /// <inheritdoc/>
    public List<TabUrlInfo> GetTabUrls(int tabId, int portalId)
    {
        return TabController.Instance.GetTabUrls(tabId, portalId);
    }

    /// <inheritdoc/>
    public void SaveTabUrl(TabUrlInfo tabUrl, int portalId, bool clearCache)
    {
        TabController.Instance.SaveTabUrl(tabUrl, portalId, clearCache);
    }

    /// <inheritdoc/>
    protected override Func<DotNetNuke.Entities.Tabs.Internal.ITabController> GetFactory()
    {
        return () => new TestableTabController();
    }
}
