// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
        [ValidateAntiForgeryToken]
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
