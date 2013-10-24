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

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Users;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Services.Journal;
using DotNetNuke.Services.Social.Notifications;
using DotNetNuke.Web.Api;

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace DotNetNuke.Modules.Journal
{
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    [ValidateAntiForgeryToken]
    public class NotificationServicesController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(NotificationServicesController));

        public class NotificationDTO
        {
            public int NotificationId { get; set; }
        }

        [HttpPost]
        public HttpResponseMessage ViewJournal(NotificationDTO postData)
        {
            try
            {
                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);

                if(notification != null && notification.Context != null && notification.Context.Contains("_"))
                {
                    //Dismiss the notification
                    NotificationsController.Instance.DeleteNotificationRecipient(postData.NotificationId, UserInfo.UserID);

                    var context = notification.Context.Split('_');
                    var userId = Convert.ToInt32(context[0]);
                    var journalId = Convert.ToInt32(context[1]);
                    var ji = JournalController.Instance.GetJournalItem(PortalSettings.PortalId, userId, journalId);
                    if (ji.ProfileId != Null.NullInteger)
                    {
                        return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", Link = Globals.UserProfileURL(ji.ProfileId) });
                    }

                    return Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", Link = Globals.UserProfileURL(userId) });
                }
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
            }

            return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
        }

    }
}