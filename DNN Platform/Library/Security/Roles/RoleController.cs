#region Copyright
// 
// DotNetNuke� - http://www.dotnetnuke.com
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

namespace DotNetNuke.Security.Roles
{
    /// <summary>
    /// The RoleController class provides Business Layer methods for Roles
    /// </summary>
    /// -----------------------------------------------------------------------------
    public partial class RoleController : ServiceLocator<IRoleController, RoleController>, IRoleController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RoleController));
        private static readonly string[] UserRoleActionsCaption = { "ASSIGNMENT", "UPDATE", "UNASSIGNMENT" };
        private static readonly RoleProvider provider = RoleProvider.Instance();

        private static event EventHandler<RoleEventArgs> RoleCreated;
        private static event EventHandler<RoleEventArgs> RoleDeleted;
        private static event EventHandler<RoleEventArgs> RoleJoined;
        private static event EventHandler<RoleEventArgs> RoleLeft;

        private enum UserRoleActions
        {
            add = 0,
            update = 1,
            delete = 2
        }

        protected override Func<IRoleController> GetFactory()
        {
            return () => new RoleController();
        }

        static RoleController()
        {
            foreach (var handlers in EventHandlersContainer<IRoleEventHandlers>.Instance.EventHandlers)
            {
                RoleCreated += handlers.Value.RoleCreated;
                RoleDeleted += handlers.Value.RoleDeleted;
                RoleJoined += handlers.Value.RoleJoined;
                RoleLeft += handlers.Value.RoleLeft;
            }
        }

        #region Private Methods

        private void AddMessage(RoleInfo roleInfo, EventLogController.EventLogType logType)
        {
            EventLogController.Instance.AddLog(roleInfo,
                                PortalController.Instance.GetCurrentPortalSettings(),
                                UserController.Instance.GetCurrentUserInfo().UserID,
                                "",
                                logType);

        }

        private void AutoAssignUsers(RoleInfo role)
        {
            if (role.AutoAssignment)
            {
                //loop through users for portal and add to role
                var arrUsers = UserController.GetUsers(role.PortalID);
                foreach (UserInfo objUser in arrUsers)
                {
                    try
                    {
                        AddUserRole(role.PortalID, objUser.UserID, role.RoleID, RoleStatus.Approved, false, Null.NullDate, Null.NullDate);
                    }
                    catch (Exception exc)
                    {
                        //user already belongs to role
                        Logger.Error(exc);
                    }
                }
            }
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
                    EventLogController.Instance.AddLog(userRole, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.ROLE_UPDATED);

                    //Remove the UserInfo from the Cache, as it has been modified
                    DataCache.ClearUserCache(portalId, user.Username);
                    Instance.ClearRoleCache(portalId);

                    if (RoleLeft != null)
                    {
                        var role = Instance.GetRoleById(portalId, roleId);
                        RoleLeft(null, new RoleEventArgs() { Role = role, User = user });
                    }
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
                    Custom.Add("");
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
                Body = Localization.GetSystemMessage(objUser.Profile.PreferredLocale,
                                                     PortalSettings,
                                                     "EMAIL_ROLE_" +
                                                     UserRoleActionsCaption[(int)Action] + "_BODY",
                                                     objUser,
                                                     Localization.GlobalResourceFile,
                                                     Custom),
                Status = MessageStatusType.Unread
            };

            //_messagingController.SaveMessage(_message);
            Mail.SendEmail(PortalSettings.Email, objUser.Email, _message.Subject, _message.Body);
        }

        #endregion

        #region Public Methods

        public int AddRole(RoleInfo role)
        {
            return Instance.AddRole(role, true);
        }

        int IRoleController.AddRole(RoleInfo role, bool addToExistUsers)
        {
            Requires.NotNull("role", role);

            var roleId = -1;
            if (provider.CreateRole(role))
            {
                AddMessage(role, EventLogController.EventLogType.ROLE_CREATED);
                if (addToExistUsers)
                {
                    AutoAssignUsers(role);
                }
                roleId = role.RoleID;

                ClearRoleCache(role.PortalID);

                if (RoleCreated != null)
                {
                    RoleCreated(null, new RoleEventArgs() {Role = role});
                }
            }

            return roleId;
        }

        public void AddUserRole(int portalId, int userId, int roleId, RoleStatus status, bool isOwner, DateTime effectiveDate, DateTime expiryDate)
        {
            UserInfo user = UserController.GetUserById(portalId, userId);
            UserRoleInfo userRole = GetUserRole(portalId, userId, roleId);
            if (userRole == null)
            {
                //Create new UserRole
                userRole = new UserRoleInfo
                {
                    UserID = userId,
                    RoleID = roleId,
                    PortalID = portalId,
                    Status = status,
                    IsOwner = isOwner,
                    EffectiveDate = effectiveDate,
                    ExpiryDate = expiryDate
                };
                provider.AddUserToRole(portalId, user, userRole);
                EventLogController.Instance.AddLog(userRole, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.USER_ROLE_CREATED);
            }
            else
            {
                userRole.Status = status;
                userRole.IsOwner = isOwner;
                userRole.EffectiveDate = effectiveDate;
                userRole.ExpiryDate = expiryDate;
                provider.UpdateUserRole(userRole);
                EventLogController.Instance.AddLog(userRole, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.USER_ROLE_UPDATED);
            }

            if (RoleJoined != null)
            {
                var role = GetRoleById(portalId, roleId);
                RoleJoined(null, new RoleEventArgs() { Role = role, User = user});
            }

            //Remove the UserInfo and Roles from the Cache, as they have been modified
            DataCache.ClearUserCache(portalId, user.Username);
            Instance.ClearRoleCache(portalId);
        }

        public void ClearRoleCache(int portalId)
        {
            DataCache.RemoveCache(String.Format(DataCache.RolesCacheKey, portalId));
            if (portalId != Null.NullInteger)
            {
                DataCache.RemoveCache(String.Format(DataCache.RolesCacheKey, Null.NullInteger));
            }
        }

        public void DeleteRole(RoleInfo role)
        {
            Requires.NotNull("role", role);

            AddMessage(role, EventLogController.EventLogType.ROLE_DELETED);

            if (role.SecurityMode != SecurityMode.SecurityRole)
            {
                //remove group artifacts
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

            //Get users before deleting role
            var users = role.UserCount > 0 ? GetUsersByRole(role.PortalID, role.RoleName) : Enumerable.Empty<UserInfo>();

            provider.DeleteRole(role);

            if (RoleDeleted != null)
            {
                RoleDeleted(null, new RoleEventArgs() { Role = role });
            }

            //Remove the UserInfo objects of users that have been members of the group from the cache, as they have been modified
            foreach (var user in users)
            {
                DataCache.ClearUserCache(role.PortalID, user.Username);
            }

            ClearRoleCache(role.PortalID);

            // queue remove role/group from search index
            var document = new SearchDocumentToDelete
            {
                //PortalId = role.PortalID,
                RoleId = role.RoleID, // this is unique and sufficient
            };

            DataProvider.Instance().AddSearchDeletedItems(document);
        }

        public RoleInfo GetRole(int portalId, Func<RoleInfo, bool> predicate)
        {
            return GetRoles(portalId).Where(predicate).FirstOrDefault();
        }

        public RoleInfo GetRoleById(int portalId, int roleId)
        {
            return GetRole(portalId, r => r.RoleID == roleId);
        }

        public RoleInfo GetRoleByName(int portalId, string roleName)
        {
            return GetRoles(portalId).SingleOrDefault(r => r.RoleName == roleName && r.PortalID == portalId);
        }

        public IList<RoleInfo> GetRoles(int portalId)
        {
            var cacheKey = String.Format(DataCache.RolesCacheKey, portalId);
            return CBO.GetCachedObject<IList<RoleInfo>>(new CacheItemArgs(cacheKey, DataCache.RolesCacheTimeOut, DataCache.RolesCachePriority),
                                                                    c => provider.GetRoles(portalId).Cast<RoleInfo>().ToList());
        }

        public IList<RoleInfo> GetRoles(int portalId, Func<RoleInfo, bool> predicate)
        {
            return GetRoles(portalId).Where(predicate).ToList();
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
        /// Gets a User/Role
        /// </summary>
        /// <param name="portalId">The Id of the Portal</param>
        /// <param name="userId">The Id of the user</param>
        /// <param name="roleId">The Id of the Role</param>
        /// <returns>A UserRoleInfo object</returns>
        public UserRoleInfo GetUserRole(int portalId, int userId, int roleId)
        {
            return provider.GetUserRole(portalId, userId, roleId);
        }

        /// <summary>
        /// Gets a list of UserRoles for the user
        /// </summary>
        /// <param name="user">A UserInfo object representaing the user</param>
        /// <param name="includePrivate">Include private roles.</param>
        /// <returns>A list of UserRoleInfo objects</returns>
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

        void IRoleController.UpdateRole(RoleInfo role)
        {
            UpdateRole(role, true);
        }

        public void UpdateRole(RoleInfo role, bool addToExistUsers)
        {
            Requires.NotNull("role", role);

            provider.UpdateRole(role);
            AddMessage(role, EventLogController.EventLogType.ROLE_UPDATED);

            if (addToExistUsers)
            {
                AutoAssignUsers(role);
            }

            ClearRoleCache(role.PortalID);
        }

        public void UpdateRoleSettings(RoleInfo role, bool clearCache)
        {
            provider.UpdateRoleSettings(role);

            if (clearCache)
            {
                ClearRoleCache(role.PortalID);
            }
        }

        public void UpdateUserRole(int portalId, int userId, int roleId, RoleStatus status, bool isOwner, bool cancel)
        {
            UserInfo user = UserController.GetUserById(portalId, userId);
            UserRoleInfo userRole = GetUserRole(portalId, userId, roleId);
            if (cancel)
            {
                if (userRole != null && userRole.ServiceFee > 0.0 && userRole.IsTrialUsed)
                {
                    //Expire Role so we retain trial used data
                    userRole.ExpiryDate = DateTime.Now.AddDays(-1);
                    userRole.Status = status;
                    userRole.IsOwner = isOwner;
                    provider.UpdateUserRole(userRole);
                    EventLogController.Instance.AddLog(userRole, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.USER_ROLE_UPDATED);
                }
                else
                {
                    //Delete Role
                    DeleteUserRoleInternal(portalId, userId, roleId);
                    EventLogController.Instance.AddLog("UserId",
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
                string Frequency = "";
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
                    EventLogController.Instance.AddLog(userRole, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.USER_ROLE_UPDATED);
                }
                else
                {
                    AddUserRole(portalId, userId, roleId, status, isOwner, EffectiveDate, ExpiryDate);
                }
            }

            //Remove the UserInfo from the Cache, as it has been modified
            DataCache.ClearUserCache(portalId, user.Username);
            Instance.ClearRoleCache(portalId);
        }


        #endregion

        #region Static Helper Methods

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Adds a Role Group
        /// </summary>
        /// <param name="objRoleGroupInfo">The RoleGroup to Add</param>
        /// <returns>The Id of the new role</returns>
        /// <history>
        /// 	[cnurse]	01/03/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static int AddRoleGroup(RoleGroupInfo objRoleGroupInfo)
        {
            EventLogController.Instance.AddLog(objRoleGroupInfo, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.USER_ROLE_CREATED);
            return provider.CreateRoleGroup(objRoleGroupInfo);
        }

        /// <summary>
        /// Adds a User to a Role
        /// </summary>
        /// <param name="user">The user to assign</param>
        /// <param name="role">The role to add</param>
        /// <param name="portalSettings">The PortalSettings of the Portal</param>
        /// <param name="status">RoleStatus</param>
        /// <param name="effectiveDate">The expiry Date of the Role membership</param>
        /// <param name="expiryDate">The expiry Date of the Role membership</param>
        /// <param name="notifyUser">A flag that indicates whether the user should be notified</param>
        /// <param name="isOwner">A flag that indicates whether this user should be one of the group owners</param>
        public static void AddUserRole(UserInfo user, RoleInfo role, PortalSettings portalSettings, RoleStatus status, DateTime effectiveDate, DateTime expiryDate, bool notifyUser, bool isOwner)
        {
            var userRole = Instance.GetUserRole(portalSettings.PortalId, user.UserID, role.RoleID);

            //update assignment
            Instance.AddUserRole(portalSettings.PortalId, user.UserID, role.RoleID, status, isOwner, effectiveDate, expiryDate);

            UserController.UpdateUser(portalSettings.PortalId, user);
            if (userRole == null)
            {
                EventLogController.Instance.AddLog("Role", role.RoleName, portalSettings, user.UserID, EventLogController.EventLogType.USER_ROLE_CREATED);

                //send notification
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

            //Remove the UserInfo from the Cache, as it has been modified
            DataCache.ClearUserCache(portalSettings.PortalId, user.Username);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Determines if the specified user can be removed from a role
        /// </summary>
        /// <remarks>
        /// Roles such as "Registered Users" and "Administrators" can only
        /// be removed in certain circumstances
        /// </remarks>
        /// <param name="PortalSettings">A <see cref="PortalSettings">PortalSettings</see> structure representing the current portal settings</param>
        /// <param name="UserId">The Id of the User that should be checked for role removability</param>
        /// <param name="RoleId">The Id of the Role that should be checked for removability</param>
        /// <returns></returns>
        /// <history>
        /// 	[anurse]	01/12/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static bool CanRemoveUserFromRole(PortalSettings PortalSettings, int UserId, int RoleId)
        {
            //[DNN-4285] Refactored this check into a method for use in SecurityRoles.ascx.vb
            //HACK: Duplicated in CanRemoveUserFromRole(PortalInfo, Integer, Integer) method below
            //changes to this method should be reflected in the other method as well
            return !((PortalSettings.AdministratorId == UserId && PortalSettings.AdministratorRoleId == RoleId) || PortalSettings.RegisteredRoleId == RoleId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Determines if the specified user can be removed from a role
        /// </summary>
        /// <remarks>
        /// Roles such as "Registered Users" and "Administrators" can only
        /// be removed in certain circumstances
        /// </remarks>
        /// <param name="PortalInfo">A <see cref="PortalInfo">PortalInfo</see> structure representing the current portal</param>
        /// <param name="UserId">The Id of the User</param>
        /// <param name="RoleId">The Id of the Role that should be checked for removability</param>
        /// <returns></returns>
        /// <history>
        /// 	[anurse]	01/12/2007	Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static bool CanRemoveUserFromRole(PortalInfo PortalInfo, int UserId, int RoleId)
        {
            //[DNN-4285] Refactored this check into a method for use in SecurityRoles.ascx.vb
            //HACK: Duplicated in CanRemoveUserFromRole(PortalSettings, Integer, Integer) method above
            //changes to this method should be reflected in the other method as well

            return !((PortalInfo.AdministratorId == UserId && PortalInfo.AdministratorRoleId == RoleId) || PortalInfo.RegisteredRoleId == RoleId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes a Role Group
        /// </summary>
        /// <history>
        /// 	[cnurse]	01/03/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void DeleteRoleGroup(int PortalID, int RoleGroupId)
        {
            DeleteRoleGroup(GetRoleGroup(PortalID, RoleGroupId));
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Deletes a Role Group
        /// </summary>
        /// <param name="objRoleGroupInfo">The RoleGroup to Delete</param>
        /// <history>
        /// 	[cnurse]	01/03/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void DeleteRoleGroup(RoleGroupInfo objRoleGroupInfo)
        {
            provider.DeleteRoleGroup(objRoleGroupInfo);
            EventLogController.Instance.AddLog(objRoleGroupInfo, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.USER_ROLE_DELETED);
        }

        /// <summary>
        /// Removes a User from a Role
        /// </summary>
        /// <param name="objUser">The user to remove</param>
        /// <param name="role">The role to remove the use from</param>
        /// <param name="portalSettings">The PortalSettings of the Portal</param>
        /// <param name="notifyUser">A flag that indicates whether the user should be notified</param>
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
        /// Fetch a single RoleGroup
        /// </summary>
		/// <param name="portalId">The Id of the Portal</param>
		/// <param name="roleGroupId">Role Group ID</param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <history>
        /// 	[cnurse]	01/03/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static RoleGroupInfo GetRoleGroup(int portalId, int roleGroupId)
        {
			return provider.GetRoleGroup(portalId, roleGroupId);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Fetch a single RoleGroup by Name
        /// </summary>
		/// <param name="portalId">The Id of the Portal</param>
        /// <param name="roleGroupName">Role Group Name</param>
        /// <returns></returns>
        /// <remarks></remarks>
        /// <history>
        /// 	[cnurse]	01/03/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
		public static RoleGroupInfo GetRoleGroupByName(int portalId, string roleGroupName)
        {
			return provider.GetRoleGroupByName(portalId, roleGroupName);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Gets an ArrayList of RoleGroups
        /// </summary>
        /// <param name="PortalID">The Id of the Portal</param>
        /// <returns>An ArrayList of RoleGroups</returns>
        /// <history>
        /// 	[cnurse]	01/03/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static ArrayList GetRoleGroups(int PortalID)
        {
            return provider.GetRoleGroups(PortalID);
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Serializes the role groups
        /// </summary>
        /// <param name="writer">An XmlWriter</param>
		/// <param name="portalID">The Id of the Portal</param>
        /// <history>
        /// 	[cnurse]	03/18/2008  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void SerializeRoleGroups(XmlWriter writer, int portalID)
        {
			//Serialize Role Groups
            writer.WriteStartElement("rolegroups");
            foreach (RoleGroupInfo objRoleGroup in GetRoleGroups(portalID))
            {
                CBO.SerializeObject(objRoleGroup, writer);
            }
			
            //Serialize Global Roles
            var globalRoleGroup = new RoleGroupInfo(Null.NullInteger, portalID, true)
                                      {
                                          RoleGroupName = "GlobalRoles",
                                          Description = "A dummy role group that represents the Global roles"
                                      };
            CBO.SerializeObject(globalRoleGroup, writer);
            writer.WriteEndElement();
        }

        /// -----------------------------------------------------------------------------
        /// <summary>
        /// Updates a Role Group
        /// </summary>
        /// <param name="roleGroup">The RoleGroup to Update</param>
        /// <history>
        /// 	[cnurse]	01/03/2006  Created
        /// </history>
        /// -----------------------------------------------------------------------------
        public static void UpdateRoleGroup(RoleGroupInfo roleGroup)
        {
            UpdateRoleGroup(roleGroup, false);
        }

        public static void UpdateRoleGroup(RoleGroupInfo roleGroup, bool includeRoles)
        {
            provider.UpdateRoleGroup(roleGroup);
            EventLogController.Instance.AddLog(roleGroup, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.USER_ROLE_UPDATED);
            if (includeRoles)
            {
                foreach (RoleInfo role in roleGroup.Roles.Values)
                {
                    Instance.UpdateRole(role);
                    EventLogController.Instance.AddLog(role, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, "", EventLogController.EventLogType.ROLE_UPDATED);
                }
            }
        }
		
		#endregion

     }
}
