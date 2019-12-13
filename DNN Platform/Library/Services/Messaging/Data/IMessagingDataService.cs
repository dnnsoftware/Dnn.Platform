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
