// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    /// <summary>
    /// Class responsible to provide settings for Tab Versioning.
    /// </summary>
    public interface ITabVersionSettings
    {
        /// <summary>
        /// Get the maximum number of version that the portal will kept for a tab.
        /// </summary>
        /// <param name="portalId">Portal Id.</param>
        /// <returns>Max number of version.</returns>
        int GetMaxNumberOfVersions(int portalId);

        /// <summary>
        /// Set the maximum number of version that the portal will kept for a tab.
        /// </summary>
        /// <param name="portalId">Portal Id.</param>
        /// <param name="maxNumberOfVersions">Max number of version.</param>
        void SetMaxNumberOfVersions(int portalId, int maxNumberOfVersions);

        /// <summary>
        /// Set the status of the tab versioning for the portal.
        /// </summary>
        /// <param name="portalId">Portal Id.</param>
        /// <param name="enabled">true for enable it, false otherwise.</param>
        void SetEnabledVersioningForPortal(int portalId, bool enabled);

        /// <summary>
        /// Set the status of the tab versioning for a tab.
        /// </summary>
        /// <param name="tabId">Tab Id.</param>
        /// <param name="enabled">true for enable it, false otherwise.</param>
        void SetEnabledVersioningForTab(int tabId, bool enabled);

        /// <summary>
        /// Get the status of the tab versioning for the portal.
        /// </summary>
        /// <param name="portalId">Portal Id.</param>
        /// <returns>Returns true if tab versioning is enabled for the portal, false otherwise.</returns>
        bool IsVersioningEnabled(int portalId);

        /// <summary>
        /// Get the status of the tab versioning for an specific tab of the portal.
        /// </summary>
        /// <remarks>
        /// If versioning is disabled at portal level, the versioning for tabs will be disabled too.
        /// </remarks>
        /// <param name="portalId">Portal Id.</param>
        /// <param name="tabId">Tab Id to be checked.</param>
        /// <returns>Returns true if tab versioning is enabled for the portal and for the tab, false otherwise.</returns>
        bool IsVersioningEnabled(int portalId, int tabId);

        /// <summary>
        /// Get the query string parameter name to especify a Tab Version using the version number (i.e.: ?version=1).
        /// </summary>
        /// <param name="portalId">Portal Id.</param>
        /// <returns>Query string parameter name.</returns>
        string GetTabVersionQueryStringParameter(int portalId);
    }
}
