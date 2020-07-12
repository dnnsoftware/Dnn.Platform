// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Messaging.Internal
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Social.Messaging.Internal.Views;

    /// <summary>Interface used for Message Controller behaviors.</summary>
    public interface IInternalMessagingController
    {
        int ReplyMessage(int conversationId, string body, IList<int> fileIDs);

        int ReplyMessage(int conversationId, string body, IList<int> fileIDs, UserInfo sender);

        Message GetMessage(int messageId);

        MessageRecipient GetMessageRecipient(int messageId, int userId);

        IList<MessageRecipient> GetMessageRecipients(int messageId);

        void DeleteMessageRecipient(int messageId, int userId);

        void DeleteUserFromConversation(int conversationId, int userId);

        void MarkRead(int conversationId, int userId);

        void MarkUnRead(int conversationId, int userId);

        void MarkArchived(int conversationId, int userId);

        void MarkUnArchived(int conversationId, int userId);

        /// <summary>How long a user needs to wait before user is allowed sending the next message.</summary>
        /// <returns>Time in seconds. Returns zero if user has never sent a message.</returns>
        /// <param name="sender">Sender's UserInfo.</param>
        int WaitTimeForNextMessage(UserInfo sender);

        /// <summary>Last message sent by the User.</summary>
        /// <returns>Message. Null when no message was sent.</returns>
        /// <param name="sender">Sender's UserInfo.</param>
        Message GetLastSentMessage(UserInfo sender);

        /// <summary>Maximum number of Recipients allowed.</summary>
        /// <returns>Count. Message to a Role is considered a single Recipient. Each User in the To list is counted as one User each.</returns>
        /// <param name="portalId">Portal Id.</param>
        int RecipientLimit(int portalId);

        /// <summary>Are attachments allowed.</summary>
        /// <returns>True or False.</returns>
        /// <param name="portalId">Portal Id.</param>
        bool AttachmentsAllowed(int portalId);

        /// <summary>Whether or not to includes the attachment in the email message.</summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns></returns>
        bool IncludeAttachments(int portalId);

        /// <summary>Whether disable regular users to send message to user/group, default is false.</summary>
        /// <param name="portalId">Portal Id.</param>
        /// <returns></returns>
        bool DisablePrivateMessage(int portalId);

        /// <summary>Converts the legacy messages.</summary>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        void ConvertLegacyMessages(int pageIndex, int pageSize);

        /// <summary>Counts the legacy messages.</summary>
        /// <returns>A count of messages.</returns>
        int CountLegacyMessages();

        /// <summary>Gets the inbox.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="afterMessageId">The after message identifier.</param>
        /// <param name="numberOfRecords">The number of records.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <param name="ascending">if set to <c>true</c> [ascending].</param>
        /// <param name="readStatus">The read status.</param>
        /// <param name="archivedStatus">The archived status.</param>
        /// <returns>A <see cref="MessageBoxView"/>.</returns>
        MessageBoxView GetInbox(int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool @ascending, MessageReadStatus readStatus, MessageArchivedStatus archivedStatus);

        /// <summary>Gets the inbox.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="afterMessageId">The after message identifier.</param>
        /// <param name="numberOfRecords">The number of records.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <param name="sortAscending">if set to <c>true</c> [sort ascending].</param>
        /// <returns>A <see cref="MessageBoxView"/>.</returns>
        MessageBoxView GetInbox(int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending);

        /// <summary>Gets the recent inbox.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A <see cref="MessageBoxView"/>.</returns>
        MessageBoxView GetRecentInbox(int userId);

        /// <summary>Gets the recent inbox.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="afterMessageId">The after message identifier.</param>
        /// <param name="numberOfRecords">The number of records.</param>
        /// <returns>A <see cref="MessageBoxView"/>.</returns>
        MessageBoxView GetRecentInbox(int userId, int afterMessageId, int numberOfRecords);

        /// <summary>Gets the sent box.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="afterMessageId">The after message identifier.</param>
        /// <param name="numberOfRecords">The number of records.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <param name="ascending">if set to <c>true</c> [ascending].</param>
        /// <param name="readStatus">The read status.</param>
        /// <param name="archivedStatus">The archived status.</param>
        /// <returns>A <see cref="MessageBoxView"/>.</returns>
        MessageBoxView GetSentbox(int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool ascending, MessageReadStatus readStatus, MessageArchivedStatus archivedStatus);

        /// <summary>Gets the sent box.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="afterMessageId">The after message identifier.</param>
        /// <param name="numberOfRecords">The number of records.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <param name="sortAscending">if set to <c>true</c> [sort ascending].</param>
        /// <returns>A <see cref="MessageBoxView"/>.</returns>
        MessageBoxView GetSentbox(int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending);

        /// <summary>Gets the recent sent box.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A <see cref="MessageBoxView"/>.</returns>
        MessageBoxView GetRecentSentbox(int userId);

        /// <summary>Gets the recent sent box.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="afterMessageId">The after message identifier.</param>
        /// <param name="numberOfRecords">The number of records.</param>
        /// <returns>A <see cref="MessageBoxView"/>.</returns>
        MessageBoxView GetRecentSentbox(int userId, int afterMessageId, int numberOfRecords);

        /// <summary>Gets the archived messages.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="afterMessageId">The after message identifier.</param>
        /// <param name="numberOfRecords">The number of records.</param>
        /// <returns>A <see cref="MessageBoxView"/>.</returns>
        MessageBoxView GetArchivedMessages(int userId, int afterMessageId, int numberOfRecords);

        /// <summary>Gets the message thread.</summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="afterMessageId">The after message identifier.</param>
        /// <param name="numberOfRecords">The number of records.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <param name="ascending">if set to <c>true</c> [ascending].</param>
        /// <param name="totalRecords">The total records.</param>
        /// <returns>A <see cref="MessageThreadsView"/>.</returns>
        MessageThreadsView GetMessageThread(int conversationId, int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool ascending, ref int totalRecords);

        /// <summary>Gets the message thread.</summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="afterMessageId">The after message identifier.</param>
        /// <param name="numberOfRecords">The number of records.</param>
        /// <param name="totalRecords">The total records.</param>
        /// <returns>A <see cref="MessageThreadsView"/>.</returns>
        MessageThreadsView GetMessageThread(int conversationId, int userId, int afterMessageId, int numberOfRecords, ref int totalRecords);

        /// <summary>Gets the attachments for the specified message.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <returns>A list of <see cref="MessageFileView"/>.</returns>
        IEnumerable<MessageFileView> GetAttachments(int messageId);

        IList<MessageRecipient> GetNextMessagesForInstantDispatch(Guid schedulerInstance, int batchSize);

        IList<MessageRecipient> GetNextMessagesForDigestDispatch(Frequency frequency, Guid schedulerInstance, int batchSize);

        void MarkMessageAsDispatched(int messageId, int recipientId);

        void MarkMessageAsSent(int messageId, int recipientId);

        int CheckReplyHasRecipients(int conversationId, int userId);

        int CountUnreadMessages(int userId, int portalId);

        int CountConversations(int userId, int portalId);

        int CountMessagesByConversation(int conversationId);

        int CountArchivedMessagesByConversation(int conversationId);

        int CountSentMessages(int userId, int portalId);

        int CountArchivedMessages(int userId, int portalId);

        int CountSentConversations(int userId, int portalId);

        int CountArchivedConversations(int userId, int portalId);
    }
}
