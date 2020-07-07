// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Social.Messaging.Internal;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Web.Api;

    [DnnAuthorize]
    public class NotificationsServiceController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(NotificationsServiceController));

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Dismiss(NotificationDTO postData)
        {
            try
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(postData.NotificationId, this.UserInfo.UserID);
                if (recipient != null)
                {
                    NotificationsController.Instance.DeleteNotificationRecipient(postData.NotificationId, this.UserInfo.UserID);
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
}
