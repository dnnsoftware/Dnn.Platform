#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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

using System.Collections.Generic;

namespace DotNetNuke.Entities.Tabs.Internal
{
    /// <summary>
    /// Do not implement.  This interface is only implemented by the DotNetNuke core framework. Outside the framework it should used as a type and for unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface ITabController
    {
        /// <summary>
        /// Delete a taburl 
        /// </summary>
        /// <param name="tabUrl">the taburl</param>
        /// <param name="portalId">the portal</param>
        /// <param name="clearCache">whether to clear the cache</param>
        void DeleteTabUrl(TabUrlInfo tabUrl, int portalId, bool clearCache);

        /// <summary>
        /// Gets the tab.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="portalId">The portal id.</param>
        /// <returns>tab info.</returns>
        TabInfo GetTab(int tabId, int portalId);

        /// <summary>
        /// Get the list of custom aliases associated with a page (tab)
        /// </summary>
        /// <param name="tabId">the tab id</param>
        /// <param name="portalId">the portal id</param>
        /// <returns>dictionary of tabid and aliases</returns>
        Dictionary<string, string> GetCustomAliases(int tabId, int portalId);
        
        /// <summary>
        /// Get the list of skins per alias at tab level
        /// </summary>
        /// <param name="tabId">the tab id</param>
        /// <param name="portalId">the portal id</param>
        /// <returns>list of TabAliasSkinInfo</returns>
        List<TabAliasSkinInfo> GetAliasSkins(int tabId, int portalId);

        /// <summary>
        /// Get the list of url's associated with a page (tab)
        /// </summary>
        /// <param name="tabId">the tab id</param>
        /// <param name="portalId">the portal id</param>
        /// <returns>list of urls associated with a tab</returns>
        List<TabUrlInfo> GetTabUrls(int tabId, int portalId);

        /// <summary>
        /// Save url information for a page (tab)
        /// </summary>
        /// <param name="tabUrl">the tab url</param>
        /// <param name="portalId">the portal id</param>
        /// <param name="clearCache">whether to clear the cache</param>
        void SaveTabUrl(TabUrlInfo tabUrl, int portalId, bool clearCache);
    }
}