// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Messaging.Data;

using System;
using System.Data;

public interface IMessagingDataService
{
    IDataReader GetMessageByID(int messageID);

    IDataReader GetUserInbox(int portalID, int userID, int pageNumber, int pageSize);

    int GetInboxCount(int portalID, int userID);

    long SaveMessage(Message objMessaging);

    int GetNewMessageCount(int portalID, int userID);

    IDataReader GetNextMessageForDispatch(Guid schedulerInstance);

    void MarkMessageAsDispatched(int messageID);

    void UpdateMessage(Message message);
}
