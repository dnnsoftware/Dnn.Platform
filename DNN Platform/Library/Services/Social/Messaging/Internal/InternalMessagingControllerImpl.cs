// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Social.Messaging.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Security;
    using DotNetNuke.Services.Cache;
    using DotNetNuke.Services.Social.Messaging.Data;
    using DotNetNuke.Services.Social.Messaging.Exceptions;
    using DotNetNuke.Services.Social.Messaging.Internal.Views;

    using Localization = DotNetNuke.Services.Localization.Localization;

    /// <summary>The Controller class for social Messaging.</summary>
    internal class InternalMessagingControllerImpl : IInternalMessagingController
    {
        internal const int ConstMaxTo = 2000;
        internal const int ConstMaxSubject = 400;
        internal const int ConstDefaultPageIndex = 0;
        internal const int ConstDefaultPageSize = 10;
        internal const string ConstSortColumnDate = "CreatedOnDate";
        internal const string ConstSortColumnFrom = "From";
        internal const string ConstSortColumnSubject = "Subject";
        internal const bool ConstAscending = true;
        internal const double DefaultMessagingThrottlingInterval = 0.5; // default MessagingThrottlingInterval set to 30 seconds.

        private readonly IDataService _dataService;

        public InternalMessagingControllerImpl()
            : this(DataService.Instance)
        {
        }

        public InternalMessagingControllerImpl(IDataService dataService)
        {
            // Argument Contract
            Requires.NotNull("dataService", dataService);

            this._dataService = dataService;
        }

        public virtual void DeleteMessageRecipient(int messageId, int userId)
        {
            this._dataService.DeleteMessageRecipientByMessageAndUser(messageId, userId);
        }

        public virtual void DeleteUserFromConversation(int conversationId, int userId)
        {
            this._dataService.DeleteUserFromConversation(conversationId, userId);
        }

        public virtual Message GetMessage(int messageId)
        {
            return CBO.FillObject<Message>(this._dataService.GetMessage(messageId));
        }

        public virtual MessageRecipient GetMessageRecipient(int messageId, int userId)
        {
            return CBO.FillObject<MessageRecipient>(this._dataService.GetMessageRecipientByMessageAndUser(messageId, userId));
        }

        public virtual IList<MessageRecipient> GetMessageRecipients(int messageId)
        {
            return CBO.FillCollection<MessageRecipient>(this._dataService.GetMessageRecipientsByMessage(messageId));
        }

        public virtual void MarkArchived(int conversationId, int userId)
        {
            this._dataService.UpdateMessageArchivedStatus(conversationId, userId, true);
        }

        public virtual void MarkRead(int conversationId, int userId)
        {
            this._dataService.UpdateMessageReadStatus(conversationId, userId, true);
        }

        public virtual void MarkUnArchived(int conversationId, int userId)
        {
            this._dataService.UpdateMessageArchivedStatus(conversationId, userId, false);
        }

        public virtual void MarkUnRead(int conversationId, int userId)
        {
            this._dataService.UpdateMessageReadStatus(conversationId, userId, false);
        }

        public virtual int ReplyMessage(int conversationId, string body, IList<int> fileIDs)
        {
            return this.ReplyMessage(conversationId, body, fileIDs, UserController.Instance.GetCurrentUserInfo());
        }

        public virtual int ReplyMessage(int conversationId, string body, IList<int> fileIDs, UserInfo sender)
        {
            if (sender == null || sender.UserID <= 0)
            {
                throw new ArgumentException(Localization.GetString("MsgSenderRequiredError", Localization.ExceptionsResourceFile));
            }

            if (string.IsNullOrEmpty(body))
            {
                throw new ArgumentException(Localization.GetString("MsgBodyRequiredError", Localization.ExceptionsResourceFile));
            }

            // Cannot have attachments if it's not enabled
            if (fileIDs != null && !InternalMessagingController.Instance.AttachmentsAllowed(sender.PortalID))
            {
                throw new AttachmentsNotAllowed(Localization.GetString("MsgAttachmentsNotAllowed", Localization.ExceptionsResourceFile));
            }

            // Profanity Filter
            var profanityFilterSetting = this.GetPortalSetting("MessagingProfanityFilters", sender.PortalID, "NO");
            if (profanityFilterSetting.Equals("YES", StringComparison.InvariantCultureIgnoreCase))
            {
                body = this.InputFilter(body);
            }

            // call ReplyMessage
            var messageId = this._dataService.CreateMessageReply(conversationId, PortalController.GetEffectivePortalId(sender.PortalID), body, sender.UserID, sender.DisplayName, this.GetCurrentUserInfo().UserID);
            if (messageId == -1) // Parent message was not found or Recipient was not found in the message
            {
                throw new MessageOrRecipientNotFoundException(Localization.GetString("MsgMessageOrRecipientNotFound", Localization.ExceptionsResourceFile));
            }

            // associate attachments
            if (fileIDs != null)
            {
                foreach (var attachment in fileIDs.Select(fileId => new MessageAttachment { MessageAttachmentID = Null.NullInteger, FileID = fileId, MessageID = messageId }))
                {
                    this._dataService.SaveMessageAttachment(attachment, UserController.Instance.GetCurrentUserInfo().UserID);
                }
            }

            // Mark reply as read and dispatched by the sender
            var recipient = this.GetMessageRecipient(messageId, sender.UserID);

            this.MarkMessageAsDispatched(messageId, recipient.RecipientID);
            this.MarkRead(conversationId, sender.UserID);

            return messageId;
        }

        /// <summary>How long a user needs to wait before sending the next message.</summary>
        /// <returns>Time in seconds. Returns zero if user is Host, Admin or has never sent a message.</returns>
        /// <param name="sender">Sender's UserInfo.</param>
        /// <exception cref="System.ArgumentNullException"></exception>
        public virtual int WaitTimeForNextMessage(UserInfo sender)
        {
            Requires.NotNull("sender", sender);

            var waitTime = 0;

            // MessagingThrottlingInterval contains the number of MINUTES to wait before sending the next message
            var interval = this.GetPortalSettingAsDouble("MessagingThrottlingInterval", sender.PortalID, DefaultMessagingThrottlingInterval) * 60;
            if (interval > 0 && !this.IsAdminOrHost(sender))
            {
                var lastSentMessage = this.GetLastSentMessage(sender);
                if (lastSentMessage != null)
                {
                    waitTime = (int)(interval - this.GetDateTimeNow().Subtract(lastSentMessage.CreatedOnDate).TotalSeconds);
                }
            }

            return waitTime < 0 ? 0 : waitTime;
        }

        /// <summary>Last message sent by the User.</summary>
        /// <returns>Message. Null when no message was sent.</returns>
        /// <param name="sender">Sender's UserInfo.</param>
        public virtual Message GetLastSentMessage(UserInfo sender)
        {
            return CBO.FillObject<Message>(this._dataService.GetLastSentMessage(sender.UserID, PortalController.GetEffectivePortalId(sender.PortalID)));
        }

        /// <summary>Whether or not attachments are included with outgoing email.</summary>
        /// <param name="portalId">Portal Id.</param>
        /// <returns>True or False.</returns>
        public virtual bool IncludeAttachments(int portalId)
        {
            return this.GetPortalSetting("MessagingIncludeAttachments", portalId, "YES") == "YES";
        }

        /// <summary>Are attachments allowed.</summary>
        /// <param name="portalId">Portal Id.</param>
        /// <returns>True or False.</returns>
        public virtual bool AttachmentsAllowed(int portalId)
        {
            return this.GetPortalSetting("MessagingAllowAttachments", portalId, "YES") == "YES";
        }

        /// <summary>Maximum number of Recipients allowed.</summary>
        /// <returns>Count. Message to a Role is considered a single Recipient. Each User in the To list is counted as one User each.</returns>
        /// <param name="portalId">Portal Id.</param>
        public virtual int RecipientLimit(int portalId)
        {
            return this.GetPortalSettingAsInteger("MessagingRecipientLimit", portalId, 5);
        }

        /// <summary>Whether disable regular users to send message to user/group, default is false.</summary>
        /// <param name="portalId">Portal Id.</param>
        /// <returns></returns>
        public virtual bool DisablePrivateMessage(int portalId)
        {
            return this.GetPortalSetting("DisablePrivateMessage", portalId, "N") == "Y";
        }

        public virtual MessageBoxView GetArchivedMessages(int userId, int afterMessageId, int numberOfRecords)
        {
            var reader = this._dataService.GetArchiveBoxView(userId, PortalController.GetEffectivePortalId(this.GetCurrentUserInfo().PortalID), afterMessageId, numberOfRecords, ConstSortColumnDate, !ConstAscending);
            return new MessageBoxView { Conversations = CBO.FillCollection<MessageConversationView>(reader) };
        }

        public virtual MessageBoxView GetInbox(int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending)
        {
            return this.GetInbox(userId, afterMessageId, numberOfRecords, sortColumn, sortAscending, MessageReadStatus.Any, MessageArchivedStatus.UnArchived);
        }

        public virtual MessageBoxView GetInbox(int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending, MessageReadStatus readStatus, MessageArchivedStatus archivedStatus)
        {
            var reader = this._dataService.GetInBoxView(userId, PortalController.GetEffectivePortalId(this.GetCurrentUserInfo().PortalID), afterMessageId, numberOfRecords, sortColumn, sortAscending, readStatus, archivedStatus, MessageSentStatus.Received);
            return new MessageBoxView { Conversations = CBO.FillCollection<MessageConversationView>(reader) };
        }

        public virtual MessageThreadsView GetMessageThread(int conversationId, int userId, int afterMessageId, int numberOfRecords, ref int totalRecords)
        {
            return this.GetMessageThread(conversationId, userId, afterMessageId, numberOfRecords, ConstSortColumnDate, !ConstAscending, ref totalRecords);
        }

        public virtual MessageThreadsView GetMessageThread(int conversationId, int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending, ref int totalRecords)
        {
            var messageThreadsView = new MessageThreadsView();

            var dr = this._dataService.GetMessageThread(conversationId, userId, afterMessageId, numberOfRecords, sortColumn, sortAscending, ref totalRecords);

            try
            {
                while (dr.Read())
                {
                    var messageThreadView = new MessageThreadView { Conversation = new MessageConversationView() };
                    messageThreadView.Conversation.Fill(dr);

                    if (messageThreadView.Conversation.AttachmentCount > 0)
                    {
                        messageThreadView.Attachments = this._dataService.GetMessageAttachmentsByMessage(messageThreadView.Conversation.MessageID);
                    }

                    if (messageThreadsView.Conversations == null)
                    {
                        messageThreadsView.Conversations = new List<MessageThreadView>();
                    }

                    messageThreadsView.Conversations.Add(messageThreadView);
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            return messageThreadsView;
        }

        public virtual MessageBoxView GetRecentInbox(int userId)
        {
            return this.GetRecentInbox(userId, ConstDefaultPageIndex, ConstDefaultPageSize);
        }

        public virtual MessageBoxView GetRecentInbox(int userId, int afterMessageId, int numberOfRecords)
        {
            return this.GetInbox(userId, afterMessageId, numberOfRecords, ConstSortColumnDate, !ConstAscending);
        }

        public virtual MessageBoxView GetRecentSentbox(int userId)
        {
            return this.GetRecentSentbox(userId, ConstDefaultPageIndex, ConstDefaultPageSize);
        }

        public virtual MessageBoxView GetRecentSentbox(int userId, int afterMessageId, int numberOfRecords)
        {
            return this.GetSentbox(userId, afterMessageId, numberOfRecords, ConstSortColumnDate, !ConstAscending);
        }

        public virtual MessageBoxView GetSentbox(int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending)
        {
            return this.GetSentbox(userId, afterMessageId, numberOfRecords, sortColumn, sortAscending, MessageReadStatus.Any, MessageArchivedStatus.UnArchived);
        }

        public virtual MessageBoxView GetSentbox(int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending, MessageReadStatus readStatus, MessageArchivedStatus archivedStatus)
        {
            var reader = this._dataService.GetSentBoxView(userId, PortalController.GetEffectivePortalId(this.GetCurrentUserInfo().PortalID), afterMessageId, numberOfRecords, sortColumn, sortAscending);
            return new MessageBoxView { Conversations = CBO.FillCollection<MessageConversationView>(reader) };
        }

        public virtual int CheckReplyHasRecipients(int conversationId, int userId)
        {
            return userId <= 0 ? 0 :
                conversationId <= 0 ? 0 : this._dataService.CheckReplyHasRecipients(conversationId, userId);
        }

        public virtual int CountArchivedMessagesByConversation(int conversationId)
        {
            return conversationId <= 0 ? 0 : this._dataService.CountArchivedMessagesByConversation(conversationId);
        }

        public virtual int CountMessagesByConversation(int conversationId)
        {
            return conversationId <= 0 ? 0 : this._dataService.CountMessagesByConversation(conversationId);
        }

        public virtual int CountConversations(int userId, int portalId)
        {
            if (userId <= 0)
            {
                return 0;
            }

            var cacheKey = string.Format(DataCache.UserNotificationsConversationCountCacheKey, portalId, userId);
            var cache = CachingProvider.Instance();
            var cacheObject = cache.GetItem(cacheKey);
            if (cacheObject is int)
            {
                return (int)cacheObject;
            }

            var count = this._dataService.CountTotalConversations(userId, portalId);
            cache.Insert(cacheKey, count, (DNNCacheDependency)null,
                DateTime.Now.AddSeconds(DataCache.NotificationsCacheTimeInSec), System.Web.Caching.Cache.NoSlidingExpiration);
            return count;
        }

        public virtual int CountUnreadMessages(int userId, int portalId)
        {
            if (userId <= 0)
            {
                return 0;
            }

            var cacheKey = string.Format(DataCache.UserNewThreadsCountCacheKey, portalId, userId);
            var cache = CachingProvider.Instance();
            var cacheObject = cache.GetItem(cacheKey);
            if (cacheObject is int)
            {
                return (int)cacheObject;
            }

            var count = this._dataService.CountNewThreads(userId, portalId);
            cache.Insert(cacheKey, count, (DNNCacheDependency)null,
                DateTime.Now.AddSeconds(DataCache.NotificationsCacheTimeInSec), System.Web.Caching.Cache.NoSlidingExpiration);
            return count;
        }

        public virtual int CountSentMessages(int userId, int portalId)
        {
            return userId <= 0 ? 0 : this._dataService.CountSentMessages(userId, portalId);
        }

        public virtual int CountArchivedMessages(int userId, int portalId)
        {
            return userId <= 0 ? 0 : this._dataService.CountArchivedMessages(userId, portalId);
        }

        public virtual int CountSentConversations(int userId, int portalId)
        {
            return userId <= 0 ? 0 : this._dataService.CountSentConversations(userId, portalId);
        }

        public virtual int CountArchivedConversations(int userId, int portalId)
        {
            return userId <= 0 ? 0 : this._dataService.CountArchivedConversations(userId, portalId);
        }

        /// <summary>Gets the attachments.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <returns>A list of message attachments for the given message.</returns>
        public IEnumerable<MessageFileView> GetAttachments(int messageId)
        {
            return this._dataService.GetMessageAttachmentsByMessage(messageId);
        }

        public void ConvertLegacyMessages(int pageIndex, int pageSize)
        {
            this._dataService.ConvertLegacyMessages(pageIndex, pageSize);
        }

        public int CountLegacyMessages()
        {
            var totalRecords = 0;
            var dr = this._dataService.CountLegacyMessages();

            try
            {
                while (dr.Read())
                {
                    totalRecords = Convert.ToInt32(dr["TotalRecords"]);
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            return totalRecords;
        }

        public IList<MessageRecipient> GetNextMessagesForInstantDispatch(Guid schedulerInstance, int batchSize)
        {
            return CBO.FillCollection<MessageRecipient>(this._dataService.GetNextMessagesForInstantDispatch(schedulerInstance, batchSize));
        }

        public IList<MessageRecipient> GetNextMessagesForDigestDispatch(Frequency frequency, Guid schedulerInstance, int batchSize)
        {
            return CBO.FillCollection<MessageRecipient>(this._dataService.GetNextMessagesForDigestDispatch(Convert.ToInt32(frequency), schedulerInstance, batchSize));
        }

        public virtual void MarkMessageAsDispatched(int messageId, int recipientId)
        {
            this._dataService.MarkMessageAsDispatched(messageId, recipientId);
        }

        public virtual void MarkMessageAsSent(int messageId, int recipientId)
        {
            this._dataService.MarkMessageAsSent(messageId, recipientId);
        }

        internal virtual DateTime GetDateTimeNow()
        {
            return DateTime.UtcNow;
        }

        internal virtual UserInfo GetCurrentUserInfo()
        {
            return UserController.Instance.GetCurrentUserInfo();
        }

        internal virtual int GetPortalSettingAsInteger(string key, int portalId, int defaultValue)
        {
            return PortalController.GetPortalSettingAsInteger(key, portalId, defaultValue);
        }

        internal virtual double GetPortalSettingAsDouble(string key, int portalId, double defaultValue)
        {
            return PortalController.GetPortalSettingAsDouble(key, portalId, defaultValue);
        }

        internal virtual string GetPortalSetting(string settingName, int portalId, string defaultValue)
        {
            return PortalController.GetPortalSetting(settingName, portalId, defaultValue);
        }

        internal virtual bool IsAdminOrHost(UserInfo userInfo)
        {
            return userInfo.IsSuperUser || userInfo.IsInRole(PortalController.Instance.GetCurrentPortalSettings().AdministratorRoleName);
        }

        internal virtual string InputFilter(string input)
        {
            var ps = PortalSecurity.Instance;
            return ps.InputFilter(input, PortalSecurity.FilterFlag.NoProfanity);
        }
    }
}
