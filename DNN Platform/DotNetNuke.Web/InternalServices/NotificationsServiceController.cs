// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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

namespace DotNetNuke.Web.InternalServices
{
    [DnnAuthorize]
    public class NotificationsServiceController : DnnApiController
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (NotificationsServiceController));
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Dismiss(NotificationDTO postData)
        {
            try
            {
                var recipient = InternalMessagingController.Instance.GetMessageRecipient(postData.NotificationId, UserInfo.UserID);
                if (recipient != null)
                {
                    NotificationsController.Instance.DeleteNotificationRecipient(postData.NotificationId, UserInfo.UserID);
                    return Request.CreateResponse(HttpStatusCode.OK, new {Result = "success"});
                }

                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Unable to dismiss notification");
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }


		[HttpGet]
		public HttpResponseMessage GetToasts()
		{
			var toasts = NotificationsController.Instance.GetToasts(this.UserInfo);
			IList<object> convertedObjects = toasts.Select(ToExpandoObject).ToList();
			return Request.CreateResponse(HttpStatusCode.OK, new { Success = true, Toasts = convertedObjects.Take(3) });
		}

		private object ToExpandoObject(Notification notification)
		{
			return new {Subject = notification.Subject, Body = notification.Body};
		}

    }
}
