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

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    /// <summary>
    /// Class responsible to provide settings for Tab Versioning
    /// </summary>
    public interface ITabVersionSettings
    {
        /// <summary>
        /// Get the maximum number of version that the portal will kept for a tab
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <returns>Max number of version</returns>
        int GetMaxNumberOfVersions(int portalId);

        /// <summary>
        /// Set the maximum number of version that the portal will kept for a tab
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <param name="maxNumberOfVersions">Max number of version</param>
        void SetMaxNumberOfVersions(int portalId, int maxNumberOfVersions);

        /// <summary>
        /// Set the status of the tab versioning for the portal
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <param name="enabled">true for enable it, false otherwise</param>
        void SetEnabledVersioningForPortal(int portalId, bool enabled);

        /// <summary>
        /// Set the status of the tab versioning for a tab
        /// </summary>
        /// <param name="tabId">Tab Id</param>
        /// <param name="enabled">true for enable it, false otherwise</param>
        void SetEnabledVersioningForTab(int tabId, bool enabled);

        /// <summary>
        /// Get the status of the tab versioning for the portal
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <returns>Returns true if tab versioning is enabled for the portal, false otherwise</returns>
        bool IsVersioningEnabled(int portalId);

        /// <summary>
        /// Get the status of the tab versioning for an specific tab of the portal.
        /// </summary>
        /// <remarks>
        /// If versioning is disabled at portal level, the versioning for tabs will be disabled too.
        /// </remarks>
        /// <param name="portalId">Portal Id</param>
        /// <param name="tabId">Tab Id to be checked</param>
        /// <returns>Returns true if tab versioning is enabled for the portal and for the tab, false otherwise</returns>
        bool IsVersioningEnabled(int portalId, int tabId);

        /// <summary>
        /// Get the query string parameter name to especify a Tab Version using the version number (i.e.: ?version=1)
        /// </summary>
        /// <param name="portalId">Portal Id</param>
        /// <returns>Query string parameter name</returns>
        string GetTabVersionQueryStringParameter(int portalId);
    }
}
