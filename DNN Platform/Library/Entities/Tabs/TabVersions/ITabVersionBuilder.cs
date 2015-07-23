﻿#region Copyright
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
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    public interface ITabVersionBuilder
    {
        /// <summary>
        /// Creates a new Tab Version checking current portal settings
        /// </summary>        
        /// <remarks>This method will need the Portal Id from the current context</remarks>
        /// <param name="tabId">Tab Id whose version will be added</param>
        /// <param name="createdByUserId">User Id which creates the version</param>
        /// <returns>TabVersion filled with the new version data</returns>
        TabVersion CreateNewVersion(int tabId, int createdByUserId);

        /// <summary>
        /// Creates a new Tab Version 
        /// </summary>
        /// <param name="tabId">Tab Id whose version will be added</param>
        /// <param name="createdByUserId">User Id which creates the version</param>
        /// <returns>TabVersion filled with the new version data</returns>
        TabVersion CreateNewVersion(int portalid, int tabId, int createdByUserId);


        /// <summary>
        /// Publish a Tab Version
        /// </summary>
        /// <param name="portalId">Portal Id where the version is </param>
        /// <param name="tabId">Tab Id whose version will be published</param>
        /// <param name="createdByUserId">User Id which publishes the version</param>        
        void Publish(int portalId, int tabId, int createdByUserId);

        /// <summary>
        /// Discards a Tab Version. If the tab only has an unpublished version, the page will keep but with no content and not published.
        /// </summary>
        /// <param name="tabId">Tab Id whose version will be discarded </param>
        /// <param name="createdByUserId">User Id which discards the version</param>
        void Discard(int tabId, int createdByUserId);
        
        /// <summary>
        /// Get all Modules Info associated with an specific version
        /// </summary>        
        /// <param name="tabId">Tab Id to be checked</param>
        /// <param name="versionNumber">Version Number whose modules will be get</param>
        /// <returns>List of ModuleInfo objects</returns>
        IEnumerable<ModuleInfo> GetVersionModules(int tabId, int versionNumber);

        /// <summary>
        /// Get the current pusblished version of the page
        /// </summary>
        /// <param name="tabId">The Tab Id to be queried</param>  
        /// <param name="ignoreCache">If true, the method will not use the Caching Storage</param>  
        /// <returns>TabVersion filled with the current version data</returns>
        /// <remarks>If Tab has not a published version yet, it will return null</remarks>
        TabVersion GetCurrentVersion(int tabId, bool ignoreCache = false);
        
        /// <summary>
        /// Get the unpublished version or Null if Tab has not any unpublished version
        /// </summary>
        /// <param name="tabId"></param>
        /// <returns>TabVersion filled with the unpublished version data</returns>
        /// <remarks>If Tab has not an unpublished version yet, it will return null</remarks>
        TabVersion GetUnPublishedVersion(int tabId);
        
        /// <summary>
        /// Get all ModuleInfo objects associated with the unpublished version of a page.
        /// </summary>        
        /// <param name="tabId"></param>
        /// <returns>List of ModuleInfo objects</returns>
        IEnumerable<ModuleInfo> GetUnPublishedVersionModules(int tabId);

        /// <summary>
        /// Get all Modules Info associated with the last published version of the page
        /// </summary>        
        /// <param name="tabId">Tab Id w</param>
        /// <returns>List of ModuleInfo objects</returns>
        IEnumerable<ModuleInfo> GetCurrentModules(int tabId);
        
        /// <summary>
        /// Rolls back an existing version
        /// </summary>
        /// <param name="tabId">The Tab Id to be queried</param>  
        /// <param name="createdByUserId">User Id which rolls back the version</param>
        /// <param name="versionNumber">Version Number of the version to be rolled back</param>
        TabVersion RollBackVesion(int tabId, int createdByUserId, int versionNumber);

        /// <summary>
        /// Deletes an existing Tab Version
        /// </summary>
        /// <param name="tabId">The Tab Id to be queried</param>  
        /// <param name="createdByUserId">User Id which deletes the version</param>
        /// <param name="version">Version Number of the version to be deleted</param>
        void DeleteVersion(int tabId, int createdByUserId, int version);

        /// <summary>
        /// Setup a first version for existing tab with modules. This method is used to create version 1 for pages created when versioning was not enabled
        /// </summary>
        /// <param name="portalId">portalId</param>
        /// <param name="tabId">tabId</param>
        void SetupFirstVersionForExistingTab(int portalId, int tabId);

        /// <summary>
        /// Get the latest version or 1 if module is not versionable
        /// </summary>
        /// <param name="module">The ModuleInfo to be queried</param>  
        /// <returns>The latest version of the module</returns>
        int GetModuleContentLatestVersion(ModuleInfo module);

    }
}
