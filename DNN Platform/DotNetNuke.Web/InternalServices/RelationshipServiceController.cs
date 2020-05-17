﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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

namespace DotNetNuke.Web.InternalServices
{
    [DnnAuthorize]
    public class RelationshipServiceController : DnnApiController
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (RelationshipServiceController));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage AcceptFriend(NotificationDTO postData)
        {
            var success = false;

            try
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(postData.NotificationId, UserInfo.UserID);
                if (recipient != null)
                {
                    var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                    int userRelationshipId;
                    if (int.TryParse(notification.Context, out userRelationshipId))
                    {
                        var userRelationship = RelationshipController.Instance.GetUserRelationship(userRelationshipId);
                        if (userRelationship != null)
                        {
                            var friend = UserController.GetUserById(PortalSettings.PortalId, userRelationship.UserId);
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
            
            if(success)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new {Result = "success"});
            }

            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage FollowBack(NotificationDTO postData)
        {
            var success = false;

            try
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(postData.NotificationId, UserInfo.UserID);
                if (recipient != null)
                {
                    var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);
                    int targetUserId;
                    if (int.TryParse(notification.Context, out targetUserId))
                    {
                        var targetUser = UserController.GetUserById(PortalSettings.PortalId, targetUserId);

                        if (targetUser == null)
                        {
                            var response = new
                            {
                                Message = Localization.GetExceptionMessage("UserDoesNotExist",
                                    "The user you are trying to follow no longer exists.")
                            };
                            return Request.CreateResponse(HttpStatusCode.InternalServerError, response);
                        }

                        FollowersController.Instance.FollowUser(targetUser);
                        NotificationsController.Instance.DeleteNotificationRecipient(postData.NotificationId, UserInfo.UserID);

                        success = true;
                    }
                }
            }
            catch (UserRelationshipExistsException exc)
            {
                Logger.Error(exc);
                var response = new
                {
                    Message = Localization.GetExceptionMessage("AlreadyFollowingUser",
                        "You are already following this user.")
                };
                return Request.CreateResponse(HttpStatusCode.InternalServerError, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc.Message);
            }

            if (success)
            {
                return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }

            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
        }
    }
}
