// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Portals
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Internal;
    using DotNetNuke.Entities.Portals.Data;
    using DotNetNuke.Entities.Profile;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Security.Roles.Internal;
    using DotNetNuke.Services.Log.EventLog;

    public class PortalGroupController : ComponentBase<IPortalGroupController, PortalGroupController>, IPortalGroupController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PortalGroupController));
        private readonly IDataService _dataService;
        private readonly IPortalController _portalController;

        public PortalGroupController()
            : this(DataService.Instance, PortalController.Instance)
        {
        }

        public PortalGroupController(IDataService dataService, IPortalController portalController)
        {
            // Argument Contract
            Requires.NotNull("dataService", dataService);
            Requires.NotNull("portalController", portalController);

            this._dataService = dataService;
            this._portalController = portalController;
        }

        public void AddPortalToGroup(PortalInfo portal, PortalGroupInfo portalGroup, UserCopiedCallback callback)
        {
            Requires.NotNull("portal", portal);
            Requires.PropertyNotNegative("portal", "PortalId", portal.PortalID);
            Requires.NotNull("portalGroup", portalGroup);
            Requires.PropertyNotNegative("portalGroup", "PortalGroupId", portalGroup.PortalGroupId);
            Requires.PropertyNotNegative("portalGroup", "MasterPortalId", portalGroup.MasterPortalId);

            this.OnAddPortalToGroupStart(callback, portal);

            var users = UserController.GetUsers(portal.PortalID);
            var masterUsers = UserController.GetUsers(portalGroup.MasterPortalId);
            var totalUsers = users.Count + masterUsers.Count;
            var userNo = 0;

            if (users.Count > 0)
            {
                var masterPortal = this._portalController.GetPortal(portalGroup.MasterPortalId);

                foreach (UserInfo user in users)
                {
                    userNo += 1;

                    UserController.MoveUserToPortal(user, masterPortal, true);

                    this.OnUserAddedToSiteGroup(callback, portal, user, totalUsers, userNo);
                }
            }

            if (masterUsers.Count > 0)
            {
                var autoAssignRoles = RoleController.Instance.GetRoles(
                    portal.PortalID,
                    role =>
                                                                               role.AutoAssignment &&
                                                                               role.Status == RoleStatus.Approved);
                foreach (UserInfo user in masterUsers)
                {
                    userNo += 1;
                    foreach (var autoAssignRole in autoAssignRoles)
                    {
                        RoleController.Instance.AddUserRole(portalGroup.MasterPortalId, user.UserID, autoAssignRole.RoleID, RoleStatus.Approved, false, Null.NullDate, Null.NullDate);
                    }

                    this.OnUserAddedToSiteGroup(callback, portal, user, totalUsers, userNo);
                }
            }

            this.OnAddPortalToGroupFinishing(callback, portal, users.Count);

            this.RemoveProfileDefinitions(portal);

            // Add portal to group
            portal.PortalGroupID = portalGroup.PortalGroupId;
            PortalController.Instance.UpdatePortalInfo(portal);
            this.LogEvent(EventLogController.EventLogType.PORTAL_ADDEDTOPORTALGROUP, portalGroup, portal);

            this.OnAddPortalToGroupFinished(callback, portal, portalGroup, users.Count);
        }

        public int AddPortalGroup(PortalGroupInfo portalGroup)
        {
            // Argument Contract
            Requires.NotNull("portalGroup", portalGroup);

            portalGroup.PortalGroupId = this._dataService.AddPortalGroup(portalGroup, UserController.Instance.GetCurrentUserInfo().UserID);

            // Update portal
            var portal = this._portalController.GetPortal(portalGroup.MasterPortalId);
            if (portal != null)
            {
                portal.PortalGroupID = portalGroup.PortalGroupId;
                this._portalController.UpdatePortalInfo(portal);
            }

            this.LogEvent(EventLogController.EventLogType.PORTALGROUP_CREATED, portalGroup, null);

            ClearCache();

            return portalGroup.PortalGroupId;
        }

        public void DeletePortalGroup(PortalGroupInfo portalGroup)
        {
            // Argument Contract
            Requires.NotNull("portalGroup", portalGroup);
            Requires.PropertyNotNegative("portalGroup", "PortalGroupId", portalGroup.PortalGroupId);

            // Update portal
            var portal = this._portalController.GetPortal(portalGroup.MasterPortalId);
            if (portal != null)
            {
                this.DeleteSharedModules(portal);
                portal.PortalGroupID = -1;
                PortalController.Instance.UpdatePortalInfo(portal);
            }

            this._dataService.DeletePortalGroup(portalGroup);
            this.LogEvent(EventLogController.EventLogType.PORTALGROUP_DELETED, portalGroup, null);

            ClearCache();
        }

        public IEnumerable<PortalGroupInfo> GetPortalGroups()
        {
            return CBO.GetCachedObject<IEnumerable<PortalGroupInfo>>(
                new CacheItemArgs(
                DataCache.PortalGroupsCacheKey,
                DataCache.PortalGroupsCacheTimeOut,
                DataCache.PortalGroupsCachePriority),
                this.GetPortalGroupsCallback);
        }

        public IEnumerable<PortalInfo> GetPortalsByGroup(int portalGroupId)
        {
            var portals = PortalController.Instance.GetPortals();

            return portals.Cast<PortalInfo>()
                            .Where(portal => portal.PortalGroupID == portalGroupId)
                            .ToList();
        }

        public void RemovePortalFromGroup(PortalInfo portal, PortalGroupInfo portalGroup, bool copyUsers, UserCopiedCallback callback)
        {
            // Argument Contract
            Requires.NotNull("portal", portal);
            Requires.PropertyNotNegative("portal", "PortalId", portal.PortalID);
            Requires.NotNull("portalGroup", portalGroup);
            Requires.PropertyNotNegative("portalGroup", "PortalGroupId", portalGroup.PortalGroupId);
            Requires.PropertyNotNegative("portalGroup", "MasterPortalId", portalGroup.MasterPortalId);

            // Callback to update progress bar
            var args = new UserCopiedEventArgs
            {
                TotalUsers = 0,
                UserNo = 0,
                UserName = string.Empty,
                PortalName = portal.PortalName,
                Stage = "startingremove",
            };
            callback(args);

            // Remove portal from group
            this.DeleteSharedModules(portal);
            portal.PortalGroupID = -1;
            PortalController.Instance.UpdatePortalInfo(portal);
            this.LogEvent(EventLogController.EventLogType.PORTAL_REMOVEDFROMPORTALGROUP, portalGroup, portal);

            this.CopyPropertyDefinitions(portal.PortalID, portalGroup.MasterPortalId);

            var userNo = 0;
            if (copyUsers)
            {
                var users = UserController.GetUsers(portalGroup.MasterPortalId);
                foreach (UserInfo masterUser in users)
                {
                    userNo += 1;

                    UserController.CopyUserToPortal(masterUser, portal, false);

                    // Callback to update progress bar
                    args = new UserCopiedEventArgs
                    {
                        TotalUsers = users.Count,
                        UserNo = userNo,
                        UserName = masterUser.Username,
                        PortalName = portal.PortalName,
                    };

                    callback(args);
                }
            }
            else
            {
                // Get admin users
                var adminUsers = RoleController.Instance.GetUsersByRole(Null.NullInteger, portal.AdministratorRoleName)
                    .Where(u => RoleController.Instance.GetUserRole(portal.PortalID, u.UserID, portal.AdministratorRoleId) != null);

                foreach (var user in adminUsers)
                {
                    UserController.CopyUserToPortal(user, portal, false);

                    // Callback to update progress bar
                    args = new UserCopiedEventArgs
                    {
                        TotalUsers = 1,
                        UserNo = ++userNo,
                        UserName = user.Username,
                        PortalName = portal.PortalName,
                    };

                    callback(args);
                }
            }

            // Callback to update progress bar
            args = new UserCopiedEventArgs
            {
                TotalUsers = 1,
                UserNo = userNo,
                UserName = string.Empty,
                PortalName = portal.PortalName,
                Stage = "finishedremove",
                PortalGroupId = portalGroup.PortalGroupId,
            };
            callback(args);
        }

        public void UpdatePortalGroup(PortalGroupInfo portalGroup)
        {
            // Argument Contract
            Requires.NotNull("portalGroup", portalGroup);
            Requires.PropertyNotNegative("portalGroup", "PortalGroupId", portalGroup.PortalGroupId);

            this._dataService.UpdatePortalGroup(portalGroup, UserController.Instance.GetCurrentUserInfo().UserID);

            ClearCache();
        }

        public bool IsModuleShared(int moduleId, PortalInfo portal)
        {
            if (portal == null)
            {
                return false;
            }

            return this.GetSharedModulesWithPortal(portal).Any(x => x.ModuleID == moduleId && !x.IsDeleted) || this.GetSharedModulesByPortal(portal).Any(x => x.ModuleID == moduleId && !x.IsDeleted);
        }

        private static void ClearCache()
        {
            DataCache.RemoveCache(DataCache.PortalGroupsCacheKey);
        }

        private object GetPortalGroupsCallback(CacheItemArgs cacheItemArgs)
        {
            return CBO.FillCollection<PortalGroupInfo>(this._dataService.GetPortalGroups());
        }

        private void OnAddPortalToGroupStart(UserCopiedCallback callback, PortalInfo portal)
        {
            if (callback == null)
            {
                return;
            }

            var args = new UserCopiedEventArgs
            {
                TotalUsers = 0,
                UserNo = 0,
                UserName = string.Empty,
                PortalName = portal.PortalName,
                Stage = "starting",
            };
            callback(args);
        }

        private void OnUserAddedToSiteGroup(UserCopiedCallback callback, PortalInfo portal, UserInfo currentUser, int totalUsers, int currentUserNumber)
        {
            if (callback == null)
            {
                return;
            }

            var args = new UserCopiedEventArgs
            {
                TotalUsers = totalUsers,
                UserNo = currentUserNumber,
                UserName = currentUser.Username,
                PortalName = portal.PortalName,
            };
            callback(args);
        }

        private void OnAddPortalToGroupFinishing(UserCopiedCallback callback, PortalInfo portal, int totalUsers)
        {
            if (callback == null)
            {
                return;
            }

            var args = new UserCopiedEventArgs
            {
                TotalUsers = totalUsers,
                UserNo = totalUsers,
                UserName = string.Empty,
                PortalName = portal.PortalName,
                Stage = "finalizing",
            };
            callback(args);
        }

        private void OnAddPortalToGroupFinished(UserCopiedCallback callback, PortalInfo portal, PortalGroupInfo portalGroup, int totalUsers)
        {
            if (callback == null)
            {
                return;
            }

            var args = new UserCopiedEventArgs
            {
                TotalUsers = totalUsers,
                UserNo = totalUsers,
                UserName = string.Empty,
                PortalName = portal.PortalName,
                Stage = "finished",
                PortalGroupId = portalGroup.PortalGroupId,
            };
            callback(args);
        }

        private void LogEvent(EventLogController.EventLogType eventType, PortalGroupInfo portalGroup, PortalInfo portal)
        {
            try
            {
                var log = new LogInfo
                {
                    BypassBuffering = true,
                    LogTypeKey = eventType.ToString(),
                };
                log.LogProperties.Add(new LogDetailInfo("PortalGroup:", portalGroup.PortalGroupName));
                log.LogProperties.Add(new LogDetailInfo("PortalGroupID:", portalGroup.PortalGroupId.ToString()));
                if (portal != null)
                {
                    log.LogProperties.Add(new LogDetailInfo("Portal:", portal.PortalName));
                    log.LogProperties.Add(new LogDetailInfo("PortalID:", portal.PortalID.ToString()));
                }

                LogController.Instance.AddLog(log);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }
        }

        private void RemoveProfileDefinitions(PortalInfo portal)
        {
            foreach (ProfilePropertyDefinition definition in ProfileController.GetPropertyDefinitionsByPortal(portal.PortalID))
            {
                ProfileController.DeletePropertyDefinition(definition);
            }
        }

        private void CopyPropertyDefinitions(int portalId, int masterPortalId)
        {
            foreach (ProfilePropertyDefinition definition in ProfileController.GetPropertyDefinitionsByPortal(masterPortalId))
            {
                var newDefinition = definition.Clone();
                newDefinition.PortalId = portalId;

                ProfileController.AddPropertyDefinition(newDefinition);
            }
        }

        private void DeleteSharedModules(PortalInfo portal)
        {
            var sharedModules = this.GetSharedModulesWithPortal(portal);
            foreach (var sharedModule in sharedModules)
            {
                ModuleController.Instance.DeleteTabModule(sharedModule.TabID, sharedModule.ModuleID, false);
            }

            sharedModules = this.GetSharedModulesByPortal(portal);
            foreach (var sharedModule in sharedModules)
            {
                ModuleController.Instance.DeleteTabModule(sharedModule.TabID, sharedModule.ModuleID, false);
            }
        }

        private IEnumerable<ModuleInfo> GetSharedModulesWithPortal(PortalInfo portal)
        {
            return CBO.GetCachedObject<IEnumerable<ModuleInfo>>(
                new CacheItemArgs(
                DataCache.SharedModulesWithPortalCacheKey,
                DataCache.SharedModulesWithPortalCacheTimeOut,
                DataCache.SharedModulesWithPortalCachePriority,
                portal),
                (p) => CBO.FillCollection<ModuleInfo>(this._dataService.GetSharedModulesWithPortal(portal)));
        }

        private IEnumerable<ModuleInfo> GetSharedModulesByPortal(PortalInfo portal)
        {
            return CBO.GetCachedObject<IEnumerable<ModuleInfo>>(
                new CacheItemArgs(
                DataCache.SharedModulesByPortalCacheKey,
                DataCache.SharedModulesByPortalCacheTimeOut,
                DataCache.SharedModulesByPortalCachePriority,
                portal),
                (p) => CBO.FillCollection<ModuleInfo>(this._dataService.GetSharedModulesByPortal(portal)));
        }
    }
}
