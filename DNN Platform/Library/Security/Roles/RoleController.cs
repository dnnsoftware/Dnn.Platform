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

    using DotNetNuke.Common;
    using DotNetNuke.Common.Internal;
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
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.Services.Messaging.Data;
    using DotNetNuke.Services.Search.Entities;

    /// <summary>
    /// The RoleController class provides Business Layer methods for Roles.
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class RoleController : ServiceLocator<IRoleController, RoleController>, IRoleController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RoleController));
        private static readonly string[] UserRoleActionsCaption = { "ASSIGNMENT", "UPDATE", "UNASSIGNMENT" };
        private static readonly RoleProvider provider = RoleProvider.Instance();

        private enum UserRoleActions
        {
            add = 0,
            update = 1,
            delete = 2,
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds a Role Group.
        /// </summary>
        /// <param name="objRoleGroupInfo">The RoleGroup to Add.</param>
        /// <returns>The Id of the new role.</returns>
        /// -----------------------------------------------------------------------------
        public static int AddRoleGroup(RoleGroupInfo objRoleGroupInfo)
        {
            var id = provider.CreateRoleGroup(objRoleGroupInfo);
            EventLogController.Instance.AddLog(objRoleGroupInfo, PortalController.Instance.GetCurrentPortalSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.USER_ROLE_CREATED);
            return id;
        }

        /// <summary>
        /// Adds a User to a Role.
        /// </summary>
        /// <param name="user">The user to assign.</param>
        /// <param name="role">The role to add.</param>
        /// <param name="portalSettings">The PortalSettings of the Portal.</param>
        /// <param name="status">RoleStatus.</param>
        /// <param name="effectiveDate">The expiry Date of the Role membership.</param>
        /// <param name="expiryDate">The expiry Date of the Role membership.</param>
        /// <param name="notifyUser">A flag that indicates whether the user should be notified.</param>
        /// <param name="isOwner">A flag that indicates whether this user should be one of the group owners.</param>
        public static void AddUserRole(UserInfo user, RoleInfo role, PortalSettings portalSettings, RoleStatus status, DateTime effectiveDate, DateTime expiryDate, bool notifyUser, bool isOwner)
        {
            var userRole = Instance.GetUserRole(portalSettings.PortalId, user.UserID, role.RoleID);

            // update assignment
            Instance.AddUserRole(portalSettings.PortalId, user.UserID, role.RoleID, status, isOwner, effectiveDate, expiryDate);

            UserController.UpdateUser(portalSettings.PortalId, user);
            if (userRole == null)
            {
                EventLogController.Instance.AddLog("Role", role.RoleName, portalSettings, user.UserID, EventLogController.EventLogType.USER_ROLE_CREATED);

                // send notification
                if (notifyUser)
                {
                    SendNotification(user, role, portalSettings, UserRoleActions.@add);
                }
            }
            else
            {
                EventLogController.Instance.AddLog("Role", role.RoleName, portalSettings, user.UserID, EventLogController.EventLogType.USER_ROLE_UPDATED);
                if (notifyUser)
                {
                    RoleController.Instance.GetUserRole(portalSettings.PortalId, user.UserID, role.RoleID);
                    SendNotification(user, role, portalSettings, UserRoleActions.update);
                }
            }

            // Remove the UserInfo from the Cache, as it has been modified
            DataCache.ClearUserCache(portalSettings.PortalId, user.Username);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Determines if the specified user can be removed from a role.
        /// </summary>
        /// <remarks>
        /// Roles such as "Registered Users" and "Administrators" can only
        /// be removed in certain circumstances.
        /// </remarks>
        /// <param name="PortalSettings">A <see cref="PortalSettings">PortalSettings</see> structure representing the current portal settings.</param>
        /// <param name="UserId">The Id of the User that should be checked for role removability.</param>
        /// <param name="RoleId">The Id of the Role that should be checked for removability.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static bool CanRemoveUserFromRole(PortalSettings PortalSettings, int UserId, int RoleId)
        {
            // [DNN-4285] Refactored this check into a method for use in SecurityRoles.ascx.vb
            // HACK: Duplicated in CanRemoveUserFromRole(PortalInfo, Integer, Integer) method below
            // changes to this method should be reflected in the other method as well
            return !((PortalSettings.AdministratorId == UserId && PortalSettings.AdministratorRoleId == RoleId) || PortalSettings.RegisteredRoleId == RoleId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Determines if the specified user can be removed from a role.
        /// </summary>
        /// <remarks>
        /// Roles such as "Registered Users" and "Administrators" can only
        /// be removed in certain circumstances.
        /// </remarks>
        /// <param name="PortalInfo">A <see cref="PortalInfo">PortalInfo</see> structure representing the current portal.</param>
        /// <param name="UserId">The Id of the User.</param>
        /// <param name="RoleId">The Id of the Role that should be checked for removability.</param>
        /// <returns></returns>
        /// -----------------------------------------------------------------------------
        public static bool CanRemoveUserFromRole(PortalInfo PortalInfo, int UserId, int RoleId)
        {
            // [DNN-4285] Refactored this check into a method for use in SecurityRoles.ascx.vb
            // HACK: Duplicated in CanRemoveUserFromRole(PortalSettings, Integer, Integer) method above
            // changes to this method should be reflected in the other method as well
            return !((PortalInfo.AdministratorId == UserId && PortalInfo.AdministratorRoleId == RoleId) || PortalInfo.RegisteredRoleId == RoleId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes a Role Group.
        /// </summary>
        /// -----------------------------------------------------------------------------
        public static void DeleteRoleGroup(int PortalID, int RoleGroupId)
        {
            DeleteRoleGroup(GetRoleGroup(PortalID, RoleGroupId));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes a Role Group.
        /// </summary>
        /// <param name="objRoleGroupInfo">The RoleGroup to Delete.</param>
        /// -----------------------------------------------------------------------------
        public static void DeleteRoleGroup(RoleGroupInfo objRoleGroupInfo)
        {
            provider.DeleteRoleGroup(objRoleGroupInfo);
            EventLogController.Instance.AddLog(objRoleGroupInfo, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.USER_ROLE_DELETED);
        }

        /// <summary>
        /// Removes a User from a Role.
        /// </summary>
        /// <param name="objUser">The user to remove.</param>
        /// <param name="role">The role to remove the use from.</param>
        /// <param name="portalSettings">The PortalSettings of the Portal.</param>
        /// <param name="notifyUser">A flag that indicates whether the user should be notified.</param>
        /// <returns></returns>
        public static bool DeleteUserRole(UserInfo objUser, RoleInfo role, PortalSettings portalSettings, bool notifyUser)
        {
            bool canDelete = DeleteUserRoleInternal(portalSettings.PortalId, objUser.UserID, role.RoleID);
            if (canDelete)
            {
                if (notifyUser)
                {
                    SendNotification(objUser, role, portalSettings, UserRoleActions.delete);
                }
            }

            return canDelete;
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fetch a single RoleGroup.
        /// </summary>
        /// <param name="portalId">The Id of the Portal.</param>
        /// <param name="roleGroupId">Role Group ID.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public static RoleGroupInfo GetRoleGroup(int portalId, int roleGroupId)
        {
            return provider.GetRoleGroup(portalId, roleGroupId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fetch a single RoleGroup by Name.
        /// </summary>
        /// <param name="portalId">The Id of the Portal.</param>
        /// <param name="roleGroupName">Role Group Name.</param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// -----------------------------------------------------------------------------
        public static RoleGroupInfo GetRoleGroupByName(int portalId, string roleGroupName)
        {
            return provider.GetRoleGroupByName(portalId, roleGroupName);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an ArrayList of RoleGroups.
        /// </summary>
        /// <param name="PortalID">The Id of the Portal.</param>
        /// <returns>An ArrayList of RoleGroups.</returns>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetRoleGroups(int PortalID)
        {
            return provider.GetRoleGroups(PortalID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Serializes the role groups.
        /// </summary>
        /// <param name="writer">An XmlWriter.</param>
        /// <param name="portalID">The Id of the Portal.</param>
        /// -----------------------------------------------------------------------------
        public static void SerializeRoleGroups(XmlWriter writer, int portalID)
        {
            // Serialize Role Groups
            writer.WriteStartElement("rolegroups");
            foreach (RoleGroupInfo objRoleGroup in GetRoleGroups(portalID))
            {
                CBO.SerializeObject(objRoleGroup, writer);
            }

            // Serialize Global Roles
            var globalRoleGroup = new RoleGroupInfo(Null.NullInteger, portalID, true)
            {
                RoleGroupName = "GlobalRoles",
                Description = "A dummy role group that represents the Global roles",
            };
            CBO.SerializeObject(globalRoleGroup, writer);
            writer.WriteEndElement();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates a Role Group.
        /// </summary>
        /// <param name="roleGroup">The RoleGroup to Update.</param>
        /// -----------------------------------------------------------------------------
        public static void UpdateRoleGroup(RoleGroupInfo roleGroup)
        {
            UpdateRoleGroup(roleGroup, false);
        }

        public static void UpdateRoleGroup(RoleGroupInfo roleGroup, bool includeRoles)
        {
            provider.UpdateRoleGroup(roleGroup);
            EventLogController.Instance.AddLog(roleGroup, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.USER_ROLE_UPDATED);
            if (includeRoles)
            {
                foreach (RoleInfo role in roleGroup.Roles.Values)
                {
                    Instance.UpdateRole(role);
                    EventLogController.Instance.AddLog(role, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.ROLE_UPDATED);
                }
            }
        }

        public int AddRole(RoleInfo role)
        {
            return Instance.AddRole(role, true);
        }

        public void AddUserRole(int portalId, int userId, int roleId, RoleStatus status, bool isOwner, DateTime effectiveDate, DateTime expiryDate)
        {
            UserInfo user = UserController.GetUserById(portalId, userId);
            UserRoleInfo userRole = this.GetUserRole(portalId, userId, roleId);
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
                provider.AddUserToRole(portalId, user, userRole);
                EventLogController.Instance.AddLog(userRole, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.USER_ROLE_CREATED);
            }
            else
            {
                userRole.Status = status;
                userRole.IsOwner = isOwner;
                userRole.EffectiveDate = effectiveDate;
                userRole.ExpiryDate = expiryDate;
                provider.UpdateUserRole(userRole);
                EventLogController.Instance.AddLog(userRole, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.USER_ROLE_UPDATED);
            }

            EventManager.Instance.OnRoleJoined(new RoleEventArgs() { Role = this.GetRoleById(portalId, roleId), User = user });

            // Remove the UserInfo and Roles from the Cache, as they have been modified
            DataCache.ClearUserCache(portalId, user.Username);
            Instance.ClearRoleCache(portalId);
        }

        public void ClearRoleCache(int portalId)
        {
            DataCache.RemoveCache(string.Format(DataCache.RolesCacheKey, portalId));
            if (portalId != Null.NullInteger)
            {
                DataCache.RemoveCache(string.Format(DataCache.RolesCacheKey, Null.NullInteger));
            }
        }

        public void DeleteRole(RoleInfo role)
        {
            Requires.NotNull("role", role);

            this.AddMessage(role, EventLogController.EventLogType.ROLE_DELETED);

            if (role.SecurityMode != SecurityMode.SecurityRole)
            {
                // remove group artifacts
                var portalSettings = PortalController.Instance.GetCurrentPortalSettings();

                IFileManager _fileManager = FileManager.Instance;
                IFolderManager _folderManager = FolderManager.Instance;

                IFolderInfo groupFolder = _folderManager.GetFolder(portalSettings.PortalId, "Groups/" + role.RoleID);
                if (groupFolder != null)
                {
                    _fileManager.DeleteFiles(_folderManager.GetFiles(groupFolder));
                    _folderManager.DeleteFolder(groupFolder);
                }

                JournalController.Instance.SoftDeleteJournalItemByGroupId(portalSettings.PortalId, role.RoleID);
            }

            // Get users before deleting role
            var users = role.UserCount > 0 ? this.GetUsersByRole(role.PortalID, role.RoleName) : Enumerable.Empty<UserInfo>();

            provider.DeleteRole(role);

            EventManager.Instance.OnRoleDeleted(new RoleEventArgs() { Role = role });

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

            DataProvider.Instance().AddSearchDeletedItems(document);
        }

        public RoleInfo GetRole(int portalId, Func<RoleInfo, bool> predicate)
        {
            return this.GetRoles(portalId).Where(predicate).FirstOrDefault();
        }

        public RoleInfo GetRoleById(int portalId, int roleId)
        {
            return this.GetRole(portalId, r => r.RoleID == roleId);
        }

        public RoleInfo GetRoleByName(int portalId, string roleName)
        {
            roleName = roleName.Trim();
            return this.GetRoles(portalId).SingleOrDefault(r => roleName.Equals(r.RoleName.Trim(), StringComparison.InvariantCultureIgnoreCase) && r.PortalID == portalId);
        }

        public IList<RoleInfo> GetRoles(int portalId)
        {
            var cacheKey = string.Format(DataCache.RolesCacheKey, portalId);
            return CBO.GetCachedObject<IList<RoleInfo>>(
                new CacheItemArgs(cacheKey, DataCache.RolesCacheTimeOut, DataCache.RolesCachePriority),
                c => provider.GetRoles(portalId).Cast<RoleInfo>().ToList());
        }

        public IList<RoleInfo> GetRoles(int portalId, Func<RoleInfo, bool> predicate)
        {
            return this.GetRoles(portalId).Where(predicate).ToList();
        }

        public IList<RoleInfo> GetRolesBasicSearch(int portalId, int pageSize, string filterBy)
        {
            return provider.GetRolesBasicSearch(portalId, pageSize, filterBy);
        }

        public IDictionary<string, string> GetRoleSettings(int roleId)
        {
            return provider.GetRoleSettings(roleId);
        }

        /// <summary>
        /// Gets a User/Role.
        /// </summary>
        /// <param name="portalId">The Id of the Portal.</param>
        /// <param name="userId">The Id of the user.</param>
        /// <param name="roleId">The Id of the Role.</param>
        /// <returns>A UserRoleInfo object.</returns>
        public UserRoleInfo GetUserRole(int portalId, int userId, int roleId)
        {
            return provider.GetUserRole(portalId, userId, roleId);
        }

        /// <summary>
        /// Gets a list of UserRoles for the user.
        /// </summary>
        /// <param name="user">A UserInfo object representaing the user.</param>
        /// <param name="includePrivate">Include private roles.</param>
        /// <returns>A list of UserRoleInfo objects.</returns>
        public IList<UserRoleInfo> GetUserRoles(UserInfo user, bool includePrivate)
        {
            return provider.GetUserRoles(user, includePrivate);
        }

        public IList<UserRoleInfo> GetUserRoles(int portalId, string userName, string roleName)
        {
            return provider.GetUserRoles(portalId, userName, roleName).Cast<UserRoleInfo>().ToList();
        }

        public IList<UserInfo> GetUsersByRole(int portalId, string roleName)
        {
            return provider.GetUsersByRoleName(portalId, roleName).Cast<UserInfo>().ToList();
        }

        public void UpdateRole(RoleInfo role, bool addToExistUsers)
        {
            Requires.NotNull("role", role);

            provider.UpdateRole(role);
            this.AddMessage(role, EventLogController.EventLogType.ROLE_UPDATED);

            if (addToExistUsers)
            {
                this.AutoAssignUsers(role);
            }

            this.ClearRoleCache(role.PortalID);
        }

        public void UpdateRoleSettings(RoleInfo role, bool clearCache)
        {
            provider.UpdateRoleSettings(role);

            if (clearCache)
            {
                this.ClearRoleCache(role.PortalID);
            }
        }

        public void UpdateUserRole(int portalId, int userId, int roleId, RoleStatus status, bool isOwner, bool cancel)
        {
            UserInfo user = UserController.GetUserById(portalId, userId);
            UserRoleInfo userRole = this.GetUserRole(portalId, userId, roleId);
            if (cancel)
            {
                if (userRole != null && userRole.ServiceFee > 0.0 && userRole.IsTrialUsed)
                {
                    // Expire Role so we retain trial used data
                    userRole.ExpiryDate = DateTime.Now.AddDays(-1);
                    userRole.Status = status;
                    userRole.IsOwner = isOwner;
                    provider.UpdateUserRole(userRole);
                    EventLogController.Instance.AddLog(userRole, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.USER_ROLE_UPDATED);
                }
                else
                {
                    // Delete Role
                    DeleteUserRoleInternal(portalId, userId, roleId);
                    EventLogController.Instance.AddLog(
                        "UserId",
                        userId.ToString(CultureInfo.InvariantCulture),
                        PortalController.Instance.GetCurrentPortalSettings(),
                        UserController.Instance.GetCurrentUserInfo().UserID,
                        EventLogController.EventLogType.USER_ROLE_DELETED);
                }
            }
            else
            {
                int UserRoleId = -1;
                DateTime ExpiryDate = DateTime.Now;
                DateTime EffectiveDate = Null.NullDate;
                bool IsTrialUsed = false;
                int Period = 0;
                string Frequency = string.Empty;
                if (userRole != null)
                {
                    UserRoleId = userRole.UserRoleID;
                    EffectiveDate = userRole.EffectiveDate;
                    ExpiryDate = userRole.ExpiryDate;
                    IsTrialUsed = userRole.IsTrialUsed;
                }

                RoleInfo role = Instance.GetRole(portalId, r => r.RoleID == roleId);
                if (role != null)
                {
                    if (IsTrialUsed == false && role.TrialFrequency != "N")
                    {
                        Period = role.TrialPeriod;
                        Frequency = role.TrialFrequency;
                    }
                    else
                    {
                        Period = role.BillingPeriod;
                        Frequency = role.BillingFrequency;
                    }
                }

                if (EffectiveDate < DateTime.Now)
                {
                    EffectiveDate = Null.NullDate;
                }

                if (ExpiryDate < DateTime.Now)
                {
                    ExpiryDate = DateTime.Now;
                }

                if (Period == Null.NullInteger)
                {
                    ExpiryDate = Null.NullDate;
                }
                else
                {
                    switch (Frequency)
                    {
                        case "N":
                            ExpiryDate = Null.NullDate;
                            break;
                        case "O":
                            ExpiryDate = new DateTime(9999, 12, 31);
                            break;
                        case "D":
                            ExpiryDate = ExpiryDate.AddDays(Period);
                            break;
                        case "W":
                            ExpiryDate = ExpiryDate.AddDays(Period * 7);
                            break;
                        case "M":
                            ExpiryDate = ExpiryDate.AddMonths(Period);
                            break;
                        case "Y":
                            ExpiryDate = ExpiryDate.AddYears(Period);
                            break;
                    }
                }

                if (UserRoleId != -1 && userRole != null)
                {
                    userRole.ExpiryDate = ExpiryDate;
                    userRole.Status = status;
                    userRole.IsOwner = isOwner;
                    provider.UpdateUserRole(userRole);
                    EventLogController.Instance.AddLog(userRole, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.USER_ROLE_UPDATED);
                }
                else
                {
                    this.AddUserRole(portalId, userId, roleId, status, isOwner, EffectiveDate, ExpiryDate);
                }
            }

            // Remove the UserInfo from the Cache, as it has been modified
            DataCache.ClearUserCache(portalId, user.Username);
            Instance.ClearRoleCache(portalId);
        }

        /// <summary>
        /// Completely remove all a user's roles for a specific portal. This method is used when
        /// anonymizing a user.
        /// </summary>
        /// <param name="user">User for which all roles must be deleted. The PortalId property
        /// is used to determine for which portal roles must be removed.</param>
        internal static void DeleteUserRoles(UserInfo user)
        {
            var ctrl = new RoleController();
            var userRoles = ctrl.GetUserRoles(user, true);
            foreach (var ur in userRoles.Where(r => r.PortalID == user.PortalID))
            {
                provider.RemoveUserFromRole(user.PortalID, user, ur);
            }
        }

        protected override Func<IRoleController> GetFactory()
        {
            return () => new RoleController();
        }

        private static bool DeleteUserRoleInternal(int portalId, int userId, int roleId)
        {
            var user = UserController.GetUserById(portalId, userId);
            var userRole = RoleController.Instance.GetUserRole(portalId, userId, roleId);
            bool delete = true;
            var portal = PortalController.Instance.GetPortal(portalId);
            if (portal != null && userRole != null)
            {
                if (CanRemoveUserFromRole(portal, userId, roleId))
                {
                    provider.RemoveUserFromRole(portalId, user, userRole);
                    EventLogController.Instance.AddLog(userRole, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.ROLE_UPDATED);

                    // Remove the UserInfo from the Cache, as it has been modified
                    DataCache.ClearUserCache(portalId, user.Username);
                    Instance.ClearRoleCache(portalId);

                    EventManager.Instance.OnRoleLeft(new RoleEventArgs() { Role = Instance.GetRoleById(portalId, roleId), User = user });
                }
                else
                {
                    delete = false;
                }
            }

            return delete;
        }

        private static void SendNotification(UserInfo objUser, RoleInfo objRole, PortalSettings PortalSettings, UserRoleActions Action)
        {
            var Custom = new ArrayList { objRole.RoleName, objRole.Description };
            switch (Action)
            {
                case UserRoleActions.add:
                case UserRoleActions.update:
                    string preferredLocale = objUser.Profile.PreferredLocale;
                    if (string.IsNullOrEmpty(preferredLocale))
                    {
                        preferredLocale = PortalSettings.DefaultLanguage;
                    }

                    var ci = new CultureInfo(preferredLocale);
                    UserRoleInfo objUserRole = RoleController.Instance.GetUserRole(PortalSettings.PortalId, objUser.UserID, objRole.RoleID);
                    Custom.Add(Null.IsNull(objUserRole.EffectiveDate)
                                   ? DateTime.Today.ToString("g", ci)
                                   : objUserRole.EffectiveDate.ToString("g", ci));
                    Custom.Add(Null.IsNull(objUserRole.ExpiryDate) ? "-" : objUserRole.ExpiryDate.ToString("g", ci));
                    break;
                case UserRoleActions.delete:
                    Custom.Add(string.Empty);
                    break;
            }

            var _message = new Message
            {
                FromUserID = PortalSettings.AdministratorId,
                ToUserID = objUser.UserID,
                Subject =
                    Localization.GetSystemMessage(objUser.Profile.PreferredLocale, PortalSettings,
                                                  "EMAIL_ROLE_" +
                                                  UserRoleActionsCaption[(int)Action] +
                                                  "_SUBJECT", objUser),
                Body = Localization.GetSystemMessage(
                    objUser.Profile.PreferredLocale,
                    PortalSettings,
                    "EMAIL_ROLE_" +
                                                     UserRoleActionsCaption[(int)Action] + "_BODY",
                    objUser,
                    Localization.GlobalResourceFile,
                    Custom),
                Status = MessageStatusType.Unread,
            };

            // _messagingController.SaveMessage(_message);
            Mail.SendEmail(PortalSettings.Email, objUser.Email, _message.Subject, _message.Body);
        }

        int IRoleController.AddRole(RoleInfo role, bool addToExistUsers)
        {
            Requires.NotNull("role", role);

            var roleId = -1;
            if (provider.CreateRole(role))
            {
                this.AddMessage(role, EventLogController.EventLogType.ROLE_CREATED);
                if (addToExistUsers)
                {
                    this.AutoAssignUsers(role);
                }

                roleId = role.RoleID;

                this.ClearRoleCache(role.PortalID);

                EventManager.Instance.OnRoleCreated(new RoleEventArgs() { Role = role });
            }

            return roleId;
        }

        private void AddMessage(RoleInfo roleInfo, EventLogController.EventLogType logType)
        {
            EventLogController.Instance.AddLog(
                roleInfo,
                PortalController.Instance.GetCurrentPortalSettings(),
                UserController.Instance.GetCurrentUserInfo().UserID,
                string.Empty,
                logType);
        }

        private void AutoAssignUsers(RoleInfo role)
        {
            if (role.AutoAssignment)
            {
                // loop through users for portal and add to role
                var arrUsers = UserController.GetUsers(role.PortalID);
                foreach (UserInfo objUser in arrUsers)
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

        void IRoleController.UpdateRole(RoleInfo role)
        {
            this.UpdateRole(role, true);
        }
    }
}
