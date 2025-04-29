// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Messaging;

using System;
using System.Collections.Generic;

using DotNetNuke.Services.Messaging.Data;

public interface IMessagingController
{
    Message GetMessageByID(int portalID, int userID, int messageID);

    List<Message> GetUserInbox(int portalID, int userID, int pageNumber, int pageSize);

    int GetInboxCount(int portalID, int userID);

    int GetNewMessageCount(int portalID, int userID);

    Message GetNextMessageForDispatch(Guid schedulerInstance);

    void SaveMessage(Message objMessage);

    void UpdateMessage(Message objMessage);

    void MarkMessageAsDispatched(int messageID);
}
