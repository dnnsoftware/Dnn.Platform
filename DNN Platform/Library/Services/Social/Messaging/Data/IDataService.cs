// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Social.Messaging.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;

    using DotNetNuke.Services.Social.Messaging.Internal.Views;

    public interface IDataService
    {
        int SaveMessage(Message message, int portalId, int createUpdateUserId);

        IDataReader GetMessage(int messageId);

        IDataReader GetMessagesBySender(int messageId, int portalId);

        IDataReader GetLastSentMessage(int userId, int portalId);

        void DeleteMessage(int messageId);

        void DeleteUserFromConversation(int conversationId, int userId);

        IDataReader GetInBoxView(int userId, int portalId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending, MessageReadStatus readStatus, MessageArchivedStatus archivedStatus, MessageSentStatus sentStatus);

        IDataReader GetSentBoxView(int userId, int portalId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending);

        IDataReader GetArchiveBoxView(int userId, int portalId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending);

        IDataReader GetMessageThread(int conversationId, int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending, ref int totalRecords);

        void UpdateMessageReadStatus(int conversationId, int userId, bool read);

        void UpdateMessageArchivedStatus(int conversationId, int userId, bool archived);

        int CreateMessageReply(int conversationId, int portalId, string body, int senderUserId, string from, int createUpdateUserId);

        int CountNewThreads(int userId, int portalId);

        int CountTotalConversations(int userId, int portalId);

        int CountMessagesByConversation(int conversationId);

        int CountArchivedMessagesByConversation(int conversationId);

        int CountSentMessages(int userId, int portalId);

        int CountArchivedMessages(int userId, int portalId);

        int CountSentConversations(int userId, int portalId);

        int CountArchivedConversations(int userId, int portalId);

        int CheckReplyHasRecipients(int conversationId, int userId);

        int SaveMessageRecipient(MessageRecipient messageRecipient, int createUpdateUserId);

        void CreateMessageRecipientsForRole(int messageId, string roleIds, int createUpdateUserId);

        IDataReader GetMessageRecipient(int messageRecipientId);

        IDataReader GetMessageRecipientsByUser(int userId);

        IDataReader GetMessageRecipientsByMessage(int messageId);

        IDataReader GetMessageRecipientByMessageAndUser(int messageId, int userId);

        void DeleteMessageRecipient(int messageRecipientId);

        void DeleteMessageRecipientByMessageAndUser(int messageId, int userId);

        int SaveMessageAttachment(MessageAttachment messageAttachment, int createUpdateUserId);

        IDataReader GetMessageAttachment(int messageAttachmentId);

        IList<MessageFileView> GetMessageAttachmentsByMessage(int messageId);

        void DeleteMessageAttachment(int messageAttachmentId);

        void ConvertLegacyMessages(int pageIndex, int pageSize);

        IDataReader CountLegacyMessages();

        IDataReader GetNextMessagesForInstantDispatch(Guid schedulerInstance, int batchSize);

        IDataReader GetNextMessagesForDigestDispatch(int frequecy, Guid schedulerInstance, int batchSize);

        void MarkMessageAsDispatched(int messageId, int recipientId);

        void MarkMessageAsSent(int messageId, int recipientId);

        IDataReader GetUserPreference(int portalId, int userId);

        void SetUserPreference(int portalId, int userId, int messagesEmailFrequency, int notificationsEmailFrequency);
    }
}
