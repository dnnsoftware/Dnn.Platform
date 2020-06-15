// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Roles.Internal
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("This class has been obsoleted in 7.3.0 - please use version in DotNetNuke.Security.Roles instead. Scheduled removal in v10.0.0.")]
    public interface IRoleController
    {
        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds a role.
        /// </summary>
        /// <param name="role">The Role to Add.</param>
        /// <returns>The Id of the new role.</returns>
        /// -----------------------------------------------------------------------------
        int AddRole(RoleInfo role);

        /// <summary>
        /// Adds a role.
        /// </summary>
        /// <param name="role">The Role to Add.</param>
        /// <param name="addToExistUsers">Add this role on all exist users if auto assignment is true.</param>
        /// <returns>The Id of the new role.</returns>
        int AddRole(RoleInfo role, bool addToExistUsers);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes a role.
        /// </summary>
        /// <param name="role">The Role to delete.</param>
        /// -----------------------------------------------------------------------------
        void DeleteRole(RoleInfo role);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fetch a single role based on a predicate.
        /// </summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="predicate">The predicate (criteria) required.</param>
        /// <returns>A RoleInfo object.</returns>
        /// -----------------------------------------------------------------------------
        RoleInfo GetRole(int portalId, Func<RoleInfo, bool> predicate);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Obtains a list of roles from the cache (or for the database if the cache has expired).
        /// </summary>
        /// <param name="portalId">The id of the portal.</param>
        /// <returns>The list of roles.</returns>
        /// -----------------------------------------------------------------------------
        IList<RoleInfo> GetRoles(int portalId);

        /// <summary>
        /// get a list of roles based on progressive search.
        /// </summary>
        /// <param name="portalID">the id of the portal.</param>
        /// <param name="pageSize">the number of items to return.</param>
        /// <param name="filterBy">the text used to trim data.</param>
        /// <returns></returns>
        IList<RoleInfo> GetRolesBasicSearch(int portalID, int pageSize, string filterBy);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get the roles based on a predicate.
        /// </summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="predicate">The predicate (criteria) required.</param>
        /// <returns>A List of RoleInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        IList<RoleInfo> GetRoles(int portalId, Func<RoleInfo, bool> predicate);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets the settings for a role.
        /// </summary>
        /// <param name="roleId">Id of the role.</param>
        /// <returns>A Dictionary of settings.</returns>
        /// -----------------------------------------------------------------------------
        IDictionary<string, string> GetRoleSettings(int roleId);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Persists a role to the Data Store.
        /// </summary>
        /// <param name="role">The role to persist.</param>
        /// -----------------------------------------------------------------------------
        void UpdateRole(RoleInfo role);

        /// <summary>
        /// Persists a role to the Data Store.
        /// </summary>
        /// <param name="role">The role to persist.</param>
        /// <param name="addToExistUsers">Add this role on all exist users if auto assignment is true.</param>
        void UpdateRole(RoleInfo role, bool addToExistUsers);

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Update the role settings.
        /// </summary>
        /// <param name="role">The Role.</param>
        /// <param name="clearCache">A flag that indicates whether the cache should be cleared.</param>
        /// -----------------------------------------------------------------------------
        void UpdateRoleSettings(RoleInfo role, bool clearCache);

        /// <summary>
        /// Clears Roles cache for the passed portal ID and for the default ID (-1) as well.
        /// </summary>
        /// <param name="portalId">Id of the portal.</param>
        void ClearRoleCache(int portalId);
    }
}
