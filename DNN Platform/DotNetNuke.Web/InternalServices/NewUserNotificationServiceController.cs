using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Common;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Web.Api;
using DotNetNuke.Services.Mail;

namespace DotNetNuke.Web.InternalServices
{
    [DnnAuthorize]
    public class NewUserNotificationServiceController : DnnApiController
    {        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Authorize(NotificationDTO postData)
        {
            var user = GetUser(postData);
            if (user == null)
            {
                NotificationsController.Instance.DeleteNotification(postData.NotificationId);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "User not found");
            }

            user.Membership.Approved = true;
            UserController.UpdateUser(PortalSettings.PortalId, user);

            //Update User Roles if needed
            if (!user.IsSuperUser && user.IsInRole("Unverified Users") && PortalSettings.UserRegistration == (int)Globals.PortalRegistrationType.VerifiedRegistration)
            {
                UserController.ApproveUser(user);
            }

            Mail.SendMail(user, MessageType.UserAuthorized, PortalSettings);

            return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Reject(NotificationDTO postData)
        {
            var user = GetUser(postData);
            if (user == null)
            {
                NotificationsController.Instance.DeleteNotification(postData.NotificationId);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "User not found");
            }

            UserController.RemoveUser(user);

            return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
        }

        [HttpPost]
        [DnnAuthorize]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage SendVerificationMail(NotificationDTO postData)
        {
            if (UserInfo.Membership.Approved)
            {
                throw new UserAlreadyVerifiedException();
            }

            if (!UserInfo.IsInRole("Unverified Users"))
            {
                throw new InvalidVerificationCodeException();
            }

            var message = Mail.SendMail(UserInfo, MessageType.UserRegistrationVerified, PortalSettings);
            if (string.IsNullOrEmpty(message))
            {
                return Request.CreateResponse(HttpStatusCode.OK, new {Result = Localization.GetSafeJSString("VerificationMailSendSuccessful", Localization.SharedResourceFile) });
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message);
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

            return UserController.GetUserById(PortalSettings.PortalId, userId);            
        }
    }
}
