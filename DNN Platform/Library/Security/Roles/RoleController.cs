// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Security.Roles;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml;

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
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Mail;
using DotNetNuke.Services.Messaging.Data;
using DotNetNuke.Services.Search.Entities;

/// <summary>The RoleController class provides Business Layer methods for Roles.</summary>
public partial class RoleController : ServiceLocator<IRoleController, RoleController>, IRoleController
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RoleController));
    private static readonly string[] UserRoleActionsCaption = { "ASSIGNMENT", "UPDATE", "UNASSIGNMENT" };
    private static readonly RoleProvider Provider = RoleProvider.Instance();

    private enum UserRoleActions
    {
        Add = 0,
        Update = 1,
        Delete = 2,
    }

    /// <summary>Adds a Role Group.</summary>
    /// <param name="objRoleGroupInfo">The RoleGroup to Add.</param>
    /// <returns>The Id of the new role.</returns>
    public static int AddRoleGroup(RoleGroupInfo objRoleGroupInfo)
    {
        var id = Provider.CreateRoleGroup(objRoleGroupInfo);
        EventLogController.Instance.AddLog(
            objRoleGroupInfo,
            PortalController.Instance.GetCurrentPortalSettings(),
            UserController.Instance.GetCurrentUserInfo().UserID,
            string.Empty,
            EventLogController.EventLogType.USER_ROLE_CREATED);
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
                SendNotification(user, role, portalSettings, UserRoleActions.Add);
            }
        }
        else
        {
            EventLogController.Instance.AddLog("Role", role.RoleName, portalSettings, user.UserID, EventLogController.EventLogType.USER_ROLE_UPDATED);
            if (notifyUser)
            {
                RoleController.Instance.GetUserRole(portalSettings.PortalId, user.UserID, role.RoleID);
                SendNotification(user, role, portalSettings, UserRoleActions.Update);
            }
        }

        // Remove the UserInfo from the Cache, as it has been modified
        DataCache.ClearUserCache(portalSettings.PortalId, user.Username);
    }

    /// <summary>Determines if the specified user can be removed from a role.</summary>
    /// <remarks>Roles such as "Registered Users" and "Administrators" can only be removed in certain circumstances.</remarks>
    /// <param name="portalSettings">A <see cref="PortalSettings">PortalSettings</see> structure representing the current portal settings.</param>
    /// <param name="userId">The Id of the User that should be checked for role removability.</param>
    /// <param name="roleId">The Id of the Role that should be checked for removability.</param>
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
    /// <param name="userId">The Id of the User.</param>
    /// <param name="roleId">The Id of the Role that should be checked for removability.</param>
    /// <returns><see langword="true"/> if the role can be removed, otherwise <see langword="false"/>.</returns>
    public static bool CanRemoveUserFromRole(PortalInfo portalInfo, int userId, int roleId)
    {
        // [DNN-4285] Refactored this check into a method for use in SecurityRoles.ascx.vb
        // HACK: Duplicated in CanRemoveUserFromRole(PortalSettings, Integer, Integer) method above
        // changes to this method should be reflected in the other method as well
        return !((portalInfo.AdministratorId == userId && portalInfo.AdministratorRoleId == roleId) || portalInfo.RegisteredRoleId == roleId);
    }

    /// <summary>Deletes a Role Group.</summary>
    public static void DeleteRoleGroup(int portalID, int roleGroupId)
    {
        DeleteRoleGroup(GetRoleGroup(portalID, roleGroupId));
    }

    /// <summary>Deletes a Role Group.</summary>
    /// <param name="objRoleGroupInfo">The RoleGroup to Delete.</param>
    public static void DeleteRoleGroup(RoleGroupInfo objRoleGroupInfo)
    {
        Provider.DeleteRoleGroup(objRoleGroupInfo);
        EventLogController.Instance.AddLog(objRoleGroupInfo, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.USER_ROLE_DELETED);
    }

    /// <summary>Removes a User from a Role.</summary>
    /// <param name="objUser">The user to remove.</param>
    /// <param name="role">The role to remove the use from.</param>
    /// <param name="portalSettings">The PortalSettings of the Portal.</param>
    /// <param name="notifyUser">A flag that indicates whether the user should be notified.</param>
    /// <returns><see langword="true"/> if the role was deleted, otherwise <see langword="false"/>.</returns>
    public static bool DeleteUserRole(UserInfo objUser, RoleInfo role, PortalSettings portalSettings, bool notifyUser)
    {
        bool canDelete = DeleteUserRoleInternal(portalSettings.PortalId, objUser.UserID, role.RoleID);
        if (canDelete)
        {
            if (notifyUser)
            {
                SendNotification(objUser, role, portalSettings, UserRoleActions.Delete);
            }
        }

        return canDelete;
    }

    /// <summary>Fetch a single RoleGroup.</summary>
    /// <param name="portalId">The Id of the Portal.</param>
    /// <param name="roleGroupId">Role Group ID.</param>
    /// <returns>A <see cref="RoleGroupInfo"/> instance or <see langword="null"/>.</returns>
    public static RoleGroupInfo GetRoleGroup(int portalId, int roleGroupId)
    {
        return Provider.GetRoleGroup(portalId, roleGroupId);
    }

    /// <summary>Fetch a single RoleGroup by Name.</summary>
    /// <param name="portalId">The Id of the Portal.</param>
    /// <param name="roleGroupName">Role Group Name.</param>
    /// <returns>A <see cref="RoleGroupInfo"/> instance or <see langword="null"/>.</returns>
    public static RoleGroupInfo GetRoleGroupByName(int portalId, string roleGroupName)
    {
        return Provider.GetRoleGroupByName(portalId, roleGroupName);
    }

    /// <summary>Gets an ArrayList of RoleGroups.</summary>
    /// <param name="portalID">The Id of the Portal.</param>
    /// <returns>An ArrayList of RoleGroups.</returns>
    public static ArrayList GetRoleGroups(int portalID)
    {
        return Provider.GetRoleGroups(portalID);
    }

    /// <summary>Serializes the role groups.</summary>
    /// <param name="writer">An XmlWriter.</param>
    /// <param name="portalID">The Id of the Portal.</param>
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
            Description = "A role group that represents the Global roles",
        };
        CBO.SerializeObject(globalRoleGroup, writer);
        writer.WriteEndElement();
    }

    /// <summary>Updates a Role Group.</summary>
    /// <param name="roleGroup">The RoleGroup to Update.</param>
    public static void UpdateRoleGroup(RoleGroupInfo roleGroup)
    {
        UpdateRoleGroup(roleGroup, false);
    }

    public static void UpdateRoleGroup(RoleGroupInfo roleGroup, bool includeRoles)
    {
        Provider.UpdateRoleGroup(roleGroup);
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

    /// <inheritdoc/>
    public int AddRole(RoleInfo role)
    {
        return Instance.AddRole(role, true);
    }

    /// <inheritdoc/>
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
            Provider.AddUserToRole(portalId, user, userRole);
            EventLogController.Instance.AddLog(userRole, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.USER_ROLE_CREATED);
        }
        else
        {
            userRole.Status = status;
            userRole.IsOwner = isOwner;
            userRole.EffectiveDate = effectiveDate;
            userRole.ExpiryDate = expiryDate;
            Provider.UpdateUserRole(userRole);
            EventLogController.Instance.AddLog(userRole, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.USER_ROLE_UPDATED);
        }

        EventManager.Instance.OnRoleJoined(new RoleEventArgs() { Role = this.GetRoleById(portalId, roleId), User = user });

        // Remove the UserInfo and Roles from the Cache, as they have been modified
        DataCache.ClearUserCache(portalId, user.Username);
        Instance.ClearRoleCache(portalId);
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

        this.AddMessage(role, EventLogController.EventLogType.ROLE_DELETED);

        if (role.SecurityMode != SecurityMode.SecurityRole)
        {
            // remove group artifacts
            var portalSettings = PortalController.Instance.GetCurrentPortalSettings();

            IFileManager fileManager = FileManager.Instance;
            IFolderManager folderManager = FolderManager.Instance;

            IFolderInfo groupFolder = folderManager.GetFolder(portalSettings.PortalId, "Groups/" + role.RoleID);
            if (groupFolder != null)
            {
                fileManager.DeleteFiles(folderManager.GetFiles(groupFolder));
                folderManager.DeleteFolder(groupFolder);
            }

            JournalController.Instance.SoftDeleteJournalItemByGroupId(portalSettings.PortalId, role.RoleID);
        }

        // Get users before deleting role
        var users = role.UserCount > 0 ? this.GetUsersByRole(role.PortalID, role.RoleName) : Enumerable.Empty<UserInfo>();

        Provider.DeleteRole(role);

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
            new CacheItemArgs(cacheKey, DataCache.RolesCacheTimeOut, DataCache.RolesCachePriority),
            c => Provider.GetRoles(portalId).Cast<RoleInfo>().ToList());
    }

    /// <inheritdoc/>
    public IList<RoleInfo> GetRoles(int portalId, Func<RoleInfo, bool> predicate)
    {
        return this.GetRoles(portalId).Where(predicate).ToList();
    }

    /// <inheritdoc/>
    public IList<RoleInfo> GetRolesBasicSearch(int portalId, int pageSize, string filterBy)
    {
        return Provider.GetRolesBasicSearch(portalId, pageSize, filterBy);
    }

    /// <inheritdoc/>
    public IDictionary<string, string> GetRoleSettings(int roleId)
    {
        return Provider.GetRoleSettings(roleId);
    }

    /// <summary>Gets a User/Role.</summary>
    /// <param name="portalId">The Id of the Portal.</param>
    /// <param name="userId">The Id of the user.</param>
    /// <param name="roleId">The Id of the Role.</param>
    /// <returns>A UserRoleInfo object.</returns>
    public UserRoleInfo GetUserRole(int portalId, int userId, int roleId)
    {
        return Provider.GetUserRole(portalId, userId, roleId);
    }

    /// <summary>Gets a list of UserRoles for the user.</summary>
    /// <param name="user">A UserInfo object representing the user.</param>
    /// <param name="includePrivate">Include private roles.</param>
    /// <returns>A list of UserRoleInfo objects.</returns>
    public IList<UserRoleInfo> GetUserRoles(UserInfo user, bool includePrivate)
    {
        return Provider.GetUserRoles(user, includePrivate);
    }

    /// <inheritdoc/>
    public IList<UserRoleInfo> GetUserRoles(int portalId, string userName, string roleName)
    {
        return Provider.GetUserRoles(portalId, userName, roleName).Cast<UserRoleInfo>().ToList();
    }

    /// <inheritdoc/>
    public IList<UserInfo> GetUsersByRole(int portalId, string roleName)
    {
        return Provider.GetUsersByRoleName(portalId, roleName).Cast<UserInfo>().ToList();
    }

    /// <inheritdoc/>
    public void UpdateRole(RoleInfo role, bool addToExistUsers)
    {
        Requires.NotNull("role", role);

        Provider.UpdateRole(role);
        this.AddMessage(role, EventLogController.EventLogType.ROLE_UPDATED);

        if (addToExistUsers)
        {
            this.AutoAssignUsers(role);
        }

        this.ClearRoleCache(role.PortalID);
    }

    /// <inheritdoc/>
    public void UpdateRoleSettings(RoleInfo role, bool clearCache)
    {
        Provider.UpdateRoleSettings(role);

        if (clearCache)
        {
            this.ClearRoleCache(role.PortalID);
        }
    }

    /// <inheritdoc/>
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
                Provider.UpdateUserRole(userRole);
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
            int userRoleId = -1;
            DateTime expiryDate = DateTime.Now;
            DateTime effectiveDate = Null.NullDate;
            bool isTrialUsed = false;
            int period = 0;
            string frequency = string.Empty;
            if (userRole != null)
            {
                userRoleId = userRole.UserRoleID;
                effectiveDate = userRole.EffectiveDate;
                expiryDate = userRole.ExpiryDate;
                isTrialUsed = userRole.IsTrialUsed;
            }

            RoleInfo role = Instance.GetRole(portalId, r => r.RoleID == roleId);
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
                switch (frequency)
                {
                    case "N":
                        expiryDate = Null.NullDate;
                        break;
                    case "O":
                        expiryDate = new DateTime(9999, 12, 31);
                        break;
                    case "D":
                        expiryDate = expiryDate.AddDays(period);
                        break;
                    case "W":
                        expiryDate = expiryDate.AddDays(period * 7);
                        break;
                    case "M":
                        expiryDate = expiryDate.AddMonths(period);
                        break;
                    case "Y":
                        expiryDate = expiryDate.AddYears(period);
                        break;
                }
            }

            if (userRoleId != -1 && userRole != null)
            {
                userRole.ExpiryDate = expiryDate;
                userRole.Status = status;
                userRole.IsOwner = isOwner;
                Provider.UpdateUserRole(userRole);
                EventLogController.Instance.AddLog(userRole, PortalController.Instance.GetCurrentPortalSettings(), UserController.Instance.GetCurrentUserInfo().UserID, string.Empty, EventLogController.EventLogType.USER_ROLE_UPDATED);
            }
            else
            {
                this.AddUserRole(portalId, userId, roleId, status, isOwner, effectiveDate, expiryDate);
            }
        }

        // Remove the UserInfo from the Cache, as it has been modified
        DataCache.ClearUserCache(portalId, user.Username);
        Instance.ClearRoleCache(portalId);
    }

    /// <inheritdoc/>
    int IRoleController.AddRole(RoleInfo role, bool addToExistUsers)
    {
        Requires.NotNull("role", role);

        var roleId = -1;
        if (Provider.CreateRole(role))
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

    /// <inheritdoc/>
    void IRoleController.UpdateRole(RoleInfo role)
    {
        this.UpdateRole(role, true);
    }

    /// <summary>Completely remove all a user's roles for a specific portal. This method is used when anonymizing a user. </summary>
    /// <param name="user">User for which all roles must be deleted. The PortalId property is used to determine for which portal roles must be removed.</param>
    internal static void DeleteUserRoles(UserInfo user)
    {
        var ctrl = new RoleController();
        var userRoles = ctrl.GetUserRoles(user, true);
        foreach (var ur in userRoles.Where(r => r.PortalID == user.PortalID))
        {
            Provider.RemoveUserFromRole(user.PortalID, user, ur);
        }
    }

    /// <inheritdoc/>
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
                Provider.RemoveUserFromRole(portalId, user, userRole);
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

    private static void SendNotification(UserInfo objUser, RoleInfo objRole, PortalSettings portalSettings, UserRoleActions action)
    {
        var custom = new ArrayList { objRole.RoleName, objRole.Description };
        switch (action)
        {
            case UserRoleActions.Add:
            case UserRoleActions.Update:
                string preferredLocale = objUser.Profile.PreferredLocale;
                if (string.IsNullOrEmpty(preferredLocale))
                {
                    preferredLocale = portalSettings.DefaultLanguage;
                }

                var ci = new CultureInfo(preferredLocale);
                UserRoleInfo objUserRole = RoleController.Instance.GetUserRole(portalSettings.PortalId, objUser.UserID, objRole.RoleID);
                custom.Add(Null.IsNull(objUserRole.EffectiveDate)
                    ? DateTime.Today.ToString("g", ci)
                    : objUserRole.EffectiveDate.ToString("g", ci));
                custom.Add(Null.IsNull(objUserRole.ExpiryDate) ? "-" : objUserRole.ExpiryDate.ToString("g", ci));
                break;
            case UserRoleActions.Delete:
                custom.Add(string.Empty);
                break;
        }

        var message = new Message
        {
            FromUserID = portalSettings.AdministratorId,
            ToUserID = objUser.UserID,
            Subject =
                Localization.GetSystemMessage(
                    objUser.Profile.PreferredLocale,
                    portalSettings,
                    $"EMAIL_ROLE_{UserRoleActionsCaption[(int)action]}_SUBJECT",
                    objUser),
            Body = Localization.GetSystemMessage(
                objUser.Profile.PreferredLocale,
                portalSettings,
                $"EMAIL_ROLE_{UserRoleActionsCaption[(int)action]}_BODY",
                objUser,
                Localization.GlobalResourceFile,
                custom),
            Status = MessageStatusType.Unread,
        };

        // _messagingController.SaveMessage(_message);
        Mail.SendEmail(portalSettings.Email, objUser.Email, message.Subject, message.Body);
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
}
