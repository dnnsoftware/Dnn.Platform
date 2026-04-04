// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.Journal
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Journal;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Web.Api;

    using Microsoft.Extensions.Logging;

    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    [ValidateAntiForgeryToken]
    public class NotificationServicesController : DnnApiController
    {
        private static readonly ILogger Logger = DnnLoggingController.GetLogger<NotificationServicesController>();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ViewJournal(NotificationDTO postData)
        {
            try
            {
                var notification = NotificationsController.Instance.GetNotification(postData.NotificationId);

                if (notification is { Context: not null, } && notification.Context.Contains("_"))
                {
                    // Dismiss the notification
                    NotificationsController.Instance.DeleteNotificationRecipient(postData.NotificationId, this.UserInfo.UserID);

                    var context = notification.Context.Split('_');
                    var userId = Convert.ToInt32(context[0]);
                    var journalId = Convert.ToInt32(context[1]);
                    var ji = JournalController.Instance.GetJournalItem(this.PortalSettings.PortalId, userId, journalId);
                    var profileLink = ji.ProfileId != Null.NullInteger
                        ? Globals.UserProfileURL(ji.ProfileId)
                        : Globals.UserProfileURL(userId);
                    return this.Request.CreateResponse(
                        HttpStatusCode.OK,
                        new { Result = "success", Link = profileLink, });
                }
            }
            catch (Exception exc)
            {
                Logger.NotificationServicesControllerViewJournalException(exc);
            }

            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "unable to process notification");
        }

        public class NotificationDTO
        {
            public int NotificationId { get; set; }
        }
    }
}
