// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Roles
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Globalization;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Membership;

    /// -----------------------------------------------------------------------------
    /// Project:    DotNetNuke
    /// Namespace:  DotNetNuke.Security.Membership
    /// Class:      DNNRoleProvider
    /// -----------------------------------------------------------------------------
    /// <summary>
    /// The DNNRoleProvider overrides the default MembershipProvider to provide
    /// a purely DNN Membership Component implementation.
    /// </summary>
    /// <remarks>
    /// </remarks>
    /// -----------------------------------------------------------------------------
    public class DNNRoleProvider : RoleProvider
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(DNNRoleProvider));
        private readonly DataProvider dataProvider = DataProvider.Instance();

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateRole persists a Role to the Data Store.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="role">The role to persist to the Data Store.</param>
        /// <returns>A Boolean indicating success or failure.</returns>
        /// -----------------------------------------------------------------------------
        public override bool CreateRole(RoleInfo role)
        {
            Requires.NotNegative("PortalId", role.PortalID);

            try
            {
                role.RoleID =
                    Convert.ToInt32(this.dataProvider.AddRole(
                        role.PortalID,
                        role.RoleGroupID,
                        role.RoleName.Trim(),
                        (role.Description ?? string.Empty).Trim(),
                        role.ServiceFee,
                        role.BillingPeriod.ToString(CultureInfo.InvariantCulture),
                        role.BillingFrequency,
                        role.TrialFee,
                        role.TrialPeriod,
                        role.TrialFrequency,
                        role.IsPublic,
                        role.AutoAssignment,
                        role.RSVPCode,
                        role.IconFile,
                        UserController.Instance.GetCurrentUserInfo().UserID,
                        (int)role.Status,
                        (int)role.SecurityMode,
                        role.IsSystemRole));
            }
            catch (SqlException e)
            {
                throw new ArgumentException(e.ToString());
            }

            return true;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteRole deletes a Role from the Data Store.
        /// </summary>
        /// <param name="role">The role to delete from the Data Store.</param>
        /// -----------------------------------------------------------------------------
        public override void DeleteRole(RoleInfo role)
        {
            this.dataProvider.DeleteRole(role.RoleID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get the roles for a portal.
        /// </summary>
        /// <param name="portalId">Id of the portal (If -1 all roles for all portals are
        /// retrieved.</param>
        /// <returns>An ArrayList of RoleInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public override ArrayList GetRoles(int portalId)
        {
            var arrRoles = CBO.FillCollection(
                portalId == Null.NullInteger
                                        ? this.dataProvider.GetRoles()
                                        : this.dataProvider.GetPortalRoles(portalId), typeof(RoleInfo));
            return arrRoles;
        }

        public override IList<RoleInfo> GetRolesBasicSearch(int portalID, int pageSize, string filterBy)
        {
            return CBO.FillCollection<RoleInfo>(this.dataProvider.GetRolesBasicSearch(portalID, -1, pageSize, filterBy));
        }

        public override IDictionary<string, string> GetRoleSettings(int roleId)
        {
            var settings = new Dictionary<string, string> { };
            using (IDataReader dr = this.dataProvider.GetRoleSettings(roleId))
            {
                while (dr.Read())
                {
                    settings.Add(dr["SettingName"].ToString(), dr["SettingValue"].ToString());
                }

                dr.Close();
            }

            return settings;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Update a role.
        /// </summary>
        /// <param name="role">The role to update.</param>
        /// -----------------------------------------------------------------------------
        public override void UpdateRole(RoleInfo role)
        {
            this.dataProvider.UpdateRole(
                role.RoleID,
                role.RoleGroupID,
                role.RoleName.Trim(),
                (role.Description ?? string.Empty).Trim(),
                role.ServiceFee,
                role.BillingPeriod.ToString(CultureInfo.InvariantCulture),
                role.BillingFrequency,
                role.TrialFee,
                role.TrialPeriod,
                role.TrialFrequency,
                role.IsPublic,
                role.AutoAssignment,
                role.RSVPCode,
                role.IconFile,
                UserController.Instance.GetCurrentUserInfo().UserID,
                (int)role.Status,
                (int)role.SecurityMode,
                role.IsSystemRole);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Update the role settings for a role.
        /// </summary>
        /// <param name="role">The role to update.</param>
        /// -----------------------------------------------------------------------------
        public override void UpdateRoleSettings(RoleInfo role)
        {
            var currentSettings = this.GetRoleSettings(role.RoleID);

            foreach (var setting in role.Settings)
            {
                if (!currentSettings.ContainsKey(setting.Key) || currentSettings[setting.Key] != setting.Value)
                {
                    this.dataProvider.UpdateRoleSetting(role.RoleID, setting.Key, setting.Value, UserController.Instance.GetCurrentUserInfo().UserID);
                }
            }
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// AddUserToRole adds a User to a Role.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="user">The user to add.</param>
        /// <param name="userRole">The role to add the user to.</param>
        /// <returns>A Boolean indicating success or failure.</returns>
        /// -----------------------------------------------------------------------------
        public override bool AddUserToRole(int portalId, UserInfo user, UserRoleInfo userRole)
        {
            bool createStatus = true;
            try
            {
                // Add UserRole to DNN
                this.AddDNNUserRole(userRole);
            }
            catch (Exception exc)
            {
                // Clear User (duplicate User information)
                Logger.Error(exc);

                createStatus = false;
            }

            return createStatus;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUserRole gets a User/Role object from the Data Store.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="userId">The Id of the User.</param>
        /// <param name="roleId">The Id of the Role.</param>
        /// <returns>The UserRoleInfo object.</returns>
        /// -----------------------------------------------------------------------------
        public override UserRoleInfo GetUserRole(int portalId, int userId, int roleId)
        {
            return CBO.FillObject<UserRoleInfo>(this.dataProvider.GetUserRole(portalId, userId, roleId));
        }

        /// <summary>
        /// Gets a list of UserRoles for the user.
        /// </summary>
        /// <param name="user">A UserInfo object representaing the user.</param>
        /// <param name="includePrivate">Include private roles.</param>
        /// <returns>A list of UserRoleInfo objects.</returns>
        public override IList<UserRoleInfo> GetUserRoles(UserInfo user, bool includePrivate)
        {
            Requires.NotNull("user", user);

            return CBO.FillCollection<UserRoleInfo>(includePrivate
                    ? this.dataProvider.GetUserRoles(user.PortalID, user.UserID)
                    : this.dataProvider.GetServices(user.PortalID, user.UserID));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetUserRoles gets a collection of User/Role objects from the Data Store.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="userName">The user to fetch roles for.</param>
        /// <param name="roleName">The role to fetch users for.</param>
        /// <returns>An ArrayList of UserRoleInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public override ArrayList GetUserRoles(int portalId, string userName, string roleName)
        {
            return CBO.FillCollection(this.dataProvider.GetUserRolesByUsername(portalId, userName, roleName), typeof(UserRoleInfo));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get the users in a role (as User objects).
        /// </summary>
        /// <param name="portalId">Id of the portal (If -1 all roles for all portals are
        /// retrieved.</param>
        /// <param name="roleName">The role to fetch users for.</param>
        /// <returns>An ArrayList of UserInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public override ArrayList GetUsersByRoleName(int portalId, string roleName)
        {
            return AspNetMembershipProvider.FillUserCollection(portalId, this.dataProvider.GetUsersByRolename(portalId, roleName));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Remove a User from a Role.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="user">The user to remove.</param>
        /// <param name="userRole">The role to remove the user from.</param>
        /// -----------------------------------------------------------------------------
        public override void RemoveUserFromRole(int portalId, UserInfo user, UserRoleInfo userRole)
        {
            this.dataProvider.DeleteUserRole(userRole.UserID, userRole.RoleID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates a User/Role.
        /// </summary>
        /// <param name="userRole">The User/Role to update.</param>
        /// -----------------------------------------------------------------------------
        public override void UpdateUserRole(UserRoleInfo userRole)
        {
            this.dataProvider.UpdateUserRole(
                userRole.UserRoleID,
                (int)userRole.Status, userRole.IsOwner,
                userRole.EffectiveDate, userRole.ExpiryDate,
                UserController.Instance.GetCurrentUserInfo().UserID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// CreateRoleGroup persists a RoleGroup to the Data Store.
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="roleGroup">The RoleGroup to persist to the Data Store.</param>
        /// <returns>The Id of the new role.</returns>
        /// -----------------------------------------------------------------------------
        public override int CreateRoleGroup(RoleGroupInfo roleGroup)
        {
            var roleGroupId = this.dataProvider.AddRoleGroup(roleGroup.PortalID, roleGroup.RoleGroupName.Trim(),
                                                        (roleGroup.Description ?? string.Empty).Trim(),
                                                        UserController.Instance.GetCurrentUserInfo().UserID);
            this.ClearRoleGroupCache(roleGroup.PortalID);
            return roleGroupId;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// DeleteRoleGroup deletes a RoleGroup from the Data Store.
        /// </summary>
        /// <param name="roleGroup">The RoleGroup to delete from the Data Store.</param>
        /// -----------------------------------------------------------------------------
        public override void DeleteRoleGroup(RoleGroupInfo roleGroup)
        {
            this.dataProvider.DeleteRoleGroup(roleGroup.RoleGroupID);
            this.ClearRoleGroupCache(roleGroup.PortalID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// GetRoleGroup gets a RoleGroup from the Data Store.
        /// </summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <param name="roleGroupId">The Id of the RoleGroup to retrieve.</param>
        /// <returns>A RoleGroupInfo object.</returns>
        /// -----------------------------------------------------------------------------
        public override RoleGroupInfo GetRoleGroup(int portalId, int roleGroupId)
        {
            return this.GetRoleGroupsInternal(portalId).SingleOrDefault(r => r.RoleGroupID == roleGroupId);
        }

        public override RoleGroupInfo GetRoleGroupByName(int portalId, string roleGroupName)
        {
            roleGroupName = roleGroupName.Trim();
            return this.GetRoleGroupsInternal(portalId).SingleOrDefault(
                r => roleGroupName.Equals(r.RoleGroupName.Trim(), StringComparison.InvariantCultureIgnoreCase));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Get the RoleGroups for a portal.
        /// </summary>
        /// <param name="portalId">Id of the portal.</param>
        /// <returns>An ArrayList of RoleGroupInfo objects.</returns>
        /// -----------------------------------------------------------------------------
        public override ArrayList GetRoleGroups(int portalId)
        {
            return new ArrayList(this.GetRoleGroupsInternal(portalId).ToList());
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Update a RoleGroup.
        /// </summary>
        /// <param name="roleGroup">The RoleGroup to update.</param>
        /// -----------------------------------------------------------------------------
        public override void UpdateRoleGroup(RoleGroupInfo roleGroup)
        {
            this.dataProvider.UpdateRoleGroup(roleGroup.RoleGroupID, roleGroup.RoleGroupName.Trim(),
                (roleGroup.Description ?? string.Empty).Trim(), UserController.Instance.GetCurrentUserInfo().UserID);
            this.ClearRoleGroupCache(roleGroup.PortalID);
        }

        private void AddDNNUserRole(UserRoleInfo userRole)
        {
            // Add UserRole to DNN
            userRole.UserRoleID = Convert.ToInt32(this.dataProvider.AddUserRole(userRole.PortalID, userRole.UserID, userRole.RoleID,
                                                                (int)userRole.Status, userRole.IsOwner,
                                                                userRole.EffectiveDate, userRole.ExpiryDate,
                                                                UserController.Instance.GetCurrentUserInfo().UserID));
        }

        private void ClearRoleGroupCache(int portalId)
        {
            DataCache.ClearCache(this.GetRoleGroupsCacheKey(portalId));
        }

        private string GetRoleGroupsCacheKey(int portalId)
        {
            return string.Format(DataCache.RoleGroupsCacheKey, portalId);
        }

        private IEnumerable<RoleGroupInfo> GetRoleGroupsInternal(int portalId)
        {
            var cacheArgs = new CacheItemArgs(
                this.GetRoleGroupsCacheKey(portalId),
                DataCache.RoleGroupsCacheTimeOut,
                DataCache.RoleGroupsCachePriority);

            return CBO.GetCachedObject<IEnumerable<RoleGroupInfo>>(cacheArgs, c =>
                                            CBO.FillCollection<RoleGroupInfo>(this.dataProvider.GetRoleGroups(portalId)));
        }
    }
}
