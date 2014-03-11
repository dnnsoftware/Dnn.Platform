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
using System.Collections.Generic;
using DotNetNuke.Entities.Tabs;

namespace DotNetNuke.Entities.Modules.Internal
{
    /// <summary>
    /// Do not implement.  This interface is only implemented by the DotNetNuke core framework. Outside the framework it should used as a type and for unit test purposes only.
    /// There is no guarantee that this interface will not change.
    /// </summary>
    public interface IModuleController
    {
        /// <summary>
        /// Gets the module.
        /// </summary>
        /// <param name="moduleId">The module ID.</param>
        /// <param name="tabId">The tab ID.</param>
        /// <returns>module info</returns>
        ModuleInfo GetModule(int moduleId, int tabId);

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

         /// -----------------------------------------------------------------------------
        /// <summary>
        /// CopyTabModuleSettings copies the TabModuleSettings from one instance to another
        /// </summary>
        /// <remarks>
        /// </remarks>
        ///	<param name="fromModule">The module to copy from</param>
        ///	<param name="toModule">The module to copy to</param>
        void CopyTabModuleSettings(ModuleInfo fromModule, ModuleInfo toModule);

        /// <summary>
        /// This method provides two functions:
        /// 1. Check and ensure that the "Module" content item type exists - if not create it
        /// 2. add a content item
        /// </summary>
        /// <param name = "module">the module to add a content item for</param>
        void CreateContentItem(ModuleInfo module);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteAllModules deletes all instances of a Module (from a collection).  This overload
        /// soft deletes the instances
        /// </summary>
        ///	<param name="moduleId">The Id of the module to copy</param>
        ///	<param name="tabId">The Id of the current tab</param>
        ///	<param name="fromTabs">An ArrayList of TabItem objects</param>
        void DeleteAllModules(int moduleId, int tabId, List<TabInfo> fromTabs);


    }
}