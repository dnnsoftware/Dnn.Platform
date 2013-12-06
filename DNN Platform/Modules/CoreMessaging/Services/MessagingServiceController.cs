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
using System.Net;
using System.Web;
using System.Web.Http;
using System.Net.Http;
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

namespace DotNetNuke.Modules.CoreMessaging.Services
{
    [SupportedModules("DotNetNuke.Modules.CoreMessaging")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    [DnnAuthorize]
    public class MessagingServiceController : DnnApiController
    {
    	private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof (MessagingServiceController));
        #region Public Methods

        [HttpGet]
        public HttpResponseMessage Inbox(int afterMessageId, int numberOfRecords)
        {
            try
            {
                var messageBoxView = InternalMessagingController.Instance.GetRecentInbox(UserInfo.UserID, afterMessageId, numberOfRecords);
                var portalId = PortalController.GetEffectivePortalId(UserController.GetCurrentUserInfo().PortalID);

                messageBoxView.TotalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(UserInfo.UserID, portalId);
                messageBoxView.TotalConversations = InternalMessagingController.Instance.CountConversations(UserInfo.UserID, portalId);

                return Request.CreateResponse(HttpStatusCode.OK, messageBoxView);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage Sentbox(int afterMessageId, int numberOfRecords)
        {
            try
            {
                var messageBoxView = InternalMessagingController.Instance.GetRecentSentbox(UserInfo.UserID, afterMessageId, numberOfRecords);
                var portalId = PortalController.GetEffectivePortalId(UserController.GetCurrentUserInfo().PortalID);
                messageBoxView.TotalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(UserInfo.UserID, portalId);
                messageBoxView.TotalConversations = InternalMessagingController.Instance.CountSentMessages(UserInfo.UserID, portalId);

                return Request.CreateResponse(HttpStatusCode.OK, messageBoxView);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage Archived(int afterMessageId, int numberOfRecords)
        {
            try
            {
                var messageBoxView = InternalMessagingController.Instance.GetArchivedMessages(UserInfo.UserID, afterMessageId, numberOfRecords);
                var portalId = PortalController.GetEffectivePortalId(UserController.GetCurrentUserInfo().PortalID);
                messageBoxView.TotalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(UserInfo.UserID, portalId);
                messageBoxView.TotalConversations = InternalMessagingController.Instance.CountArchivedMessages(UserInfo.UserID, portalId);

                return Request.CreateResponse(HttpStatusCode.OK, messageBoxView);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage Thread(int conversationId, int afterMessageId, int numberOfRecords)
        {
            try
            {
                var totalRecords = 0;
                var messageThreadsView = InternalMessagingController.Instance.GetMessageThread(conversationId, UserInfo.UserID, afterMessageId, numberOfRecords, ref totalRecords);
                var portalId = PortalController.GetEffectivePortalId(UserController.GetCurrentUserInfo().PortalID);
                messageThreadsView.TotalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(UserInfo.UserID, portalId);
                messageThreadsView.TotalThreads = InternalMessagingController.Instance.CountMessagesByConversation(conversationId);
                messageThreadsView.TotalArchivedThreads = InternalMessagingController.Instance.CountArchivedMessagesByConversation(conversationId);

                return Request.CreateResponse(HttpStatusCode.OK, messageThreadsView);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
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
				var message = ToExpandoObject(InternalMessagingController.Instance.GetMessage(messageId));
                var portalId = PortalController.GetEffectivePortalId(UserController.GetCurrentUserInfo().PortalID);

                var totalNewThreads = InternalMessagingController.Instance.CountUnreadMessages(UserInfo.UserID, portalId);
                var totalThreads = InternalMessagingController.Instance.CountMessagesByConversation(postData.ConversationId);
                var totalArchivedThreads = InternalMessagingController.Instance.CountArchivedMessagesByConversation(postData.ConversationId);

                return Request.CreateResponse(HttpStatusCode.OK, new { Conversation = message, TotalNewThreads = totalNewThreads, TotalThreads = totalThreads, TotalArchivedThreads = totalArchivedThreads });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MarkArchived(ConversationDTO postData)
        {
            try
            {
                InternalMessagingController.Instance.MarkArchived(postData.ConversationId, UserInfo.UserID);
                return Request.CreateResponse(HttpStatusCode.OK, new {Result = "success"});
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MarkUnArchived(ConversationDTO postData)
        {
            try
            {
                InternalMessagingController.Instance.MarkUnArchived(postData.ConversationId, UserInfo.UserID);
                return Request.CreateResponse(HttpStatusCode.OK, new {Result = "success"});
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MarkRead(ConversationDTO postData)
        {
            try
            {
                InternalMessagingController.Instance.MarkRead(postData.ConversationId, UserInfo.UserID);
                return Request.CreateResponse(HttpStatusCode.OK, new {Result = "success"});
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage MarkUnRead(ConversationDTO postData)
        {
            try
            {
                InternalMessagingController.Instance.MarkUnRead(postData.ConversationId, UserInfo.UserID);
                return Request.CreateResponse(HttpStatusCode.OK, new {Result = "success"});
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage Notifications(int afterNotificationId, int numberOfRecords)
        {
            try
            {
                var portalId = PortalController.GetEffectivePortalId(UserController.GetCurrentUserInfo().PortalID);
                var notificationsDomainModel = NotificationsController.Instance.GetNotifications(UserInfo.UserID, portalId, afterNotificationId, numberOfRecords);

                var notificationsViewModel = new NotificationsViewModel
                {
                    TotalNotifications = NotificationsController.Instance.CountNotifications(UserInfo.UserID, portalId),
                    Notifications = new List<NotificationViewModel>(notificationsDomainModel.Count)
                };

                foreach (var notification in notificationsDomainModel)
                {
                    var notificationViewModel = new NotificationViewModel
                    {
                        NotificationId = notification.NotificationID,
                        Subject = notification.Subject,
                        From = notification.From,
                        Body = notification.Body,
                        DisplayDate = Common.Utilities.DateUtils.CalculateDateForDisplay(notification.CreatedOnDate),
                        SenderAvatar = string.Format(Globals.UserProfilePicFormattedUrl(), notification.SenderUserID, 64, 64),
                        SenderProfileUrl = Globals.UserProfileURL(notification.SenderUserID),
                        Actions = new List<NotificationActionViewModel>()
                    };

                    var notificationType = NotificationsController.Instance.GetNotificationType(notification.NotificationTypeID);
                    var notificationTypeActions = NotificationsController.Instance.GetNotificationTypeActions(notification.NotificationTypeID);

                    foreach (var notificationTypeAction in notificationTypeActions)
                    {
                        var notificationActionViewModel = new NotificationActionViewModel
                        {
                            Name = LocalizeActionString(notificationTypeAction.NameResourceKey, notificationType.DesktopModuleId),
                            Description = LocalizeActionString(notificationTypeAction.DescriptionResourceKey, notificationType.DesktopModuleId),
                            Confirm = LocalizeActionString(notificationTypeAction.ConfirmResourceKey, notificationType.DesktopModuleId),
                            APICall = notificationTypeAction.APICall
                        };

                        notificationViewModel.Actions.Add(notificationActionViewModel);
                    }

                    if (notification.IncludeDismissAction)
                    {
                        notificationViewModel.Actions.Add(new NotificationActionViewModel
                        {
                            Name = Localization.GetString("Dismiss.Text"),
                            Description = Localization.GetString("DismissNotification.Text"),
                            Confirm = "",
                            APICall = "DesktopModules/InternalServices/API/NotificationsService/Dismiss"
                        });
                    }

                    notificationsViewModel.Notifications.Add(notificationViewModel);
                }

                return Request.CreateResponse(HttpStatusCode.OK, notificationsViewModel);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage CountNotifications()
        {
            try
            {
                var portalId = PortalController.GetEffectivePortalId(UserController.GetCurrentUserInfo().PortalID);
                int notifications = NotificationsController.Instance.CountNotifications(UserInfo.UserID, portalId);
                return Request.CreateResponse(HttpStatusCode.OK, notifications);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage CountUnreadMessages()
        {
            try
            {
                var portalId = PortalController.GetEffectivePortalId(UserController.GetCurrentUserInfo().PortalID);
                var unreadMessages = InternalMessagingController.Instance.CountUnreadMessages(UserInfo.UserID, portalId);
                return Request.CreateResponse(HttpStatusCode.OK, unreadMessages);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetTotals()
        {
            try
            {
                var portalId = PortalController.GetEffectivePortalId(UserController.GetCurrentUserInfo().PortalID);
                var totalsViewModel = new TotalsViewModel
                {
                    TotalUnreadMessages = InternalMessagingController.Instance.CountUnreadMessages(UserInfo.UserID, portalId),
                    TotalNotifications = NotificationsController.Instance.CountNotifications(UserInfo.UserID, portalId)
                };

                return Request.CreateResponse(HttpStatusCode.OK, totalsViewModel);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        #endregion

        #region DTO

        public class ConversationDTO
        {
            public int ConversationId { get; set; }
        }

        public class ReplyDTO : ConversationDTO
        {
            public string Body { get; set; }
            public IList<int> FileIds { get; set; }
        }

        #endregion

        #region Private Methods

        private string LocalizeActionString(string key, int desktopModuleId)
        {
            if (string.IsNullOrEmpty(key)) return "";

            string actionString;

            if (desktopModuleId > 0)
            {
                var desktopModule = DesktopModuleController.GetDesktopModule(desktopModuleId, PortalSettings.PortalId);

                var resourceFile = string.Format("~/DesktopModules/{0}/{1}/{2}",
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
			//base entity properties
			messageObj.CreatedByUserID = message.CreatedByUserID;
			messageObj.CreatedOnDate = message.CreatedOnDate;
			messageObj.LastModifiedByUserID = message.LastModifiedByUserID;
			messageObj.LastModifiedOnDate = message.LastModifiedOnDate;
			
			return messageObj;
		}

        #endregion
    }
}