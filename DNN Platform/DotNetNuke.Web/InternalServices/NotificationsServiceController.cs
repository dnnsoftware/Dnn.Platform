#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

using System;
using System.Collections.Generic;
using System.Dynamic;
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
        public class DismissDTO
        {
            public int NotificationId { get; set; }
        }
        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Dismiss(DismissDTO postData)
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