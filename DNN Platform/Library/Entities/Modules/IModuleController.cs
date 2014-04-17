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
using System;
using System.Collections;
using System.Collections.Generic;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Modules
{
    /// <summary>
    /// Do not implement.  This interface is only implemented by the DotNetNuke core framework. Outside the framework it should used as a type and for unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface IModuleController
    {
        /// <summary>
        /// add a module to a page
        /// </summary>
        /// <param name="module">moduleInfo for the module to create</param>
        /// <returns>ID of the created module</returns>
        int AddModule(ModuleInfo module);

        /// <summary>
        /// Clears the module cache based on the page (tabid)
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
        /// This method provides two functions:
        /// 1. Check and ensure that the "Module" content item type exists - if not create it
        /// 2. add a content item
        /// </summary>
        /// <param name = "module">the module to add a content item for</param>
        void CreateContentItem(ModuleInfo module);

        /// <summary>
        /// DeleteAllModules deletes all instances of a Module (from a collection).  This overload
        /// soft deletes the instances
        /// </summary>
        ///	<param name="moduleId">The Id of the module to copy</param>
        ///	<param name="tabId">The Id of the current tab</param>
        ///	<param name="fromTabs">An ArrayList of TabItem objects</param>
        void DeleteAllModules(int moduleId, int tabId, List<TabInfo> fromTabs, bool softDelete, bool includeCurrent, bool deleteBaseModule);

        /// <summary>
        /// Delete a module instance permanently from the database
        /// </summary>
        /// <param name="moduleId">ID of the module instance</param>
        void DeleteModule(int moduleId);

        /// <summary>
        /// Delete a Setting of a module instance
        /// </summary>
        /// <param name="moduleId">ID of the affected module</param>
        /// <param name="settingName">Name of the setting to be deleted</param>
        void DeleteModuleSetting(int moduleId, string settingName);

        /// <summary>
        /// Delete a module reference permanently from the database.
        /// if there are no other references, the module instance is deleted as well
        /// </summary>
        /// <param name="tabId">ID of the page</param>
        /// <param name="moduleId">ID of the module instance</param>
        /// <param name="softDelete">A flag that determines whether the instance should be soft-deleted</param>
        void DeleteTabModule(int tabId, int moduleId, bool softDelete);

        /// <summary>
        /// Delete a specific setting of a tabmodule reference
        /// </summary>
        /// <param name="tabModuleId">ID of the affected tabmodule</param>
        /// <param name="settingName">Name of the setting to remove</param>
        void DeleteTabModuleSetting(int tabModuleId, string settingName);

        /// <summary>
        /// Des the localize module.
        /// </summary>
        /// <param name="sourceModule">The source module.</param>
        /// <returns>new module id</returns>
        int DeLocalizeModule(ModuleInfo sourceModule);

        /// <summary>
        /// get info of all modules in any portal of the installation
        /// </summary>
        /// <returns>moduleInfo of all modules</returns>
        /// <remarks>created for upgrade purposes</remarks>
        ArrayList GetAllModules();

        /// <summary>
        /// get Module objects of a portal, either only those, to be placed on all tabs or not
        /// </summary>
        /// <param name="portalID">ID of the portal</param>
        /// <param name="allTabs">specify, whether to return modules to be shown on all tabs or those to be shown on specified tabs</param>
        /// <returns>ArrayList of TabModuleInfo objects</returns>
        ArrayList GetAllTabsModules(int portalID, bool allTabs);

        /// <summary>
        ///   get TabModule objects that are linked to a particular ModuleID
        /// </summary>
        /// <param name = "moduleID">ID of the module</param>
        /// <returns>ArrayList of TabModuleInfo objects</returns>
        ArrayList GetAllTabsModulesByModuleID(int moduleID);

        /// <summary>
        /// Gets the module.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="tabId">The tab ID.</param>
        /// <param name="ignoreCache">Optionally bypass the cache</param>
        /// <returns>module info</returns>
        ModuleInfo GetModule(int moduleId, int tabId, bool ignoreCache);

        /// <summary>
        ///   get Module by specific locale
        /// </summary>
        /// <param name = "ModuleId">ID of the module</param>
        /// <param name = "tabid">ID of the tab</param>
        /// <param name = "portalId">ID of the portal</param>
        /// <param name = "locale">The wanted locale</param>
        /// <returns>ModuleInfo associated to submitted locale</returns>
        ModuleInfo GetModuleByCulture(int ModuleId, int tabid, int portalId, Locale locale);

        /// <summary>
        /// Get ModuleInfo object of first module instance with a given friendly name of the module definition
        /// </summary>
        /// <param name="portalId">ID of the portal, where to look for the module</param>
        /// <param name="friendlyName">friendly name of module definition</param>
        /// <returns>ModuleInfo of first module instance</returns>
        /// <remarks>preferably used for admin and host modules</remarks>
        ModuleInfo GetModuleByDefinition(int portalId, string friendlyName);

        /// <summary>
        ///   get a Module object
        /// </summary>
        /// <param name = "uniqueID"></param>
        /// <returns></returns>
        /// <remarks>
        /// </remarks>
        ModuleInfo GetModuleByUniqueID(Guid uniqueID);

        /// <summary>
        /// get all Module objects of a portal
        /// </summary>
        /// <param name="portalID">ID of the portal</param>
        /// <returns>ArrayList of ModuleInfo objects</returns>
        ArrayList GetModules(int portalID);
        
        
        
        /// <summary>
        /// Adds or updates a module's setting value
        /// </summary>
        /// <param name="moduleId">ID of the tabmodule, the setting belongs to</param>
        /// <param name="settingName">name of the setting property</param>
        /// <param name="settingValue">value of the setting (String).</param>
        /// <remarks>Empty SettingValue will remove the setting</remarks>
        void UpdateModuleSetting(int moduleId, string settingName, string settingValue);

        /// <summary>
        /// Adds or updates a tab module's setting value
        /// </summary>
        /// <param name="tabModuleId">ID of the tabmodule, the setting belongs to</param>
        /// <param name="settingName">name of the setting property</param>
        /// <param name="settingValue">value of the setting (String).</param>
        /// <remarks>Empty SettingValue will remove the setting</remarks>
        void UpdateTabModuleSetting(int tabModuleId, string settingName, string settingValue);
    }
}