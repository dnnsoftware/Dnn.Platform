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

using System.Collections;
using System.Collections.Generic;

using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Tabs
{
    /// <summary>
    /// Do not implement.  This interface is only implemented by the DotNetNuke core framework. Outside the framework it should used as a type and for unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface ITabController
    {
        /// <summary>
        /// Adds localized copies of the page in all missing languages
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="tabId"></param>
        void AddMissingLanguages(int portalId, int tabId);

        /// <summary>
        /// Adds a tab
        /// </summary>
        /// <param name="tab">The tab to be added</param>
        /// <remarks>The tab is added to the end of the current Level.</remarks>
        int AddTab(TabInfo tab);

        /// <summary>
        /// Adds a tab
        /// </summary>
        /// <param name="tab">The tab to be added</param>
        /// <param name="includeAllTabsModules">Flag that indicates whether to add the "AllTabs"
        /// Modules</param>
        /// <remarks>The tab is added to the end of the current Level.</remarks>
        int AddTab(TabInfo tab, bool includeAllTabsModules);

        /// <summary>
        /// Adds a tab after the specified tab
        /// </summary>
        /// <param name="tab">The tab to be added</param>
        /// <param name="afterTabId">Id of the tab after which this tab is added</param>
        int AddTabAfter(TabInfo tab, int afterTabId);

        /// <summary>
        /// Adds a tab before the specified tab
        /// </summary>
        /// <param name="objTab">The tab to be added</param>
        /// <param name="beforeTabId">Id of the tab before which this tab is added</param>
        int AddTabBefore(TabInfo objTab, int beforeTabId);

        /// <summary>
        /// Clears tabs and portal cache for the specific portal.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        void ClearCache(int portalId);

        /// <summary>
        /// Converts one single tab to a neutral culture
        /// clears the tab cache optionally
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="tabId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="clearCache"></param>
        void ConvertTabToNeutralLanguage(int portalId, int tabId, string cultureCode, bool clearCache);
            
        /// <summary>
        /// Creates content item for the tab..
        /// </summary>
        /// <param name="tab">The updated tab.</param>
        void CreateContentItem(TabInfo tab);

        /// <summary>
        /// Deletes a tab permanently from the database
        /// </summary>
        /// <param name="tabId">TabId of the tab to be deleted</param>
        /// <param name="portalId">PortalId of the portal</param>
        /// <remarks>
        /// The tab will not delete if it has child tab(s).
        /// </remarks>
        void DeleteTab(int tabId, int portalId);

        /// <summary>
        /// Deletes a tab permanently from the database
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="deleteDescendants">if set to <c>true</c> will delete all child tabs.</param>
        void DeleteTab(int tabId, int portalId, bool deleteDescendants);

        /// <summary>
        /// Delete a Setting of a tab instance
        /// </summary>
        /// <param name="tabId">ID of the affected tab</param>
        /// <param name="settingName">Name of the setting to be deleted</param>
        void DeleteTabSetting(int tabId, string settingName);

        /// <summary>
        /// Delete all Settings of a tab instance
        /// </summary>
        /// <param name="tabId">ID of the affected tab</param>
        void DeleteTabSettings(int tabId);

        /// <summary>
        /// Delete a taburl 
        /// </summary>
        /// <param name="tabUrl">the taburl</param>
        /// <param name="portalId">the portal</param>
        /// <param name="clearCache">whether to clear the cache</param>
        void DeleteTabUrl(TabUrlInfo tabUrl, int portalId, bool clearCache);

        /// <summary>
        /// Deletes all tabs for a specific language. Double checks if we are not deleting pages for the default language
        /// Clears the tab cache optionally
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="clearCache"></param>
        bool DeleteTranslatedTabs(int portalId, string cultureCode, bool clearCache);

        /// <summary>
        /// Reverts page culture back to Neutral (Null), to ensure a non localized site
        /// clears the tab cache optionally
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="clearCache"></param>
        void EnsureNeutralLanguage(int portalId, string cultureCode, bool clearCache);

        /// <summary>
        /// Get the list of skins per alias at tab level
        /// </summary>
        /// <param name="tabId">the tab id</param>
        /// <param name="portalId">the portal id</param>
        /// <returns>list of TabAliasSkinInfo</returns>
        List<TabAliasSkinInfo> GetAliasSkins(int tabId, int portalId);

        /// <summary>
        /// Get the list of custom aliases associated with a page (tab)
        /// </summary>
        /// <param name="tabId">the tab id</param>
        /// <param name="portalId">the portal id</param>
        /// <returns>dictionary of tabid and aliases</returns>
        Dictionary<string, string> GetCustomAliases(int tabId, int portalId);

        /// <summary>
        /// Gets the tab.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="portalId">The portal id.</param>
        /// <returns>tab info.</returns>
        TabInfo GetTab(int tabId, int portalId);

        /// <summary>
        /// Gets the tab.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="ignoreCache">if set to <c>true</c> will get tab info directly from database.</param>
        /// <returns>tab info.</returns>
        TabInfo GetTab(int tabId, int portalId, bool ignoreCache);

        /// <summary>
        /// Gets the tab by culture.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="locale">The locale.</param>
        /// <returns>tab info.</returns>
        TabInfo GetTabByCulture(int tabId, int portalId, Locale locale);

        /// <summary>
        /// Gets the name of the tab by name.
        /// </summary>
        /// <param name="tabName">Name of the tab.</param>
        /// <param name="portalId">The portal id.</param>
        /// <returns>tab info.</returns>
        TabInfo GetTabByName(string tabName, int portalId);

        /// <summary>
        /// Gets the name of the tab by name and parent id.
        /// </summary>
        /// <param name="tabName">Name of the tab.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="parentId">The parent id.</param>
        /// <returns>tab info</returns>
        TabInfo GetTabByName(string tabName, int portalId, int parentId);

        /// <summary>
        /// Gets the tabs by portal.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>tab collection</returns>
        TabCollection GetTabsByPortal(int portalId);

        /// <summary>
        /// read all settings for a tab from TabSettings table
        /// </summary>
        /// <param name="tabId">ID of the Tab to query</param>
        /// <returns>
        /// (cached) hashtable containing all settings
        /// </returns>
        Hashtable GetTabSettings(int tabId);

        /// <summary>
        /// Get the list of url's associated with a page (tab)
        /// </summary>
        /// <param name="tabId">the tab id</param>
        /// <param name="portalId">the portal id</param>
        /// <returns>list of urls associated with a tab</returns>
        List<TabUrlInfo> GetTabUrls(int tabId, int portalId);

        /// <summary>
        /// Returns True if a page is missing a translated version in at least one other language
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="tabId"></param>
        /// <returns></returns>
        bool HasMissingLanguages(int portalId, int tabId);



        /// <summary>
        /// Save url information for a page (tab)
        /// </summary>
        /// <param name="tabUrl">the tab url</param>
        /// <param name="portalId">the portal id</param>
        /// <param name="clearCache">whether to clear the cache</param>
        void SaveTabUrl(TabUrlInfo tabUrl, int portalId, bool clearCache);
    }
}