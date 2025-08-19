// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Roles
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Xml;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.Services.Messaging.Data;
    using DotNetNuke.Services.Search.Entities;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>The RoleController class provides Business Layer methods for Roles.</summary>
    public class RoleController : ServiceLocator<IRoleController, RoleController>, IRoleController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RoleController));
        private static readonly string[] UserRoleActionsCaption = ["ASSIGNMENT", "UPDATE", "UNASSIGNMENT"];
        private readonly RoleProvider roleProvider;
        private readonly IHostSettings hostSettings;
        private readonly IEventLogger eventLogger;
        private readonly IPortalController portalController;
        private readonly IUserController userController;
        private readonly IEventManager eventManager;
        private readonly IFileManager fileManager;
        private readonly IFolderManager folderManager;
        private readonly DataProvider dataProvider;

        /// <summary>Initializes a new instance of the <see cref="RoleController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public RoleController()
            : this(null, null, null, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="RoleController"/> class.</summary>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="eventManager">The event manager.</param>
        /// <param name="fileManager">The file manager.</param>
        /// <param name="folderManager">The folder manager.</param>
        /// <param name="dataProvider">The data provider.</param>
        public RoleController(RoleProvider roleProvider, IHostSettings hostSettings, IEventLogger eventLogger, IPortalController portalController, IUserController userController, IEventManager eventManager, IFileManager fileManager, IFolderManager folderManager, DataProvider dataProvider)
        {
            this.roleProvider = roleProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<RoleProvider>();
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
            this.portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
            this.userController = userController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>();
            this.eventManager = eventManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventManager>();
            this.fileManager = fileManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFileManager>();
            this.folderManager = folderManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<IFolderManager>();
            this.dataProvider = dataProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<DataProvider>();
        }

        private enum UserRoleActions
        {
            Add = 0,
            Update = 1,
            Delete = 2,
        }

        /// <summary>Adds a Role Group.</summary>
        /// <param name="objRoleGroupInfo">The RoleGroup to Add.</param>
        /// <returns>The ID of the new role.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with RoleProvider. Scheduled removal in v12.0.0.")]
        public static int AddRoleGroup(RoleGroupInfo objRoleGroupInfo)
            => AddRoleGroup(
                Globals.GetCurrentServiceProvider().GetRequiredService<RoleProvider>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>().GetCurrentSettings(),
                objRoleGroupInfo);

        /// <summary>Adds a Role Group.</summary>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="roleGroupInfo">The RoleGroup to Add.</param>
        /// <returns>The ID of the new role.</returns>
        public static int AddRoleGroup(RoleProvider roleProvider, IEventLogger eventLogger, IUserController userController, IPortalSettings portalSettings, RoleGroupInfo roleGroupInfo)
        {
            var id = roleProvider.CreateRoleGroup(roleGroupInfo);
            eventLogger.AddLog(
                roleGroupInfo,
                portalSettings,
                userController.GetCurrentUserInfo().UserID,
                string.Empty,
                EventLogType.USER_ROLE_CREATED);
            return id;
        }

        /// <summary>Adds a User to a Role.</summary>
        /// <param name="user">The user to assign.</param>
        /// <param name="role">The role to add.</param>
        /// <param name="portalSettings">The PortalSettings of the Portal.</param>
        /// <param name="status">RoleStatus.</param>
        /// <param name="effectiveDate">The effective Date of the Role membership.</param>
        /// <param name="expiryDate">The expiry Date of the Role membership.</param>
        /// <param name="notifyUser">A flag that indicates whether the user should be notified.</param>
        /// <param name="isOwner">A flag that indicates whether this user should be one of the group owners.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IRoleController. Scheduled removal in v12.0.0.")]
        public static void AddUserRole(
            UserInfo user,
            RoleInfo role,
            PortalSettings portalSettings,
            RoleStatus status,
            DateTime effectiveDate,
            DateTime expiryDate,
            bool notifyUser,
            bool isOwner)
            => AddUserRole(
                Globals.GetCurrentServiceProvider().GetRequiredService<IRoleController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                user,
                role,
                portalSettings,
                status,
                effectiveDate,
                expiryDate,
                notifyUser,
                isOwner);

        /// <summary>Adds a User to a Role.</summary>
        /// <param name="roleController">The role controller.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="user">The user to assign.</param>
        /// <param name="role">The role to add.</param>
        /// <param name="portalSettings">The PortalSettings of the Portal.</param>
        /// <param name="status">RoleStatus.</param>
        /// <param name="effectiveDate">The effective Date of the Role membership.</param>
        /// <param name="expiryDate">The expiry Date of the Role membership.</param>
        /// <param name="notifyUser">A flag that indicates whether the user should be notified.</param>
        /// <param name="isOwner">A flag that indicates whether this user should be one of the group owners.</param>
        public static void AddUserRole(IRoleController roleController, IUserController userController, IEventLogger eventLogger, UserInfo user, RoleInfo role, PortalSettings portalSettings, RoleStatus status, DateTime effectiveDate, DateTime expiryDate, bool notifyUser, bool isOwner)
        {
            var userRole = roleController.GetUserRole(portalSettings.PortalId, user.UserID, role.RoleID);

            // update assignment
            roleController.AddUserRole(portalSettings.PortalId, user.UserID, role.RoleID, status, isOwner, effectiveDate, expiryDate);

            UserController.UpdateUser(portalSettings.PortalId, user);
            if (userRole == null)
            {
                eventLogger.AddLog("Role", role.RoleName, portalSettings, user.UserID, EventLogType.USER_ROLE_CREATED);

                // send notification
                if (notifyUser)
                {
                    SendNotification(roleController, userController, user, role, portalSettings, UserRoleActions.Add);
                }
            }
            else
            {
                eventLogger.AddLog("Role", role.RoleName, portalSettings, user.UserID, EventLogType.USER_ROLE_UPDATED);
                if (notifyUser)
                {
                    roleController.GetUserRole(portalSettings.PortalId, user.UserID, role.RoleID);
                    SendNotification(roleController, userController, user, role, portalSettings, UserRoleActions.Update);
                }
            }

            // Remove the UserInfo from the Cache, as it has been modified
            DataCache.ClearUserCache(portalSettings.PortalId, user.Username);
        }

        /// <summary>Determines if the specified user can be removed from a role.</summary>
        /// <remarks>Roles such as "Registered Users" and "Administrators" can only be removed in certain circumstances.</remarks>
        /// <param name="portalSettings">A <see cref="PortalSettings">PortalSettings</see> structure representing the current portal settings.</param>
        /// <param name="userId">The ID of the User that should be checked for role removability.</param>
        /// <param name="roleId">The ID of the Role that should be checked for removability.</param>
        /// <returns><see langword="true"/> if the role can be removed, otherwise <see langword="false"/>.</returns>
        public static bool CanRemoveUserFromRole(PortalSettings portalSettings, int userId, int roleId)
        {
            // [DNN-4285] Refactored this check into a method for use in SecurityRoles.ascx.vb
            // HACK: Duplicated in CanRemoveUserFromRole(PortalInfo, Integer, Integer) method below
            // changes to this method should be reflected in the other method as well
            return !((portalSettings.AdministratorId == userId && portalSettings.AdministratorRoleId == roleId) || portalSettings.RegisteredRoleId == roleId);
        }

        /// <summary>Determines if the specified user can be removed from a role.</summary>
        /// <remarks>Roles such as "Registered Users" and "Administrators" can only be removed in certain circumstances.</remarks>
        /// <param name="portalInfo">A <see cref="PortalInfo">PortalInfo</see> structure representing the current portal.</param>
        /// <param name="userId">The ID of the User.</param>
        /// <param name="roleId">The ID of the Role that should be checked for removability.</param>
        /// <returns><see langword="true"/> if the role can be removed, otherwise <see langword="false"/>.</returns>
        public static bool CanRemoveUserFromRole(PortalInfo portalInfo, int userId, int roleId)
        {
            // [DNN-4285] Refactored this check into a method for use in SecurityRoles.ascx.vb
            // HACK: Duplicated in CanRemoveUserFromRole(PortalSettings, Integer, Integer) method above
            // changes to this method should be reflected in the other method as well
            return !((portalInfo.AdministratorId == userId && portalInfo.AdministratorRoleId == roleId) || portalInfo.RegisteredRoleId == roleId);
        }

        /// <summary>Deletes a Role Group.</summary>
        /// <param name="portalID">The portal ID.</param>
        /// <param name="roleGroupId">The role group ID.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with RoleProvider. Scheduled removal in v12.0.0.")]
        public static void DeleteRoleGroup(int portalID, int roleGroupId)
            => DeleteRoleGroup(
                Globals.GetCurrentServiceProvider().GetRequiredService<RoleProvider>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>().GetCurrentSettings(),
                portalID,
                roleGroupId);

        /// <summary>Deletes a Role Group.</summary>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="portalSettings">The portal settings for the current portal.</param>
        /// <param name="portalId">The portal ID of the role group.</param>
        /// <param name="roleGroupId">The role group ID.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with RoleProvider. Scheduled removal in v12.0.0.")]
        public static void DeleteRoleGroup(RoleProvider roleProvider, IEventLogger eventLogger, IUserController userController, IPortalSettings portalSettings, int portalId, int roleGroupId)
            => DeleteRoleGroup(roleProvider, eventLogger, userController, portalSettings, GetRoleGroup(portalId, roleGroupId));

        /// <summary>Deletes a Role Group.</summary>
        /// <param name="objRoleGroupInfo">The RoleGroup to Delete.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with RoleProvider. Scheduled removal in v12.0.0.")]
        public static void DeleteRoleGroup(RoleGroupInfo objRoleGroupInfo)
            => DeleteRoleGroup(
                Globals.GetCurrentServiceProvider().GetRequiredService<RoleProvider>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>().GetCurrentSettings(),
                objRoleGroupInfo);

        /// <summary>Deletes a Role Group.</summary>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="roleGroupInfo">The RoleGroup to Delete.</param>
        public static void DeleteRoleGroup(RoleProvider roleProvider, IEventLogger eventLogger, IUserController userController, IPortalSettings portalSettings, RoleGroupInfo roleGroupInfo)
        {
            roleProvider.DeleteRoleGroup(roleGroupInfo);
            eventLogger.AddLog(
                roleGroupInfo,
                portalSettings,
                userController.GetCurrentUserInfo().UserID,
                string.Empty,
                EventLogType.USER_ROLE_DELETED);
        }

        /// <summary>Removes a User from a Role.</summary>
        /// <param name="objUser">The user to remove.</param>
        /// <param name="role">The role to remove the use from.</param>
        /// <param name="portalSettings">The PortalSettings of the Portal.</param>
        /// <param name="notifyUser">A flag that indicates whether the user should be notified.</param>
        /// <returns><see langword="true"/> if the role was deleted, otherwise <see langword="false"/>.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IRoleController. Scheduled removal in v12.0.0.")]
        public static bool DeleteUserRole(UserInfo objUser, RoleInfo role, PortalSettings portalSettings, bool notifyUser)
            => DeleteUserRole(
                Globals.GetCurrentServiceProvider().GetRequiredService<RoleProvider>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IRoleController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventManager>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                objUser,
                role,
                portalSettings,
                notifyUser);

        /// <summary>Removes a User from a Role.</summary>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="roleController">The role controller.</param>
        /// <param name="eventManager">The event manager.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="user">The user to remove.</param>
        /// <param name="role">The role to remove the use from.</param>
        /// <param name="portalSettings">The PortalSettings of the Portal.</param>
        /// <param name="notifyUser">A flag that indicates whether the user should be notified.</param>
        /// <returns><see langword="true"/> if the role was deleted, otherwise <see langword="false"/>.</returns>
        public static bool DeleteUserRole(RoleProvider roleProvider, IRoleController roleController, IEventManager eventManager, IPortalController portalController, IUserController userController, IEventLogger eventLogger, UserInfo user, RoleInfo role, PortalSettings portalSettings, bool notifyUser)
        {
            var canDelete = DeleteUserRoleInternal(roleProvider, roleController, eventManager, portalController, userController, eventLogger, portalSettings.PortalId, user.UserID, role.RoleID);
            if (!canDelete)
            {
                return false;
            }

            if (notifyUser)
            {
                SendNotification(roleController, userController, user, role, portalSettings, UserRoleActions.Delete);
            }

            return true;
        }

        /// <summary>Fetch a single RoleGroup.</summary>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="roleGroupId">Role Group ID.</param>
        /// <returns>A <see cref="RoleGroupInfo"/> instance or <see langword="null"/>.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with RoleProvider. Scheduled removal in v12.0.0.")]
        public static RoleGroupInfo GetRoleGroup(int portalId, int roleGroupId)
            => GetRoleGroup(Globals.GetCurrentServiceProvider().GetRequiredService<RoleProvider>(), portalId, roleGroupId);

        /// <summary>Fetch a single RoleGroup.</summary>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="roleGroupId">Role Group ID.</param>
        /// <returns>A <see cref="RoleGroupInfo"/> instance or <see langword="null"/>.</returns>
        public static RoleGroupInfo GetRoleGroup(RoleProvider roleProvider, int portalId, int roleGroupId)
        {
            return roleProvider.GetRoleGroup(portalId, roleGroupId);
        }

        /// <summary>Fetch a single RoleGroup by Name.</summary>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="roleGroupName">Role Group Name.</param>
        /// <returns>A <see cref="RoleGroupInfo"/> instance or <see langword="null"/>.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with RoleProvider. Scheduled removal in v12.0.0.")]
        public static RoleGroupInfo GetRoleGroupByName(int portalId, string roleGroupName)
            => GetRoleGroupByName(Globals.GetCurrentServiceProvider().GetRequiredService<RoleProvider>(), portalId, roleGroupName);

        /// <summary>Fetch a single RoleGroup by Name.</summary>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="roleGroupName">Role Group Name.</param>
        /// <returns>A <see cref="RoleGroupInfo"/> instance or <see langword="null"/>.</returns>
        public static RoleGroupInfo GetRoleGroupByName(RoleProvider roleProvider, int portalId, string roleGroupName)
        {
            return roleProvider.GetRoleGroupByName(portalId, roleGroupName);
        }

        /// <summary>Gets an ArrayList of RoleGroups.</summary>
        /// <param name="portalID">The ID of the Portal.</param>
        /// <returns>An ArrayList of RoleGroups.</returns>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with RoleProvider. Scheduled removal in v12.0.0.")]
        public static ArrayList GetRoleGroups(int portalID)
            => GetRoleGroups(Globals.GetCurrentServiceProvider().GetRequiredService<RoleProvider>(), portalID);

        /// <summary>Gets an ArrayList of RoleGroups.</summary>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <returns>An ArrayList of RoleGroups.</returns>
        public static ArrayList GetRoleGroups(RoleProvider roleProvider, int portalId)
        {
            return roleProvider.GetRoleGroups(portalId);
        }

        /// <summary>Serializes the role groups.</summary>
        /// <param name="writer">An XmlWriter.</param>
        /// <param name="portalID">The ID of the Portal.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with RoleProvider. Scheduled removal in v12.0.0.")]
        public static void SerializeRoleGroups(XmlWriter writer, int portalID)
            => SerializeRoleGroups(Globals.GetCurrentServiceProvider().GetRequiredService<RoleProvider>(), writer, portalID);

        /// <summary>Serializes the role groups.</summary>
        /// <param name="roleProvider">A role provider.</param>
        /// <param name="writer">An XmlWriter.</param>
        /// <param name="portalId">The ID of the Portal.</param>
        public static void SerializeRoleGroups(RoleProvider roleProvider, XmlWriter writer, int portalId)
        {
            // Serialize Role Groups
            writer.WriteStartElement("rolegroups");
            foreach (RoleGroupInfo objRoleGroup in GetRoleGroups(roleProvider, portalId))
            {
                CBO.SerializeObject(objRoleGroup, writer);
            }

            // Serialize Global Roles
            var globalRoleGroup = new RoleGroupInfo(Null.NullInteger, portalId, true)
            {
                RoleGroupName = "GlobalRoles",
                Description = "A role group that represents the Global roles",
            };
            CBO.SerializeObject(globalRoleGroup, writer);
            writer.WriteEndElement();
        }

        /// <summary>Updates a Role Group.</summary>
        /// <param name="roleGroup">The RoleGroup to Update.</param>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with RoleProvider. Scheduled removal in v12.0.0.")]
        public static void UpdateRoleGroup(RoleGroupInfo roleGroup)
            => UpdateRoleGroup(
                Globals.GetCurrentServiceProvider().GetRequiredService<RoleProvider>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IRoleController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>().GetCurrentSettings(),
                roleGroup);

        /// <summary>Updates a Role Group.</summary>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="roleController">The role controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="portalSettings">The portal settings.</param>
        /// <param name="roleGroup">The RoleGroup to Update.</param>
        public static void UpdateRoleGroup(RoleProvider roleProvider, IRoleController roleController, IEventLogger eventLogger, IUserController userController, IPortalSettings portalSettings, RoleGroupInfo roleGroup)
        {
            UpdateRoleGroup(roleProvider, roleController, eventLogger, userController, portalSettings, roleGroup, false);
        }

        public static void UpdateRoleGroup(RoleGroupInfo roleGroup, bool includeRoles)
            => UpdateRoleGroup(
                Globals.GetCurrentServiceProvider().GetRequiredService<RoleProvider>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IRoleController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>().GetCurrentSettings(),
                roleGroup,
                includeRoles);

        public static void UpdateRoleGroup(RoleProvider roleProvider, IRoleController roleController, IEventLogger eventLogger, IUserController userController, IPortalSettings portalSettings, RoleGroupInfo roleGroup, bool includeRoles)
        {
            roleProvider.UpdateRoleGroup(roleGroup);
            eventLogger.AddLog(roleGroup, portalSettings, userController.GetCurrentUserInfo().UserID, string.Empty, EventLogType.USER_ROLE_UPDATED);

            if (!includeRoles)
            {
                return;
            }

            foreach (var role in roleGroup.Roles.Values)
            {
                roleController.UpdateRole(role);
                eventLogger.AddLog(role, portalSettings, userController.GetCurrentUserInfo().UserID, string.Empty, EventLogType.ROLE_UPDATED);
            }
        }

        /// <inheritdoc/>
        public int AddRole(RoleInfo role)
        {
            return ((IRoleController)this).AddRole(role, true);
        }

        /// <inheritdoc/>
        public void AddUserRole(int portalId, int userId, int roleId, RoleStatus status, bool isOwner, DateTime effectiveDate, DateTime expiryDate)
        {
            var user = UserController.GetUserById(portalId, userId);
            var userRole = this.GetUserRole(portalId, userId, roleId);
            if (userRole == null)
            {
                // Create new UserRole
                userRole = new UserRoleInfo
                {
                    UserID = userId,
                    RoleID = roleId,
                    PortalID = portalId,
                    Status = status,
                    IsOwner = isOwner,
                    EffectiveDate = effectiveDate,
                    ExpiryDate = expiryDate,
                };
                this.roleProvider.AddUserToRole(portalId, user, userRole);
                this.eventLogger.AddLog(userRole, this.portalController.GetCurrentSettings(), this.userController.GetCurrentUserInfo().UserID, string.Empty, EventLogType.USER_ROLE_CREATED);
            }
            else
            {
                userRole.Status = status;
                userRole.IsOwner = isOwner;
                userRole.EffectiveDate = effectiveDate;
                userRole.ExpiryDate = expiryDate;
                this.roleProvider.UpdateUserRole(userRole);
                this.eventLogger.AddLog(userRole, this.portalController.GetCurrentSettings(), this.userController.GetCurrentUserInfo().UserID, string.Empty, EventLogType.USER_ROLE_UPDATED);
            }

            this.eventManager.OnRoleJoined(new RoleEventArgs() { Role = this.GetRoleById(portalId, roleId), User = user });

            // Remove the UserInfo and Roles from the Cache, as they have been modified
            DataCache.ClearUserCache(portalId, user.Username);
            this.ClearRoleCache(portalId);
        }

        /// <inheritdoc/>
        public void ClearRoleCache(int portalId)
        {
            DataCache.RemoveCache(string.Format(DataCache.RolesCacheKey, portalId));
            if (portalId != Null.NullInteger)
            {
                DataCache.RemoveCache(string.Format(DataCache.RolesCacheKey, Null.NullInteger));
            }
        }

        /// <inheritdoc/>
        public void DeleteRole(RoleInfo role)
        {
            Requires.NotNull("role", role);

            this.AddMessage(role, EventLogType.ROLE_DELETED);

            if (role.SecurityMode != SecurityMode.SecurityRole)
            {
                // remove group artifacts
                var portalSettings = this.portalController.GetCurrentSettings();

                var groupFolder = this.folderManager.GetFolder(portalSettings.PortalId, $"Groups/{role.RoleID}");
                if (groupFolder != null)
                {
                    this.fileManager.DeleteFiles(this.folderManager.GetFiles(groupFolder));
                    this.folderManager.DeleteFolder(groupFolder);
                }

                JournalController.Instance.SoftDeleteJournalItemByGroupId(portalSettings.PortalId, role.RoleID);
            }

            // Get users before deleting role
            var users = role.UserCount > 0 ? this.GetUsersByRole(role.PortalID, role.RoleName) : Enumerable.Empty<UserInfo>();

            this.roleProvider.DeleteRole(role);

            this.eventManager.OnRoleDeleted(new RoleEventArgs() { Role = role });

            // Remove the UserInfo objects of users that have been members of the group from the cache, as they have been modified
            foreach (var user in users)
            {
                DataCache.ClearUserCache(role.PortalID, user.Username);
            }

            this.ClearRoleCache(role.PortalID);

            // queue remove role/group from search index
            var document = new SearchDocumentToDelete
            {
                // PortalId = role.PortalID,
                RoleId = role.RoleID, // this is unique and sufficient
            };

            this.dataProvider.AddSearchDeletedItems(document);
        }

        /// <inheritdoc/>
        public RoleInfo GetRole(int portalId, Func<RoleInfo, bool> predicate)
        {
            return this.GetRoles(portalId).Where(predicate).FirstOrDefault();
        }

        /// <inheritdoc/>
        public RoleInfo GetRoleById(int portalId, int roleId)
        {
            return this.GetRole(portalId, r => r.RoleID == roleId);
        }

        /// <inheritdoc/>
        public RoleInfo GetRoleByName(int portalId, string roleName)
        {
            roleName = roleName.Trim();
            return this.GetRoles(portalId).SingleOrDefault(r => roleName.Equals(r.RoleName.Trim(), StringComparison.InvariantCultureIgnoreCase) && r.PortalID == portalId);
        }

        /// <inheritdoc/>
        public IList<RoleInfo> GetRoles(int portalId)
        {
            var cacheKey = string.Format(DataCache.RolesCacheKey, portalId);
            return CBO.GetCachedObject<IList<RoleInfo>>(
                this.hostSettings,
                new CacheItemArgs(cacheKey, DataCache.RolesCacheTimeOut, DataCache.RolesCachePriority),
                c => this.roleProvider.GetRoles(portalId).Cast<RoleInfo>().ToList());
        }

        /// <inheritdoc/>
        public IList<RoleInfo> GetRoles(int portalId, Func<RoleInfo, bool> predicate)
        {
            return this.GetRoles(portalId).Where(predicate).ToList();
        }

        /// <inheritdoc/>
        public IList<RoleInfo> GetRolesBasicSearch(int portalId, int pageSize, string filterBy)
        {
            return this.roleProvider.GetRolesBasicSearch(portalId, pageSize, filterBy);
        }

        /// <inheritdoc/>
        public IDictionary<string, string> GetRoleSettings(int roleId)
        {
            return this.roleProvider.GetRoleSettings(roleId);
        }

        /// <summary>Gets a User/Role.</summary>
        /// <param name="portalId">The ID of the Portal.</param>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="roleId">The ID of the Role.</param>
        /// <returns>A UserRoleInfo object.</returns>
        public UserRoleInfo GetUserRole(int portalId, int userId, int roleId)
        {
            return this.roleProvider.GetUserRole(portalId, userId, roleId);
        }

        /// <summary>Gets a list of UserRoles for the user.</summary>
        /// <param name="user">A UserInfo object representing the user.</param>
        /// <param name="includePrivate">Include private roles.</param>
        /// <returns>A list of UserRoleInfo objects.</returns>
        public IList<UserRoleInfo> GetUserRoles(UserInfo user, bool includePrivate)
        {
            return this.roleProvider.GetUserRoles(user, includePrivate);
        }

        /// <inheritdoc/>
        public IList<UserRoleInfo> GetUserRoles(int portalId, string userName, string roleName)
        {
            return this.roleProvider.GetUserRoles(portalId, userName, roleName).Cast<UserRoleInfo>().ToList();
        }

        /// <inheritdoc/>
        public IList<UserInfo> GetUsersByRole(int portalId, string roleName)
        {
            return this.roleProvider.GetUsersByRoleName(portalId, roleName).Cast<UserInfo>().ToList();
        }

        /// <inheritdoc/>
        public void UpdateRole(RoleInfo role, bool addToExistUsers)
        {
            Requires.NotNull("role", role);

            this.roleProvider.UpdateRole(role);
            this.AddMessage(role, EventLogType.ROLE_UPDATED);

            if (addToExistUsers)
            {
                this.AutoAssignUsers(role);
            }

            this.ClearRoleCache(role.PortalID);

            this.eventManager.OnRoleUpdated(new RoleEventArgs() { Role = role });
        }

        /// <inheritdoc/>
        public void UpdateRoleSettings(RoleInfo role, bool clearCache)
        {
            this.roleProvider.UpdateRoleSettings(role);

            if (clearCache)
            {
                this.ClearRoleCache(role.PortalID);
            }
        }

        /// <inheritdoc/>
        public void UpdateUserRole(int portalId, int userId, int roleId, RoleStatus status, bool isOwner, bool cancel)
        {
            var user = UserController.GetUserById(portalId, userId);
            var userRole = this.GetUserRole(portalId, userId, roleId);
            if (cancel)
            {
                if (userRole != null && userRole.ServiceFee > 0.0 && userRole.IsTrialUsed)
                {
                    // Expire Role so we retain trial used data
                    userRole.ExpiryDate = DateTime.Now.AddDays(-1);
                    userRole.Status = status;
                    userRole.IsOwner = isOwner;
                    this.roleProvider.UpdateUserRole(userRole);
                    this.eventLogger.AddLog(userRole, this.portalController.GetCurrentSettings(), this.userController.GetCurrentUserInfo().UserID, string.Empty, EventLogType.USER_ROLE_UPDATED);
                }
                else
                {
                    // Delete Role
                    DeleteUserRoleInternal(this.roleProvider, this, this.eventManager, this.portalController, this.userController, this.eventLogger, portalId, userId, roleId);
                    this.eventLogger.AddLog(
                        "UserId",
                        userId.ToString(CultureInfo.InvariantCulture),
                        this.portalController.GetCurrentSettings(),
                        this.userController.GetCurrentUserInfo().UserID,
                        EventLogType.USER_ROLE_DELETED);
                }
            }
            else
            {
                var userRoleId = -1;
                var expiryDate = DateTime.Now;
                var effectiveDate = Null.NullDate;
                var isTrialUsed = false;
                var period = 0;
                var frequency = string.Empty;
                if (userRole != null)
                {
                    userRoleId = userRole.UserRoleID;
                    effectiveDate = userRole.EffectiveDate;
                    expiryDate = userRole.ExpiryDate;
                    isTrialUsed = userRole.IsTrialUsed;
                }

                var role = this.GetRole(portalId, r => r.RoleID == roleId);
                if (role != null)
                {
                    if (isTrialUsed == false && role.TrialFrequency != "N")
                    {
                        period = role.TrialPeriod;
                        frequency = role.TrialFrequency;
                    }
                    else
                    {
                        period = role.BillingPeriod;
                        frequency = role.BillingFrequency;
                    }
                }

                if (effectiveDate < DateTime.Now)
                {
                    effectiveDate = Null.NullDate;
                }

                if (expiryDate < DateTime.Now)
                {
                    expiryDate = DateTime.Now;
                }

                if (period == Null.NullInteger)
                {
                    expiryDate = Null.NullDate;
                }
                else
                {
                    expiryDate = frequency switch
                    {
                        "N" => Null.NullDate,
                        "O" => new DateTime(9999, 12, 31),
                        "D" => expiryDate.AddDays(period),
                        "W" => expiryDate.AddDays(period * 7),
                        "M" => expiryDate.AddMonths(period),
                        "Y" => expiryDate.AddYears(period),
                        _ => expiryDate,
                    };
                }

                if (userRoleId != -1 && userRole != null)
                {
                    userRole.ExpiryDate = expiryDate;
                    userRole.Status = status;
                    userRole.IsOwner = isOwner;
                    this.roleProvider.UpdateUserRole(userRole);
                    this.eventLogger.AddLog(userRole, this.portalController.GetCurrentSettings(), this.userController.GetCurrentUserInfo().UserID, string.Empty, EventLogType.USER_ROLE_UPDATED);
                }
                else
                {
                    this.AddUserRole(portalId, userId, roleId, status, isOwner, effectiveDate, expiryDate);
                }
            }

            // Remove the UserInfo from the Cache, as it has been modified
            DataCache.ClearUserCache(portalId, user.Username);
            this.ClearRoleCache(portalId);
        }

        /// <inheritdoc/>
        int IRoleController.AddRole(RoleInfo role, bool addToExistUsers)
        {
            Requires.NotNull("role", role);

            var roleId = -1;
            if (this.roleProvider.CreateRole(role))
            {
                this.AddMessage(role, EventLogType.ROLE_CREATED);
                if (addToExistUsers)
                {
                    this.AutoAssignUsers(role);
                }

                roleId = role.RoleID;

                this.ClearRoleCache(role.PortalID);

                this.eventManager.OnRoleCreated(new RoleEventArgs() { Role = role });
            }

            return roleId;
        }

        /// <inheritdoc/>
        void IRoleController.UpdateRole(RoleInfo role)
        {
            this.UpdateRole(role, true);
        }

        /// <summary>Completely remove all a user's roles for a specific portal. This method is used when anonymizing a user. </summary>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="roleController">The role controller.</param>
        /// <param name="user">User for which all roles must be deleted. The PortalId property is used to determine for which portal roles must be removed.</param>
        internal static void DeleteUserRoles(RoleProvider roleProvider, IRoleController roleController, UserInfo user)
        {
            var userRoles = roleController.GetUserRoles(user, true);
            foreach (var ur in userRoles.Where(r => r.PortalID == user.PortalID))
            {
                roleProvider.RemoveUserFromRole(user.PortalID, user, ur);
            }
        }

        /// <inheritdoc/>
        protected override Func<IRoleController> GetFactory()
        {
            return () => Globals.GetCurrentServiceProvider().GetRequiredService<IRoleController>();
        }

        private static bool DeleteUserRoleInternal(RoleProvider roleProvider, IRoleController roleController, IEventManager eventManager, IPortalController portalController, IUserController userController, IEventLogger eventLogger, int portalId, int userId, int roleId)
        {
            var user = UserController.GetUserById(portalId, userId);
            var userRole = roleController.GetUserRole(portalId, userId, roleId);
            var portal = portalController.GetPortal(portalId);
            if (portal == null || userRole == null)
            {
                return true;
            }

            if (!CanRemoveUserFromRole(portal, userId, roleId))
            {
                return false;
            }

            roleProvider.RemoveUserFromRole(portalId, user, userRole);
            eventLogger.AddLog(
                userRole,
                portalController.GetCurrentSettings(),
                userController.GetCurrentUserInfo().UserID,
                string.Empty,
                EventLogType.ROLE_UPDATED);

            // Remove the UserInfo from the Cache, as it has been modified
            DataCache.ClearUserCache(portalId, user.Username);
            roleController.ClearRoleCache(portalId);

            eventManager.OnRoleLeft(new RoleEventArgs() { Role = Instance.GetRoleById(portalId, roleId), User = user });
            return true;
        }

        private static void SendNotification(IRoleController roleController, IUserController userController, UserInfo user, RoleInfo role, PortalSettings portalSettings, UserRoleActions action)
        {
            var profile = userController.GetUserById(user.PortalID, user.UserID).Profile;
            var custom = new ArrayList { role.RoleName, role.Description };
            switch (action)
            {
                case UserRoleActions.Add:
                case UserRoleActions.Update:
                    var preferredLocale = profile.PreferredLocale;
                    if (string.IsNullOrEmpty(preferredLocale))
                    {
                        preferredLocale = portalSettings.DefaultLanguage;
                    }

                    var ci = new CultureInfo(preferredLocale);
                    var userRole = roleController.GetUserRole(portalSettings.PortalId, user.UserID, role.RoleID);
                    custom.Add(Null.IsNull(userRole.EffectiveDate)
                                   ? DateTime.Today.ToString("g", ci)
                                   : userRole.EffectiveDate.ToString("g", ci));
                    custom.Add(Null.IsNull(userRole.ExpiryDate) ? "-" : userRole.ExpiryDate.ToString("g", ci));
                    break;
                case UserRoleActions.Delete:
                    custom.Add(string.Empty);
                    break;
            }

            var message = new Message
            {
                FromUserID = portalSettings.AdministratorId,
                ToUserID = user.UserID,
                Subject =
                    Localization.GetSystemMessage(
                        profile.PreferredLocale,
                        portalSettings,
                        $"EMAIL_ROLE_{UserRoleActionsCaption[(int)action]}_SUBJECT",
                        user),
                Body = Localization.GetSystemMessage(
                    profile.PreferredLocale,
                    portalSettings,
                    $"EMAIL_ROLE_{UserRoleActionsCaption[(int)action]}_BODY",
                    user,
                    Localization.GlobalResourceFile,
                    custom),
                Status = MessageStatusType.Unread,
            };

            // _messagingController.SaveMessage(_message);
            Mail.SendEmail(portalSettings.Email, user.Email, message.Subject, message.Body);
        }

        private void AddMessage(RoleInfo roleInfo, EventLogType logType)
        {
            this.eventLogger.AddLog(
                roleInfo,
                this.portalController.GetCurrentSettings(),
                this.userController.GetCurrentUserInfo().UserID,
                string.Empty,
                logType);
        }

        private void AutoAssignUsers(RoleInfo role)
        {
            if (!role.AutoAssignment)
            {
                return;
            }

            // loop through users for portal and add to role
            foreach (UserInfo objUser in UserController.GetUsers(role.PortalID))
            {
                try
                {
                    this.AddUserRole(role.PortalID, objUser.UserID, role.RoleID, RoleStatus.Approved, false, Null.NullDate, Null.NullDate);
                }
                catch (Exception exc)
                {
                    // user already belongs to role
                    Logger.Error(exc);
                }
            }
        }
    }
}
