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
    public class PagesExportService : Portable2Base
    {
        private int _progressPercentage;

        public override string Category => Constants.Category_Pages;

        public override string ParentCategory => null;

        public override uint Priority => 20;

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

            ProcessExportPages(exportJob, exportDto, exportDto.Pages);
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

        private void ProcessExportPages(ExportImportJob exportJob, ExportDto exportDto, int[] selectedPages)
        {
            var totalExportedTabs = 0;
            var totalExportedSettings = 0;
            var totalExportedPermissions = 0;
            var totalExportedModules = 0;
            var totalExportedModuleSettings = 0;
            var portalId = exportJob.PortalId;

            int lastProcessedTabId;
            int.TryParse(CheckPoint.StageData, out lastProcessedTabId);

            var sinceDate = exportDto.SinceTime;
            var tillDate = exportJob.CreatedOnDate;
            var isAllIncluded = selectedPages.Any(id => id == -1);

            var tabController = TabController.Instance;
            var allTabs = EntitiesController.Instance.GetPortalTabs(
                portalId, exportJob.CreatedOnDate, exportDto.SinceTime?.DateTime); // ordered by TabID

            foreach (var pg in allTabs)
            {
                if (CheckCancelled(exportJob)) break;

                if (lastProcessedTabId > pg.TabId) continue;
                if (pg.IsDeleted && !exportDto.IncludeDeletions) continue;
                if (pg.LastUpdatedOn < sinceDate || pg.LastUpdatedOn >= tillDate) continue;

                var tab = tabController.GetTab(pg.TabId, portalId);
                if (isAllIncluded || IsTabIncluded(pg, allTabs, selectedPages))
                {
                    var exportPage = SaveExportPage(tab);

                    totalExportedSettings +=
                        SaveTabSettings(exportPage, exportJob.CreatedOnDate, exportDto.SinceTime?.DateTime);

                    totalExportedPermissions +=
                        SaveTabPermission(exportPage, exportJob.CreatedOnDate, exportDto.SinceTime?.DateTime);

                    totalExportedModules +=
                        SaveTabModules(exportPage, exportDto.IncludeDeletions);

                    totalExportedModuleSettings +=
                        SaveTabModuleSettings(exportPage, exportDto.IncludeDeletions);

                    totalExportedTabs++;
                    CheckPoint.StageData = tab.TabID.ToString(); // last processed TAB ID
                    if (CheckPointStageCallback(this)) break;
                }
            }

            Result.AddSummary("Exported Tabs", totalExportedTabs.ToString());
            Result.AddLogEntry("Exported Tab Settings", totalExportedSettings.ToString());
            Result.AddLogEntry("Exported Tab Permissions", totalExportedPermissions.ToString());
            Result.AddLogEntry("Exported Tab Modules", totalExportedModules.ToString());
            Result.AddLogEntry("Exported Tab Module Settings", totalExportedModuleSettings.ToString());
        }

        private int SaveTabSettings(ExportTab exportPage, DateTime tillDate, DateTime? sinceDate)
        {
            var tabSettings = EntitiesController.Instance.GetTabSettings(exportPage.TabId, tillDate, sinceDate);
            if (tabSettings.Count > 0)
                Repository.CreateItems(tabSettings, exportPage.ReferenceId);
            return tabSettings.Count;
        }

        private int SaveTabPermission(ExportTab exportPage, DateTime tillDate, DateTime? sinceDate)
        {
            var tabPermissions = EntitiesController.Instance.GetTabPermissions(exportPage.TabId, tillDate, sinceDate);
            if (tabPermissions.Count > 0)
                Repository.CreateItems(tabPermissions, exportPage.ReferenceId);
            return tabPermissions.Count;
        }

        private int SaveTabModules(ExportTab exportPage, bool includeDeleted)
        {
            var tabModules = EntitiesController.Instance.GetTabModules(exportPage.TabId, includeDeleted);
            if (tabModules.Count > 0)
                Repository.CreateItems(tabModules, exportPage.ReferenceId);
            return tabModules.Count;
        }

        private int SaveTabModuleSettings(ExportTab exportPage, bool includeDeleted)
        {
            var tabModuleSettings = EntitiesController.Instance.GetTabModuleSettings(exportPage.TabId, includeDeleted);
            if (tabModuleSettings.Count > 0)
                Repository.CreateItems(tabModuleSettings, exportPage.ReferenceId);
            return tabModuleSettings.Count;
        }

        private static bool IsTabIncluded(ExportTabInfo tab,
            IList<ExportTabInfo> allTabs, int[] selectedPages)
        {
            do
            {
                if (selectedPages.Any(id => id == tab.TabId))
                    return true;

                tab = allTabs.FirstOrDefault(t => t.TabId == tab.ParentId);
            } while (tab != null);

            return false;
        }

        private ExportTab SaveExportPage(TabInfo tab)
        {
            var exportPage = new ExportTab
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
            Repository.CreateItem(exportPage, null);
            Result.AddLogEntry("Exported page", tab.TabName + "(" + tab.TabPath + ")");
            return exportPage;
        }
    }
}