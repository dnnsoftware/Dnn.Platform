// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using DotNetNuke.Instrumentation;
using DotNetNuke.Internal.SourceGenerators;
using DotNetNuke.Services.Social.Messaging.Internal;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Web.Api;

/// <summary>A web API controller for notifications.</summary>
[DnnAuthorize]
public partial class NotificationsServiceController : DnnApiController
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(NotificationsServiceController));

    /// <summary>Dismisses a notification.</summary>
    /// <param name="postData">Information about the notification.</param>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    [DnnDeprecated(10, 3, 3, "Use overload taking NotificationRequest")]
    public partial HttpResponseMessage Dismiss(NotificationDTO postData)
        => this.Dismiss(postData?.ToNotificationRequest());

    /// <summary>Dismisses a notification.</summary>
    /// <param name="requestBody">Information about the notification.</param>
    /// <returns>A response indicating success.</returns>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public HttpResponseMessage Dismiss(NotificationRequest requestBody)
    {
        try
        {
            var recipient = InternalMessagingController.Instance.GetMessageRecipient(requestBody.NotificationId, this.UserInfo.UserID);
            if (recipient != null)
            {
                NotificationsController.Instance.DeleteNotificationRecipient(requestBody.NotificationId, this.UserInfo.UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }

            return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Unable to dismiss notification");
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
        }
    }

    /// <summary>Gets toasts for the user.</summary>
    /// <returns>A response with an object that has a <c>Toasts</c> field.</returns>
    [HttpGet]
    public HttpResponseMessage GetToasts()
    {
        var toasts = NotificationsController.Instance.GetToasts(this.UserInfo);
        IList<object> convertedObjects = toasts.Select(this.ToExpandoObject).ToList();
        return this.Request.CreateResponse(HttpStatusCode.OK, new { Success = true, Toasts = convertedObjects.Take(3) });
    }

    private object ToExpandoObject(Notification notification)
    {
        return new { Subject = notification.Subject, Body = notification.Body };
    }
}
