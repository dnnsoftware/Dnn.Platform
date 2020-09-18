// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Social.Messaging.Data
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Data;
    using DotNetNuke.Services.FileSystem;
    using DotNetNuke.Services.Social.Messaging.Internal.Views;

    /// <summary>Data Service component for core messaging functions.</summary>
    internal class DataService : ComponentBase<IDataService, DataService>, IDataService
    {
        /// <summary>The provider instance.</summary>
        private readonly DataProvider _provider = DataProvider.Instance();

        /// <summary>Saves the message.</summary>
        /// <param name="message">The message.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="createUpdateUserId">The create update user identifier.</param>
        /// <returns>The id of the newly created message.</returns>
        public int SaveMessage(Message message, int portalId, int createUpdateUserId)
        {
            // need to fix groupmail
            return this._provider.ExecuteScalar<int>("CoreMessaging_SaveMessage", message.MessageID, portalId, message.To, message.From, message.Subject, message.Body, message.ConversationId, message.ReplyAllAllowed, message.SenderUserID, createUpdateUserId);
        }

        /// <summary>Gets the message.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <returns>A <see cref="IDataReader"/> containing the message data.</returns>
        public IDataReader GetMessage(int messageId)
        {
            return this._provider.ExecuteReader("CoreMessaging_GetMessage", messageId);
        }

        /// <summary>Gets the last sent message.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>A <see cref="IDataReader"/> containing the last sent message data.</returns>
        public IDataReader GetLastSentMessage(int userId, int portalId)
        {
            return this._provider.ExecuteReader("CoreMessaging_GetLastSentMessage", userId, portalId);
        }

        /// <summary>Gets the messages by sender.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>A <see cref="IDataReader"/> containing the messages for a given portal Id.</returns>
        public IDataReader GetMessagesBySender(int messageId, int portalId)
        {
            return this._provider.ExecuteReader("CoreMessaging_GetMessagesBySender", messageId, portalId);
        }

        /// <summary>Deletes the message.</summary>
        /// <param name="messageId">The message identifier.</param>
        public void DeleteMessage(int messageId)
        {
            this._provider.ExecuteNonQuery("CoreMessaging_DeleteMessage", messageId);
        }

        /// <summary>Deletes the user from conversation.</summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public void DeleteUserFromConversation(int conversationId, int userId)
        {
            this._provider.ExecuteNonQuery("CoreMessaging_DeleteUserFromConversation", conversationId, userId);
        }

        /// <summary>Creates a message reply.</summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="body">The body.</param>
        /// <param name="senderUserId">The sender user identifier.</param>
        /// <param name="fromName">From name.</param>
        /// <param name="createUpdateUserId">The create update user identifier.</param>
        /// <returns>The id of the reply created.</returns>
        public int CreateMessageReply(int conversationId, int portalId, string body, int senderUserId, string fromName, int createUpdateUserId)
        {
            return this._provider.ExecuteScalar<int>("CoreMessaging_CreateMessageReply", conversationId, portalId, body, senderUserId, fromName, createUpdateUserId);
        }

        /// <summary>
        /// check if an attempt to reply to an existing mail has valid users.
        /// </summary>
        /// <param name="conversationId">the particular reply within the message.</param>
        /// <param name="userId">the user sending the message - as they are a recipient they must be excluded from the count.</param>
        /// <returns>The count of recipients.</returns>
        public int CheckReplyHasRecipients(int conversationId, int userId)
        {
            return this._provider.ExecuteScalar<int>("CoreMessaging_CheckReplyHasRecipients", conversationId, userId);
        }

        /// <summary>Gets the in box view.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="afterMessageId">The after message identifier.</param>
        /// <param name="numberOfRecords">The number of records.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <param name="sortAscending">if set to <c>true</c> [sort ascending].</param>
        /// <param name="readStatus">The read status.</param>
        /// <param name="archivedStatus">The archived status.</param>
        /// <param name="sentStatus">The sent status.</param>
        /// <returns>A <see cref="IDataReader"/> containing the inbox data.</returns>
        public IDataReader GetInBoxView(int userId, int portalId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending, MessageReadStatus readStatus, MessageArchivedStatus archivedStatus, MessageSentStatus sentStatus)
        {
            object read = null;
            object archived = null;
            object sent = null;

            switch (readStatus)
            {
                case MessageReadStatus.Read:
                    read = true;
                    break;
                case MessageReadStatus.UnRead:
                    read = false;
                    break;
            }

            switch (archivedStatus)
            {
                case MessageArchivedStatus.Archived:
                    archived = true;
                    break;
                case MessageArchivedStatus.UnArchived:
                    archived = false;
                    break;
            }

            switch (sentStatus)
            {
                case MessageSentStatus.Received:
                    sent = false;
                    break;
                case MessageSentStatus.Sent:
                    sent = true;
                    break;
            }

            return this._provider.ExecuteReader("CoreMessaging_GetMessageConversations", userId, portalId, afterMessageId, numberOfRecords, sortColumn, sortAscending, read, archived, sent);
        }

        /// <summary>Gets the sent box view.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="afterMessageId">The after message identifier.</param>
        /// <param name="numberOfRecords">The number of records.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <param name="sortAscending">if set to <c>true</c> [sort ascending].</param>
        /// <returns>A <see cref="IDataReader"/> containing the sent message box data.</returns>
        public IDataReader GetSentBoxView(int userId, int portalId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending)
        {
            return this._provider.ExecuteReader("CoreMessaging_GetSentBox", userId, portalId, afterMessageId, numberOfRecords, sortColumn, sortAscending);
        }

        /// <summary>Gets the archive box view.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="afterMessageId">The after message identifier.</param>
        /// <param name="numberOfRecords">The number of records.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <param name="sortAscending">if set to <c>true</c> [sort ascending].</param>
        /// <returns>A <see cref="IDataReader"/> containing the archived messages data.</returns>
        public IDataReader GetArchiveBoxView(int userId, int portalId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending)
        {
            return this._provider.ExecuteReader("CoreMessaging_GetArchiveBox", userId, portalId, afterMessageId, numberOfRecords, sortColumn, sortAscending);
        }

        /// <summary>Gets the message thread.</summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="afterMessageId">The after message identifier.</param>
        /// <param name="numberOfRecords">The number of records.</param>
        /// <param name="sortColumn">The sort column.</param>
        /// <param name="sortAscending">if set to <c>true</c> [sort ascending].</param>
        /// <param name="totalRecords">The total records.</param>
        /// <returns>A <see cref="IDataReader"/> containing the message thread data.</returns>
        public IDataReader GetMessageThread(int conversationId, int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool @sortAscending, ref int totalRecords)
        {
            return this._provider.ExecuteReader("CoreMessaging_GetMessageThread", conversationId, userId, afterMessageId, numberOfRecords, sortColumn, sortAscending);
        }

        /// <summary>Updates the message read status for a given conversation.</summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="read">if read is set to <c>true</c> otherwise false.</param>
        public void UpdateMessageReadStatus(int conversationId, int userId, bool read)
        {
            this._provider.ExecuteNonQuery("CoreMessaging_UpdateMessageReadStatus", conversationId, userId, read);
        }

        /// <summary>Updates the message archived status.</summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="archived">if set to <c>true</c> archived.</param>
        public void UpdateMessageArchivedStatus(int conversationId, int userId, bool archived)
        {
            this._provider.ExecuteNonQuery("CoreMessaging_UpdateMessageArchivedStatus", conversationId, userId, archived);
        }

        /// <summary>Counts the new threads.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The count of new threads for a given user.</returns>
        public int CountNewThreads(int userId, int portalId)
        {
            return this._provider.ExecuteScalar<int>("CoreMessaging_CountNewThreads", userId, portalId);
        }

        /// <summary>Counts the total conversations.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The count of new conversations for a given user.</returns>
        public int CountTotalConversations(int userId, int portalId)
        {
            return this._provider.ExecuteScalar<int>("CoreMessaging_CountTotalConversations", userId, portalId);
        }

        /// <summary>Counts the messages by conversation.</summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <returns>The count of new messages for a given conversation.</returns>
        public int CountMessagesByConversation(int conversationId)
        {
            return this._provider.ExecuteScalar<int>("CoreMessaging_CountMessagesByConversation", conversationId);
        }

        /// <summary>Counts the archived messages by conversation.</summary>
        /// <param name="conversationId">The conversation identifier.</param>
        /// <returns>The count of archived messages for a given conversation.</returns>
        public int CountArchivedMessagesByConversation(int conversationId)
        {
            return this._provider.ExecuteScalar<int>("CoreMessaging_CountArchivedMessagesByConversation", conversationId);
        }

        /// <summary>Counts the sent messages.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The count of messages sent for a given user.</returns>
        public int CountSentMessages(int userId, int portalId)
        {
            return this._provider.ExecuteScalar<int>("CoreMessaging_CountSentMessages", userId, portalId);
        }

        /// <summary>Counts the archived messages.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The count of archived messages for a given user.</returns>
        public int CountArchivedMessages(int userId, int portalId)
        {
            return this._provider.ExecuteScalar<int>("CoreMessaging_CountArchivedMessages", userId, portalId);
        }

        /// <summary>Counts the sent conversations.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The count of sent conversations for a given user.</returns>
        public int CountSentConversations(int userId, int portalId)
        {
            return this._provider.ExecuteScalar<int>("CoreMessaging_CountSentConversations", userId, portalId);
        }

        /// <summary>Counts the archived conversations.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="portalId">The portal identifier.</param>
        /// <returns>The count of archived conversations for a given user.</returns>
        public int CountArchivedConversations(int userId, int portalId)
        {
            return this._provider.ExecuteScalar<int>("CoreMessaging_CountArchivedConversations", userId, portalId);
        }

        /// <summary>Saves the message recipient.</summary>
        /// <param name="messageRecipient">The message recipient.</param>
        /// <param name="createUpdateUserId">The create update user identifier.</param>
        /// <returns>The new message recipient Id.</returns>
        public int SaveMessageRecipient(MessageRecipient messageRecipient, int createUpdateUserId)
        {
            return this._provider.ExecuteScalar<int>("CoreMessaging_SaveMessageRecipient", messageRecipient.RecipientID, messageRecipient.MessageID, messageRecipient.UserID, messageRecipient.Read, messageRecipient.Archived, createUpdateUserId);
        }

        /// <summary>Creates the message recipients for role.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="roleIds">The role ids.</param>
        /// <param name="createUpdateUserId">The create update user identifier.</param>
        public void CreateMessageRecipientsForRole(int messageId, string roleIds, int createUpdateUserId)
        {
            this._provider.ExecuteNonQuery("CoreMessaging_CreateMessageRecipientsForRole", messageId, roleIds, createUpdateUserId);
        }

        /// <summary>Gets the message recipient.</summary>
        /// <param name="messageRecipientId">The message recipient identifier.</param>
        /// <returns>A <see cref="IDataReader" /> containing the message recipient data.</returns>
        public IDataReader GetMessageRecipient(int messageRecipientId)
        {
            return this._provider.ExecuteReader("CoreMessaging_GetMessageRecipient", messageRecipientId);
        }

        /// <summary>Gets the message recipients by user.</summary>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A <see cref="IDataReader" /> containing the message recipient data.</returns>
        public IDataReader GetMessageRecipientsByUser(int userId)
        {
            return this._provider.ExecuteReader("CoreMessaging_GetMessageRecipientsByUser", userId);
        }

        /// <summary>Gets the message recipients by message.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <returns>A <see cref="IDataReader" /> containing the message recipient data.</returns>
        public IDataReader GetMessageRecipientsByMessage(int messageId)
        {
            return this._provider.ExecuteReader("CoreMessaging_GetMessageRecipientsByMessage", messageId);
        }

        /// <summary>Gets the message recipient by message and user.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A <see cref="IDataReader" /> containing the message recipient data.</returns>
        public IDataReader GetMessageRecipientByMessageAndUser(int messageId, int userId)
        {
            return this._provider.ExecuteReader("CoreMessaging_GetMessageRecipientsByMessageAndUser", messageId, userId);
        }

        /// <summary>Deletes the message recipient.</summary>
        /// <param name="messageRecipientId">The message recipient identifier.</param>
        public void DeleteMessageRecipient(int messageRecipientId)
        {
            this._provider.ExecuteNonQuery("CoreMessaging_DeleteMessageRecipient", messageRecipientId);
        }

        /// <summary>Deletes the message recipient by message and user.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="userId">The user identifier.</param>
        public void DeleteMessageRecipientByMessageAndUser(int messageId, int userId)
        {
            this._provider.ExecuteNonQuery("CoreMessaging_DeleteMessageRecipientByMessageAndUser", messageId, userId);
        }

        /// <summary>Saves the message attachment.</summary>
        /// <param name="messageAttachment">The message attachment.</param>
        /// <param name="createUpdateUserId">The create update user identifier.</param>
        /// <returns>The message attachment Id.</returns>
        public int SaveMessageAttachment(MessageAttachment messageAttachment, int createUpdateUserId)
        {
            return this._provider.ExecuteScalar<int>("CoreMessaging_SaveMessageAttachment", messageAttachment.MessageAttachmentID, messageAttachment.MessageID, messageAttachment.FileID, createUpdateUserId);
        }

        /// <summary>Gets the message attachment.</summary>
        /// <param name="messageAttachmentId">The message attachment identifier.</param>
        /// <returns>A <see cref="IDataReader" /> containing the message attachment data.</returns>
        public IDataReader GetMessageAttachment(int messageAttachmentId)
        {
            return this._provider.ExecuteReader("CoreMessaging_GetMessageAttachment", messageAttachmentId);
        }

        /// <summary>Gets the message attachments by message id.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <returns>A list of <see cref="MessageFileView"/>.</returns>
        public IList<MessageFileView> GetMessageAttachmentsByMessage(int messageId)
        {
            var attachments = new List<MessageFileView>();
            var dr = this._provider.ExecuteReader("CoreMessaging_GetMessageAttachmentsByMessage", messageId);

            try
            {
                while (dr.Read())
                {
                    var fileId = Convert.ToInt32(dr["FileID"]);
                    var file = FileManager.Instance.GetFile(fileId);

                    if (file == null)
                    {
                        continue;
                    }

                    var attachment = new MessageFileView
                    {
                        Name = file.FileName,
                        Size = file.Size.ToString(CultureInfo.InvariantCulture),
                        Url = FileManager.Instance.GetUrl(file),
                        FileId = fileId,
                    };

                    attachments.Add(attachment);
                }
            }
            finally
            {
                CBO.CloseDataReader(dr, true);
            }

            return attachments;
        }

        /// <summary>Deletes the message attachment.</summary>
        /// <param name="messageAttachmentId">The message attachment identifier.</param>
        public void DeleteMessageAttachment(int messageAttachmentId)
        {
            this._provider.ExecuteNonQuery("CoreMessaging_DeleteMessageAttachment", messageAttachmentId);
        }

        /// <summary>Converts the legacy messages.</summary>
        /// <param name="pageIndex">Index of the page.</param>
        /// <param name="pageSize">Size of the page.</param>
        public void ConvertLegacyMessages(int pageIndex, int pageSize)
        {
            this._provider.ExecuteNonQuery("CoreMessaging_ConvertLegacyMessages", pageIndex, pageSize);
        }

        /// <summary>Counts the legacy messages.</summary>
        /// <returns>A <see cref="IDataReader" /> containing the messages data.</returns>
        public IDataReader CountLegacyMessages()
        {
            return this._provider.ExecuteReader("CoreMessaging_CountLegacyMessages");
        }

        /// <summary>Gets the next messages for instant dispatch.</summary>
        /// <param name="schedulerInstance">The scheduler instance.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <returns>A <see cref="IDataReader" /> containing the messages data.</returns>
        public IDataReader GetNextMessagesForInstantDispatch(Guid schedulerInstance, int batchSize)
        {
            return this._provider.ExecuteReader("CoreMessaging_GetNextMessagesForInstantDispatch", schedulerInstance, batchSize);
        }

        /// <summary>Gets the next messages for digest dispatch.</summary>
        /// <param name="frequecy">The frequency.</param>
        /// <param name="schedulerInstance">The scheduler instance.</param>
        /// <param name="batchSize">Size of the batch.</param>
        /// <returns>A <see cref="IDataReader" /> containing the messages data.</returns>
        public IDataReader GetNextMessagesForDigestDispatch(int frequecy, Guid schedulerInstance, int batchSize)
        {
            return this._provider.ExecuteReader("CoreMessaging_GetNextMessagesForDigestDispatch", frequecy, schedulerInstance, batchSize);
        }

        /// <summary>Marks the message as dispatched.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="recipientId">The recipient identifier.</param>
        public void MarkMessageAsDispatched(int messageId, int recipientId)
        {
            this._provider.ExecuteNonQuery("CoreMessaging_MarkMessageAsDispatched", messageId, recipientId);
        }

        /// <summary>Marks the message as sent.</summary>
        /// <param name="messageId">The message identifier.</param>
        /// <param name="recipientId">The recipient identifier.</param>
        public void MarkMessageAsSent(int messageId, int recipientId)
        {
            this._provider.ExecuteNonQuery("CoreMessaging_MarkMessageAsSent", messageId, recipientId);
        }

        /// <summary>Gets the user preference.</summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <returns>A <see cref="IDataReader" /> containing the user data.</returns>
        public IDataReader GetUserPreference(int portalId, int userId)
        {
            return this._provider.ExecuteReader("CoreMessaging_GetUserPreference", portalId, userId);
        }

        /// <summary>Sets the user preference.</summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="userId">The user identifier.</param>
        /// <param name="messagesEmailFrequency">The messages email frequency.</param>
        /// <param name="notificationsEmailFrequency">The notifications email frequency.</param>
        public void SetUserPreference(int portalId, int userId, int messagesEmailFrequency, int notificationsEmailFrequency)
        {
            this._provider.ExecuteNonQuery("CoreMessaging_SetUserPreference", portalId, userId, messagesEmailFrequency, notificationsEmailFrequency);
        }
    }
}
