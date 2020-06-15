// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Do not implement.  This interface is only implemented by the DotNetNuke core framework. Outside the framework it should used as a type and for unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class has been obsoleted in 7.3.0 - please use version in DotNetNuke.Entities.Tabs instead. Scheduled removal in v10.0.0.")]
    public interface ITabController
    {
        void DeleteTabUrl(TabUrlInfo tabUrl, int portalId, bool clearCache);

        /// <summary>
        /// Gets the tab.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="portalId">The portal id.</param>
        /// <returns>tab info.</returns>
        TabInfo GetTab(int tabId, int portalId);

        Dictionary<string, string> GetCustomAliases(int tabId, int portalId);

        List<TabAliasSkinInfo> GetAliasSkins(int tabId, int portalId);

        List<TabUrlInfo> GetTabUrls(int tabId, int portalId);

        void SaveTabUrl(TabUrlInfo tabUrl, int portalId, bool clearCache);
    }
}
