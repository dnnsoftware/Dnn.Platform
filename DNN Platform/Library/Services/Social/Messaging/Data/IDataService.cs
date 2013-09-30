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
#region Usings

using System;
using System.Collections.Generic;
using System.Data;

using DotNetNuke.Services.Social.Messaging.Internal.Views;

#endregion

namespace DotNetNuke.Services.Social.Messaging.Data
{
    public interface IDataService
    {
        #region Messages CRUD

        int SaveMessage(Message message, int portalId, int createUpdateUserId);
        IDataReader GetMessage(int messageId);
        IDataReader GetMessagesBySender(int messageId, int portalId);
        IDataReader GetLastSentMessage(int userId, int portalId);
        void DeleteMessage(int messageId);

        IDataReader GetInBoxView(int userId, int portalId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending, MessageReadStatus readStatus, MessageArchivedStatus archivedStatus, MessageSentStatus sentStatus);
        IDataReader GetSentBoxView(int userId, int portalId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending);
        IDataReader GetArchiveBoxView(int userId, int portalId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending);
        IDataReader GetMessageThread(int conversationId, int userId, int afterMessageId, int numberOfRecords, string sortColumn, bool sortAscending, ref int totalRecords);        
        void UpdateMessageReadStatus(int conversationId, int userId, bool read);
        void UpdateMessageArchivedStatus(int conversationId, int userId, bool archived);
        int CreateMessageReply(int conversationId, int portalId,string body, int senderUserId, string from, int createUpdateUserId);
        int CountNewThreads(int userId, int portalId);
        int CountTotalConversations(int userId, int portalId);
        int CountMessagesByConversation(int conversationId);
        int CountArchivedMessagesByConversation(int conversationId);
        int CountSentMessages(int userId, int portalId);
        int CountArchivedMessages(int userId, int portalId);
        
        #endregion

        #region Message_Recipients CRUD

        int SaveMessageRecipient(MessageRecipient messageRecipient, int createUpdateUserId);
        void CreateMessageRecipientsForRole(int messageId, string roleIds, int createUpdateUserId);
        IDataReader GetMessageRecipient(int messageRecipientId);
        IDataReader GetMessageRecipientsByUser(int userId);
        IDataReader GetMessageRecipientsByMessage(int messageId);
        IDataReader GetMessageRecipientByMessageAndUser(int messageId, int userId);
        void DeleteMessageRecipient(int messageRecipientId);
        void DeleteMessageRecipientByMessageAndUser(int messageId, int userId);

        #endregion

        #region Message_Attachments CRUD

        int SaveMessageAttachment(MessageAttachment messageAttachment, int createUpdateUserId);
        IDataReader GetMessageAttachment(int messageAttachmentId);
        IList<MessageFileView> GetMessageAttachmentsByMessage(int messageId);
        void DeleteMessageAttachment(int messageAttachmentId);

        #endregion

        #region Upgrade APIs
        
        void ConvertLegacyMessages(int pageIndex, int pageSize);

        IDataReader CountLegacyMessages();        

        #endregion

        #region Queued email API's

        IDataReader GetNextMessagesForInstantDispatch(Guid schedulerInstance, int batchSize);
        IDataReader GetNextMessagesForDigestDispatch(int frequecy, Guid schedulerInstance, int batchSize);
        void MarkMessageAsDispatched(int messageId,int recipientId);
        void MarkMessageAsSent(int messageId, int recipientId);

        #endregion

        #region User Preferences

        IDataReader GetUserPreference(int portalId, int userId);
        void SetUserPreference(int portalId, int userId, int messagesEmailFrequency, int notificationsEmailFrequency);

        #endregion
    }
}