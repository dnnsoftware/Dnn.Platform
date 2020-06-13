// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Messaging
{
    using System;
    using System.Collections.Generic;

    using DotNetNuke.Services.Messaging.Data;

    public interface IMessagingController
    {
        Message GetMessageByID(int PortalID, int UserID, int MessageID);

        List<Message> GetUserInbox(int PortalID, int UserID, int PageNumber, int PageSize);

        int GetInboxCount(int PortalID, int UserID);

        int GetNewMessageCount(int PortalID, int UserID);

        Message GetNextMessageForDispatch(Guid SchedulerInstance);

        void SaveMessage(Message objMessage);

        void UpdateMessage(Message objMessage);

        void MarkMessageAsDispatched(int MessageID);
    }
}
