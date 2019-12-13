// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Data;

using DotNetNuke.Data;

#endregion

namespace DotNetNuke.Services.Messaging.Data
{
    public class MessagingDataService : IMessagingDataService
    {
        private readonly DataProvider provider = DataProvider.Instance();

        #region IMessagingDataService Members

        public IDataReader GetMessageByID(int messageId)
        {
            return provider.ExecuteReader("Messaging_GetMessage", messageId);
        }

        public IDataReader GetUserInbox(int PortalID, int UserID, int PageNumber, int PageSize)
        {
            return provider.ExecuteReader("Messaging_GetInbox", PortalID, UserID, PageNumber, PageSize);
        }

        public int GetInboxCount(int PortalID, int UserID)
        {
            return provider.ExecuteScalar<int>("Messaging_GetInboxCount", PortalID, UserID);
        }

        public long SaveMessage(Message objMessaging)
        {
            return provider.ExecuteScalar<long>("Messaging_Save_Message",
                                                      objMessaging.PortalID,
                                                      objMessaging.FromUserID,
                                                      objMessaging.ToUserID,
                                                      objMessaging.ToRoleID,
                                                      (int) objMessaging.Status,
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
            return provider.ExecuteScalar<int>("Messaging_GetNewMessageCount", PortalID, UserID);
        }

        public IDataReader GetNextMessageForDispatch(Guid SchedulerInstance)
        {
            return provider.ExecuteReader("Messaging_GetNextMessageForDispatch", SchedulerInstance);
        }

        public void MarkMessageAsDispatched(int MessageID)
        {
            provider.ExecuteNonQuery("Messaging_MarkMessageAsDispatched", MessageID);
        }

        public void UpdateMessage(Message message)
        {
            provider.ExecuteNonQuery("Messaging_UpdateMessage",
                                     message.MessageID,
                                     message.ToUserID,
                                     message.ToRoleID,
                                     (int) message.Status,
                                     message.Subject,
                                     message.Body,
                                     message.MessageDate,
                                     message.ReplyTo,
                                     message.AllowReply,
                                     message.SkipInbox);
        }

        #endregion
    }
}
