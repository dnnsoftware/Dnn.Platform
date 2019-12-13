using System;
using System.Collections.Generic;

namespace DotNetNuke.Services.Journal.Internal
{
    public interface IInternalJournalController
    {
        IList<JournalItem> GetJournalItemsByProfile  (int portalId, int moduleId, int userID, int profileId, int currentIndex, int rows);
        IList<JournalItem> GetJournalItemsByGroup(int portalId, int moduleId, int userID, int socialGroupId, int currentIndex, int rows);
        IList<JournalItem> GetJournalItems(int portalId, int moduleId, int userID, int currentIndex, int rows);

        void DeleteFilters(int portalId, int moduleId);
        void SaveFilters(int portalId, int moduleId, int toInt32);
    }
}
