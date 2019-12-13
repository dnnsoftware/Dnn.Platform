// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
#region Usings

using System;
using System.Collections.Generic;

using DotNetNuke.Services.Messaging.Data;

#endregion

namespace DotNetNuke.Services.Messaging
{
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
