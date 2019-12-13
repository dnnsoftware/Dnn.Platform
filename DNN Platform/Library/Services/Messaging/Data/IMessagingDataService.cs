// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Data;

#endregion

namespace DotNetNuke.Services.Messaging.Data
{
    public interface IMessagingDataService
    {
        IDataReader GetMessageByID(int MessageID);

        IDataReader GetUserInbox(int PortalID, int UserID, int PageNumber, int PageSize);

        int GetInboxCount(int PortalID, int UserID);

        long SaveMessage(Message objMessaging);

        int GetNewMessageCount(int PortalID, int UserID);

        IDataReader GetNextMessageForDispatch(Guid SchedulerInstance);

        void MarkMessageAsDispatched(int MessageID);

        void UpdateMessage(Message message);
    }
}
