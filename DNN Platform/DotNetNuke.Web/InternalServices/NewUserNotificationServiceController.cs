#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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

using System.Net;
using System.Net.Http;
using System.Web.Http;

using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Web.InternalServices
{
    [DnnAuthorize]
    [DnnExceptionFilter]
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
