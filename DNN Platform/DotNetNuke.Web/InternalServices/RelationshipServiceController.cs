// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices;

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using DotNetNuke.Abstractions.Application;
using DotNetNuke.Common;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Social;
using DotNetNuke.Instrumentation;
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Social.Messaging.Internal;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Web.Api;

using Microsoft.Extensions.DependencyInjection;

/// <summary>A web API controller for relationships.</summary>
/// <param name="hostSettings">The host settings.</param>
[DnnAuthorize]
public partial class RelationshipServiceController(IHostSettings hostSettings)
    : DnnApiController
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(RelationshipServiceController));
    private readonly IHostSettings hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();

    /// <summary>Initializes a new instance of the <see cref="RelationshipServiceController"/> class.</summary>
    [Obsolete("Deprecated in DotNetNuke 10.2.4. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
    public RelationshipServiceController()
        : this(null)
    {
    }

    /// <summary>Accept a friend.</summary>
    /// <param name="postData">The request.</param>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DnnDeprecated(10, 3, 3, "Use overload taking NotificationRequest")]
    public partial HttpResponseMessage AcceptFriend(NotificationDTO postData)
        => this.AcceptFriend(postData?.ToNotificationRequest());

    /// <summary>Accept a friend.</summary>
    /// <param name="requestBody">The request.</param>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public HttpResponseMessage AcceptFriend(NotificationRequest requestBody)
    {
        var success = false;

        try
        {
            var recipient = InternalMessagingController.Instance.GetMessageRecipient(requestBody.NotificationId, this.UserInfo.UserID);
            if (recipient != null)
            {
                var notification = NotificationsController.Instance.GetNotification(requestBody.NotificationId);
                if (int.TryParse(notification.Context, out var userRelationshipId))
                {
                    var userRelationship = RelationshipController.Instance.GetUserRelationship(userRelationshipId);
                    if (userRelationship != null)
                    {
                        var friend = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, userRelationship.UserId);
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
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
        }

        return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
    }

    /// <summary>Follow a user who has requested to follow you.</summary>
    /// <param name="postData">The request.</param>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DnnDeprecated(10, 3, 3, "Use overload taking NotificationRequest")]
    public partial HttpResponseMessage FollowBack(NotificationDTO postData)
        => this.FollowBack(postData?.ToNotificationRequest());

    /// <summary>Follow a user who has requested to follow you.</summary>
    /// <param name="requestBody">The request.</param>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public HttpResponseMessage FollowBack(NotificationRequest requestBody)
    {
        var success = false;

        try
        {
            var recipient = InternalMessagingController.Instance.GetMessageRecipient(requestBody.NotificationId, this.UserInfo.UserID);
            if (recipient != null)
            {
                var notification = NotificationsController.Instance.GetNotification(requestBody.NotificationId);
                if (int.TryParse(notification.Context, out var targetUserId))
                {
                    var targetUser = UserController.GetUserById(this.hostSettings, this.PortalSettings.PortalId, targetUserId);

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
                    NotificationsController.Instance.DeleteNotificationRecipient(requestBody.NotificationId, this.UserInfo.UserID);

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
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", });
        }

        return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
    }
}
