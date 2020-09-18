// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;

    /// <summary>
    /// Do not implement.  This interface is only implemented by the DotNetNuke core framework. Outside the framework it should used as a type and for unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface ITabController
    {
        /// <summary>
        /// Adds localized copies of the page in all missing languages.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="tabId"></param>
        void AddMissingLanguages(int portalId, int tabId);

        /// <summary>
        /// Adds a tab.
        /// </summary>
        /// <param name="tab">The tab to be added.</param>
        /// <remarks>The tab is added to the end of the current Level.</remarks>
        /// <returns></returns>
        int AddTab(TabInfo tab);

        /// <summary>
        /// Adds a tab.
        /// </summary>
        /// <param name="tab">The tab to be added.</param>
        /// <param name="includeAllTabsModules">Flag that indicates whether to add the "AllTabs"
        /// Modules.</param>
        /// <remarks>The tab is added to the end of the current Level.</remarks>
        /// <returns></returns>
        int AddTab(TabInfo tab, bool includeAllTabsModules);

        /// <summary>
        /// Adds a tab after the specified tab.
        /// </summary>
        /// <param name="tab">The tab to be added.</param>
        /// <param name="afterTabId">Id of the tab after which this tab is added.</param>
        /// <returns></returns>
        int AddTabAfter(TabInfo tab, int afterTabId);

        /// <summary>
        /// Adds a tab before the specified tab.
        /// </summary>
        /// <param name="objTab">The tab to be added.</param>
        /// <param name="beforeTabId">Id of the tab before which this tab is added.</param>
        /// <returns></returns>
        int AddTabBefore(TabInfo objTab, int beforeTabId);

        /// <summary>
        /// Clears tabs and portal cache for the specific portal.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        void ClearCache(int portalId);

        /// <summary>
        /// Converts one single tab to a neutral culture
        /// clears the tab cache optionally.
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
        /// Creates the localized copies.
        /// </summary>
        /// <param name="originalTab">The original tab.</param>
        void CreateLocalizedCopies(TabInfo originalTab);

        /// <summary>
        /// Creates the localized copy.
        /// </summary>
        /// <param name="originalTab">The original tab.</param>
        /// <param name="locale">The locale.</param>
        /// <param name="clearCache">Clear the cache?.</param>
        void CreateLocalizedCopy(TabInfo originalTab, Locale locale, bool clearCache);

        /// <summary>
        /// Deletes a tab permanently from the database.
        /// </summary>
        /// <param name="tabId">TabId of the tab to be deleted.</param>
        /// <param name="portalId">PortalId of the portal.</param>
        /// <remarks>
        /// The tab will not delete if it has child tab(s).
        /// </remarks>
        void DeleteTab(int tabId, int portalId);

        /// <summary>
        /// Deletes a tab permanently from the database.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="portalId">The portal id.</param>
        /// <param name="deleteDescendants">if set to <c>true</c> will delete all child tabs.</param>
        void DeleteTab(int tabId, int portalId, bool deleteDescendants);

        /// <summary>
        /// Delete a Setting of a tab instance.
        /// </summary>
        /// <param name="tabId">ID of the affected tab.</param>
        /// <param name="settingName">Name of the setting to be deleted.</param>
        void DeleteTabSetting(int tabId, string settingName);

        /// <summary>
        /// Delete all Settings of a tab instance.
        /// </summary>
        /// <param name="tabId">ID of the affected tab.</param>
        void DeleteTabSettings(int tabId);

        /// <summary>
        /// Delete a taburl.
        /// </summary>
        /// <param name="tabUrl">the taburl.</param>
        /// <param name="portalId">the portal.</param>
        /// <param name="clearCache">whether to clear the cache.</param>
        void DeleteTabUrl(TabUrlInfo tabUrl, int portalId, bool clearCache);

        /// <summary>
        /// Deletes all tabs for a specific language. Double checks if we are not deleting pages for the default language
        /// Clears the tab cache optionally.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="clearCache"></param>
        /// <returns></returns>
        bool DeleteTranslatedTabs(int portalId, string cultureCode, bool clearCache);

        /// <summary>
        /// Reverts page culture back to Neutral (Null), to ensure a non localized site
        /// clears the tab cache optionally.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="cultureCode"></param>
        /// <param name="clearCache"></param>
        void EnsureNeutralLanguage(int portalId, string cultureCode, bool clearCache);

        /// <summary>
        /// Get the list of skins per alias at tab level.
        /// </summary>
        /// <param name="tabId">the tab id.</param>
        /// <param name="portalId">the portal id.</param>
        /// <returns>list of TabAliasSkinInfo.</returns>
        List<TabAliasSkinInfo> GetAliasSkins(int tabId, int portalId);

        /// <summary>
        /// Get the list of custom aliases associated with a page (tab).
        /// </summary>
        /// <param name="tabId">the tab id.</param>
        /// <param name="portalId">the portal id.</param>
        /// <returns>dictionary of tabid and aliases.</returns>
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
        /// <returns>tab info.</returns>
        TabInfo GetTabByName(string tabName, int portalId, int parentId);

        /// <summary>
        /// Gets the tabs which use the module.
        /// </summary>
        /// <param name="moduleID">The module ID.</param>
        /// <returns>tab collection.</returns>
        IDictionary<int, TabInfo> GetTabsByModuleID(int moduleID);

        /// <summary>
        /// Gets the tabs which use the package.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="packageID">The package ID.</param>
        /// <param name="forHost">if set to <c>true</c> [for host].</param>
        /// <returns>tab collection.</returns>
        IDictionary<int, TabInfo> GetTabsByPackageID(int portalID, int packageID, bool forHost);

        /// <summary>
        /// Gets the tabs by portal.
        /// </summary>
        /// <param name="portalId">The portal id.</param>
        /// <returns>tab collection.</returns>
        TabCollection GetUserTabsByPortal(int portalId);

        /// <summary>
        /// Get the actual visible tabs for a given portal id.
        /// System Tabs and Admin Tabs are excluded from the result set.
        /// </summary>
        /// <param name="portalId"></param>
        ///
        /// <returns>tab collection.</returns>
        TabCollection GetTabsByPortal(int portalId);

        /// <summary>
        /// Gets the tabs which use the module.
        /// </summary>
        /// <param name="tabModuleId">The tabmodule ID.</param>
        /// <returns>tab collection.</returns>
        IDictionary<int, TabInfo> GetTabsByTabModuleID(int tabModuleId);

        /// <summary>
        /// read all settings for a tab from TabSettings table.
        /// </summary>
        /// <param name="tabId">ID of the Tab to query.</param>
        /// <returns>
        /// (cached) hashtable containing all settings.
        /// </returns>
        Hashtable GetTabSettings(int tabId);

        /// <summary>
        /// Get the list of url's associated with a page (tab).
        /// </summary>
        /// <param name="tabId">the tab id.</param>
        /// <param name="portalId">the portal id.</param>
        /// <returns>list of urls associated with a tab.</returns>
        List<TabUrlInfo> GetTabUrls(int tabId, int portalId);

        /// <summary>
        /// Gives the translator role edit rights.
        /// </summary>
        /// <param name="localizedTab">The localized tab.</param>
        /// <param name="users">The users.</param>
        void GiveTranslatorRoleEditRights(TabInfo localizedTab, Dictionary<int, UserInfo> users);

        /// <summary>
        /// Returns True if a page is missing a translated version in at least one other language.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="tabId"></param>
        /// <returns></returns>
        bool HasMissingLanguages(int portalId, int tabId);

        /// <summary>
        /// Checks whether the tab is published. Published means: view permissions of tab are identical to the DefaultLanguageTab.
        /// </summary>
        /// <param name="publishTab">The tab that is checked.</param>
        /// <returns>true if tab is published.</returns>
        bool IsTabPublished(TabInfo publishTab);

        /// <summary>
        /// Determines whether is host or admin tab.
        /// </summary>
        /// <param name="tab">The tab info.</param>
        /// <returns></returns>
        bool IsHostOrAdminPage(TabInfo tab);

        /// <summary>
        /// Localizes the tab.
        /// </summary>
        /// <param name="originalTab">The original tab.</param>
        /// <param name="locale">The locale.</param>
        void LocalizeTab(TabInfo originalTab, Locale locale);

        /// <summary>
        /// Localizes the tab, with optional clear cache.
        /// </summary>
        /// <param name="originalTab"></param>
        /// <param name="locale"></param>
        /// <param name="clearCache"></param>
        void LocalizeTab(TabInfo originalTab, Locale locale, bool clearCache);

        /// <summary>
        /// Moves the tab after a specific tab.
        /// </summary>
        /// <param name="tab">The tab want to move.</param>
        /// <param name="afterTabId">will move objTab after this tab.</param>
        void MoveTabAfter(TabInfo tab, int afterTabId);

        /// <summary>
        /// Moves the tab before a specific tab.
        /// </summary>
        /// <param name="tab">The tab want to move.</param>
        /// <param name="beforeTabId">will move objTab before this tab.</param>
        void MoveTabBefore(TabInfo tab, int beforeTabId);

        /// <summary>
        /// Moves the tab to a new parent.
        /// </summary>
        /// <param name="tab">The tab want to move.</param>
        /// <param name="parentId">will move tab to this parent.</param>
        void MoveTabToParent(TabInfo tab, int parentId);

        /// <summary>
        /// Populates the bread crumbs.
        /// </summary>
        /// <param name="tab">The tab.</param>
        void PopulateBreadCrumbs(ref TabInfo tab);

        /// <summary>
        /// Populates the bread crumbs.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="breadCrumbs">The bread crumbs.</param>
        /// <param name="tabID">The tab ID.</param>
        void PopulateBreadCrumbs(int portalID, ref ArrayList breadCrumbs, int tabID);

        /// <summary>
        /// Publishes the tab. Set the VIEW permissions to All Users.
        /// </summary>
        /// <param name="publishTab">The publish tab.</param>
        void PublishTab(TabInfo publishTab);

        /// <summary>
        /// Publishes the tab. Set the VIEW permissions to All Users.
        /// </summary>
        /// <param name="tabs">The tabs.</param>
        void PublishTabs(List<TabInfo> tabs);

        /// <summary>
        /// It marks a page as published at least once.
        /// </summary>
        /// <param name="tab">The Tab to be marked.</param>
        void MarkAsPublished(TabInfo tab);

        /// <summary>
        /// Restores the tab.
        /// </summary>
        /// <param name="tab">The obj tab.</param>
        /// <param name="portalSettings">The portal settings.</param>
        void RestoreTab(TabInfo tab, PortalSettings portalSettings);

        /// <summary>
        /// Save url information for a page (tab).
        /// </summary>
        /// <param name="tabUrl">the tab url.</param>
        /// <param name="portalId">the portal id.</param>
        /// <param name="clearCache">whether to clear the cache.</param>
        void SaveTabUrl(TabUrlInfo tabUrl, int portalId, bool clearCache);

        /// <summary>
        /// Soft Deletes the tab by setting the IsDeleted property to true.
        /// </summary>
        /// <param name="tabId">The tab id.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <returns></returns>
        bool SoftDeleteTab(int tabId, PortalSettings portalSettings);

        /// <summary>
        /// Updates the tab to databse.
        /// </summary>
        /// <param name="updatedTab">The updated tab.</param>
        void UpdateTab(TabInfo updatedTab);

        /// <summary>
        /// Adds or updates a tab's setting value.
        /// </summary>
        /// <param name="tabId">ID of the tab to update.</param>
        /// <param name="settingName">name of the setting property.</param>
        /// <param name="settingValue">value of the setting (String).</param>
        /// <remarks>empty SettingValue will remove the setting, if not preserveIfEmpty is true.</remarks>
        void UpdateTabSetting(int tabId, string settingName, string settingValue);

        /// <summary>
        /// Updates the translation status.
        /// </summary>
        /// <param name="localizedTab">The localized tab.</param>
        /// <param name="isTranslated">if set to <c>true</c> means the tab has already been translated.</param>
        void UpdateTranslationStatus(TabInfo localizedTab, bool isTranslated);

        /// <summary>
        /// Refresh the tabinfo in cache object of portal tabs collection, use this instead of clear the whole cache to improve performance.
        /// </summary>
        /// <param name="portalId"></param>
        /// <param name="tabId"></param>
        void RefreshCache(int portalId, int tabId);
    }
}
