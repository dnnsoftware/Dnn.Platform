// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Groups
{
    using System;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.Groups.Components;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Social.Messaging.Internal;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Web.Api;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A web API controller for moderation in the Groups module.</summary>
    /// <param name="navigationManager">The navigation manager.</param>
    /// <param name="roleProvider">The role provider.</param>
    /// <param name="roleController">The role controller.</param>
    /// <param name="eventManager">The event manager.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="userController">The user controller.</param>
    /// <param name="eventLogger">The event logger.</param>
    /// <param name="hostSettings">The host settings.</param>
    [DnnAuthorize]
    public class ModerationServiceController(INavigationManager navigationManager, RoleProvider roleProvider, IRoleController roleController, IEventManager eventManager, IPortalController portalController, IUserController userController, IEventLogger eventLogger, IHostSettings hostSettings)
        : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ModerationServiceController));
        private readonly RoleProvider roleProvider = roleProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<RoleProvider>();
        private readonly IRoleController roleController = roleController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IRoleController>();
        private readonly IEventManager eventManager = eventManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventManager>();
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        private readonly IUserController userController = userController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>();
        private readonly IEventLogger eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        private int tabId;
        private int moduleId;
        private int roleId;
        private int memberId;
        private RoleInfo roleInfo;

        /// <summary>Initializes a new instance of the <see cref="ModerationServiceController"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with RoleProvider. Scheduled removal in v12.0.0.")]
        public ModerationServiceController(INavigationManager navigationManager)
            : this(navigationManager, null, null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ModerationServiceController"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="roleController">The role controller.</param>
        /// <param name="eventManager">The event manager.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ModerationServiceController(INavigationManager navigationManager, RoleProvider roleProvider, IRoleController roleController, IEventManager eventManager, IPortalController portalController, IUserController userController, IEventLogger eventLogger)
            : this(navigationManager, roleProvider, roleController, eventManager, portalController, userController, eventLogger, null)
        {
        }

        /// <summary>Gets the navigation manager.</summary>
        protected INavigationManager NavigationManager { get; } = navigationManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<INavigationManager>();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ApproveGroup(NotificationDTO postData)
        {
            try
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(postData.NotificationId, this.UserInfo.UserID);
                if (recipient == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate recipient");
                }

                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                this.ParseKey(notification.Context);
                if (this.roleInfo == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate role");
                }

                if (!this.IsMod())
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Not Authorized!");
                }

                this.roleInfo.Status = RoleStatus.Approved;
                RoleController.Instance.UpdateRole(this.roleInfo);
                var roleCreator = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, this.roleInfo.CreatedByUserID);

                // Update the original creator's role
                RoleController.Instance.UpdateUserRole(this.PortalSettings.PortalId, roleCreator.UserID, this.roleInfo.RoleID, RoleStatus.Approved, true, false);
                GroupUtilities.CreateJournalEntry(this.roleInfo, roleCreator);

                var notifications = new Notifications(this.hostSettings);
                var siteAdmin = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, this.PortalSettings.AdministratorId);
                notifications.AddGroupNotification(Constants.GroupApprovedNotification, this.tabId, this.moduleId, this.roleInfo, siteAdmin, new List<RoleInfo> { this.roleInfo });
                NotificationsController.Instance.DeleteAllNotificationRecipients(postData.NotificationId);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RejectGroup(NotificationDTO postData)
        {
            try
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(postData.NotificationId, this.UserInfo.UserID);
                if (recipient == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate recipient");
                }

                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                this.ParseKey(notification.Context);
                if (this.roleInfo == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate role");
                }

                if (!this.IsMod())
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Not Authorized!");
                }

                var notifications = new Notifications(this.hostSettings);
                var roleCreator = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, this.roleInfo.CreatedByUserID);
                var siteAdmin = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, this.PortalSettings.AdministratorId);
                notifications.AddGroupNotification(Constants.GroupRejectedNotification, this.tabId, this.moduleId, this.roleInfo, siteAdmin, new List<RoleInfo> { this.roleInfo }, roleCreator);

                var role = RoleController.Instance.GetRole(this.PortalSettings.PortalId, r => r.RoleID == this.roleId);
                RoleController.Instance.DeleteRole(role);
                NotificationsController.Instance.DeleteAllNotificationRecipients(postData.NotificationId);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
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
                if (this.UserInfo.UserID >= 0 && postData.RoleId > -1)
                {
                    this.roleInfo = RoleController.Instance.GetRoleById(this.PortalSettings.PortalId, postData.RoleId);
                    if (this.roleInfo != null)
                    {
                        var requireApproval = false;

                        if (this.roleInfo.Settings.ContainsKey("ReviewMembers"))
                        {
                            requireApproval = Convert.ToBoolean(this.roleInfo.Settings["ReviewMembers"]);
                        }

                        if ((this.roleInfo.IsPublic || this.UserInfo.IsInRole(this.PortalSettings.AdministratorRoleName)) && !requireApproval)
                        {
                            RoleController.Instance.AddUserRole(this.PortalSettings.PortalId, this.UserInfo.UserID, this.roleInfo.RoleID, RoleStatus.Approved, false, Null.NullDate, Null.NullDate);
                            RoleController.Instance.UpdateRole(this.roleInfo);

                            var url = this.NavigationManager.NavigateURL(postData.GroupViewTabId, string.Empty, new[] { "groupid=" + this.roleInfo.RoleID });
                            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", URL = url });
                        }

                        if (this.roleInfo.IsPublic && requireApproval)
                        {
                            RoleController.Instance.AddUserRole(this.PortalSettings.PortalId, this.UserInfo.UserID, this.roleInfo.RoleID, RoleStatus.Pending, false, Null.NullDate, Null.NullDate);
                            var notifications = new Notifications(this.hostSettings);
                            notifications.AddGroupOwnerNotification(Constants.MemberPendingNotification, this.tabId, this.moduleId, this.roleInfo, this.UserInfo);
                            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", URL = string.Empty });
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }

            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unknown Error");
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
                if (this.UserInfo.UserID >= 0 && postData.RoleId > 0)
                {
                    this.roleInfo = RoleController.Instance.GetRoleById(this.PortalSettings.PortalId, postData.RoleId);

                    if (this.roleInfo != null)
                    {
                        if (this.UserInfo.IsInRole(this.roleInfo.RoleName))
                        {
                            RoleController.DeleteUserRole(this.roleProvider, this.roleController, this.eventManager, this.portalController, this.userController, this.eventLogger, this.UserInfo, this.roleInfo, this.PortalSettings, false);
                        }

                        success = true;
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }

            if (success)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }

            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unknown Error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ApproveMember(NotificationDTO postData)
        {
            try
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(postData.NotificationId, this.UserInfo.UserID);
                if (recipient == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate recipient");
                }

                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                this.ParseKey(notification.Context);
                if (this.memberId <= 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate Member");
                }

                if (this.roleInfo == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate Role");
                }

                var member = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, this.memberId);

                if (member != null)
                {
                    var memberRoleInfo = RoleController.Instance.GetUserRole(this.PortalSettings.PortalId, this.memberId, this.roleInfo.RoleID);
                    memberRoleInfo.Status = RoleStatus.Approved;
                    RoleController.Instance.UpdateUserRole(this.PortalSettings.PortalId, this.memberId, this.roleInfo.RoleID, RoleStatus.Approved, false, false);

                    var notifications = new Notifications(this.hostSettings);
                    var groupOwner = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, this.roleInfo.CreatedByUserID);
                    notifications.AddMemberNotification(Constants.MemberApprovedNotification, this.tabId, this.moduleId, this.roleInfo, groupOwner, member);
                    NotificationsController.Instance.DeleteAllNotificationRecipients(postData.NotificationId);

                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }

            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unknown Error");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RejectMember(NotificationDTO postData)
        {
            try
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(postData.NotificationId, this.UserInfo.UserID);
                if (recipient == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate recipient");
                }

                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                this.ParseKey(notification.Context);
                if (this.memberId <= 0)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate Member");
                }

                if (this.roleInfo == null)
                {
                    return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unable to locate Role");
                }

                var member = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, this.memberId);

                if (member != null)
                {
                    RoleController.DeleteUserRole(this.roleProvider, this.roleController, this.eventManager, this.portalController, this.userController, this.eventLogger, member, this.roleInfo, this.PortalSettings, false);
                    var notifications = new Notifications(this.hostSettings);
                    var groupOwner = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, this.roleInfo.CreatedByUserID);
                    notifications.AddMemberNotification(Constants.MemberRejectedNotification, this.tabId, this.moduleId, this.roleInfo, groupOwner, member);
                    NotificationsController.Instance.DeleteAllNotificationRecipients(postData.NotificationId);

                    return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }

            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unknown Error");
        }

        private void ParseKey(string key)
        {
            this.tabId = -1;
            this.moduleId = -1;
            this.roleId = -1;
            this.memberId = -1;
            this.roleInfo = null;
            if (!string.IsNullOrEmpty(key))
            {
                string[] keys = key.Split(':');
                this.tabId = Convert.ToInt32(keys[0]);
                this.moduleId = Convert.ToInt32(keys[1]);
                this.roleId = Convert.ToInt32(keys[2]);
                if (keys.Length > 3)
                {
                    this.memberId = Convert.ToInt32(keys[3]);
                }
            }

            if (this.roleId > 0)
            {
                this.roleInfo = RoleController.Instance.GetRoleById(this.PortalSettings.PortalId, this.roleId);
            }
        }

        private bool IsMod()
        {
            var objModulePermissions = new ModulePermissionCollection(CBO.FillCollection(DataProvider.Instance().GetModulePermissionsByModuleID(this.moduleId, -1), typeof(ModulePermissionInfo)));
            return ModulePermissionController.HasModulePermission(objModulePermissions, "MODGROUP");
        }

        public class NotificationDTO
        {
            public int NotificationId { get; set; }
        }

        public class RoleDTO
        {
            public int RoleId { get; set; }

            public int GroupViewTabId { get; set; }
        }
    }
}
