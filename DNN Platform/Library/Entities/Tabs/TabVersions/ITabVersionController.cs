#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    /// <summary>
    /// Controller interface responsible to manage tab versions
    /// </summary>
    public interface ITabVersionController
    {
        /// <summary>
        /// Gets a Tab Version object of an existing Tab
        /// </summary>     
        /// <param name="tabVersionId">The Tab Version Id to be get</param>        
        /// <param name="tabId">The Tab Id to be queried</param>        
        /// <param name="ignoreCache">If true, the method will not use the Caching Storage</param>     
        /// <returns>TabVersion filled with the specific version data</returns>
        TabVersion GetTabVersion(int tabVersionId, int tabId, bool ignoreCache = false);

        /// <summary>
        /// Gets all Tab Versions of an existing Tab
        /// </summary>        
        /// <param name="tabId">Tha Tab ID to be quiered</param>                
        /// <param name="ignoreCache">If true, the method will not use the Caching Storage</param>        
        /// <returns>List of TabVersion objects</returns>
        IEnumerable<TabVersion> GetTabVersions(int tabId, bool ignoreCache = false);

        /// <summary>
        /// Saves a Tab Version object. Adds or updates an existing one
        /// </summary>        
        /// <param name="tabVersion">TabVersion object to be saved</param>        
        /// <param name="createdByUserId">User Id who creates the TabVersion</param>   
        void SaveTabVersion(TabVersion tabVersion, int createdByUserId);

        /// <summary>
        /// Saves a Tab Version object. Adds or updates an existing one
        /// </summary>        
        /// <param name="tabVersion">TabVersion object to be saved</param>        
        /// <param name="createdByUserId">User Id who creates the TabVersion</param>   
        /// <param name="modifiedByUserId">User Id who modifies the TabVersion</param>
        void SaveTabVersion(TabVersion tabVersion, int createdByUserId, int modifiedByUserId);
        
        /// <summary>
        /// Saves a Tab Version object. Adds or updates an existing one
        /// </summary>        
        /// <param name="tabVersion">TabVersion object to be saved</param>        
        void SaveTabVersion(TabVersion tabVersion);

        /// <summary>
        /// Creates a new version for a existing Tab
        /// </summary>
        /// <param name="tabId">The Tab Id to be queried</param>
        /// <param name="createdByUserId">User Id who creates the version</param>
        /// <param name="isPublished">If true, the version is automatically published</param>
        /// <returns>TabVersion filled with the new version data</returns>
        TabVersion CreateTabVersion(int tabId, int createdByUserId, bool isPublished = false);
        
        /// <summary>
        /// Deletes a Tab Version
        /// </summary>
        /// <param name="tabId">The Tab Id to be queried</param>
        /// <param name="tabVersionId">The TabVersion Id to be deleted</param>
        void DeleteTabVersion(int tabId, int tabVersionId);

        /// <summary>
        /// Deletes a Tab Version details for a module
        /// </summary>
        /// <param name="moduleId">The Module Id to be queried</param>
        void DeleteTabVersionDetailByModule(int moduleId);
    }
}
