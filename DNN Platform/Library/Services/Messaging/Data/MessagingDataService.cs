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

        public IDataReader GetMessageByID(int messageId)
        {
            return this.provider.ExecuteReader("Messaging_GetMessage", messageId);
        }

        public IDataReader GetUserInbox(int PortalID, int UserID, int PageNumber, int PageSize)
        {
            return this.provider.ExecuteReader("Messaging_GetInbox", PortalID, UserID, PageNumber, PageSize);
        }

        public int GetInboxCount(int PortalID, int UserID)
        {
            return this.provider.ExecuteScalar<int>("Messaging_GetInboxCount", PortalID, UserID);
        }

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

        public int GetNewMessageCount(int PortalID, int UserID)
        {
            return this.provider.ExecuteScalar<int>("Messaging_GetNewMessageCount", PortalID, UserID);
        }

        public IDataReader GetNextMessageForDispatch(Guid SchedulerInstance)
        {
            return this.provider.ExecuteReader("Messaging_GetNextMessageForDispatch", SchedulerInstance);
        }

        public void MarkMessageAsDispatched(int MessageID)
        {
            this.provider.ExecuteNonQuery("Messaging_MarkMessageAsDispatched", MessageID);
        }

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
