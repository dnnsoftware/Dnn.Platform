// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Messaging.Data
{
    using System;
    using System.Data;

    using DotNetNuke.Data;

    public class MessagingDataService : IMessagingDataService
    {
        private readonly DataProvider provider = DataProvider.Instance();

        /// <inheritdoc/>
        public IDataReader GetMessageByID(int messageId)
        {
            return this.provider.ExecuteReader("Messaging_GetMessage", messageId);
        }

        /// <inheritdoc/>
        public IDataReader GetUserInbox(int PortalID, int UserID, int PageNumber, int PageSize)
        {
            return this.provider.ExecuteReader("Messaging_GetInbox", PortalID, UserID, PageNumber, PageSize);
        }

        /// <inheritdoc/>
        public int GetInboxCount(int PortalID, int UserID)
        {
            return this.provider.ExecuteScalar<int>("Messaging_GetInboxCount", PortalID, UserID);
        }

        /// <inheritdoc/>
        public long SaveMessage(Message objMessaging)
        {
            return this.provider.ExecuteScalar<long>(
                "Messaging_Save_Message",
                objMessaging.PortalID,
                objMessaging.FromUserID,
                objMessaging.ToUserID,
                objMessaging.ToRoleID,
                (int)objMessaging.Status,
                objMessaging.Subject,
                objMessaging.Body,
                objMessaging.MessageDate,
                objMessaging.Conversation,
                objMessaging.ReplyTo,
                objMessaging.AllowReply,
                objMessaging.SkipInbox);
        }

        /// <inheritdoc/>
        public int GetNewMessageCount(int PortalID, int UserID)
        {
            return this.provider.ExecuteScalar<int>("Messaging_GetNewMessageCount", PortalID, UserID);
        }

        /// <inheritdoc/>
        public IDataReader GetNextMessageForDispatch(Guid SchedulerInstance)
        {
            return this.provider.ExecuteReader("Messaging_GetNextMessageForDispatch", SchedulerInstance);
        }

        /// <inheritdoc/>
        public void MarkMessageAsDispatched(int MessageID)
        {
            this.provider.ExecuteNonQuery("Messaging_MarkMessageAsDispatched", MessageID);
        }

        /// <inheritdoc/>
        public void UpdateMessage(Message message)
        {
            this.provider.ExecuteNonQuery(
                "Messaging_UpdateMessage",
                message.MessageID,
                message.ToUserID,
                message.ToRoleID,
                (int)message.Status,
                message.Subject,
                message.Body,
                message.MessageDate,
                message.ReplyTo,
                message.AllowReply,
                message.SkipInbox);
        }
    }
}
