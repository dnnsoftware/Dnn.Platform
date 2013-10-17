#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
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
#region Usings

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Portals.Data;
using DotNetNuke.Entities.Profile;
using DotNetNuke.Entities.Users;
using DotNetNuke.Security.Roles;
using DotNetNuke.Security.Roles.Internal;

#endregion

namespace DotNetNuke.Entities.Portals
{
    public class PortalGroupController : ComponentBase<IPortalGroupController, PortalGroupController>, IPortalGroupController
    {
        private readonly IDataService _dataService;
        private readonly IPortalController _portalController;

        #region Constructors

        public PortalGroupController()
            : this(DataService.Instance, new PortalController())
        {
        }

        public PortalGroupController(IDataService dataService, IPortalController portalController)
        {
            //Argument Contract
            Requires.NotNull("dataService", dataService);
            Requires.NotNull("portalController", portalController);

            _dataService = dataService;
            _portalController = portalController;
        }

        #endregion

        #region Private Methods

        private object GetPortalGroupsCallback(CacheItemArgs cacheItemArgs)
        {
            return CBO.FillCollection<PortalGroupInfo>(_dataService.GetPortalGroups());
        }

        private static void ClearCache()
        {
            DataCache.RemoveCache(DataCache.PortalGroupsCacheKey);
        }

        private void OnAddPortalToGroupStart(UserCopiedCallback callback, PortalInfo portal)
        {
            if (callback == null) return;

            var args = new UserCopiedEventArgs
                           {
                               TotalUsers = 0,
                               UserNo = 0,
                               UserName = "",
                               PortalName = portal.PortalName,
                               Stage = "starting"
                           };
            callback(args);
        }

        private void OnUserAddedToSiteGroup(UserCopiedCallback callback, PortalInfo portal,  UserInfo currentUser, int totalUsers, int currentUserNumber)
        {
            if (callback == null) return;

            var args = new UserCopiedEventArgs
                    {
                        TotalUsers = totalUsers,
                        UserNo = currentUserNumber,
                        UserName = currentUser.Username,
                        PortalName = portal.PortalName
                    };
            callback(args);
        }

        private void OnAddPortalToGroupFinishing(UserCopiedCallback callback, PortalInfo portal, int totalUsers)
        {
            if (callback == null) return;

            var args = new UserCopiedEventArgs
            {
                TotalUsers = totalUsers,
                UserNo = totalUsers,
                UserName = "",
                PortalName = portal.PortalName,
                Stage = "finalizing"
            };
            callback(args);
        }

        private void OnAddPortalToGroupFinished(UserCopiedCallback callback, PortalInfo portal, PortalGroupInfo portalGroup, int totalUsers)
        {
            if (callback == null) return;

            var args = new UserCopiedEventArgs
            {
                TotalUsers = totalUsers,
                UserNo = totalUsers,
                UserName = "",
                PortalName = portal.PortalName,
                Stage = "finished",
                PortalGroupId = portalGroup.PortalGroupId
            };
            callback(args);
        }

        #endregion

        #region IPortalGroupController Members

        public void AddPortalToGroup(PortalInfo portal, PortalGroupInfo portalGroup, UserCopiedCallback callback)
        {
            Requires.NotNull("portal", portal);
            Requires.PropertyNotNegative("portal", "PortalId", portal.PortalID);
            Requires.NotNull("portalGroup", portalGroup);
            Requires.PropertyNotNegative("portalGroup", "PortalGroupId", portalGroup.PortalGroupId);
            Requires.PropertyNotNegative("portalGroup", "MasterPortalId", portalGroup.MasterPortalId);

            OnAddPortalToGroupStart(callback, portal);
            
            var users = UserController.GetUsers(portal.PortalID);
            var masterUsers = UserController.GetUsers(portalGroup.MasterPortalId);
            var totalUsers = users.Count + masterUsers.Count;
            var userNo = 0;
            
            if (users.Count > 0)
            {
                var masterPortal = _portalController.GetPortal(portalGroup.MasterPortalId);

                foreach (UserInfo user in users)
                {
                    userNo += 1;

                    //move user to master portal
                    UserController.CopyUserToPortal(user, masterPortal, true, true);

                    OnUserAddedToSiteGroup(callback, portal, user, totalUsers, userNo);
                }
            }

            if (masterUsers.Count > 0)
            {
                var autoAssignRoles = TestableRoleController.Instance.GetRoles(portal.PortalID,
                                                                               role =>
                                                                               role.AutoAssignment &&
                                                                               role.Status == RoleStatus.Approved);
                foreach (UserInfo user in masterUsers)
                {
                    userNo += 1;
                    foreach (var autoAssignRole in autoAssignRoles)
                    {
                        var roleController = new RoleController();
                        roleController.AddUserRole(portalGroup.MasterPortalId, user.UserID, autoAssignRole.RoleID,
                                                   Null.NullDate, Null.NullDate);
                    }
                    OnUserAddedToSiteGroup(callback, portal, user, totalUsers, userNo);
                }
            }

            OnAddPortalToGroupFinishing(callback, portal, users.Count);

            RemoveProfileDefinitions(portal);

            //Add portal to group
            portal.PortalGroupID = portalGroup.PortalGroupId;
            _portalController.UpdatePortalInfo(portal);

            OnAddPortalToGroupFinished(callback, portal, portalGroup, users.Count);
        }
        
        private static void RemoveProfileDefinitions(PortalInfo portal)
        {
            foreach (ProfilePropertyDefinition definition in ProfileController.GetPropertyDefinitionsByPortal(portal.PortalID))
            {
                ProfileController.DeletePropertyDefinition(definition);
            }
        }

        public int AddPortalGroup(PortalGroupInfo portalGroup)
        {
            //Argument Contract
            Requires.NotNull("portalGroup", portalGroup);

            portalGroup.PortalGroupId = _dataService.AddPortalGroup(portalGroup, UserController.GetCurrentUserInfo().UserID);

            //Update portal
            var portal = _portalController.GetPortal(portalGroup.MasterPortalId);
            if (portal != null)
            {
                portal.PortalGroupID = portalGroup.PortalGroupId;
                _portalController.UpdatePortalInfo(portal);
            }

            ClearCache();

            return portalGroup.PortalGroupId;
        }

        public void DeletePortalGroup(PortalGroupInfo portalGroup)
        {
            //Argument Contract
            Requires.NotNull("portalGroup", portalGroup);
            Requires.PropertyNotNegative("portalGroup", "PortalGroupId", portalGroup.PortalGroupId);

            //Update portal
            var portal = _portalController.GetPortal(portalGroup.MasterPortalId);
            if (portal != null)
            {
                portal.PortalGroupID = -1;
                _portalController.UpdatePortalInfo(portal);
            }

            _dataService.DeletePortalGroup(portalGroup);

            ClearCache();
        }

        public IEnumerable<PortalGroupInfo> GetPortalGroups()
        {
            return CBO.GetCachedObject<IEnumerable<PortalGroupInfo>>(new CacheItemArgs(DataCache.PortalGroupsCacheKey,
                                                                                    DataCache.PortalGroupsCacheTimeOut,
                                                                                    DataCache.PortalGroupsCachePriority),
                                                                                GetPortalGroupsCallback);
        }

        public IEnumerable<PortalInfo> GetPortalsByGroup(int portalGroupId)
        {
            var controller = new PortalController();
            var portals = controller.GetPortals();

            return portals.Cast<PortalInfo>()
                            .Where(portal => portal.PortalGroupID == portalGroupId)
                            .ToList();
        }

        public void RemovePortalFromGroup(PortalInfo portal, PortalGroupInfo portalGroup, bool copyUsers, UserCopiedCallback callback)
        {
            //Argument Contract
            Requires.NotNull("portal", portal);
            Requires.PropertyNotNegative("portal", "PortalId", portal.PortalID);
            Requires.NotNull("portalGroup", portalGroup);
            Requires.PropertyNotNegative("portalGroup", "PortalGroupId", portalGroup.PortalGroupId);
            Requires.PropertyNotNegative("portalGroup", "MasterPortalId", portalGroup.MasterPortalId);

            //Callback to update progress bar
            var args = new UserCopiedEventArgs
            {
                TotalUsers = 0,
                UserNo = 0,
                UserName = "",
                PortalName = portal.PortalName,
                Stage = "startingremove"
            };
            callback(args);

            //Remove portal from group
            portal.PortalGroupID = -1;
            _portalController.UpdatePortalInfo(portal);

            var userNo = 0;
            if (copyUsers)
            {
                var users = UserController.GetUsers(portalGroup.MasterPortalId);
                foreach (UserInfo masterUser in users)
                {
                    userNo += 1;

                    //Copy user to portal
                    UserController.CopyUserToPortal(masterUser, portal, false, false);

                    //Callback to update progress bar
                    args = new UserCopiedEventArgs
                    {
                        TotalUsers = users.Count,
                        UserNo = userNo,
                        UserName = masterUser.Username,
                        PortalName = portal.PortalName
                    };

                    callback(args);
                }
            }
            else
            {
                //Get admin users
                var roleController = new RoleController();
                var adminUsers = roleController.GetUsersByRoleName(Null.NullInteger, portal.AdministratorRoleName)
                    .Cast<UserInfo>()
                    .Where(u => roleController.GetUserRole(portal.PortalID, u.UserID, portal.AdministratorRoleId) != null);

                foreach (var user in adminUsers)
                {
                    //Copy Administrator to portal
                    UserController.CopyUserToPortal(user, portal, false, false);

                    //Callback to update progress bar
                    args = new UserCopiedEventArgs
                    {
                        TotalUsers = 1,
                        UserNo = ++userNo,
                        UserName = user.Username,
                        PortalName = portal.PortalName
                    };

                    callback(args);
                }
            }
            //Callback to update progress bar
            args = new UserCopiedEventArgs
            {
                TotalUsers = 1,
                UserNo = userNo,
                UserName = "",
                PortalName = portal.PortalName,
                Stage = "finishedremove",
                PortalGroupId = portalGroup.PortalGroupId
            };
            callback(args);
        }

        public void UpdatePortalGroup(PortalGroupInfo portalGroup)
        {
            //Argument Contract
            Requires.NotNull("portalGroup", portalGroup);
            Requires.PropertyNotNegative("portalGroup", "PortalGroupId", portalGroup.PortalGroupId);

            _dataService.UpdatePortalGroup(portalGroup, UserController.GetCurrentUserInfo().UserID);

            ClearCache();
        }

        #endregion
    }
}
