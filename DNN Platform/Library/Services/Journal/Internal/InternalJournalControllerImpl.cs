// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Journal.Internal
{
    using System;
    using System.Collections.Generic;
    using System.Web;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Security;

    public class InternalJournalControllerImpl : IInternalJournalController
    {
        private readonly IJournalDataService _dataService;

        public InternalJournalControllerImpl()
        {
            this._dataService = JournalDataService.Instance;
        }

        public IList<JournalItem> GetJournalItemsByProfile(int portalId, int moduleId, int currentUserId, int profileId,
                                                           int rowIndex, int maxRows)
        {
            return
                CBO.FillCollection<JournalItem>(this._dataService.Journal_ListForProfile(portalId, moduleId, currentUserId,
                                                                                    profileId, rowIndex, maxRows));
        }

        public IList<JournalItem> GetJournalItemsByGroup(int portalId, int moduleId, int currentUserId, int groupId,
                                                         int rowIndex, int maxRows)
        {
            return
                CBO.FillCollection<JournalItem>(this._dataService.Journal_ListForGroup(portalId, moduleId, currentUserId,
                                                                                  groupId, rowIndex, maxRows));
        }

        public IList<JournalItem> GetJournalItems(int portalId, int moduleId, int currentUserId, int rowIndex,
                                                  int maxRows)
        {
            return
                CBO.FillCollection<JournalItem>(this._dataService.Journal_ListForSummary(portalId, moduleId, currentUserId,
                                                                                    rowIndex, maxRows));
        }

        public void DeleteFilters(int portalId, int moduleId)
        {
            this._dataService.Journal_TypeFilters_Delete(portalId, moduleId);
        }

        public void SaveFilters(int portalId, int moduleId, int journalTypeId)
        {
            this._dataService.Journal_TypeFilters_Save(portalId, moduleId, journalTypeId);
        }
    }
}
