#region Copyright
//
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
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

using System.Collections.Generic;
using System.Linq;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Dto.Pages;
using DotNetNuke.Entities.Tabs;

namespace Dnn.ExportImport.Components.Services
{
    /// <summary>
    /// Service to export/import pages/tabs.
    /// </summary>
    public class PagesExportService : Potable2Base
    {
        private int _progressPercentage;

        public override string Category => Constants.Category_Pages;
        public override string ParentCategory => Constants.Category_Users;
        public override uint Priority => 10;

        public int ProgressPercentage
        {
            get { return _progressPercentage; }
            private set
            {
                if (value < 0) value = 0;
                else if (value > 100) value = 100;
                _progressPercentage = value;
            }
        }

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            if (CancellationToken.IsCancellationRequested) return;
            ProgressPercentage = 0;
            var selectedPages = exportDto.Pages.Where(pg => pg.CheckedState == TriCheckedState.Checked).ToList();
            var totalExported = ProcessPages(exportJob, exportDto, selectedPages);
            Result.AddSummary("Exported Pages", totalExported.ToString());
            ProgressPercentage = 100;
        }

        public override void ImportData(ExportImportJob importJob, ExportDto exportDto)
        {
            if (CancellationToken.IsCancellationRequested) return;
            ProgressPercentage = 0;
            //TODO
        }

        private int ProcessPages(ExportImportJob exportJob, ExportDto exportDto,
            IEnumerable<PageToExport> selectedPages)
        {
            var processed = 0;
            var portalId = exportJob.PortalId;
            var tabController = TabController.Instance;
            var allTabs = tabController.GetTabsByPortal(portalId).AsList()
                .AsEnumerable().Where(t => !t.IsDeleted && !t.DisableLink && !t.IsSystem);

            foreach (var page in selectedPages.OrderBy(pg => pg.TabId))
            {
                if (CancellationToken.IsCancellationRequested) break;

                var tab = tabController.GetTab(portalId, page.TabId);
                if (!tabController.IsTabPublished(tab)) continue;

                if (tab.IsDeleted && !exportDto.IncludeDeletions) continue;

                SaveExportPage(tab, tabController, portalId, page, ref processed);
            }

            return processed;
        }

        private void SaveExportPage(TabInfo tab, ITabController tabController, int portalId, PageToExport page, ref int processed)
        {
            var parent = tab.ParentId <= 0 ? null : tabController.GetTab(portalId, page.ParentTabId);
            var dbPage = new ExportPage
            {
                TabId = tab.TabID,
                ParentTabId = tab.ParentId,
                TabName = tab.TabName,
                ParentTabName = parent?.TabName
            };

            Repository.CreateItem(dbPage, null);
            Result.AddLogEntry("Exported page", tab.TabName);
            processed++;
        }
    }
}