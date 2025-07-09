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

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Extensions;
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

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Provides messaging web services.</summary>
    [SupportedModules("DotNetNuke.Modules.CoreMessaging")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    [DnnAuthorize]
    public class MessagingServiceController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(MessagingServiceController));
        private readonly IPortalController portalController;
        private readonly IApplicationStatusInfo appStatus;
        private readonly IPortalGroupController portalGroupController;

        /// <summary>Initializes a new instance of the <see cref="MessagingServiceController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IPortalController. Scheduled removal in v12.0.0.")]
        public MessagingServiceController()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="MessagingServiceController"/> class.</summary>
        /// <param name="portalController">The portal controller.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalGroupController">The portal group controller.</param>
        public MessagingServiceController(IPortalController portalController, IApplicationStatusInfo appStatus, IPortalGroupController portalGroupController)
        {
            this.portalController = portalController ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IPortalController>();
            this.appStatus = appStatus ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IApplicationStatusInfo>();
            this.portalGroupController = portalGroupController ?? HttpContextSource.Current.GetScope().ServiceProvider.GetRequiredService<IPortalGroupController>();
        }

        /// <summary>Provides access to the user inbox.</summary>
        /// <param name="afterMessageId">After which message id to start returning new messages.</param>
        /// <param name="numberOfRecords">How many messages to get.</param>
        /// <returns>A <see cref="DotNetNuke.Services.Social.Messaging.Internal.Views.MessageBoxView"/>.</returns>
        [HttpGet]
        public HttpResponseMessage Inbox(int afterMessageId, int numberOfRecords)
        {
            try
            {
                var messageBoxView = InternalMessagingController.Instance.GetRecentInbox(this.UserInfo.UserID, afterMessageId, numberOfRecords);
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, UserController.Instance.GetCurrentUserInfo().PortalID);

                messageBoxView.TotalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId);
                messageBoxView.TotalConversations = InternalMessagingController.Instance.CountConversations(this.UserInfo.UserID, portalId);

                return this.Request.CreateResponse(HttpStatusCode.OK, messageBoxView);
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while trying to fetch the inbox, consult the server logs for more information.";
                Logger.Error(message, ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Provides access to the sent box view.</summary>
        /// <param name="afterMessageId">After which message to obtain new messages.</param>
        /// <param name="numberOfRecords">How many messages to get.</param>
        /// <returns>A <see cref="DotNetNuke.Services.Social.Messaging.Internal.Views.MessageBoxView"/>.</returns>
        [HttpGet]
        public HttpResponseMessage Sentbox(int afterMessageId, int numberOfRecords)
        {
            try
            {
                var messageBoxView = InternalMessagingController.Instance.GetRecentSentbox(this.UserInfo.UserID, afterMessageId, numberOfRecords);
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, UserController.Instance.GetCurrentUserInfo().PortalID);
                messageBoxView.TotalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId);
                messageBoxView.TotalConversations = InternalMessagingController.Instance.CountSentConversations(this.UserInfo.UserID, portalId);

                return this.Request.CreateResponse(HttpStatusCode.OK, messageBoxView);
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to fetch the Sent box, consult the server logs for more information.";
                Logger.Error(message, ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Provides access to the archived box.</summary>
        /// <param name="afterMessageId">After which message to get new messages.</param>
        /// <param name="numberOfRecords">How many messages to get.</param>
        /// <returns>A <see cref="DotNetNuke.Services.Social.Messaging.Internal.Views.MessageBoxView"/>.</returns>
        [HttpGet]
        public HttpResponseMessage Archived(int afterMessageId, int numberOfRecords)
        {
            try
            {
                var messageBoxView = InternalMessagingController.Instance.GetArchivedMessages(this.UserInfo.UserID, afterMessageId, numberOfRecords);
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, UserController.Instance.GetCurrentUserInfo().PortalID);
                messageBoxView.TotalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId);
                messageBoxView.TotalConversations = InternalMessagingController.Instance.CountArchivedConversations(this.UserInfo.UserID, portalId);

                return this.Request.CreateResponse(HttpStatusCode.OK, messageBoxView);
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to fetch the archived box.";
                Logger.Error(message, ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Provides access to a message thread.</summary>
        /// <param name="conversationId">The conversation id to get the thread from.</param>
        /// <param name="afterMessageId">After which message to get new messages.</param>
        /// <param name="numberOfRecords">How many messages to get.</param>
        /// <returns>A <see cref="DotNetNuke.Services.Social.Messaging.Internal.Views.MessageThreadsView"/>.</returns>
        [HttpGet]
        public HttpResponseMessage Thread(int conversationId, int afterMessageId, int numberOfRecords)
        {
            try
            {
                var totalRecords = 0;
                var messageThreadsView = InternalMessagingController.Instance.GetMessageThread(conversationId, this.UserInfo.UserID, afterMessageId, numberOfRecords, ref totalRecords);
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, UserController.Instance.GetCurrentUserInfo().PortalID);
                messageThreadsView.TotalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId);
                messageThreadsView.TotalThreads = InternalMessagingController.Instance.CountMessagesByConversation(conversationId);
                messageThreadsView.TotalArchivedThreads = InternalMessagingController.Instance.CountArchivedMessagesByConversation(conversationId);

                return this.Request.CreateResponse(HttpStatusCode.OK, messageThreadsView);
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to fetch the thread, consult the server logs for more information.";
                Logger.Error(message, ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Provides access to post a reply to a message.</summary>
        /// <param name="postData">The information about the reply, <see cref="ReplyDTO"/>.</param>
        /// <returns>Information about the conversation and message thread.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage Reply(ReplyDTO postData)
        {
            try
            {
                var body = HttpUtility.UrlDecode(postData.Body);
                body = WebUtility.HtmlEncode(body);
                var messageId = InternalMessagingController.Instance.ReplyMessage(postData.ConversationId, body, postData.FileIds);
                var message = this.ToExpandoObject(InternalMessagingController.Instance.GetMessage(messageId));
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, UserController.Instance.GetCurrentUserInfo().PortalID);

                var totalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId);
                var totalThreads = InternalMessagingController.Instance.CountMessagesByConversation(postData.ConversationId);
                var totalArchivedThreads = InternalMessagingController.Instance.CountArchivedMessagesByConversation(postData.ConversationId);

                return this.Request.CreateResponse(HttpStatusCode.OK, new { Conversation = message, TotalNewThreads = totalNewThreads, TotalThreads = totalThreads, TotalArchivedThreads = totalArchivedThreads });
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to reply to a conversation, see the server logs for more information.";
                Logger.Error(message, ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Marks a conversation as archived.</summary>
        /// <param name="postData">The information about the conversation, <see cref="ConversationDTO"/>.</param>
        /// <returns>A "success" result or an InternalServerError.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MarkArchived(ConversationDTO postData)
        {
            try
            {
                InternalMessagingController.Instance.MarkArchived(postData.ConversationId, this.UserInfo.UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to fetch the archived box, consult the server logs for more information.";
                Logger.Error(message, ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Restores a conversation out of the archive.</summary>
        /// <param name="postData">The information about the conversation, <see cref="ConversationDTO"/>.</param>
        /// <returns>A "success" result or an InternalServerError.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MarkUnArchived(ConversationDTO postData)
        {
            try
            {
                InternalMessagingController.Instance.MarkUnArchived(postData.ConversationId, this.UserInfo.UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to restore an archived conversation, consult the server logs for more information.";
                Logger.Error(message, ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Marks a conversation as read.</summary>
        /// <param name="postData">The information about the conversation to mark as read, <see cref="ConversationDTO"/>.</param>
        /// <returns>A "success" Result or an InternalServerError.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MarkRead(ConversationDTO postData)
        {
            try
            {
                InternalMessagingController.Instance.MarkRead(postData.ConversationId, this.UserInfo.UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting mark a conversation as read, consult the server logs for more information.";
                Logger.Error(message, ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Marks a conversation as unread.</summary>
        /// <param name="postData">The information about the conversation to mark unread, <see cref="ConversationDTO"/>.</param>
        /// <returns>A "success" Result or an InternalServerError.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MarkUnRead(ConversationDTO postData)
        {
            try
            {
                InternalMessagingController.Instance.MarkUnRead(postData.ConversationId, this.UserInfo.UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to restore an archived conversation, consult the server logs for more information.";
                Logger.Error(message, ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Deletes a user from a conversation.</summary>
        /// <param name="postData">The information about the conversation, <see cref="ConversationDTO"/>.</param>
        /// <returns>A "success" Result or an InternalServerError.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DeleteUserFromConversation(ConversationDTO postData)
        {
            try
            {
                InternalMessagingController.Instance.DeleteUserFromConversation(postData.ConversationId, this.UserInfo.UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success" });
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to delete a user from a conversation, consult the server logs for more information.";
                Logger.Error(message, ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Gets the user notifications.</summary>
        /// <param name="afterNotificationId">After which notification to get new ones.</param>
        /// <param name="numberOfRecords">How many notifications to get.</param>
        /// <returns>A see <see cref="NotificationViewModel"/> object.</returns>
        [HttpGet]
        public HttpResponseMessage Notifications(int afterNotificationId, int numberOfRecords)
        {
            try
            {
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, UserController.Instance.GetCurrentUserInfo().PortalID);
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
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to fetch messaging notifications, consult the server logs for more information.";
                Logger.Error(message, ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Checks if a reply has recipients.</summary>
        /// <param name="conversationId">The id of conversation to check./>.</param>
        /// <returns>The recipient count or an InternalServerError.</returns>
        [HttpGet]
        public HttpResponseMessage CheckReplyHasRecipients(int conversationId)
        {
            try
            {
                var recipientCount = InternalMessagingController.Instance.CheckReplyHasRecipients(conversationId, UserController.Instance.GetCurrentUserInfo().UserID);
                return this.Request.CreateResponse(HttpStatusCode.OK, recipientCount);
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to check the recipient count on a reply, consult the server logs for more information.";
                Logger.Error(message, ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Gets the notifications count.</summary>
        /// <returns>A number representing the notification count.</returns>
        [HttpGet]
        public HttpResponseMessage CountNotifications()
        {
            try
            {
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, UserController.Instance.GetCurrentUserInfo().PortalID);
                int notifications = NotificationsController.Instance.CountNotifications(this.UserInfo.UserID, portalId);
                return this.Request.CreateResponse(HttpStatusCode.OK, notifications);
            }
            catch (Exception ex)
            {
                const string message = "An unexpected error occurred while attempting to get the notification count, consult the server logs for more information.";
                Logger.Error(message, ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Gets the number of unread messages.</summary>
        /// <returns>The number of unread messages.</returns>
        [HttpGet]
        public HttpResponseMessage CountUnreadMessages()
        {
            try
            {
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, UserController.Instance.GetCurrentUserInfo().PortalID);
                var unreadMessages = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId);
                return this.Request.CreateResponse(HttpStatusCode.OK, unreadMessages);
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to , consult the server logs for more information.";
                Logger.Error(message, ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Gets both the total number of unread messages and new notifications.</summary>
        /// <returns><see cref="TotalsViewModel"/>.</returns>
        [HttpGet]
        public HttpResponseMessage GetTotals()
        {
            try
            {
                var portalId = PortalController.GetEffectivePortalId(this.portalController, this.appStatus, this.portalGroupController, UserController.Instance.GetCurrentUserInfo().PortalID);
                var totalsViewModel = new TotalsViewModel
                {
                    TotalUnreadMessages = InternalMessagingController.Instance.CountUnreadMessages(this.UserInfo.UserID, portalId),
                    TotalNotifications = NotificationsController.Instance.CountNotifications(this.UserInfo.UserID, portalId),
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, totalsViewModel);
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to get the unread messages and new notifications count, consult the server logs for more information.";
                Logger.Error(message, ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, message);
            }
        }

        /// <summary>Dismisses all new notifications.</summary>
        /// <returns>A "success" Result and a deleteCount representing how many notifications where deleted.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage DismissAllNotifications()
        {
            try
            {
                var deletedCount = NotificationsController.Instance.DeleteUserNotifications(this.UserInfo);
                return this.Request.CreateResponse(HttpStatusCode.OK, new { Result = "success", count = deletedCount });
            }
            catch (Exception ex)
            {
                var message = "An unexpected error occurred while attempting to dismiss notifications, consult the server logs for more information.";
                Logger.Error(message, ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, message);
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

        /// <summary>Represents a conversation.</summary>
        public class ConversationDTO
        {
            /// <summary>Gets or sets id of the conversation.</summary>
            public int ConversationId { get; set; }
        }

        /// <summary>Represents a message reply.</summary>
        public class ReplyDTO : ConversationDTO
        {
            /// <summary>Gets or sets the body of the reply.</summary>
            public string Body { get; set; }

            /// <summary>Gets or sets the ids of the attached files.</summary>
            public IList<int> FileIds { get; set; }
        }
    }
}
