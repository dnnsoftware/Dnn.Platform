// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Roles
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Users;

    public interface IRoleController
    {
        /// <summary>
        ///     Adds a role.
        /// </summary>
        /// <param name="role">The Role to Add.</param>
        /// <returns>The Id of the new role.</returns>
        int AddRole(RoleInfo role);

        /// <summary>
        ///     Adds a role.
        /// </summary>
        /// <param name="role">The Role to Add.</param>
        /// <param name="addToExistUsers">Add this role on all exist users if auto assignment is true.</param>
        /// <returns>The Id of the new role.</returns>
        int AddRole(RoleInfo role, bool addToExistUsers);

        /// <summary>
        ///     Adds a User to a Role.
        /// </summary>
        /// <remarks>Overload adds Effective Date.</remarks>
        /// <param name="portalId">The Id of the Portal.</param>
        /// <param name="userId">The Id of the User.</param>
        /// <param name="roleId">The Id of the Role.</param>
        /// <param name="status">The status of the Role.</param>
        /// <param name="isOwner">If the user is the owner of the Role.</param>
        /// <param name="effectiveDate">The expiry Date of the Role membership.</param>
        /// <param name="expiryDate">The expiry Date of the Role membership.</param>
        void AddUserRole(int portalId, int userId, int roleId, RoleStatus status, bool isOwner, DateTime effectiveDate, DateTime expiryDate);

        /// <summary>
        ///     Clears Roles cache for the passed portal ID and for the default ID (-1) as well.
        /// </summary>
        /// <param name="portalId">Id of the portal.</param>
        void ClearRoleCache(int portalId);

        /// <summary>
        ///     Deletes a role.
        /// </summary>
        /// <param name="role">The Role to delete.</param>
        void DeleteRole(RoleInfo role);

        /// <summary>
        ///     Fetch a single role based on a predicate.
        /// </summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="predicate">The predicate (criteria) required.</param>
        /// <returns>A RoleInfo object.</returns>
        RoleInfo GetRole(int portalId, Func<RoleInfo, bool> predicate);

        /// <summary>
        ///     Fetch a single role.
        /// </summary>
        /// <param name="roleId">the roleid.</param>
        /// <param name="portalId">the portalid.</param>
        /// <returns>>A RoleInfo object.</returns>
        RoleInfo GetRoleById(int portalId, int roleId);

        /// <summary>
        ///     Fetch a role by rolename and portal id.
        /// </summary>
        /// <param name="portalId">the portalid.</param>
        /// <param name="roleName">the role name.</param>
        /// <returns>>A RoleInfo object.</returns>
        RoleInfo GetRoleByName(int portalId, string roleName);

        /// <summary>
        ///     Obtains a list of roles from the cache (or for the database if the cache has expired).
        /// </summary>
        /// <param name="portalId">The id of the portal.</param>
        /// <returns>The list of roles.</returns>
        IList<RoleInfo> GetRoles(int portalId);

        /// <summary>
        ///     Get the roles based on a predicate.
        /// </summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="predicate">The predicate (criteria) required.</param>
        /// <returns>A List of RoleInfo objects.</returns>
        IList<RoleInfo> GetRoles(int portalId, Func<RoleInfo, bool> predicate);

        /// <summary>
        ///     get a list of roles based on progressive search.
        /// </summary>
        /// <param name="portalId">the id of the portal.</param>
        /// <param name="pageSize">the number of items to return.</param>
        /// <param name="filterBy">the text used to trim data.</param>
        /// <returns></returns>
        IList<RoleInfo> GetRolesBasicSearch(int portalId, int pageSize, string filterBy);

        /// <summary>
        ///     Gets the settings for a role.
        /// </summary>
        /// <param name="roleId">Id of the role.</param>
        /// <returns>A Dictionary of settings.</returns>
        IDictionary<string, string> GetRoleSettings(int roleId);

        /// <summary>
        ///     Gets a User/Role.
        /// </summary>
        /// <param name="portalId">The Id of the Portal.</param>
        /// <param name="userId">The Id of the user.</param>
        /// <param name="roleId">The Id of the Role.</param>
        /// <returns>A UserRoleInfo object.</returns>
        UserRoleInfo GetUserRole(int portalId, int userId, int roleId);

        /// <summary>
        ///     Gets a list of UserRoles for the user.
        /// </summary>
        /// <param name="user">A UserInfo object representaing the user.</param>
        /// <param name="includePrivate">Include private roles.</param>
        /// <returns>A list of UserRoleInfo objects.</returns>
        IList<UserRoleInfo> GetUserRoles(UserInfo user, bool includePrivate);

        /// <summary>
        ///     Gets a list of UserRoles for the user.
        /// </summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="userName">The user to fetch roles for.</param>
        /// <param name="roleName">The role to fetch users for.</param>
        /// <returns>A list of UserRoleInfo objects.</returns>
        IList<UserRoleInfo> GetUserRoles(int portalId, string userName, string roleName);

        /// <summary>
        ///     Get the users in a role (as User objects).
        /// </summary>
        /// <param name="portalId">
        ///     Id of the portal (If -1 all roles for all portals are
        ///     retrieved.
        /// </param>
        /// <param name="roleName">The role to fetch users for.</param>
        /// <returns>A List of UserInfo objects.</returns>
        IList<UserInfo> GetUsersByRole(int portalId, string roleName);

        /// <summary>
        ///     Persists a role to the Data Store.
        /// </summary>
        /// <param name="role">The role to persist.</param>
        void UpdateRole(RoleInfo role);

        /// <summary>
        ///     Persists a role to the Data Store.
        /// </summary>
        /// <param name="role">The role to persist.</param>
        /// <param name="addToExistUsers">Add this role on all exist users if auto assignment is true.</param>
        void UpdateRole(RoleInfo role, bool addToExistUsers);

        /// <summary>
        ///     Update the role settings.
        /// </summary>
        /// <param name="role">The Role.</param>
        /// <param name="clearCache">A flag that indicates whether the cache should be cleared.</param>
        void UpdateRoleSettings(RoleInfo role, bool clearCache);

        /// <summary>
        ///     Updates a Service (UserRole).
        /// </summary>
        /// <param name="portalId">The Id of the Portal.</param>
        /// <param name="userId">The Id of the User.</param>
        /// <param name="roleId">The Id of the Role.</param>
        /// <param name="status">The status of the Role.</param>
        /// <param name="isOwner">If the user is the owner of the Role.</param>
        /// <param name="cancel">A flag that indicates whether to cancel (delete) the userrole.</param>
        void UpdateUserRole(int portalId, int userId, int roleId, RoleStatus status, bool isOwner, bool cancel);
    }
}
