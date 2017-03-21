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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Controllers;
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
            ProgressPercentage = 0;
            if (CheckPoint.Stage > 0) return;
            if (CheckCancelled(exportJob)) return;

            var selectedPages = exportDto.Pages.Where(pg => pg.CheckedState == TriCheckedState.Checked).ToList();
            var totalExported = ProcessExportPages(exportJob, exportDto, selectedPages);
            Result.AddSummary("Exported Pages", totalExported.ToString());
            ProgressPercentage = 100;

            CheckPoint.Stage++;
            CheckPoint.StageData = null;
            CheckPointStageCallback(this);
        }

        public override void ImportData(ExportImportJob importJob, ExportDto exportDto)
        {
            if (CheckCancelled(importJob)) return;
            ProgressPercentage = 0;
            //TODO
        }

        private int ProcessExportPages(ExportImportJob exportJob, ExportDto exportDto,
            IList<PageToExport> selectedPages)
        {
            var totalExported = 0;
            var portalId = exportJob.PortalId;

            int lastProcessedTabId;
            int.TryParse(CheckPoint.StageData, out lastProcessedTabId);

            var sinceDate = exportDto.SinceTime;
            var tillDate = exportJob.CreatedOnDate;

            var root = selectedPages.FirstOrDefault(pg => pg.TabId == -1);
            var isAllIncluded = root != null && root.CheckedState == TriCheckedState.Checked;

            var tabController = TabController.Instance;
            var allTabs = EntitiesController.Instance.GetPortalTabs(portalId); // ordered by TabID

            foreach (var pg in allTabs)
            {
                if (CheckCancelled(exportJob)) break;

                if (lastProcessedTabId > pg.TabId) continue;
                if (pg.IsDeleted && !exportDto.IncludeDeletions) continue;
                if (pg.LastUpdatedOn < sinceDate || pg.LastUpdatedOn >= tillDate) continue;

                var tab = tabController.GetTab(portalId, pg.TabId);
                if (isAllIncluded || IsTabIncluded(pg, allTabs, selectedPages))
                {
                    SaveExportPage(tab);
                    totalExported++;

                    CheckPoint.StageData = tab.TabID.ToString(); // last processed TAB ID
                    if (CheckPointStageCallback(this)) break;
                }
            }

            return totalExported;
        }

        private static bool IsTabIncluded(ShortTabData tab,
            IList<ShortTabData> allTabs, IList<PageToExport> selectedPages)
        {
            do
            {
                if (selectedPages.Any(pg => pg.TabId == tab.TabId))
                    return true;

                tab = allTabs.FirstOrDefault(t => t.TabId == tab.ParentId);
            } while (tab?.ParentId != null);

            return false;
        }

        private void SaveExportPage(TabInfo tab)
        {
            var dbPage = GetDbPageFromTab(tab);
            Repository.CreateItem(dbPage, null);

            //TODO: save page related data

            Result.AddLogEntry("Exported page", tab.TabName);
        }

        private static ExportPage GetDbPageFromTab(TabInfo tab)
        {
            return new ExportPage
            {
                TabId = tab.TabID,
                TabOrder = tab.TabOrder,
                TabName = tab.TabName,
                IsVisible = tab.IsVisible,
                ParentId = tab.ParentId <= 0 ? null : (int?)tab.ParentId,
                IconFile = tab.IconFile,
                DisableLink = tab.DisableLink,
                Title = tab.Title,
                Description = tab.Description,
                KeyWords = tab.KeyWords,
                IsDeleted = tab.IsDeleted,
                Url = tab.Url,
                ContainerSrc = tab.ContainerSrc,
                StartDate = tab.StartDate == DateTime.MinValue ? null : (DateTime?)tab.StartDate,
                EndtDate = tab.EndDate == DateTime.MinValue ? null : (DateTime?)tab.EndDate,
                RefreshInterval = tab.RefreshInterval <= 0 ? null : (int?)tab.RefreshInterval,
                PageHeadText = tab.PageHeadText,
                IsSecure = tab.IsSecure,
                PermanentRedirect = tab.PermanentRedirect,
                SiteMapPriority = tab.SiteMapPriority,
                CreatedByUserID = tab.CreatedByUserID <= 0 ? null : (int?)tab.CreatedByUserID,
                CreatedOnDate = tab.CreatedOnDate == DateTime.MinValue ? null : (DateTime?)tab.CreatedOnDate,
                LastModifiedByUserID = tab.LastModifiedByUserID <= 0 ? null : (int?)tab.LastModifiedByUserID,
                LastModifiedOnDate = tab.LastModifiedOnDate == DateTime.MinValue ? null : (DateTime?)tab.LastModifiedOnDate,
                IconFileLarge = tab.IconFileLarge,
                CultureCode = tab.CultureCode,
                ContentItemID = tab.ContentItemId < 0 ? null : (int?)tab.ContentItemId,
                UniqueId = tab.UniqueId,
                VersionGuid = tab.VersionGuid,
                DefaultLanguageGuid = tab.DefaultLanguageGuid == Guid.Empty ? null : (Guid?)tab.DefaultLanguageGuid,
                LocalizedVersionGuid = tab.LocalizedVersionGuid,
                Level = tab.Level,
                TabPath = tab.TabPath,
                HasBeenPublished = tab.HasBeenPublished,
                IsSystem = tab.IsSystem,
            };
        }
    }
}