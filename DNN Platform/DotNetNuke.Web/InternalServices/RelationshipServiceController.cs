// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Entities.Users.Social;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Social.Messaging.Internal;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Web.Api;

    [DnnAuthorize]
    public class RelationshipServiceController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RelationshipServiceController));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage AcceptFriend(NotificationDTO postData)
        {
            var success = false;

            try
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(postData.NotificationId, this.UserInfo.UserID);
                if (recipient != null)
                {
                    var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                    int userRelationshipId;
                    if (int.TryParse(notification.Context, out userRelationshipId))
                    {
                        var userRelationship = RelationshipController.Instance.GetUserRelationship(userRelationshipId);
                        if (userRelationship != null)
                        {
                            var friend = UserController.GetUserById(this.PortalSettings.PortalId, userRelationship.UserId);
                            FriendsController.Instance.AcceptFriend(friend);
                            success = true;
                        }
                    }
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            if (success)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }

            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage FollowBack(NotificationDTO postData)
        {
            var success = false;

            try
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(postData.NotificationId, this.UserInfo.UserID);
                if (recipient != null)
                {
                    var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                    int targetUserId;
                    if (int.TryParse(notification.Context, out targetUserId))
                    {
                        var targetUser = UserController.GetUserById(this.PortalSettings.PortalId, targetUserId);

                        if (targetUser == null)
                        {
                            var response = new
                            {
                                Message = Localization.GetExceptionMessage(
                                    "UserDoesNotExist",
                                    "The user you are trying to follow no longer exists."),
                            };
                            return this.Request.CreateResponse(HttpStatusCode.InternalServerError, response);
                        }

                        FollowersController.Instance.FollowUser(targetUser);
                        NotificationsController.Instance.DeleteNotificationRecipient(postData.NotificationId, this.UserInfo.UserID);

                        success = true;
                    }
                }
            }
            catch (UserRelationshipExistsException exc)
            {
                Logger.Error(exc);
                var response = new
                {
                    Message = Localization.GetExceptionMessage(
                        "AlreadyFollowingUser",
                        "You are already following this user."),
                };
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }

            if (success)
            {
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }

            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
        }
    }
}
