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
using DotNetNuke.Entities.Modules;

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    public interface ITabVersionMaker
    {
        /// <summary>
        /// Creates a new Tab Version checking current portal versioning settings
        /// </summary>        
        TabVersion CreateNewVersion(int tabId, int createdByUserId);

        /// <summary>
        /// Creates a new Tab Version 
        /// </summary>        
        TabVersion CreateNewVersion(int portalid, int tabId, int createdByUserID);


        /// <summary>
        /// Publish a Tab Version
        /// </summary>
        void Publish(int portalId, int tabId, int createdByUserId);

        /// <summary>
        /// Discards a Tab Version
        /// </summary>
        void Discard(int tabId, int createdByUserId);
        
        /// <summary>
        /// Get all Modules Info associated with an specific version
        /// </summary>        
        IEnumerable<ModuleInfo> GetVersionModules(int tabId, int version);

        /// <summary>
        /// Get the current pusblished version of the page
        /// </summary>
        /// <param name="tabId">The Tab Id to be queried</param>  
        TabVersion GetCurrentVersion(int tabId, bool ignoreCache = false);
        
        /// <summary>
        /// Get the unpublished version or Null if Tab has not any unpublished version
        /// </summary>
        /// <param name="tabId"></param>
        /// <returns></returns>
        TabVersion GetUnPublishedVersion(int tabId);
        
        /// <summary>
        /// Get all Modules Info associated with the last unpublished version of a page.
        /// </summary>        
        IEnumerable<ModuleInfo> GetUnPublishedVersionModules(int tabId);

        /// <summary>
        /// Get all Modules Info associated with the last published version of the page
        /// </summary>        
        IEnumerable<ModuleInfo> GetCurrentModules(int tabId);
        
        /// <summary>
        /// Rolls back an existing version
        /// </summary>
        TabVersion RollBackVesion(int tabId, int createdByUserId, int version);

        /// <summary>
        /// Deletes an existing Tab Version
        /// </summary>
        void DeleteVersion(int tabId, int createdByUserId, int version);

        /// <summary>
        /// Setup a first version for existing tab with modules. This method is used to create version 1 for pages created when versioning was not enabled
        /// </summary>
        /// <param name="portalId">portalId</param>
        /// <param name="tabId">tabId</param>
        void SetupFirstVersionForExistingTab(int portalId, int tabId);
    }
}
