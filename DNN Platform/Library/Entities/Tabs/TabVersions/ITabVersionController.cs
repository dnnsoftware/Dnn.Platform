// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    using System.Collections.Generic;

    /// <summary>
    /// Controller interface responsible to manage tab versions.
    /// </summary>
    public interface ITabVersionController
    {
        /// <summary>
        /// Gets a Tab Version object of an existing Tab.
        /// </summary>
        /// <param name="tabVersionId">The Tab Version Id to be get.</param>
        /// <param name="tabId">The Tab Id to be queried.</param>
        /// <param name="ignoreCache">If true, the method will not use the Caching Storage.</param>
        /// <returns>TabVersion filled with the specific version data.</returns>
        TabVersion GetTabVersion(int tabVersionId, int tabId, bool ignoreCache = false);

        /// <summary>
        /// Gets all Tab Versions of an existing Tab.
        /// </summary>
        /// <param name="tabId">Tha Tab ID to be quiered.</param>
        /// <param name="ignoreCache">If true, the method will not use the Caching Storage.</param>
        /// <returns>List of TabVersion objects.</returns>
        IEnumerable<TabVersion> GetTabVersions(int tabId, bool ignoreCache = false);

        /// <summary>
        /// Saves a Tab Version object. Adds or updates an existing one.
        /// </summary>
        /// <param name="tabVersion">TabVersion object to be saved.</param>
        /// <param name="createdByUserId">User Id who creates the TabVersion.</param>
        void SaveTabVersion(TabVersion tabVersion, int createdByUserId);

        /// <summary>
        /// Saves a Tab Version object. Adds or updates an existing one.
        /// </summary>
        /// <param name="tabVersion">TabVersion object to be saved.</param>
        /// <param name="createdByUserId">User Id who creates the TabVersion.</param>
        /// <param name="modifiedByUserId">User Id who modifies the TabVersion.</param>
        void SaveTabVersion(TabVersion tabVersion, int createdByUserId, int modifiedByUserId);

        /// <summary>
        /// Saves a Tab Version object. Adds or updates an existing one.
        /// </summary>
        /// <param name="tabVersion">TabVersion object to be saved.</param>
        void SaveTabVersion(TabVersion tabVersion);

        /// <summary>
        /// Creates a new version for a existing Tab.
        /// </summary>
        /// <param name="tabId">The Tab Id to be queried.</param>
        /// <param name="createdByUserId">User Id who creates the version.</param>
        /// <param name="isPublished">If true, the version is automatically published.</param>
        /// <returns>TabVersion filled with the new version data.</returns>
        TabVersion CreateTabVersion(int tabId, int createdByUserId, bool isPublished = false);

        /// <summary>
        /// Deletes a Tab Version.
        /// </summary>
        /// <param name="tabId">The Tab Id to be queried.</param>
        /// <param name="tabVersionId">The TabVersion Id to be deleted.</param>
        void DeleteTabVersion(int tabId, int tabVersionId);

        /// <summary>
        /// Deletes a Tab Version details for a module.
        /// </summary>
        /// <param name="moduleId">The Module Id to be queried.</param>
        void DeleteTabVersionDetailByModule(int moduleId);
    }
}
