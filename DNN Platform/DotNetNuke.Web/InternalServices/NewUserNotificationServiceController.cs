// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Entities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Web.Api;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A web API controller for new user notifications.</summary>
    /// <param name="roleProvider">The role provider.</param>
    /// <param name="roleController">The role controller.</param>
    /// <param name="eventManager">The event manager.</param>
    /// <param name="portalController">The portal controller.</param>
    /// <param name="userController">The user controller.</param>
    /// <param name="eventLogger">The event logger.</param>
    /// <param name="hostSettings">The host settings.</param>
    [DnnAuthorize]
    public class NewUserNotificationServiceController(RoleProvider roleProvider, IRoleController roleController, IEventManager eventManager, IPortalController portalController, IUserController userController, IEventLogger eventLogger, IHostSettings hostSettings)
        : DnnApiController
    {
        private readonly RoleProvider roleProvider = roleProvider ?? Globals.GetCurrentServiceProvider().GetRequiredService<RoleProvider>();
        private readonly IRoleController roleController = roleController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IRoleController>();
        private readonly IEventManager eventManager = eventManager ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventManager>();
        private readonly IPortalController portalController = portalController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IPortalController>();
        private readonly IUserController userController = userController ?? Globals.GetCurrentServiceProvider().GetRequiredService<IUserController>();
        private readonly IEventLogger eventLogger = eventLogger ?? Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>();
        private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

        /// <summary>Initializes a new instance of the <see cref="NewUserNotificationServiceController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public NewUserNotificationServiceController()
            : this(null, null, null, null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="NewUserNotificationServiceController"/> class.</summary>
        /// <param name="roleProvider">The role provider.</param>
        /// <param name="roleController">The role controller.</param>
        /// <param name="eventManager">The event manager.</param>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="userController">The user controller.</param>
        /// <param name="eventLogger">The event logger.</param>
        [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public NewUserNotificationServiceController(RoleProvider roleProvider, IRoleController roleController, IEventManager eventManager, IPortalController portalController, IUserController userController, IEventLogger eventLogger)
            : this(roleProvider, roleController, eventManager, portalController, userController, eventLogger, null)
        {
        }

        /// <summary>Authorizes a new user.</summary>
        /// <param name="postData">Information about the request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Authorize(NotificationDTO postData)
        {
            var user = this.GetUser(postData);
            if (user == null)
            {
                NotificationsController.Instance.DeleteNotification(postData.NotificationId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "User not found");
            }

            user.Membership.Approved = true;
            UserController.UpdateUser(this.eventLogger, this.PortalSettings.PortalId, user);

            // Update User Roles if needed
            if (!user.IsSuperUser && user.IsInRole("Unverified Users") && this.PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration)
            {
                UserController.ApproveUser(this.roleProvider, this.roleController, this.eventManager, this.portalController, this.userController, this.eventLogger, this.PortalSettings, user);
            }

            Mail.SendMail(user, MessageType.UserAuthorized, this.PortalSettings);

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
        }

        /// <summary>Rejects a new user.</summary>
        /// <param name="postData">Information about the request.</param>
        /// <returns>A response indicating success.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Reject(NotificationDTO postData)
        {
            var user = this.GetUser(postData);
            if (user == null)
            {
                NotificationsController.Instance.DeleteNotification(postData.NotificationId);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "User not found");
            }

            UserController.RemoveUser(user);

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
        }

        /// <summary>Sends a verification email to the current user.</summary>
        /// <param name="postData">Information about the request.</param>
        /// <returns>A response with an object that has a <c>Result</c> field.</returns>
        /// <exception cref="UserAlreadyVerifiedException">The user is already verified.</exception>
        /// <exception cref="InvalidVerificationCodeException">The user is not unverified.</exception>
        [HttpPost]
        [DnnAuthorize]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SendVerificationMail(NotificationDTO postData)
        {
            if (this.UserInfo.Membership.Approved)
            {
                throw new UserAlreadyVerifiedException();
            }

            if (!this.UserInfo.IsInRole("Unverified Users"))
            {
                throw new InvalidVerificationCodeException();
            }

            var message = Mail.SendMail(this.UserInfo, MessageType.UserRegistrationVerified, this.PortalSettings);
            if (string.IsNullOrEmpty(message))
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = Localization.GetSafeJSString("VerificationMailSendSuccessful", Localization.SharedResourceFile), });
            }
            else
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        private UserInfo GetUser(NotificationDTO notificationDto)
        {
            var notification = NotificationsController.Instance.GetNotification(notificationDto.NotificationId);

            if (!int.TryParse(notification.Context, out var userId))
            {
                return null;
            }

            return UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, userId);
        }
    }
}
