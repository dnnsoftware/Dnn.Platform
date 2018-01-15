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
    /// Interface controller responsible to manage the tab version details
    /// </summary>
    public interface ITabVersionDetailController
    {
        /// <summary>
        /// Gets a Tab Version Detail object of an existing Tab Version
        /// </summary>     
        /// <param name="tabVersionDetailId">The Tab Version Detail Id to be get</param>        
        /// <param name="tabVersionId">The Tab Version Id to be queried</param>        
        /// <param name="ignoreCache">If true, the method will not use the Caching Storage</param>        
        /// <returns>TabVersionDetail object filled with specific data</returns>
        TabVersionDetail GetTabVersionDetail(int tabVersionDetailId, int tabVersionId, bool ignoreCache = false);

        /// <summary>
        /// Get all Tab Version Details of a existing version and earlier
        /// </summary>
        /// <param name="tabId">The Tab Id to be queried</param>
        /// <param name="version">The Tab Id to be queried</param>        
        /// <returns>List of TabVersionDetail objects</returns>
        IEnumerable<TabVersionDetail> GetVersionHistory(int tabId, int version);
        
        /// <summary>
        /// Gets all TabVersionDetail objects of an existing TabVersion
        /// </summary>        
        /// <param name="tabVersionId">Tha TabVersion Id to be quiered</param>                
        /// <param name="ignoreCache">If true, the method will not use the Caching Storage</param>        
        /// <returns>List of TabVersionDetail objects</returns>
        IEnumerable<TabVersionDetail> GetTabVersionDetails(int tabVersionId, bool ignoreCache = false);

        /// <summary>
        /// Saves a Tab Version Detail object. Adds or updates an existing one
        /// </summary>      
        /// <param name="tabVersionDetail">TabVersionDetail object to be saved</param>        
        void SaveTabVersionDetail(TabVersionDetail tabVersionDetail);
        
        /// <summary>
        /// Saves a TabVersionDetail object. Adds or updates an existing one
        /// </summary>        
        /// <param name="tabVersionDetail">TabVersionDetail object to be saved</param>        
        /// <param name="createdByUserId">User Id who creates the TabVersionDetail</param>        
        void SaveTabVersionDetail(TabVersionDetail tabVersionDetail, int createdByUserId);

        /// <summary>
        /// Saves a TabVersionDetail object. Adds or updates an existing one
        /// </summary>        
        /// <param name="tabVersionDetail">TabVersionDetail object to be saved</param>        
        /// <param name="createdByUserId">User Id who created the TabVersionDetail</param>        
        /// <param name="modifiedByUserId">User Id who modifies the TabVersionDetail</param>        
        void SaveTabVersionDetail(TabVersionDetail tabVersionDetail, int createdByUserId, int modifiedByUserId);

        /// <summary>
        /// Deletes a TabVersionDetail
        /// </summary>
        /// <param name="tabVersionId">The TabVersion Id to be queried</param>
        /// <param name="tabVersionDetailId">The TabVersionDetail Id to be deleted</param>
        void DeleteTabVersionDetail(int tabVersionId, int tabVersionDetailId);

        /// <summary>
        /// Clears the tab version cache based on the tab version identifier.
        /// </summary>
        /// <param name="tabVersionId">The tab version identifier.</param>
        void ClearCache(int tabVersionId);
    }
}
