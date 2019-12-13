#region Usings

using System;
using System.Collections.Generic;
using System.Web;

using DotNetNuke.Common.Utilities;
using DotNetNuke.Security;

#endregion

namespace DotNetNuke.Services.Journal.Internal
{
    public class InternalJournalControllerImpl : IInternalJournalController
    {
        private readonly IJournalDataService _dataService;

        #region Constructors

        public InternalJournalControllerImpl()
        {
            _dataService = JournalDataService.Instance;
        }

        #endregion

        public IList<JournalItem> GetJournalItemsByProfile(int portalId, int moduleId, int currentUserId, int profileId,
                                                           int rowIndex, int maxRows)
        {
            return
                CBO.FillCollection<JournalItem>(_dataService.Journal_ListForProfile(portalId, moduleId, currentUserId,
                                                                                    profileId, rowIndex, maxRows));
        }

        public IList<JournalItem> GetJournalItemsByGroup(int portalId, int moduleId, int currentUserId, int groupId,
                                                         int rowIndex, int maxRows)
        {
            return
                CBO.FillCollection<JournalItem>(_dataService.Journal_ListForGroup(portalId, moduleId, currentUserId,
                                                                                  groupId, rowIndex, maxRows));
        }

        public IList<JournalItem> GetJournalItems(int portalId, int moduleId, int currentUserId, int rowIndex,
                                                  int maxRows)
        {
            return
                CBO.FillCollection<JournalItem>(_dataService.Journal_ListForSummary(portalId, moduleId, currentUserId,
                                                                                    rowIndex, maxRows));
        }
        
        public void DeleteFilters(int portalId, int moduleId)
        {
            _dataService.Journal_TypeFilters_Delete(portalId, moduleId);
        }

        public void SaveFilters(int portalId, int moduleId, int journalTypeId)
        {
            _dataService.Journal_TypeFilters_Save(portalId, moduleId, journalTypeId);
        }
    }
}
