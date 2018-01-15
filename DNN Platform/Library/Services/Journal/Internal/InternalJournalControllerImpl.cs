#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

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