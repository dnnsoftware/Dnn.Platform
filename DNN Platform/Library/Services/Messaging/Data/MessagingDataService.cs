#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
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