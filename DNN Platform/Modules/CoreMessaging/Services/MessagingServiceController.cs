// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.CoreMessaging.Services
{
    using System;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;

    using DotNetNuke.Common;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.CoreMessaging.ViewModels;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Social.Messaging;
    using DotNetNuke.Services.Social.Messaging.Internal;
    using DotNetNuke.Services.Social.Notifications;
    using DotNetNuke.Web.Api;

    [SupportedModules("DotNetNuke.Modules.CoreMessaging")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    [DnnAuthorize]
    public class MessagingServiceController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MessagingServiceController));

        [HttpGet]
        public HttpResponseMessage Inbox(int afterMessageId, int numberOfRecords)
        {
            try
            {
                var messageBoxView = InternalMessagingController.Instance.GetRecentInbox(this.UserInfo.UserID, afterMessageId, numberOfRecords);
                var portalId = PortalController.GetEffectivePortalId(UserController.Instance.GetCurrentUserInfo().PortalID);

                messageBoxView.TotalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId);
                messageBoxView.TotalConversations = InternalMessagingController.Instance.CountConversations(this.UserInfo.UserID, portalId);

                return this.Request.CreateResponse(HttpStatusCode.OK, messageBoxView);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage Sentbox(int afterMessageId, int numberOfRecords)
        {
            try
            {
                var messageBoxView = InternalMessagingController.Instance.GetRecentSentbox(this.UserInfo.UserID, afterMessageId, numberOfRecords);
                var portalId = PortalController.GetEffectivePortalId(UserController.Instance.GetCurrentUserInfo().PortalID);
                messageBoxView.TotalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId);
                messageBoxView.TotalConversations = InternalMessagingController.Instance.CountSentConversations(this.UserInfo.UserID, portalId);

                return this.Request.CreateResponse(HttpStatusCode.OK, messageBoxView);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage Archived(int afterMessageId, int numberOfRecords)
        {
            try
            {
                var messageBoxView = InternalMessagingController.Instance.GetArchivedMessages(this.UserInfo.UserID, afterMessageId, numberOfRecords);
                var portalId = PortalController.GetEffectivePortalId(UserController.Instance.GetCurrentUserInfo().PortalID);
                messageBoxView.TotalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId);
                messageBoxView.TotalConversations = InternalMessagingController.Instance.CountArchivedConversations(this.UserInfo.UserID, portalId);

                return this.Request.CreateResponse(HttpStatusCode.OK, messageBoxView);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage Thread(int conversationId, int afterMessageId, int numberOfRecords)
        {
            try
            {
                var totalRecords = 0;
                var messageThreadsView = InternalMessagingController.Instance.GetMessageThread(conversationId, this.UserInfo.UserID, afterMessageId, numberOfRecords, ref totalRecords);
                var portalId = PortalController.GetEffectivePortalId(UserController.Instance.GetCurrentUserInfo().PortalID);
                messageThreadsView.TotalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId);
                messageThreadsView.TotalThreads = InternalMessagingController.Instance.CountMessagesByConversation(conversationId);
                messageThreadsView.TotalArchivedThreads = InternalMessagingController.Instance.CountArchivedMessagesByConversation(conversationId);

                return this.Request.CreateResponse(HttpStatusCode.OK, messageThreadsView);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Reply(ReplyDTO postData)
        {
            try
            {
                postData.Body = HttpUtility.UrlDecode(postData.Body);
                var messageId = InternalMessagingController.Instance.ReplyMessage(postData.ConversationId, postData.Body, postData.FileIds);
                var message = this.ToExpandoObject(InternalMessagingController.Instance.GetMessage(messageId));
                var portalId = PortalController.GetEffectivePortalId(UserController.Instance.GetCurrentUserInfo().PortalID);

                var totalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId);
                var totalThreads = InternalMessagingController.Instance.CountMessagesByConversation(postData.ConversationId);
                var totalArchivedThreads = InternalMessagingController.Instance.CountArchivedMessagesByConversation(postData.ConversationId);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Conversation = message, TotalNewThreads = totalNewThreads, TotalThreads = totalThreads, TotalArchivedThreads = totalArchivedThreads });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MarkArchived(ConversationDTO postData)
        {
            try
            {
                InternalMessagingController.Instance.MarkArchived(postData.ConversationId, this.UserInfo.UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MarkUnArchived(ConversationDTO postData)
        {
            try
            {
                InternalMessagingController.Instance.MarkUnArchived(postData.ConversationId, this.UserInfo.UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MarkRead(ConversationDTO postData)
        {
            try
            {
                InternalMessagingController.Instance.MarkRead(postData.ConversationId, this.UserInfo.UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MarkUnRead(ConversationDTO postData)
        {
            try
            {
                InternalMessagingController.Instance.MarkUnRead(postData.ConversationId, this.UserInfo.UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteUserFromConversation(ConversationDTO postData)
        {
            try
            {
                InternalMessagingController.Instance.DeleteUserFromConversation(postData.ConversationId, this.UserInfo.UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage Notifications(int afterNotificationId, int numberOfRecords)
        {
            try
            {
                var portalId = PortalController.GetEffectivePortalId(UserController.Instance.GetCurrentUserInfo().PortalID);
                var notificationsDomainModel = NotificationsController.Instance.GetNotifications(this.UserInfo.UserID, portalId, afterNotificationId, numberOfRecords);

                var notificationsViewModel = new NotificationsViewModel
                {
                    TotalNotifications = NotificationsController.Instance.CountNotifications(this.UserInfo.UserID, portalId),
                    Notifications = new List<NotificationViewModel>(notificationsDomainModel.Count),
                };

                foreach (var notification in notificationsDomainModel)
                {
                    var user = UserController.Instance.GetUser(this.PortalSettings.PortalId, notification.SenderUserID);
                    var displayName = user != null ? user.DisplayName : string.Empty;

                    var notificationViewModel = new NotificationViewModel
                    {
                        NotificationId = notification.NotificationID,
                        Subject = notification.Subject,
                        From = notification.From,
                        Body = notification.Body,
                        DisplayDate = Common.Utilities.DateUtils.CalculateDateForDisplay(notification.CreatedOnDate),
                        SenderAvatar = UserController.Instance.GetUserProfilePictureUrl(notification.SenderUserID, 64, 64),
                        SenderProfileUrl = Globals.UserProfileURL(notification.SenderUserID),
                        SenderDisplayName = displayName,
                        Actions = new List<NotificationActionViewModel>(),
                    };

                    var notificationType = NotificationsController.Instance.GetNotificationType(notification.NotificationTypeID);
                    var notificationTypeActions = NotificationsController.Instance.GetNotificationTypeActions(notification.NotificationTypeID);

                    foreach (var notificationTypeAction in notificationTypeActions)
                    {
                        var notificationActionViewModel = new NotificationActionViewModel
                        {
                            Name = this.LocalizeActionString(notificationTypeAction.NameResourceKey, notificationType.DesktopModuleId),
                            Description = this.LocalizeActionString(notificationTypeAction.DescriptionResourceKey, notificationType.DesktopModuleId),
                            Confirm = this.LocalizeActionString(notificationTypeAction.ConfirmResourceKey, notificationType.DesktopModuleId),
                            APICall = notificationTypeAction.APICall,
                        };

                        notificationViewModel.Actions.Add(notificationActionViewModel);
                    }

                    if (notification.IncludeDismissAction)
                    {
                        notificationViewModel.Actions.Add(new NotificationActionViewModel
                        {
                            Name = Localization.GetString("Dismiss.Text"),
                            Description = Localization.GetString("DismissNotification.Text"),
                            Confirm = string.Empty,
                            APICall = "API/InternalServices/NotificationsService/Dismiss",
                        });
                    }

                    notificationsViewModel.Notifications.Add(notificationViewModel);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, notificationsViewModel);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage CheckReplyHasRecipients(int conversationId)
        {
            try
            {
                var recipientCount = InternalMessagingController.Instance.CheckReplyHasRecipients(conversationId, UserController.Instance.GetCurrentUserInfo().UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, recipientCount);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage CountNotifications()
        {
            try
            {
                var portalId = PortalController.GetEffectivePortalId(UserController.Instance.GetCurrentUserInfo().PortalID);
                int notifications = NotificationsController.Instance.CountNotifications(this.UserInfo.UserID, portalId);
                return this.Request.CreateResponse(HttpStatusCode.OK, notifications);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage CountUnreadMessages()
        {
            try
            {
                var portalId = PortalController.GetEffectivePortalId(UserController.Instance.GetCurrentUserInfo().PortalID);
                var unreadMessages = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId);
                return this.Request.CreateResponse(HttpStatusCode.OK, unreadMessages);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetTotals()
        {
            try
            {
                var portalId = PortalController.GetEffectivePortalId(UserController.Instance.GetCurrentUserInfo().PortalID);
                var totalsViewModel = new TotalsViewModel
                {
                    TotalUnreadMessages = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId),
                    TotalNotifications = NotificationsController.Instance.CountNotifications(this.UserInfo.UserID, portalId),
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, totalsViewModel);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DismissAllNotifications()
        {
            try
            {
                var deletedCount = NotificationsController.Instance.DeleteUserNotifications(this.UserInfo);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", count = deletedCount });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        private string LocalizeActionString(string key, int desktopModuleId)
        {
            if (string.IsNullOrEmpty(key))
            {
                return string.Empty;
            }

            string actionString;

            if (desktopModuleId > 0)
            {
                var desktopModule = DesktopModuleController.GetDesktopModule(desktopModuleId, this.PortalSettings.PortalId);

                var resourceFile = string.Format(
                    "~/DesktopModules/{0}/{1}/{2}",
                    desktopModule.FolderName.Replace("\\", "/"),
                    Localization.LocalResourceDirectory,
                    Localization.LocalSharedResourceFile);

                actionString = Localization.GetString(key, resourceFile);
            }
            else
            {
                actionString = Localization.GetString(key);
            }

            return string.IsNullOrEmpty(actionString) ? key : actionString;
        }

        private dynamic ToExpandoObject(Message message)
        {
            dynamic messageObj = new ExpandoObject();
            messageObj.PortalID = message.PortalID;
            messageObj.KeyID = message.KeyID;
            messageObj.MessageID = message.MessageID;
            messageObj.ConversationId = message.ConversationId;
            messageObj.SenderUserID = message.SenderUserID;
            messageObj.From = message.From;
            messageObj.To = message.To;
            messageObj.Subject = message.Subject;
            messageObj.Body = message.Body;
            messageObj.DisplayDate = message.DisplayDate;
            messageObj.ReplyAllAllowed = message.ReplyAllAllowed;

            // base entity properties
            messageObj.CreatedByUserID = message.CreatedByUserID;
            messageObj.CreatedOnDate = message.CreatedOnDate;
            messageObj.LastModifiedByUserID = message.LastModifiedByUserID;
            messageObj.LastModifiedOnDate = message.LastModifiedOnDate;

            return messageObj;
        }

        public class ConversationDTO
        {
            public int ConversationId { get; set; }
        }

        public class ReplyDTO : ConversationDTO
        {
            public string Body { get; set; }

            public IList<int> FileIds { get; set; }
        }
    }
}
