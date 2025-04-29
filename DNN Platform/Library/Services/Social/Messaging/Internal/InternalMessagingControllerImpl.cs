// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Social.Messaging.Internal;

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
    private const int DefaultPageIndex = 0;
    private const int DefaultPageSize = 10;
    private const string ConstSortColumnDate = "CreatedOnDate";
    private const bool ConstAscending = true;
    private const double DefaultMessagingThrottlingIntervalMinutes = 0.5;

    private readonly IDataService dataService;

    /// <summary>Initializes a new instance of the <see cref="InternalMessagingControllerImpl"/> class.</summary>
    public InternalMessagingControllerImpl()
        : this(DataService.Instance)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="InternalMessagingControllerImpl"/> class.</summary>
    /// <param name="dataService">The data service to use.</param>
    public InternalMessagingControllerImpl(IDataService dataService)
    {
        // Argument Contract
        Requires.NotNull("dataService", dataService);

        this.dataService = dataService;
    }

    /// <inheritdoc/>
    public virtual void DeleteMessageRecipient(int messageId, int userId)
    {
        this.dataService.DeleteMessageRecipientByMessageAndUser(messageId, userId);
    }

    /// <inheritdoc/>
    public virtual void DeleteUserFromConversation(int conversationId, int userId)
    {
        this.dataService.DeleteUserFromConversation(conversationId, userId);
    }

    /// <inheritdoc/>
    public virtual Message GetMessage(int messageId)
    {
        return CBO.FillObject<Message>(this.dataService.GetMessage(messageId));
    }

    /// <inheritdoc/>
    public virtual MessageRecipient GetMessageRecipient(int messageId, int userId)
    {
        return CBO.FillObject<MessageRecipient>(this.dataService.GetMessageRecipientByMessageAndUser(messageId, userId));
    }

    /// <inheritdoc/>
    public virtual IList<MessageRecipient> GetMessageRecipients(int messageId)
    {
        return CBO.FillCollection<MessageRecipient>(this.dataService.GetMessageRecipientsByMessage(messageId));
    }

    /// <inheritdoc/>
    public virtual void MarkArchived(int conversationId, int userId)
    {
        this.dataService.UpdateMessageArchivedStatus(conversationId, userId, true);
    }

    /// <inheritdoc/>
    public virtual void MarkRead(int conversationId, int userId)
    {
        this.dataService.UpdateMessageReadStatus(conversationId, userId, true);
    }

    /// <inheritdoc/>
    public virtual void MarkUnArchived(int conversationId, int userId)
    {
        this.dataService.UpdateMessageArchivedStatus(conversationId, userId, false);
    }

    /// <inheritdoc/>
    public virtual void MarkUnRead(int conversationId, int userId)
    {
        this.dataService.UpdateMessageReadStatus(conversationId, userId, false);
    }

    /// <inheritdoc/>
    public virtual int ReplyMessage(int conversationId, string body, IList<int> fileIDs)
    {
        return this.ReplyMessage(conversationId, body, fileIDs, UserController.Instance.GetCurrentUserInfo());
    }

    /// <inheritdoc/>
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
        var messageId = this.dataService.CreateMessageReply(conversationId, PortalController.GetEffectivePortalId(sender.PortalID), body, sender.UserID, sender.DisplayName, this.GetCurrentUserInfo().UserID);
        if (messageId == -1)
        {
            // Parent message was not found or Recipient was not found in the message
            throw new MessageOrRecipientNotFoundException(Localization.GetString("MsgMessageOrRecipientNotFound", Localization.ExceptionsResourceFile));
        }

        // associate attachments
        if (fileIDs != null)
        {
            foreach (var attachment in fileIDs.Select(fileId => new MessageAttachment { MessageAttachmentID = Null.NullInteger, FileID = fileId, MessageID = messageId }))
            {
                this.dataService.SaveMessageAttachment(attachment, UserController.Instance.GetCurrentUserInfo().UserID);
            }
        }

        // Mark reply as read and dispatched by the sender
        var recipient = this.GetMessageRecipient(messageId, sender.UserID);

        this.MarkMessageAsDispatched(messageId, recipient.RecipientID);
        this.MarkRead(conversationId, sender.UserID);

        return messageId;
    }

    /// <inheritdoc />
    public virtual int WaitTimeForNextMessage(UserInfo sender)
    {
        Requires.NotNull("sender", sender);

        var waitTime = 0;

        // MessagingThrottlingInterval contains the number of MINUTES to wait before sending the next message
        var interval = this.GetPortalSettingAsDouble("MessagingThrottlingInterval", sender.PortalID, DefaultMessagingThrottlingIntervalMinutes) * 60;
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

    /// <inheritdoc />
    public virtual Message GetLastSentMessage(UserInfo sender)
    {
        return CBO.FillObject<Message>(this.dataService.GetLastSentMessage(sender.UserID, PortalController.GetEffectivePortalId(sender.PortalID)));
    }

    /// <inheritdoc />
    public virtual bool IncludeAttachments(int portalId)
    {
        return this.GetPortalSetting("MessagingIncludeAttachments", portalId, "YES") == "YES";
    }

    /// <inheritdoc />
    public virtual bool AttachmentsAllowed(int portalId)
    {
        return this.GetPortalSetting("MessagingAllowAttachments", portalId, "YES") == "YES";
    }

    /// <inheritdoc />
    public virtual int RecipientLimit(int portalId)
    {
        return this.GetPortalSettingAsInteger("MessagingRecipientLimit", portalId, 5);
    }

    /// <inheritdoc />
    public virtual bool DisablePrivateMessage(int portalId)
    {
        return this.GetPortalSetting("DisablePrivateMessage", portalId, "N") == "Y";
    }

    /// <inheritdoc/>
    public virtual MessageBoxView GetArchivedMessages(int userId, int afterMessageId, int numberOfRecords)
    {
        var reader = this.dataService.GetArchiveBoxView(userId, PortalController.GetEffectivePortalId(this.GetCurrentUserInfo().PortalID), afterMessageId, numberOfRecords, ConstSortColumnDate, !ConstAscending);
        return new MessageBoxView { Conversations = CBO.FillCollection<MessageConversationView>(reader) };
    }

    /// <inheritdoc/>
    public virtual MessageBoxView GetInbox(int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending)
    {
        return this.GetInbox(userId, afterMessageId, numberOfRecords, sortColumn, sortAscending, MessageReadStatus.Any, MessageArchivedStatus.UnArchived);
    }

    /// <inheritdoc/>
    public virtual MessageBoxView GetInbox(int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending, MessageReadStatus readStatus, MessageArchivedStatus archivedStatus)
    {
        var reader = this.dataService.GetInBoxView(userId, PortalController.GetEffectivePortalId(this.GetCurrentUserInfo().PortalID), afterMessageId, numberOfRecords, sortColumn, sortAscending, readStatus, archivedStatus, MessageSentStatus.Received);
        return new MessageBoxView { Conversations = CBO.FillCollection<MessageConversationView>(reader) };
    }

    /// <inheritdoc/>
    public virtual MessageThreadsView GetMessageThread(int conversationId, int userId, int afterMessageId, int numberOfRecords, ref int totalRecords)
    {
        return this.GetMessageThread(conversationId, userId, afterMessageId, numberOfRecords, ConstSortColumnDate, !ConstAscending, ref totalRecords);
    }

    /// <inheritdoc/>
    public virtual MessageThreadsView GetMessageThread(int conversationId, int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending, ref int totalRecords)
    {
        var messageThreadsView = new MessageThreadsView();

        var dr = this.dataService.GetMessageThread(conversationId, userId, afterMessageId, numberOfRecords, sortColumn, sortAscending, ref totalRecords);

        try
        {
            while (dr.Read())
            {
                var messageThreadView = new MessageThreadView { Conversation = new MessageConversationView() };
                messageThreadView.Conversation.Fill(dr);

                if (messageThreadView.Conversation.AttachmentCount > 0)
                {
                    messageThreadView.Attachments = this.dataService.GetMessageAttachmentsByMessage(messageThreadView.Conversation.MessageID);
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

    /// <inheritdoc/>
    public virtual MessageBoxView GetRecentInbox(int userId)
    {
        return this.GetRecentInbox(userId, DefaultPageIndex, DefaultPageSize);
    }

    /// <inheritdoc/>
    public virtual MessageBoxView GetRecentInbox(int userId, int afterMessageId, int numberOfRecords)
    {
        return this.GetInbox(userId, afterMessageId, numberOfRecords, ConstSortColumnDate, !ConstAscending);
    }

    /// <inheritdoc/>
    public virtual MessageBoxView GetRecentSentbox(int userId)
    {
        return this.GetRecentSentbox(userId, DefaultPageIndex, DefaultPageSize);
    }

    /// <inheritdoc/>
    public virtual MessageBoxView GetRecentSentbox(int userId, int afterMessageId, int numberOfRecords)
    {
        return this.GetSentbox(userId, afterMessageId, numberOfRecords, ConstSortColumnDate, !ConstAscending);
    }

    /// <inheritdoc/>
    public virtual MessageBoxView GetSentbox(int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending)
    {
        return this.GetSentbox(userId, afterMessageId, numberOfRecords, sortColumn, sortAscending, MessageReadStatus.Any, MessageArchivedStatus.UnArchived);
    }

    /// <inheritdoc/>
    public virtual MessageBoxView GetSentbox(int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending, MessageReadStatus readStatus, MessageArchivedStatus archivedStatus)
    {
        var reader = this.dataService.GetSentBoxView(userId, PortalController.GetEffectivePortalId(this.GetCurrentUserInfo().PortalID), afterMessageId, numberOfRecords, sortColumn, sortAscending);
        return new MessageBoxView { Conversations = CBO.FillCollection<MessageConversationView>(reader) };
    }

    /// <inheritdoc/>
    public virtual int CheckReplyHasRecipients(int conversationId, int userId)
    {
        return userId <= 0 ? 0 :
            conversationId <= 0 ? 0 : this.dataService.CheckReplyHasRecipients(conversationId, userId);
    }

    /// <inheritdoc/>
    public virtual int CountArchivedMessagesByConversation(int conversationId)
    {
        return conversationId <= 0 ? 0 : this.dataService.CountArchivedMessagesByConversation(conversationId);
    }

    /// <inheritdoc/>
    public virtual int CountMessagesByConversation(int conversationId)
    {
        return conversationId <= 0 ? 0 : this.dataService.CountMessagesByConversation(conversationId);
    }

    /// <inheritdoc/>
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

        var count = this.dataService.CountTotalConversations(userId, portalId);
        cache.Insert(
            cacheKey,
            count,
            (DNNCacheDependency)null,
            DateTime.Now.AddSeconds(DataCache.NotificationsCacheTimeInSec),
            System.Web.Caching.Cache.NoSlidingExpiration);
        return count;
    }

    /// <inheritdoc/>
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

        var count = this.dataService.CountNewThreads(userId, portalId);
        cache.Insert(
            cacheKey,
            count,
            (DNNCacheDependency)null,
            DateTime.Now.AddSeconds(DataCache.NotificationsCacheTimeInSec),
            System.Web.Caching.Cache.NoSlidingExpiration);
        return count;
    }

    /// <inheritdoc/>
    public virtual int CountSentMessages(int userId, int portalId)
    {
        return userId <= 0 ? 0 : this.dataService.CountSentMessages(userId, portalId);
    }

    /// <inheritdoc/>
    public virtual int CountArchivedMessages(int userId, int portalId)
    {
        return userId <= 0 ? 0 : this.dataService.CountArchivedMessages(userId, portalId);
    }

    /// <inheritdoc/>
    public virtual int CountSentConversations(int userId, int portalId)
    {
        return userId <= 0 ? 0 : this.dataService.CountSentConversations(userId, portalId);
    }

    /// <inheritdoc/>
    public virtual int CountArchivedConversations(int userId, int portalId)
    {
        return userId <= 0 ? 0 : this.dataService.CountArchivedConversations(userId, portalId);
    }

    /// <inheritdoc />
    public IEnumerable<MessageFileView> GetAttachments(int messageId)
    {
        return this.dataService.GetMessageAttachmentsByMessage(messageId);
    }

    /// <inheritdoc/>
    public void ConvertLegacyMessages(int pageIndex, int pageSize)
    {
        this.dataService.ConvertLegacyMessages(pageIndex, pageSize);
    }

    /// <inheritdoc/>
    public int CountLegacyMessages()
    {
        var totalRecords = 0;
        var dr = this.dataService.CountLegacyMessages();

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

    /// <inheritdoc/>
    public IList<MessageRecipient> GetNextMessagesForInstantDispatch(Guid schedulerInstance, int batchSize)
    {
        return CBO.FillCollection<MessageRecipient>(this.dataService.GetNextMessagesForInstantDispatch(schedulerInstance, batchSize));
    }

    /// <inheritdoc/>
    public IList<MessageRecipient> GetNextMessagesForDigestDispatch(Frequency frequency, Guid schedulerInstance, int batchSize)
    {
        return CBO.FillCollection<MessageRecipient>(this.dataService.GetNextMessagesForDigestDispatch(Convert.ToInt32(frequency), schedulerInstance, batchSize));
    }

    /// <inheritdoc/>
    public virtual void MarkMessageAsDispatched(int messageId, int recipientId)
    {
        this.dataService.MarkMessageAsDispatched(messageId, recipientId);
    }

    /// <inheritdoc/>
    public virtual void MarkMessageAsSent(int messageId, int recipientId)
    {
        this.dataService.MarkMessageAsSent(messageId, recipientId);
    }

    /// <summary>
    /// Gets the date time now (virtual so it can be mocked in test).
    /// </summary>
    /// <returns>The current <see cref="DateTime"/>.</returns>
    internal virtual DateTime GetDateTimeNow()
    {
        return DateTime.UtcNow;
    }

    /// <summary>
    /// Gets the current user information (virtual so it can be mocked in tests).
    /// </summary>
    /// <returns><see cref="UserInfo"/>.</returns>
    internal virtual UserInfo GetCurrentUserInfo()
    {
        return UserController.Instance.GetCurrentUserInfo();
    }

    /// <summary>
    /// Gets a portal setting (virtual so it can be mocked in tests).
    /// </summary>
    /// <param name="key">The key of the setting.</param>
    /// <param name="portalId">The portal identifier.</param>
    /// <param name="defaultValue">The default value in case the setting does not exist.</param>
    /// <returns>The value of the setting as an integer.</returns>
    internal virtual int GetPortalSettingAsInteger(string key, int portalId, int defaultValue)
    {
        return PortalController.GetPortalSettingAsInteger(key, portalId, defaultValue);
    }

    /// <summary>
    /// Gets a portal setting (virtual so it can be mocked in tests).
    /// </summary>
    /// <param name="key">The key of the setting.</param>
    /// <param name="portalId">The portal identifier.</param>
    /// <param name="defaultValue">The default value if the setting does not exist.</param>
    /// <returns>The setting value as a double.</returns>
    internal virtual double GetPortalSettingAsDouble(string key, int portalId, double defaultValue)
    {
        return PortalController.GetPortalSettingAsDouble(key, portalId, defaultValue);
    }

    /// <summary>
    /// Gets a portal setting (virtual so it can be mocked in tests).
    /// </summary>
    /// <param name="settingName">Name of the setting.</param>
    /// <param name="portalId">The portal identifier.</param>
    /// <param name="defaultValue">The default value if the setting does not exist.</param>
    /// <returns>The setting value as a string.</returns>
    internal virtual string GetPortalSetting(string settingName, int portalId, string defaultValue)
    {
        return PortalController.GetPortalSetting(settingName, portalId, defaultValue);
    }

    /// <summary>
    /// Determines whether the user is an admin or a host.
    /// </summary>
    /// <param name="userInfo">The user information.</param>
    /// <returns>A value indicating whether the user is an admin or a host.</returns>
    internal virtual bool IsAdminOrHost(UserInfo userInfo)
    {
        return userInfo.IsSuperUser || userInfo.IsInRole(PortalController.Instance.GetCurrentSettings().AdministratorRoleName);
    }

    /// <summary>
    /// Filters user input.
    /// </summary>
    /// <param name="input">The input to filter.</param>
    /// <returns>The filtered string.</returns>
    internal virtual string InputFilter(string input)
    {
        var ps = PortalSecurity.Instance;
        return ps.InputFilter(input, PortalSecurity.FilterFlag.NoProfanity);
    }
}
