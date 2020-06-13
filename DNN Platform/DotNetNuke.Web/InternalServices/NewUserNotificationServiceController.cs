// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Mail;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Web.Api;

    [DnnAuthorize]
    public class NewUserNotificationServiceController : DnnApiController
    {
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
            UserController.UpdateUser(this.PortalSettings.PortalId, user);

            // Update User Roles if needed
            if (!user.IsSuperUser && user.IsInRole("Unverified Users") && this.PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration)
            {
                UserController.ApproveUser(user);
            }

            Mail.SendMail(user, MessageType.UserAuthorized, this.PortalSettings);

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }

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

            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }

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
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = Localization.GetSafeJSString("VerificationMailSendSuccessful", Localization.SharedResourceFile) });
            }
            else
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        private UserInfo GetUser(NotificationDTO notificationDto)
        {
            var notification = NotificationsController.Instance.GetNotification(notificationDto.NotificationId);

            int userId;
            if (!int.TryParse(notification.Context, out userId))
            {
                return null;
            }

            return UserController.GetUserById(this.PortalSettings.PortalId, userId);
        }
    }
}
