// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Modules.Groups.Components;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Security.Roles;
using DotNetNuke.Services.Social.Messaging.Internal;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Web.Api;
using DotNetNuke.Security;
using DotNetNuke.Abstractions;

namespace DotNetNuke.Modules.Groups
{
    [DnnAuthorize]
    public class ModerationServiceController : DnnApiController
    {
        protected INavigationManager NavigationManager { get; }
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (ModerationServiceController));
        private int _tabId;
        private int _moduleId;
        private int _roleId;
        private int _memberId;
        private RoleInfo _roleInfo;

        public ModerationServiceController(INavigationManager navigationManager)
        {
            NavigationManager = navigationManager;
        }

        public class NotificationDTO
        {
            public int NotificationId { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ApproveGroup(NotificationDTO postData)
        {
            try
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(postData.NotificationId, UserInfo.UserID);
                if (recipient == null) return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate recipient");

                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                ParseKey(notification.Context);
                if (_roleInfo == null)
                {
                    return  Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate role");
                }
                if (!IsMod())
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Not Authorized!");
                }
                _roleInfo.Status = RoleStatus.Approved;
                RoleController.Instance.UpdateRole(_roleInfo);
                var roleCreator = UserController.GetUserById(PortalSettings.PortalId, _roleInfo.CreatedByUserID);
                //Update the original creator's role
                RoleController.Instance.UpdateUserRole(PortalSettings.PortalId, roleCreator.UserID, _roleInfo.RoleID, RoleStatus.Approved, true, false);
                GroupUtilities.CreateJournalEntry(_roleInfo, roleCreator);

                var notifications = new Notifications();
                var siteAdmin = UserController.GetUserById(PortalSettings.PortalId, PortalSettings.AdministratorId);
                notifications.AddGroupNotification(Constants.GroupApprovedNotification, _tabId, _moduleId, _roleInfo, siteAdmin, new List<RoleInfo> { _roleInfo });
                NotificationsController.Instance.DeleteAllNotificationRecipients(postData.NotificationId);

                return Request.CreateResponse(HttpStatusCode.OK, new {Result = "success"});
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RejectGroup(NotificationDTO postData)
        {
            try
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(postData.NotificationId, UserInfo.UserID);
                if (recipient == null) return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate recipient");

                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                ParseKey(notification.Context);
                if (_roleInfo == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate role");
                }
                if (!IsMod())
                {
                    return Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Not Authorized!");
                }
                var notifications = new Notifications();
                var roleCreator = UserController.GetUserById(PortalSettings.PortalId, _roleInfo.CreatedByUserID);
                var siteAdmin = UserController.GetUserById(PortalSettings.PortalId, PortalSettings.AdministratorId);
                notifications.AddGroupNotification(Constants.GroupRejectedNotification, _tabId, _moduleId, _roleInfo, siteAdmin, new List<RoleInfo> { _roleInfo }, roleCreator);

                var role = RoleController.Instance.GetRole(PortalSettings.PortalId, r => r.RoleID == _roleId);
                RoleController.Instance.DeleteRole(role);
                NotificationsController.Instance.DeleteAllNotificationRecipients(postData.NotificationId);
                return Request.CreateResponse(HttpStatusCode.OK, new {Result = "success"});
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [SupportedModules("Social Groups")]
        public HttpResponseMessage JoinGroup(RoleDTO postData)
        {
            try
            {
                if (UserInfo.UserID >= 0 && postData.RoleId > -1)
                {
                    _roleInfo = RoleController.Instance.GetRoleById(PortalSettings.PortalId, postData.RoleId);
                    if (_roleInfo != null)
                    {

                        var requireApproval = false;

                        if(_roleInfo.Settings.ContainsKey("ReviewMembers"))
                            requireApproval = Convert.ToBoolean(_roleInfo.Settings["ReviewMembers"]);


                        if ((_roleInfo.IsPublic || UserInfo.IsInRole(PortalSettings.AdministratorRoleName)) && !requireApproval)
                        {
                            RoleController.Instance.AddUserRole(PortalSettings.PortalId, UserInfo.UserID, _roleInfo.RoleID, RoleStatus.Approved, false, Null.NullDate, Null.NullDate);
                            RoleController.Instance.UpdateRole(_roleInfo);

                            var url = NavigationManager.NavigateURL(postData.GroupViewTabId, "", new[] { "groupid=" + _roleInfo.RoleID });
                            return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", URL = url });
                        
                        }
                        if (_roleInfo.IsPublic && requireApproval)
                        {
                            RoleController.Instance.AddUserRole(PortalSettings.PortalId, UserInfo.UserID, _roleInfo.RoleID, RoleStatus.Pending, false, Null.NullDate, Null.NullDate);
                            var notifications = new Notifications();
                            notifications.AddGroupOwnerNotification(Constants.MemberPendingNotification, _tabId, _moduleId, _roleInfo, UserInfo);
                            return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", URL = string.Empty });
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }

            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unknown Error");
        }

        public class RoleDTO
        {
            public int RoleId { get; set; }
            public int GroupViewTabId { get; set; }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
        [SupportedModules("Social Groups")]
        public HttpResponseMessage LeaveGroup(RoleDTO postData)
        {
            var success = false;

            try
            {
                if (UserInfo.UserID >= 0 && postData.RoleId > 0)
                {
                    _roleInfo = RoleController.Instance.GetRoleById(PortalSettings.PortalId, postData.RoleId);

                    if (_roleInfo != null)
                    {
                        if (UserInfo.IsInRole(_roleInfo.RoleName))
                        {
                            RoleController.DeleteUserRole(UserInfo, _roleInfo, PortalSettings, false);
                        }
                        success = true;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }

            if(success)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new {Result = "success"});
            }
            
            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unknown Error");
        }
       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ApproveMember(NotificationDTO postData)
        {
            try
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(postData.NotificationId, UserInfo.UserID);
                if (recipient == null) return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate recipient");

                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                ParseKey(notification.Context);
                if (_memberId <= 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate Member");
                }

                if (_roleInfo == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate Role");
                }

                var member = UserController.GetUserById(PortalSettings.PortalId, _memberId);

                if (member != null)
                {
                    var memberRoleInfo = RoleController.Instance.GetUserRole(PortalSettings.PortalId, _memberId, _roleInfo.RoleID);
                    memberRoleInfo.Status = RoleStatus.Approved;
                    RoleController.Instance.UpdateUserRole(PortalSettings.PortalId, _memberId, _roleInfo.RoleID, RoleStatus.Approved, false, false);
                    
                    var notifications = new Notifications();
                    var groupOwner = UserController.GetUserById(PortalSettings.PortalId, _roleInfo.CreatedByUserID);
                    notifications.AddMemberNotification(Constants.MemberApprovedNotification, _tabId, _moduleId, _roleInfo, groupOwner, member);
                    NotificationsController.Instance.DeleteAllNotificationRecipients(postData.NotificationId);

                    return Request.CreateResponse(HttpStatusCode.OK, new {Result = "success"});
                }
            } catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }

            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unknown Error");
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RejectMember(NotificationDTO postData)
        {
            try
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(postData.NotificationId, UserInfo.UserID);
                if (recipient == null) return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate recipient");

                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                ParseKey(notification.Context);
                if (_memberId <= 0)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate Member");
                }
                if (_roleInfo == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate Role");
                }

                var member = UserController.GetUserById(PortalSettings.PortalId, _memberId);



                if (member != null)
                {
                    RoleController.DeleteUserRole(member, _roleInfo, PortalSettings, false) ;
                    var notifications = new Notifications();
                    var groupOwner = UserController.GetUserById(PortalSettings.PortalId, _roleInfo.CreatedByUserID);
                    notifications.AddMemberNotification(Constants.MemberRejectedNotification, _tabId, _moduleId, _roleInfo, groupOwner, member);
                    NotificationsController.Instance.DeleteAllNotificationRecipients(postData.NotificationId);

                    return Request.CreateResponse(HttpStatusCode.OK, new {Result = "success"});
                }
            } catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }

            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unknown Error");
        }
        
        private void ParseKey(string key)
        {
            _tabId = -1;
            _moduleId = -1;
            _roleId = -1;
            _memberId = -1;
            _roleInfo = null;
            if (!String.IsNullOrEmpty(key))
            {
                string[] keys = key.Split(':');
                _tabId = Convert.ToInt32(keys[0]);
                _moduleId = Convert.ToInt32(keys[1]);
                _roleId = Convert.ToInt32(keys[2]);
                if (keys.Length > 3)
                {
                    _memberId = Convert.ToInt32(keys[3]);
                }
            }
            if (_roleId > 0)
            {
                _roleInfo = RoleController.Instance.GetRoleById(PortalSettings.PortalId, _roleId);
            }
        }

        private bool IsMod()
        {
            var objModulePermissions = new ModulePermissionCollection(CBO.FillCollection(DataProvider.Instance().GetModulePermissionsByModuleID(_moduleId, -1), typeof(ModulePermissionInfo)));
            return ModulePermissionController.HasModulePermission(objModulePermissions, "MODGROUP");
        }
    }
}
