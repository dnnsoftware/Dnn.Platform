// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Messaging.Data
{
    using System;
    using System.Data;

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
