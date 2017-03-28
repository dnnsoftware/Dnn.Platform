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
    public class PagesExportService : BasePortableService
    {
        public override string Category => Constants.Category_Pages;

        public override string ParentCategory => null;

        public override uint Priority => 20;

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            if (CheckPoint.Stage > 0) return;
            if (CheckCancelled(exportJob)) return;

            ProcessExportPages(exportJob, exportDto, exportDto.Pages);
            CheckPoint.Progress = 100;
            CheckPoint.Stage++;
            CheckPoint.StageData = null;
            CheckPointStageCallback(this);
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (CheckPoint.Stage > 0) return;
            if (CheckCancelled(importJob)) return;

            ProcessImportPages(importJob, importDto);
            CheckPoint.Progress = 100;
            CheckPoint.Stage++;
            CheckPoint.StageData = null;
            CheckPointStageCallback(this);
        }

        public override int GetImportTotal()
        {
            return Repository.GetCount<ExportTab>();
        }

        #region import methods

        private void ProcessImportPages(ExportImportJob importJob, ImportDto importDto)
        {
            var totalImportedTabs = 0;
            var totalImportedSettings = 0;
            var totalImportedPermissions = 0;
            var totalImportedUrls = 0;
            var totalImportedAliasSkins = 0;
            var totalImportedModules = 0;
            var totalImportedModuleSettings = 0;
            var portalId = importJob.PortalId;

            int lastProcessedId; // this is the exported DB row ID; not the TabID
            int.TryParse(CheckPoint.StageData, out lastProcessedId);

            var tabController = TabController.Instance;
            var localTabs = tabController.GetTabsByPortal(portalId).Values;

            var exportedTabs = Repository.GetAllItems<ExportTab>().ToList(); // ordered by ID
            CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? exportedTabs.Count : CheckPoint.TotalItems;
            var progressStep = 100.0 / exportedTabs.Count;

            foreach (var otherTab in exportedTabs)
            {
                if (CheckCancelled(importJob)) break;
                if (lastProcessedId > otherTab.Id) continue;

                var createdBy = Util.GetUserIdOrName(importJob, otherTab.CreatedByUserID, otherTab.CreatedByUserName);
                var modifiedBy = Util.GetUserIdOrName(importJob, otherTab.LastModifiedByUserID, otherTab.LastModifiedByUserName);
                var localTab = localTabs.FirstOrDefault(t => t.TabName == otherTab.TabName && t.TabPath == otherTab.TabPath);

                if (localTab != null)
                {
                    switch (importDto.CollisionResolution)
                    {
                        case CollisionResolution.Ignore:
                            Result.AddLogEntry("Ignored Tab", otherTab.TabName + "(" + otherTab.TabPath + ")");
                            break;
                        case CollisionResolution.Overwrite:
                            SetTabData(localTab, otherTab);
                            tabController.UpdateTab(localTab);
                            //TODO: add related items
                            Result.AddLogEntry("Updated page", otherTab.TabName + "(" + otherTab.TabPath + ")");
                            totalImportedTabs++;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(importDto.CollisionResolution.ToString());
                    }
                }
                else
                {
                    localTab = new TabInfo { PortalID = portalId };
                    SetTabData(localTab, otherTab);
                    localTab.ParentId = GetParentLocalTabId(tabController, portalId, otherTab.TabId, exportedTabs, localTabs);
                    if (localTab.ParentId == -1 && otherTab.ParentId > 0)
                    {
                        Result.AddLogEntry("WARN: Imported Tab Parent not found", otherTab.TabName + "(" + otherTab.TabPath + ")");
                    }

                    otherTab.LocalId = tabController.AddTab(localTab);
                    //TODO: add related items
                    Result.AddLogEntry("Added new page", otherTab.TabName + "(" + otherTab.TabPath + ")");
                    totalImportedTabs++;
                }
                CheckPoint.ProcessedItems++;
                CheckPoint.Progress += progressStep;
                if (CheckPointStageCallback(this)) return;
            }

            Result.AddSummary("Imported Tabs", totalImportedTabs.ToString());
            Result.AddLogEntry("Imported Tab Settings", totalImportedSettings.ToString());
            Result.AddLogEntry("Imported Tab Permissions", totalImportedPermissions.ToString());
            Result.AddLogEntry("Imported Tab Urls", totalImportedUrls.ToString());
            Result.AddLogEntry("Imported Tab Alias Skins", totalImportedAliasSkins.ToString());
            Result.AddLogEntry("Imported Tab Modules", totalImportedModules.ToString());
            Result.AddLogEntry("Imported Tab Module Settings", totalImportedModuleSettings.ToString());
        }

        private static int GetParentLocalTabId(ITabController tabController, int portalId,
            int otherTabId, IEnumerable<ExportTab> exportedTabs, IEnumerable<TabInfo> localTabs)
        {
            var otherTab = exportedTabs.FirstOrDefault(t => t.TabId == otherTabId);
            if (otherTab != null)
            {
                var localTab = localTabs.FirstOrDefault(t => t.TabName == otherTab.TabName && t.TabPath == otherTab.TabPath);
                if (localTab != null)
                    return localTab.TabID;
            }

            return -1;
        }

        private static void SetTabData(TabInfo localTab, ExportTab otherTab)
        {
            localTab.TabOrder = otherTab.TabOrder;
            localTab.TabName = otherTab.TabName;
            localTab.IsVisible = otherTab.IsVisible;
            localTab.ParentId = otherTab.ParentId ?? -1; // TODO: get actual parent from local tabs
            localTab.IconFile = otherTab.IconFile;
            localTab.DisableLink = otherTab.DisableLink;
            localTab.Title = otherTab.Title;
            localTab.Description = otherTab.Description;
            localTab.KeyWords = otherTab.KeyWords;
            localTab.IsDeleted = otherTab.IsDeleted;
            localTab.Url = otherTab.Url;
            localTab.SkinSrc = otherTab.SkinSrc;
            localTab.ContainerSrc = otherTab.ContainerSrc;
            localTab.StartDate = otherTab.StartDate ?? DateTime.MinValue;
            localTab.EndDate = otherTab.EndDate ?? DateTime.MinValue;
            localTab.RefreshInterval = otherTab.RefreshInterval ?? -1;
            localTab.PageHeadText = otherTab.PageHeadText;
            localTab.IsSecure = otherTab.IsSecure;
            localTab.PermanentRedirect = otherTab.PermanentRedirect;
            localTab.SiteMapPriority = otherTab.SiteMapPriority;
            //localTab.CreatedByUserID = otherTab.CreatedByUserID ?? -1;    //TODO: set these
            //localTab.CreatedOnDate = otherTab.CreatedOnDate ?? DateTime.MinValue;
            //localTab.LastModifiedByUserID = otherTab.LastModifiedByUserID ?? -1;
            //localTab.LastModifiedOnDate = otherTab.LastModifiedOnDate ?? DateTime.MinValue;
            localTab.IconFileLarge = otherTab.IconFileLarge;
            localTab.CultureCode = otherTab.CultureCode;
            //localTab.ContentItemID = otherTab.ContentItemID ?? -1;
            localTab.UniqueId = otherTab.UniqueId;
            localTab.VersionGuid = otherTab.VersionGuid;
            localTab.DefaultLanguageGuid = otherTab.DefaultLanguageGuid ?? Guid.Empty;
            localTab.LocalizedVersionGuid = otherTab.LocalizedVersionGuid;
            localTab.Level = otherTab.Level;
            localTab.TabPath = otherTab.TabPath;
            localTab.HasBeenPublished = otherTab.HasBeenPublished;
            localTab.IsSystem = otherTab.IsSystem;
        }

        #endregion

        #region export methods

        private void ProcessExportPages(ExportImportJob exportJob, ExportDto exportDto, int[] selectedPages)
        {
            var totalExportedTabs = 0;
            var totalExportedSettings = 0;
            var totalExportedPermissions = 0;
            var totalExportedUrls = 0;
            var totalExportedAliasSkins = 0;
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
                        SaveTabPermissions(exportPage, exportJob.CreatedOnDate, exportDto.SinceTime?.DateTime);

                    totalExportedUrls +=
                        SaveTabUrls(exportPage, exportJob.CreatedOnDate, exportDto.SinceTime?.DateTime);

                    totalExportedAliasSkins +=
                        SaveTabAliasSkins(exportPage, exportJob.CreatedOnDate, exportDto.SinceTime?.DateTime);

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
            Result.AddLogEntry("Exported Tab Urls", totalExportedUrls.ToString());
            Result.AddLogEntry("Exported Tab Alias Skins", totalExportedAliasSkins.ToString());
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

        private int SaveTabPermissions(ExportTab exportPage, DateTime tillDate, DateTime? sinceDate)
        {
            var tabPermissions = EntitiesController.Instance.GetTabPermissions(exportPage.TabId, tillDate, sinceDate);
            if (tabPermissions.Count > 0)
                Repository.CreateItems(tabPermissions, exportPage.ReferenceId);
            return tabPermissions.Count;
        }

        private int SaveTabUrls(ExportTab exportPage, DateTime tillDate, DateTime? sinceDate)
        {
            var tabUrls = EntitiesController.Instance.GetTabUrls(exportPage.TabId, tillDate, sinceDate);
            if (tabUrls.Count > 0)
                Repository.CreateItems(tabUrls, exportPage.ReferenceId);
            return tabUrls.Count;
        }

        private int SaveTabAliasSkins(ExportTab exportPage, DateTime tillDate, DateTime? sinceDate)
        {
            var tabSkins = EntitiesController.Instance.GetTabAliasSkins(exportPage.TabId, tillDate, sinceDate);
            if (tabSkins.Count > 0)
                Repository.CreateItems(tabSkins, exportPage.ReferenceId);
            return tabSkins.Count;
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
                EndDate = tab.EndDate == DateTime.MinValue ? null : (DateTime?)tab.EndDate,
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

        #endregion

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

    }
}