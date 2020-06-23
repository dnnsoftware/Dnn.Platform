// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    using System.Collections.Generic;

    /// <summary>
    /// Interface controller responsible to manage the tab version details.
    /// </summary>
    public interface ITabVersionDetailController
    {
        /// <summary>
        /// Gets a Tab Version Detail object of an existing Tab Version.
        /// </summary>
        /// <param name="tabVersionDetailId">The Tab Version Detail Id to be get.</param>
        /// <param name="tabVersionId">The Tab Version Id to be queried.</param>
        /// <param name="ignoreCache">If true, the method will not use the Caching Storage.</param>
        /// <returns>TabVersionDetail object filled with specific data.</returns>
        TabVersionDetail GetTabVersionDetail(int tabVersionDetailId, int tabVersionId, bool ignoreCache = false);

        /// <summary>
        /// Get all Tab Version Details of a existing version and earlier.
        /// </summary>
        /// <param name="tabId">The Tab Id to be queried.</param>
        /// <param name="version">The Tab Id to be queried.</param>
        /// <returns>List of TabVersionDetail objects.</returns>
        IEnumerable<TabVersionDetail> GetVersionHistory(int tabId, int version);

        /// <summary>
        /// Gets all TabVersionDetail objects of an existing TabVersion.
        /// </summary>
        /// <param name="tabVersionId">Tha TabVersion Id to be quiered.</param>
        /// <param name="ignoreCache">If true, the method will not use the Caching Storage.</param>
        /// <returns>List of TabVersionDetail objects.</returns>
        IEnumerable<TabVersionDetail> GetTabVersionDetails(int tabVersionId, bool ignoreCache = false);

        /// <summary>
        /// Saves a Tab Version Detail object. Adds or updates an existing one.
        /// </summary>
        /// <param name="tabVersionDetail">TabVersionDetail object to be saved.</param>
        void SaveTabVersionDetail(TabVersionDetail tabVersionDetail);

        /// <summary>
        /// Saves a TabVersionDetail object. Adds or updates an existing one.
        /// </summary>
        /// <param name="tabVersionDetail">TabVersionDetail object to be saved.</param>
        /// <param name="createdByUserId">User Id who creates the TabVersionDetail.</param>
        void SaveTabVersionDetail(TabVersionDetail tabVersionDetail, int createdByUserId);

        /// <summary>
        /// Saves a TabVersionDetail object. Adds or updates an existing one.
        /// </summary>
        /// <param name="tabVersionDetail">TabVersionDetail object to be saved.</param>
        /// <param name="createdByUserId">User Id who created the TabVersionDetail.</param>
        /// <param name="modifiedByUserId">User Id who modifies the TabVersionDetail.</param>
        void SaveTabVersionDetail(TabVersionDetail tabVersionDetail, int createdByUserId, int modifiedByUserId);

        /// <summary>
        /// Deletes a TabVersionDetail.
        /// </summary>
        /// <param name="tabVersionId">The TabVersion Id to be queried.</param>
        /// <param name="tabVersionDetailId">The TabVersionDetail Id to be deleted.</param>
        void DeleteTabVersionDetail(int tabVersionId, int tabVersionDetailId);

        /// <summary>
        /// Clears the tab version cache based on the tab version identifier.
        /// </summary>
        /// <param name="tabVersionId">The tab version identifier.</param>
        void ClearCache(int tabVersionId);
    }
}
