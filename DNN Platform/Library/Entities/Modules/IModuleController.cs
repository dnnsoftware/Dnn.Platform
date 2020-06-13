// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Modules
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;

    /// <summary>
    /// Do not implement.  This interface is only implemented by the DotNetNuke core framework. Outside the framework it should used as a type and for unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface IModuleController
    {
        /// <summary>
        /// add a module to a page.
        /// </summary>
        /// <param name="module">moduleInfo for the module to create.</param>
        /// <returns>ID of the created module.</returns>
        int AddModule(ModuleInfo module);

        /// <summary>
        /// Clears the module cache based on the page (tabid).
        /// </summary>
        /// <param name="TabId">The tab id.</param>
        void ClearCache(int TabId);

        /// <summary>
        /// Copies the module to a new page.
        /// </summary>
        /// <param name="sourceModule">The source module.</param>
        /// <param name="destinationTab">The destination tab.</param>
        /// <param name="toPaneName">Name of to pane.</param>
        /// <param name="includeSettings">if set to <c>true</c> include settings.</param>
        void CopyModule(ModuleInfo sourceModule, TabInfo destinationTab, string toPaneName, bool includeSettings);

        /// <summary>
        /// Copies all modules in source page to a new page.
        /// </summary>
        /// <param name="sourceTab">The source tab.</param>
        /// <param name="destinationTab">The destination tab.</param>
        /// <param name="asReference">if set to <c>true</c> will use source module directly, else will create new module info by source module.</param>
        void CopyModules(TabInfo sourceTab, TabInfo destinationTab, bool asReference);

        /// <summary>
        /// Copies all modules in source page to a new page.
        /// </summary>
        /// <param name="sourceTab">The source tab.</param>
        /// <param name="destinationTab">The destination tab.</param>
        /// <param name="asReference">if set to <c>true</c> will use source module directly, else will create new module info by source module.</param>
        /// <param name="includeAllTabsMobules">if set to <c>true</c> will include modules which shown on all pages, this is used when create localized copy.</param>
        void CopyModules(TabInfo sourceTab, TabInfo destinationTab, bool asReference, bool includeAllTabsMobules);

        /// <summary>
        /// This method provides two functions:
        /// 1. Check and ensure that the "Module" content item type exists - if not create it
        /// 2. add a content item.
        /// </summary>
        /// <param name = "module">the module to add a content item for.</param>
        void CreateContentItem(ModuleInfo module);

        /// <summary>
        /// DeleteAllModules deletes all instances of a Module (from a collection).  This overload
        /// soft deletes the instances.
        /// </summary>
        ///     <param name="moduleId">The Id of the module to copy.</param>
        ///     <param name="tabId">The Id of the current tab.</param>
        ///     <param name="fromTabs">An ArrayList of TabItem objects.</param>
        void DeleteAllModules(int moduleId, int tabId, List<TabInfo> fromTabs, bool softDelete, bool includeCurrent, bool deleteBaseModule);

        /// <summary>
        /// Delete a module instance permanently from the database.
        /// </summary>
        /// <param name="moduleId">ID of the module instance.</param>
        void DeleteModule(int moduleId);

        /// <summary>
        /// Delete a Setting of a module instance.
        /// </summary>
        /// <param name="moduleId">ID of the affected module.</param>
        /// <param name="settingName">Name of the setting to be deleted.</param>
        void DeleteModuleSetting(int moduleId, string settingName);

        /// <summary>
        /// Delete a module reference permanently from the database.
        /// if there are no other references, the module instance is deleted as well.
        /// </summary>
        /// <param name="tabId">ID of the page.</param>
        /// <param name="moduleId">ID of the module instance.</param>
        /// <param name="softDelete">A flag that determines whether the instance should be soft-deleted.</param>
        void DeleteTabModule(int tabId, int moduleId, bool softDelete);

        /// <summary>
        /// Delete a specific setting of a tabmodule reference.
        /// </summary>
        /// <param name="tabModuleId">ID of the affected tabmodule.</param>
        /// <param name="settingName">Name of the setting to remove.</param>
        void DeleteTabModuleSetting(int tabModuleId, string settingName);

        /// <summary>
        /// Des the localize module.
        /// </summary>
        /// <param name="sourceModule">The source module.</param>
        /// <returns>new module id.</returns>
        int DeLocalizeModule(ModuleInfo sourceModule);

        /// <summary>
        /// get info of all modules in any portal of the installation.
        /// </summary>
        /// <returns>moduleInfo of all modules.</returns>
        /// <remarks>created for upgrade purposes.</remarks>
        ArrayList GetAllModules();

        /// <summary>
        /// get Module objects of a portal, either only those, to be placed on all tabs or not.
        /// </summary>
        /// <param name="portalID">ID of the portal.</param>
        /// <param name="allTabs">specify, whether to return modules to be shown on all tabs or those to be shown on specified tabs.</param>
        /// <returns>ArrayList of TabModuleInfo objects.</returns>
        ArrayList GetAllTabsModules(int portalID, bool allTabs);

        /// <summary>
        ///   get TabModule objects that are linked to a particular ModuleID.
        /// </summary>
        /// <param name = "moduleID">ID of the module.</param>
        /// <returns>ArrayList of TabModuleInfo objects.</returns>
        ArrayList GetAllTabsModulesByModuleID(int moduleID);

        /// <summary>
        /// Gets the module.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="ignoreCache">Optionally bypass the cache.</param>
        /// <returns>module info.</returns>
        ModuleInfo GetModule(int moduleId, int tabId, bool ignoreCache);

        /// <summary>
        ///   get Module by specific locale.
        /// </summary>
        /// <param name = "ModuleId">ID of the module.</param>
        /// <param name = "tabid">ID of the tab.</param>
        /// <param name = "portalId">ID of the portal.</param>
        /// <param name = "locale">The wanted locale.</param>
        /// <returns>ModuleInfo associated to submitted locale.</returns>
        ModuleInfo GetModuleByCulture(int ModuleId, int tabid, int portalId, Locale locale);

        /// <summary>
        /// Get ModuleInfo object of first module instance with a given name of the module definition.
        /// </summary>
        /// <param name="portalId">ID of the portal, where to look for the module.</param>
        /// <param name="definitionName">The name of module definition (NOTE: this looks at <see cref="ModuleDefinitionInfo.DefinitionName"/>, not <see cref="ModuleDefinitionInfo.FriendlyName"/>).</param>
        /// <returns>ModuleInfo of first module instance.</returns>
        /// <remarks>preferably used for admin and host modules.</remarks>
        ModuleInfo GetModuleByDefinition(int portalId, string definitionName);

        /// <summary>
        ///   get a Module object.
        /// </summary>
        /// <param name = "uniqueID"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        ModuleInfo GetModuleByUniqueID(Guid uniqueID);

        /// <summary>
        /// get all Module objects of a portal.
        /// </summary>
        /// <param name="portalID">ID of the portal.</param>
        /// <returns>ArrayList of ModuleInfo objects.</returns>
        ArrayList GetModules(int portalID);

        /// <summary>
        /// Gets the modules by definition.
        /// </summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="definitionName">The name of the module definition.</param>
        /// <returns>module collection.</returns>
        ArrayList GetModulesByDefinition(int portalID, string definitionName);

        /// <summary>
        /// Gets the modules by DesktopModuleId.
        /// </summary>
        /// <param name="desktopModuleId">The Desktop Module Id.</param>
        /// <returns>module collection.</returns>
        ArrayList GetModulesByDesktopModuleId(int desktopModuleId);

        /// <summary>
        /// For a portal get a list of all active module and tabmodule references that are Searchable
        /// either by inheriting from ModuleSearchBase or implementing the older ISearchable interface.
        /// </summary>
        /// <param name="portalID">ID of the portal to be searched.</param>
        /// <returns>Arraylist of ModuleInfo for modules supporting search.</returns>
        ArrayList GetSearchModules(int portalID);

        /// <summary>
        ///   get a Module object.
        /// </summary>
        /// <param name = "tabModuleID">ID of the tabmodule.</param>
        /// <returns>An ModuleInfo object.</returns>
        ModuleInfo GetTabModule(int tabModuleID);

        /// <summary>
        /// Get all Module references on a tab.
        /// </summary>
        /// <param name="tabId"></param>
        /// <returns>Dictionary of ModuleID and ModuleInfo.</returns>
        Dictionary<int, ModuleInfo> GetTabModules(int tabId);

        /// <summary>
        ///   Get a list of all TabModule references of a module instance.
        /// </summary>
        /// <param name = "moduleID">ID of the Module.</param>
        /// <returns>ArrayList of ModuleInfo.</returns>
        IList<ModuleInfo> GetTabModulesByModule(int moduleID);

        void InitialModulePermission(ModuleInfo module, int tabId, int permissionType);

        void LocalizeModule(ModuleInfo sourceModule, Locale locale);

        /// <summary>
        /// MoveModule moes a Module from one Tab to another including all the
        ///     TabModule settings.
        /// </summary>
        ///     <param name="moduleId">The Id of the module to move.</param>
        ///     <param name="fromTabId">The Id of the source tab.</param>
        ///     <param name="toTabId">The Id of the destination tab.</param>
        ///     <param name="toPaneName">The name of the Pane on the destination tab where the module will end up.</param>
        void MoveModule(int moduleId, int fromTabId, int toTabId, string toPaneName);

        /// <summary>
        /// Restores the module.
        /// </summary>
        /// <param name="objModule">The module.</param>
        void RestoreModule(ModuleInfo objModule);

        /// <summary>
        /// Update module settings and permissions in database from ModuleInfo.
        /// </summary>
        /// <param name="module">ModuleInfo of the module to update.</param>
        void UpdateModule(ModuleInfo module);

        /// <summary>
        /// set/change the module position within a pane on a page.
        /// </summary>
        /// <param name="TabId">ID of the page.</param>
        /// <param name="ModuleId">ID of the module on the page.</param>
        /// <param name="ModuleOrder">position within the controls list on page, -1 if to be added at the end.</param>
        /// <param name="PaneName">name of the pane, the module is placed in on the page.</param>
        void UpdateModuleOrder(int TabId, int ModuleId, int ModuleOrder, string PaneName);

        /// <summary>
        /// Adds or updates a module's setting value.
        /// </summary>
        /// <param name="moduleId">ID of the tabmodule, the setting belongs to.</param>
        /// <param name="settingName">name of the setting property.</param>
        /// <param name="settingValue">value of the setting (String).</param>
        /// <remarks>Empty SettingValue will remove the setting.</remarks>
        void UpdateModuleSetting(int moduleId, string settingName, string settingValue);

        /// <summary>
        /// set/change all module's positions within a page.
        /// </summary>
        /// <param name="TabId">ID of the page.</param>
        void UpdateTabModuleOrder(int TabId);

        /// <summary>
        /// Adds or updates a tab module's setting value.
        /// </summary>
        /// <param name="tabModuleId">ID of the tabmodule, the setting belongs to.</param>
        /// <param name="settingName">name of the setting property.</param>
        /// <param name="settingValue">value of the setting (String).</param>
        /// <remarks>Empty SettingValue will remove the setting.</remarks>
        void UpdateTabModuleSetting(int tabModuleId, string settingName, string settingValue);

        /// <summary>
        /// Updates the translation status.
        /// </summary>
        /// <param name="localizedModule">The localized module.</param>
        /// <param name="isTranslated">if set to <c>true</c> will mark the module as translated].</param>
        void UpdateTranslationStatus(ModuleInfo localizedModule, bool isTranslated);

        /// <summary>
        /// Check if a ModuleInfo belongs to the referenced Tab or not.
        /// </summary>
        /// <param name="module">A ModuleInfo object to be checked.</param>
        /// <returns>True is TabId points to a different tab from initial Tab where the module was added. Otherwise, False.</returns>
        bool IsSharedModule(ModuleInfo module);

        /// <summary>
        /// Get the Tab ID corresponding to the initial Tab where the module was added.
        /// </summary>
        /// <param name="module">A ModuleInfo object to be checked.</param>
        /// <returns>The Tab Id from initial Tab where the module was added.</returns>
        int GetMasterTabId(ModuleInfo module);
    }
}
