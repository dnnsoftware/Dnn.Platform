// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

// ReSharper disable SuggestBaseTypeForParameter
namespace Dnn.ExportImport.Components.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Controllers;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Engines;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Components.Providers;
    using Dnn.ExportImport.Dto.Pages;
    using Dnn.ExportImport.Dto.Workflow;
    using Dnn.ExportImport.Repository;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Definitions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Security.Permissions;
    using DotNetNuke.Services.Installer.Packages;
    using DotNetNuke.Services.Localization;
    using Newtonsoft.Json;

    using InstallerUtil = DotNetNuke.Services.Installer.Util;
    using TermHelper = DotNetNuke.Entities.Content.Taxonomy.TermHelper;
    using Util = Dnn.ExportImport.Components.Common.Util;

    /// <summary>
    /// Service to export/import pages/tabs.
    /// </summary>
    public class PagesExportService : BasePortableService
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ExportImportEngine));

        private ProgressTotals _totals;
        private DataProvider _dataProvider;
        private ITabController _tabController;
        private IModuleController _moduleController;
        private ExportImportJob _exportImportJob;
        private ImportDto _importDto;
        private ExportDto _exportDto;

        private IList<int> _exportedModuleDefinitions = new List<int>();
        private Dictionary<int, int> _partialImportedTabs = new Dictionary<int, int>();
        private Dictionary<int, bool> _searchedParentTabs = new Dictionary<int, bool>();
        private IList<ImportModuleMapping> _importContentList = new List<ImportModuleMapping>(); // map the exported module and local module.

        public override string Category => Constants.Category_Pages;

        public override string ParentCategory => null;

        public override uint Priority => 20;

        public virtual bool IncludeSystem { get; set; } = false;

        public virtual bool IgnoreParentMatch { get; set; } = false;

        protected ImportDto ImportDto => this._importDto;

        public static void ResetContentsFlag(ExportImportRepository repository)
        {
            // reset restored flag; if it same extracted db is reused, then content will be restored
            var toSkip = 0;
            const int batchSize = 100;
            var totalCount = repository.GetCount<ExportModuleContent>();
            while (totalCount > 0)
            {
                var items = repository.GetAllItems<ExportModuleContent>(skip: toSkip, max: batchSize)
                    .Where(item => item.IsRestored).ToList();
                if (items.Count > 0)
                {
                    items.ForEach(item => item.IsRestored = false);
                    repository.UpdateItems(items);
                }

                toSkip += batchSize;
                totalCount -= batchSize;
            }
        }

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            if (this.CheckPoint.Stage > 0)
            {
                return;
            }

            if (this.CheckCancelled(exportJob))
            {
                return;
            }

            var checkedPages = exportDto.Pages.Where(p => p.CheckedState == TriCheckedState.Checked || p.CheckedState == TriCheckedState.CheckedWithAllChildren);
            if (checkedPages.Any())
            {
                this._exportImportJob = exportJob;
                this._exportDto = exportDto;
                this._tabController = TabController.Instance;
                this._moduleController = ModuleController.Instance;
                this.ProcessExportPages();
            }

            this.CheckPoint.Progress = 100;
            this.CheckPoint.Completed = true;
            this.CheckPoint.Stage++;
            this.CheckPoint.StageData = null;
            this.CheckPointStageCallback(this);
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (this.CheckPoint.Stage > 0)
            {
                return;
            }

            if (this.CheckCancelled(importJob))
            {
                return;
            }

            this._exportImportJob = importJob;
            this._importDto = importDto;
            this._exportDto = importDto.ExportDto;
            this._tabController = TabController.Instance;
            this._moduleController = ModuleController.Instance;

            this.ProcessImportPages();

            this.CheckPoint.Progress = 100;
            this.CheckPoint.Completed = true;
            this.CheckPoint.Stage++;
            this.CheckPoint.StageData = null;
            this.CheckPointStageCallback(this);
        }

        public override int GetImportTotal()
        {
            return this.Repository.GetCount<ExportTab>(x => x.IsSystem == this.IncludeSystem);
        }

        public void RestoreTab(TabInfo tab, PortalSettings portalSettings)
        {
            var changeControlStateForTab = TabChangeSettings.Instance.GetChangeControlState(tab.PortalID, tab.TabID);
            if (changeControlStateForTab.IsChangeControlEnabledForTab)
            {
                TabVersionSettings.Instance.SetEnabledVersioningForTab(tab.TabID, false);
                TabWorkflowSettings.Instance.SetWorkflowEnabled(tab.PortalID, tab.TabID, false);
            }

            this._tabController.RestoreTab(tab, portalSettings);

            if (changeControlStateForTab.IsChangeControlEnabledForTab)
            {
                TabVersionSettings.Instance.SetEnabledVersioningForTab(tab.TabID, changeControlStateForTab.IsVersioningEnabledForTab);
                TabWorkflowSettings.Instance.SetWorkflowEnabled(tab.PortalID, tab.TabID, changeControlStateForTab.IsWorkflowEnabledForTab);
            }
        }

        protected virtual void ProcessImportPage(ExportTab otherTab, IList<ExportTab> exportedTabs, IList<TabInfo> localTabs, IList<int> referenceTabs)
        {
            var portalId = this._exportImportJob.PortalId;
            var createdBy = Util.GetUserIdByName(this._exportImportJob, otherTab.CreatedByUserID, otherTab.CreatedByUserName);
            var modifiedBy = Util.GetUserIdByName(this._exportImportJob, otherTab.LastModifiedByUserID, otherTab.LastModifiedByUserName);
            var localTab = localTabs.FirstOrDefault(t => otherTab.UniqueId.Equals(t.UniqueId)) ?? localTabs.FirstOrDefault(t =>
                  otherTab.TabPath.Equals(t.TabPath, StringComparison.InvariantCultureIgnoreCase)
                  && IsSameCulture(t.CultureCode, otherTab.CultureCode));

            var isParentPresent = this.IsParentTabPresentInExport(otherTab, exportedTabs, localTabs);

            if (localTab != null)
            {
                localTab.TabSettings.Remove("TabImported");
                otherTab.LocalId = localTab.TabID;
                switch (this._importDto.CollisionResolution)
                {
                    case CollisionResolution.Ignore:
                        this.Result.AddLogEntry("Ignored Tab", $"{otherTab.TabName} ({otherTab.TabPath})");
                        break;
                    case CollisionResolution.Overwrite:
                        if (!this.IsTabPublished(localTab))
                        {
                            return;
                        }

                        SetTabData(localTab, otherTab);
                        localTab.StateID = this.GetLocalStateId(otherTab.StateID);
                        var parentId = this.IgnoreParentMatch ? otherTab.ParentId.GetValueOrDefault(Null.NullInteger) : TryFindLocalParentTabId(otherTab, exportedTabs, localTabs);
                        if (parentId == -1 && otherTab.ParentId > 0)
                        {
                            if (!isParentPresent)
                            {
                                this.Result.AddLogEntry("Importing existing tab skipped as its parent was not found", $"{otherTab.TabName} ({otherTab.TabPath})", ReportLevel.Warn);
                                return;
                            }

                            this.CheckForPartialImportedTabs(otherTab);
                        }

                        var tabType = Globals.GetURLType(otherTab.Url);
                        if (tabType == TabType.Tab && !referenceTabs.Contains(localTab.TabID))
                        {
                            referenceTabs.Add(localTab.TabID);
                        }

                        // this is not saved when adding the tab; so set it explicitly
                        localTab.IsVisible = otherTab.IsVisible;
                        EntitiesController.Instance.SetTabSpecificData(localTab.TabID, false, localTab.IsVisible);

                        // Try to set the unique id of existing page same as source page unique id, if possible. This will help for future updates etc.
                        if (localTab.UniqueId != otherTab.UniqueId && !DataProvider.Instance().CheckTabUniqueIdExists(otherTab.UniqueId))
                        {
                            localTab.UniqueId = otherTab.UniqueId;
                            this.UpdateTabUniqueId(localTab.TabID, localTab.UniqueId);
                        }

                        try
                        {
                            localTab.TabPermissions.Clear(); // without this the UpdateTab() could fail
                            localTab.ParentId = parentId;
                            if (tabType == TabType.Url)
                            {
                                localTab.Url = otherTab.Url;
                            }

                            this.SetPartialImportSettings(otherTab, localTab);
                            this._tabController.UpdateTab(localTab);

                            DotNetNuke.Data.DataProvider.Instance().UpdateTabOrder(localTab.TabID, localTab.TabOrder, localTab.ParentId, Null.NullInteger);
                        }
                        catch (Exception ex)
                        {
                            this.Result.AddLogEntry($"Importing tab '{otherTab.TabName}' exception", ex.Message, ReportLevel.Error);
                            return;
                        }

                        this.UpdateTabChangers(localTab.TabID, createdBy, modifiedBy);
                        this.UpdateDefaultLanguageGuid(portalId, localTab, otherTab, exportedTabs);
                        this.AddTabRelatedItems(localTab, otherTab, false);
                        this.TriggerImportEvent(localTab);
                        this.Result.AddLogEntry("Updated Tab", $"{otherTab.TabName} ({otherTab.TabPath})");
                        this._totals.TotalTabs++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(this._importDto.CollisionResolution.ToString());
                }
            }
            else
            {
                localTab = new TabInfo { PortalID = portalId };
                SetTabData(localTab, otherTab);
                localTab.StateID = this.GetLocalStateId(otherTab.StateID);
                var parentId = this.IgnoreParentMatch ? otherTab.ParentId.GetValueOrDefault(Null.NullInteger) : TryFindLocalParentTabId(otherTab, exportedTabs, localTabs);
                var checkPartial = false;
                if (parentId == -1 && otherTab.ParentId > 0)
                {
                    if (!isParentPresent)
                    {
                        this.Result.AddLogEntry("Importing new tab skipped as its parent was not found", $"{otherTab.TabName} ({otherTab.TabPath})", ReportLevel.Warn);
                        return;
                    }

                    checkPartial = true;
                }

                var tabType = Globals.GetURLType(otherTab.Url);
                try
                {
                    localTab.ParentId = parentId;
                    if (tabType == TabType.Url)
                    {
                        localTab.Url = otherTab.Url;
                    }

                    localTab.UniqueId = Guid.NewGuid();
                    this.SetPartialImportSettings(otherTab, localTab);
                    otherTab.LocalId = localTab.TabID = this._tabController.AddTab(localTab, false);
                    DotNetNuke.Data.DataProvider.Instance().UpdateTabOrder(localTab.TabID, localTab.TabOrder, localTab.ParentId, Null.NullInteger);
                    localTabs.Add(localTab);

                    if (tabType == TabType.Tab && !referenceTabs.Contains(localTab.TabID))
                    {
                        referenceTabs.Add(localTab.TabID);
                    }

                    if (checkPartial)
                    {
                        this.CheckForPartialImportedTabs(otherTab);
                    }
                }
                catch (Exception ex)
                {
                    this.Result.AddLogEntry($"Importing tab '{otherTab.TabName}' exception", ex.Message, ReportLevel.Error);
                    return;
                }

                this.UpdateTabChangers(localTab.TabID, createdBy, modifiedBy);

                // this is not saved upon updating the tab
                localTab.IsVisible = otherTab.IsVisible;
                EntitiesController.Instance.SetTabSpecificData(localTab.TabID, false, localTab.IsVisible);

                // _tabController.UpdateTab(localTab); // to clear cache
                // Try to set the unique id of existing page same as source page unique id, if possible. This will help for future updates etc.
                if (!DataProvider.Instance().CheckTabUniqueIdExists(otherTab.UniqueId))
                {
                    localTab.UniqueId = otherTab.UniqueId;
                    this.UpdateTabUniqueId(localTab.TabID, localTab.UniqueId);
                }

                this.Result.AddLogEntry("Added Tab", $"{otherTab.TabName} ({otherTab.TabPath})");
                this._totals.TotalTabs++;
                this.UpdateDefaultLanguageGuid(portalId, localTab, otherTab, exportedTabs);
                this.AddTabRelatedItems(localTab, otherTab, true);
                this.TriggerImportEvent(localTab);
            }

            var portalSettings = new PortalSettings(portalId);

            if (otherTab.IsDeleted)
            {
                this._tabController.SoftDeleteTab(localTab.TabID, portalSettings);
            }
            else
            {
                var tab = this._tabController.GetTab(localTab.TabID, portalId);
                if (tab.IsDeleted)
                {
                    this.RestoreTab(tab, portalSettings);
                }
            }

            this.UpdateParentInPartialImportTabs(localTab, otherTab, portalId, exportedTabs, localTabs);
        }

        private static int TryFindLocalParentTabId(ExportTab exportedTab, IEnumerable<ExportTab> exportedTabs, IEnumerable<TabInfo> localTabs)
        {
            return TryFindLocalTabId(exportedTab, exportedTabs, localTabs, exportedTab.ParentId);
        }

        private static int TryFindLocalTabId(ExportTab exportedTab, IEnumerable<ExportTab> exportedTabs, IEnumerable<TabInfo> localTabs, int? tabId)
        {
            if (tabId.HasValue && tabId.Value > 0)
            {
                var otherParent = exportedTabs.FirstOrDefault(t => t.TabId == tabId);
                if (otherParent != null)
                {
                    if (otherParent.LocalId.HasValue)
                    {
                        var localTab = localTabs.FirstOrDefault(t => t.TabID == otherParent.LocalId);
                        if (localTab != null)
                        {
                            return localTab.TabID;
                        }
                    }
                }
                else if (exportedTab.TabPath.HasValue())
                {
                    var index = exportedTab.TabPath.LastIndexOf(@"//", StringComparison.Ordinal);
                    if (index > 0)
                    {
                        var path = exportedTab.TabPath.Substring(0, index);
                        var localTab = localTabs.FirstOrDefault(t =>
                            path.Equals(t.TabPath, StringComparison.InvariantCultureIgnoreCase)
                            && IsSameCulture(t.CultureCode, exportedTab.CultureCode));
                        if (localTab != null)
                        {
                            return localTab.TabID;
                        }
                    }
                }
            }

            return -1;
        }

        private static bool IsSameCulture(string sourceCultureCode, string targetCultureCode)
        {
            sourceCultureCode = !string.IsNullOrWhiteSpace(sourceCultureCode) ? sourceCultureCode : Localization.SystemLocale;
            targetCultureCode = !string.IsNullOrWhiteSpace(targetCultureCode) ? targetCultureCode : Localization.SystemLocale;

            return sourceCultureCode == targetCultureCode;
        }

        private static void SetTabData(TabInfo localTab, ExportTab otherTab)
        {
            localTab.TabOrder = otherTab.TabOrder;
            localTab.TabName = otherTab.TabName;
            localTab.IsVisible = otherTab.IsVisible;
            localTab.IconFile = otherTab.IconFile;
            localTab.DisableLink = otherTab.DisableLink;
            localTab.Title = otherTab.Title;
            localTab.Description = otherTab.Description;
            localTab.KeyWords = otherTab.KeyWords;

            // localTab.IsDeleted = otherTab.IsDeleted; // DO NOT enable this; leave this to other logic
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
            localTab.IconFileLarge = otherTab.IconFileLarge;
            localTab.CultureCode = otherTab.CultureCode;

            // localTab.UniqueId = otherTab.UniqueId;
            localTab.VersionGuid = otherTab.VersionGuid;
            localTab.LocalizedVersionGuid = otherTab.LocalizedVersionGuid;
            localTab.Level = otherTab.Level;
            localTab.TabPath = otherTab.TabPath;
            localTab.HasBeenPublished = otherTab.HasBeenPublished;
            localTab.IsSystem = otherTab.IsSystem;
            localTab.Terms.Clear();
            localTab.Terms.AddRange(TermHelper.ToTabTerms(otherTab.Tags, localTab.PortalID));
        }

        private static bool IsTabIncluded(ExportTabInfo tab, IList<ExportTabInfo> allTabs, PageToExport[] selectedPages)
        {
            var first = true;
            while (tab != null)
            {
                var pg = selectedPages.FirstOrDefault(p => p.TabId == tab.TabID);
                if (pg != null)
                {
                    if (first)
                    {
                        // this is the current page we are checking for.
                        return pg.CheckedState == TriCheckedState.Checked || pg.CheckedState == TriCheckedState.CheckedWithAllChildren;
                    }

                    // this is a [grand] parent of the page we are checking for.
                    if (pg.CheckedState == TriCheckedState.CheckedWithAllChildren)
                    {
                        return true;
                    }
                }

                first = false;
                tab = allTabs.FirstOrDefault(t => t.TabID == tab.ParentID);
            }

            return false;
        }

        private void ProcessImportPages()
        {
            this._dataProvider = DataProvider.Instance();
            this._totals = string.IsNullOrEmpty(this.CheckPoint.StageData)
                ? new ProgressTotals()
                : JsonConvert.DeserializeObject<ProgressTotals>(this.CheckPoint.StageData);

            var portalId = this._exportImportJob.PortalId;

            var localTabs = this._tabController.GetTabsByPortal(portalId).Values.ToList();

            var exportedTabs = this.Repository.GetItems<ExportTab>(x => x.IsSystem == (this.Category == Constants.Category_Templates))
                .OrderBy(t => t.Level).ThenBy(t => t.ParentId).ThenBy(t => t.TabOrder).ToList();

            // Update the total items count in the check points. This should be updated only once.
            this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? exportedTabs.Count : this.CheckPoint.TotalItems;
            if (this.CheckPointStageCallback(this))
            {
                return;
            }

            var progressStep = 100.0 / exportedTabs.OrderByDescending(x => x.Id).Count(x => x.Id < this._totals.LastProcessedId);

            var index = 0;
            var referenceTabs = new List<int>();
            this._importContentList.Clear();
            foreach (var otherTab in exportedTabs)
            {
                if (this.CheckCancelled(this._exportImportJob))
                {
                    break;
                }

                if (this._totals.LastProcessedId > index)
                {
                    continue; // this is the exported DB row ID; not the TabID
                }

                this.ProcessImportPage(otherTab, exportedTabs, localTabs, referenceTabs);

                this.CheckPoint.ProcessedItems++;
                this.CheckPoint.Progress += progressStep;
                if (this.CheckPointStageCallback(this))
                {
                    break;
                }

                this._totals.LastProcessedId = index++;
                this.CheckPoint.StageData = JsonConvert.SerializeObject(this._totals);
            }

            // repair pages which linked to other pages
            this.RepairReferenceTabs(referenceTabs, localTabs, exportedTabs);

            this._searchedParentTabs.Clear();
            this.ReportImportTotals();
        }

        /// <summary>
        /// Updates the default language unique identifier for the local page.
        /// </summary>
        /// <param name="portalId">The portal identifier.</param>
        /// <param name="localTab">The local tab.</param>
        /// <param name="exportedTab">The exported tab.</param>
        /// <param name="exportedTabs">The list of all exported tabs.</param>
        private void UpdateDefaultLanguageGuid(int portalId, TabInfo localTab, ExportTab exportedTab, IList<ExportTab> exportedTabs)
        {
            // Tab language GUID should correspond to the unique ID of the default locale page (see dbo.[Tabs] for reference)
            // see sample below, DefaultLanguageGuid of translated pages is equal to UniqueId of default locale page

            // Page          UniqueId                               DefaultLanguageGuid                    Culture
            // Home          E568189C-CE35-40FB-9A5F-4AC28920FAA0   NULL                                   en-US
            // Home (fr-FR)  DAD43443-206B-4932-9052-BDBE8A8473B1   E568189C-CE35-40FB-9A5F-4AC28920FAA0   fr-FR
            // Home (it-IT)  7D5C7185-16AC-4AE8-AB29-A90CCB84E7BE   E568189C-CE35-40FB-9A5F-4AC28920FAA0   it-IT

            // On import, we should care about DefaultLanguageGuid for the pages we override and newly created tabs

            // 1. Define whether DefaultLanguageGuid of exported page is not null
            // 2. Find exported page where UniqueId = DefaultLanguageGuid to define default lang page
            // 3. Find corresponding id of local default lang tab
            // 4. Take UniqueId of local default lang tab and set it for the page we are going to create/update
            // 5. Use fallback value if something from above scenario does not work
            if (exportedTab.DefaultLanguageGuid == Null.NullGuid)
            {
                return;
            }

            var defaultLanguagePageToImport = exportedTabs.FirstOrDefault(tab => tab.UniqueId == exportedTab.DefaultLanguageGuid);

            if (defaultLanguagePageToImport != null &&
                defaultLanguagePageToImport.LocalId.HasValue)
            {
                var defaultLanguagePageLocal = this._tabController.GetTab(defaultLanguagePageToImport.LocalId.Value, portalId);

                if (defaultLanguagePageLocal != null)
                {
                    localTab.DefaultLanguageGuid = defaultLanguagePageLocal.UniqueId;
                    return;
                }
            }

            localTab.DefaultLanguageGuid = exportedTab.DefaultLanguageGuid ?? Null.NullGuid;
        }

        private void TriggerImportEvent(TabInfo localTab)
        {
            try
            {
                // update tab with import flag, to trigger update event handler.
                if (localTab.TabSettings.ContainsKey("TabImported"))
                {
                    localTab.TabSettings["TabImported"] = "Y";
                }
                else
                {
                    localTab.TabSettings.Add("TabImported", "Y");
                }

                this._tabController.UpdateTab(localTab);
                TabController.Instance.DeleteTabSetting(localTab.TabID, "TabImported");
            }
            catch (Exception)
            {
                TabController.Instance.DeleteTabSetting(localTab.TabID, "TabImported");
            }
        }

        private void AddTabRelatedItems(TabInfo localTab, ExportTab otherTab, bool isNew)
        {
            this._totals.TotalTabSettings += this.ImportTabSettings(localTab, otherTab, isNew);
            this._totals.TotalTabPermissions += this.ImportTabPermissions(localTab, otherTab, isNew);
            this._totals.TotalTabUrls += this.ImportTabUrls(localTab, otherTab, isNew);
            this._totals.TotalTabModules += this.ImportTabModulesAndRelatedItems(localTab, otherTab, isNew);
        }

        private int ImportTabSettings(TabInfo localTab, ExportTab otherTab, bool isNew)
        {
            var tabSettings = this.Repository.GetRelatedItems<ExportTabSetting>(otherTab.Id).ToList();
            foreach (var other in tabSettings)
            {
                var localValue = isNew ? string.Empty : Convert.ToString(localTab.TabSettings[other.SettingName]);
                if (string.IsNullOrEmpty(localValue))
                {
                    this._tabController.UpdateTabSetting(localTab.TabID, other.SettingName, other.SettingValue);
                    var createdBy = Util.GetUserIdByName(this._exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                    var modifiedBy = Util.GetUserIdByName(this._exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                    this._dataProvider.UpdateSettingRecordChangers("TabSettings", "TabID",
                        localTab.TabID, other.SettingName, createdBy, modifiedBy);
                    this.Result.AddLogEntry("Added tab setting", $"{other.SettingName} - {other.TabID}");
                }
                else
                {
                    switch (this._importDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            if (localValue != other.SettingValue)
                            {
                                // the next will clear the cache
                                this._tabController.UpdateTabSetting(localTab.TabID, other.SettingName, other.SettingValue);
                                var createdBy = Util.GetUserIdByName(this._exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                                var modifiedBy = Util.GetUserIdByName(this._exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                                this._dataProvider.UpdateSettingRecordChangers("TabSettings", "TabID",
                                    localTab.TabID, other.SettingName, createdBy, modifiedBy);
                                this.Result.AddLogEntry("Updated tab setting", $"{other.SettingName} - {other.TabID}");
                            }
                            else
                            {
                                goto case CollisionResolution.Ignore;
                            }

                            break;
                        case CollisionResolution.Ignore:
                            this.Result.AddLogEntry("Ignored tab setting", other.SettingName);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(this._importDto.CollisionResolution.ToString());
                    }
                }
            }

            return tabSettings.Count;
        }

        private int ImportTabPermissions(TabInfo localTab, ExportTab otherTab, bool isNew)
        {
            if (!this._exportDto.IncludePermissions)
            {
                return 0;
            }

            var noRole = Convert.ToInt32(Globals.glbRoleNothing);
            var count = 0;
            var tabPermissions = this.Repository.GetRelatedItems<ExportTabPermission>(otherTab.Id).ToList();
            var localTabPermissions = localTab.TabPermissions.OfType<TabPermissionInfo>().ToList();
            foreach (var other in tabPermissions)
            {
                var roleId = Util.GetRoleIdByName(this._importDto.PortalId, other.RoleID ?? noRole, other.RoleName);
                var userId = UserController.GetUserByName(this._importDto.PortalId, other.Username)?.UserID;

                var local = isNew ? null : localTabPermissions.FirstOrDefault(
                    x => x.PermissionCode == other.PermissionCode && x.PermissionKey == other.PermissionKey
                    && x.PermissionName.Equals(other.PermissionName, StringComparison.InvariantCultureIgnoreCase) &&
                    x.RoleID == roleId && x.UserID == userId);
                var isUpdate = false;
                if (local != null)
                {
                    switch (this._importDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            isUpdate = true;
                            break;
                        case CollisionResolution.Ignore:
                            this.Result.AddLogEntry("Ignored tab permission", other.PermissionKey);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(this._importDto.CollisionResolution.ToString());
                    }
                }

                if (isUpdate)
                {
                    // UNDONE: Do we really need to update an existing permission? It won't do anything; permissions are immutable
                    // Result.AddLogEntry("Updated tab permission", other.PermissionKey);
                }
                else
                {
                    var permissionId = DataProvider.Instance().GetPermissionId(other.PermissionCode, other.PermissionKey, other.PermissionName);
                    if (permissionId != null)
                    {
                        local = new TabPermissionInfo
                        {
                            TabID = localTab.TabID,
                            UserID = Null.NullInteger,
                            RoleID = noRole,
                            Username = other.Username,
                            RoleName = other.RoleName,
                            ModuleDefID = Util.GeModuleDefIdByFriendltName(other.FriendlyName) ?? -1,
                            PermissionKey = other.PermissionKey,
                            PermissionName = other.PermissionName,
                            AllowAccess = other.AllowAccess,
                            PermissionID = permissionId.Value,
                        };
                        if (other.UserID != null && other.UserID > 0 && !string.IsNullOrEmpty(other.Username))
                        {
                            if (userId == null)
                            {
                                this.Result.AddLogEntry(
                                    "Couldn't add tab permission; User is undefined!",
                                    $"{other.PermissionKey} - {other.PermissionID}", ReportLevel.Warn);
                                continue;
                            }

                            local.UserID = userId.Value;
                        }

                        if (other.RoleID != null && other.RoleID > noRole && !string.IsNullOrEmpty(other.RoleName))
                        {
                            if (roleId == null)
                            {
                                this.Result.AddLogEntry(
                                    "Couldn't add tab permission; Role is undefined!",
                                    $"{other.PermissionKey} - {other.PermissionID}", ReportLevel.Warn);
                                continue;
                            }

                            local.RoleID = roleId.Value;
                        }

                        localTab.TabPermissions.Add(local, true);

                        // UNDONE: none set; not possible until after saving all tab permissions as donbefore exiting this method
                        // var createdBy = Util.GetUserIdByName(_exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                        // var modifiedBy = Util.GetUserIdByName(_exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                        // UpdateTabPermissionChangers(local.TabPermissionID, createdBy, modifiedBy);
                        this.Result.AddLogEntry("Added tab permission", $"{other.PermissionKey} - {other.PermissionID}");
                        count++;
                    }
                    else
                    {
                        this.Result.AddLogEntry(
                            "Couldn't add tab permission; Permission is undefined!",
                            $"{other.PermissionKey} - {other.PermissionID}", ReportLevel.Warn);
                    }
                }
            }

            if (count > 0)
            {
                TabPermissionController.SaveTabPermissions(localTab);
            }

            return count;
        }

        private int ImportTabUrls(TabInfo localTab, ExportTab otherTab, bool isNew)
        {
            var count = 0;
            var tabUrls = this.Repository.GetRelatedItems<ExportTabUrl>(otherTab.Id).ToList();
            var localUrls = localTab.TabUrls;
            foreach (var other in tabUrls)
            {
                var local = isNew ? null : localUrls.FirstOrDefault(url => url.SeqNum == other.SeqNum);
                if (local != null)
                {
                    switch (this._importDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            try
                            {
                                local.Url = other.Url;
                                TabController.Instance.SaveTabUrl(local, this._importDto.PortalId, true);
                                this.Result.AddLogEntry("Update Tab Url", other.Url);
                                count++;
                            }
                            catch (Exception ex)
                            {
                                this.Result.AddLogEntry("EXCEPTION updating tab, Tab ID=" + local.TabId, ex.Message, ReportLevel.Error);
                            }

                            break;
                        case CollisionResolution.Ignore:
                            this.Result.AddLogEntry("Ignored tab url", other.Url);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(this._importDto.CollisionResolution.ToString());
                    }
                }
                else
                {
                    var alias = PortalAliasController.Instance.GetPortalAliasesByPortalId(this._importDto.PortalId).FirstOrDefault(a => a.IsPrimary);
                    local = new TabUrlInfo
                    {
                        TabId = localTab.TabID,
                        CultureCode = other.CultureCode,
                        HttpStatus = other.HttpStatus,
                        IsSystem = other.IsSystem,
                        PortalAliasId = alias?.PortalAliasID ?? -1,
                        PortalAliasUsage = (PortalAliasUsageType)(other.PortalAliasUsage ?? 0), // reset to default
                        QueryString = other.QueryString,
                        SeqNum = other.SeqNum,
                        Url = other.Url,
                    };

                    try
                    {
                        TabController.Instance.SaveTabUrl(local, this._importDto.PortalId, true);

                        var createdBy = Util.GetUserIdByName(this._exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                        var modifiedBy = Util.GetUserIdByName(this._exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                        this._dataProvider.UpdateTabUrlChangers(local.TabId, local.SeqNum, createdBy, modifiedBy);

                        this.Result.AddLogEntry("Added Tab Url", other.Url);
                        count++;
                    }
                    catch (Exception ex)
                    {
                        this.Result.AddLogEntry("EXCEPTION adding tab, Tab ID=" + local.TabId, ex.Message, ReportLevel.Error);
                    }
                }
            }

            return count;
        }

        private int ImportTabModulesAndRelatedItems(TabInfo localTab, ExportTab otherTab, bool isNew)
        {
            var count = 0;
            var exportedModules = this.Repository.GetRelatedItems<ExportModule>(otherTab.Id).ToList();
            var exportedTabModules = this.Repository.GetRelatedItems<ExportTabModule>(otherTab.Id)
                .OrderBy(m => m.PaneName?.ToLowerInvariant()).ThenBy(m => m.ModuleOrder).ToList();
            var localExportModules = isNew ? new List<ExportModule>()
                : EntitiesController.Instance.GetModules(localTab.TabID, true, Constants.MaxDbTime, null).ToList();
            var localTabModules = isNew ? new List<ModuleInfo>() : this._moduleController.GetTabModules(localTab.TabID).Values.ToList();
            var allExistingIds = localTabModules.Select(l => l.ModuleID).ToList();
            var allImportedIds = new List<int>();

            var localOrders = this.BuildModuleOrders(localTabModules);
            var exportOrders = this.BuildModuleOrders(exportedTabModules);
            foreach (var other in exportedTabModules)
            {
                var locals = new List<ModuleInfo>(localTabModules.Where(m => m.UniqueId == other.UniqueId && m.IsDeleted == other.IsDeleted));
                if (locals.Count == 0)
                {
                    locals = new List<ModuleInfo>(localTabModules.Where(m => m.ModuleDefinition.FriendlyName == other.FriendlyName
                                                                             && m.PaneName == other.PaneName
                                                                             && this.ModuleOrderMatched(m, other, localOrders, exportOrders)
                                                                             && m.IsDeleted == other.IsDeleted)).ToList();
                }

                var otherModule = exportedModules.FirstOrDefault(m => m.ModuleID == other.ModuleID);
                if (otherModule == null)
                {
                    continue; // must not happen
                }

                var moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(other.FriendlyName);
                if (moduleDefinition == null)
                {
                    this.Result.AddLogEntry(
                        "Error adding tab module, ModuleDef=" + other.FriendlyName,
                        "The modue definition is not present in the system", ReportLevel.Error);
                    continue; // the module is not installed, therefore ignore it
                }

                var sharedModules = this.Repository.FindItems<ExportModule>(m => m.ModuleID == other.ModuleID);
                var sharedModule = sharedModules.FirstOrDefault(m => m.LocalId.HasValue);

                if (locals.Count == 0)
                {
                    var local = new ModuleInfo
                    {
                        TabID = localTab.TabID,
                        ModuleID = sharedModule?.LocalId ?? -1,
                        ModuleDefID = moduleDefinition.ModuleDefID,
                        PaneName = other.PaneName,
                        ModuleOrder = other.ModuleOrder,
                        CacheTime = other.CacheTime,
                        Alignment = other.Alignment,
                        Color = other.Color,
                        Border = other.Border,
                        IconFile = other.IconFile,
                        Visibility = (VisibilityState)other.Visibility,
                        ContainerSrc = other.ContainerSrc,
                        DisplayTitle = other.DisplayTitle,
                        DisplayPrint = other.DisplayPrint,
                        DisplaySyndicate = other.DisplaySyndicate,
                        IsWebSlice = other.IsWebSlice,
                        WebSliceTitle = other.WebSliceTitle,
                        WebSliceExpiryDate = other.WebSliceExpiryDate ?? DateTime.MinValue,
                        WebSliceTTL = other.WebSliceTTL ?? -1,
                        IsDeleted = false,
                        CacheMethod = other.CacheMethod,
                        ModuleTitle = other.ModuleTitle,
                        Header = other.Header,
                        Footer = other.Footer,
                        CultureCode = other.CultureCode,

                        // UniqueId = other.UniqueId,
                        UniqueId = DataProvider.Instance().CheckTabModuleUniqueIdExists(other.UniqueId) ? Guid.NewGuid() : other.UniqueId,
                        VersionGuid = other.VersionGuid,
                        DefaultLanguageGuid = other.DefaultLanguageGuid ?? Guid.Empty,
                        LocalizedVersionGuid = other.LocalizedVersionGuid,
                        InheritViewPermissions = other.InheritViewPermissions,
                        IsShareable = other.IsShareable,
                        IsShareableViewOnly = other.IsShareableViewOnly,
                        StartDate = otherModule.StartDate.GetValueOrDefault(DateTime.MinValue),
                        EndDate = otherModule.EndDate.GetValueOrDefault(DateTime.MinValue),
                        PortalID = this._exportImportJob.PortalId,
                    };

                    // Logger.Error($"Local Tab ID={local.TabID}, ModuleID={local.ModuleID}, ModuleDefID={local.ModuleDefID}");
                    try
                    {
                        // this will create up to 2 records:  Module (if it is not already there) and TabModule
                        otherModule.LocalId = this._moduleController.AddModule(local);
                        other.LocalId = local.TabModuleID;
                        this.Repository.UpdateItem(otherModule);
                        allImportedIds.Add(local.ModuleID);

                        // this is not saved upon adding the module
                        if (other.IsDeleted && !otherTab.IsDeleted)
                        {
                            local.IsDeleted = other.IsDeleted;
                            this._moduleController.DeleteTabModule(local.TabID, local.ModuleID, true);
                        }

                        var createdBy = Util.GetUserIdByName(this._exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                        var modifiedBy = Util.GetUserIdByName(this._exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                        this.UpdateTabModuleChangers(local.TabModuleID, createdBy, modifiedBy);

                        if (sharedModule == null)
                        {
                            createdBy = Util.GetUserIdByName(this._exportImportJob, otherModule.CreatedByUserID, otherModule.CreatedByUserName);
                            modifiedBy = Util.GetUserIdByName(this._exportImportJob, otherModule.LastModifiedByUserID, otherModule.LastModifiedByUserName);
                            this.UpdateModuleChangers(local.ModuleID, createdBy, modifiedBy);

                            this._totals.TotalModuleSettings += this.ImportModuleSettings(local, otherModule, isNew);
                            this._totals.TotalModulePermissions += this.ImportModulePermissions(local, otherModule, isNew);
                            this._totals.TotalTabModuleSettings += this.ImportTabModuleSettings(local, other, isNew);

                            if (this._exportDto.IncludeContent)
                            {
                                this._totals.TotalContents += this.ImportPortableContent(localTab.TabID, local, otherModule, isNew);
                            }

                            this.Result.AddLogEntry("Added module", local.ModuleID.ToString());
                        }

                        this.Result.AddLogEntry("Added tab module", local.TabModuleID.ToString());
                        count++;
                    }
                    catch (Exception ex)
                    {
                        this.Result.AddLogEntry("EXCEPTION importing tab module, Module ID=" + local.ModuleID, ex.Message, ReportLevel.Error);
                        Logger.Error(ex);
                    }
                }
                else
                {
                    for (var i = 0; i < locals.Count; i++)
                    {
                        var local = locals.ElementAt(i);
                        var isDeleted = local.IsDeleted;
                        try
                        {
                            var localExpModule = localExportModules.FirstOrDefault(
                                m => m.ModuleID == local.ModuleID && m.FriendlyName == local.ModuleDefinition.FriendlyName);
                            if (localExpModule == null)
                            {
                                local = new ModuleInfo
                                {
                                    TabID = localTab.TabID,
                                    ModuleID = sharedModule?.LocalId ?? -1,
                                    ModuleDefID = moduleDefinition.ModuleDefID,
                                    PaneName = other.PaneName,
                                    ModuleOrder = other.ModuleOrder,
                                    CacheTime = other.CacheTime,
                                    Alignment = other.Alignment,
                                    Color = other.Color,
                                    Border = other.Border,
                                    IconFile = other.IconFile,
                                    Visibility = (VisibilityState)other.Visibility,
                                    ContainerSrc = other.ContainerSrc,
                                    DisplayTitle = other.DisplayTitle,
                                    DisplayPrint = other.DisplayPrint,
                                    DisplaySyndicate = other.DisplaySyndicate,
                                    IsWebSlice = other.IsWebSlice,
                                    WebSliceTitle = other.WebSliceTitle,
                                    WebSliceExpiryDate = other.WebSliceExpiryDate ?? DateTime.MinValue,
                                    WebSliceTTL = other.WebSliceTTL ?? -1,
                                    IsDeleted = other.IsDeleted,
                                    CacheMethod = other.CacheMethod,
                                    ModuleTitle = other.ModuleTitle,
                                    Header = other.Header,
                                    Footer = other.Footer,
                                    CultureCode = other.CultureCode,

                                    // UniqueId = other.UniqueId,
                                    UniqueId = DataProvider.Instance().CheckTabModuleUniqueIdExists(other.UniqueId) ? Guid.NewGuid() : other.UniqueId,
                                    VersionGuid = other.VersionGuid,
                                    DefaultLanguageGuid = other.DefaultLanguageGuid ?? Guid.Empty,
                                    LocalizedVersionGuid = other.LocalizedVersionGuid,
                                    InheritViewPermissions = other.InheritViewPermissions,
                                    IsShareable = other.IsShareable,
                                    IsShareableViewOnly = other.IsShareableViewOnly,
                                    PortalID = this._exportImportJob.PortalId,
                                };

                                // this will create up to 2 records:  Module (if it is not already there) and TabModule
                                otherModule.LocalId = this._moduleController.AddModule(local);
                                other.LocalId = local.TabModuleID;
                                this.Repository.UpdateItem(otherModule);
                                allImportedIds.Add(local.ModuleID);

                                // this is not saved upon updating the module
                                if (isDeleted != other.IsDeleted && !otherTab.IsDeleted)
                                {
                                    local.IsDeleted = other.IsDeleted;
                                    if (other.IsDeleted)
                                    {
                                        this._moduleController.DeleteTabModule(local.TabID, local.ModuleID, true);
                                    }
                                    else
                                    {
                                        this._moduleController.RestoreModule(local);
                                    }
                                }
                            }
                            else
                            {
                                // setting module properties
                                localExpModule.AllTabs = otherModule.AllTabs;
                                localExpModule.StartDate = otherModule.StartDate;
                                localExpModule.EndDate = otherModule.EndDate;
                                localExpModule.InheritViewPermissions = otherModule.InheritViewPermissions;
                                localExpModule.IsDeleted = otherModule.IsDeleted;
                                localExpModule.IsShareable = otherModule.IsShareable;
                                localExpModule.IsShareableViewOnly = otherModule.IsShareableViewOnly;

                                local.AllTabs = otherModule.AllTabs;
                                local.StartDate = otherModule.StartDate ?? DateTime.MinValue;
                                local.EndDate = otherModule.EndDate ?? DateTime.MaxValue;
                                local.InheritViewPermissions = otherModule.InheritViewPermissions ?? true;
                                local.IsDeleted = otherModule.IsDeleted;
                                local.IsShareable = otherModule.IsShareable;
                                local.IsShareableViewOnly = otherModule.IsShareableViewOnly;

                                // setting tab module properties
                                local.AllTabs = otherModule.AllTabs;
                                local.ModuleTitle = other.ModuleTitle;
                                local.Header = other.Header;
                                local.Footer = other.Footer;
                                local.ModuleOrder = other.ModuleOrder;
                                local.PaneName = other.PaneName;
                                local.CacheMethod = other.CacheMethod;
                                local.CacheTime = other.CacheTime;
                                local.Alignment = other.Alignment;
                                local.Color = other.Color;
                                local.Border = other.Border;
                                local.IconFile = other.IconFile;
                                local.Visibility = (VisibilityState)other.Visibility;
                                local.ContainerSrc = other.ContainerSrc;
                                local.DisplayTitle = other.DisplayTitle;
                                local.DisplayPrint = other.DisplayPrint;
                                local.DisplaySyndicate = other.DisplaySyndicate;
                                local.IsShareable = otherModule.IsShareable;
                                local.IsShareableViewOnly = otherModule.IsShareableViewOnly;
                                local.IsWebSlice = other.IsWebSlice;
                                local.WebSliceTitle = other.WebSliceTitle;
                                local.WebSliceExpiryDate = other.WebSliceExpiryDate ?? DateTime.MaxValue;
                                local.WebSliceTTL = other.WebSliceTTL ?? -1;
                                local.VersionGuid = other.VersionGuid;
                                local.DefaultLanguageGuid = other.DefaultLanguageGuid ?? Guid.Empty;
                                local.LocalizedVersionGuid = other.LocalizedVersionGuid;
                                local.CultureCode = other.CultureCode;
                                if (local.UniqueId != other.UniqueId && !DataProvider.Instance().CheckTabModuleUniqueIdExists(other.UniqueId))
                                {
                                    local.UniqueId = other.UniqueId;
                                    this.UpdateModuleUniqueId(local.TabModuleID, other.UniqueId);
                                }

                                // updates both module and tab module db records
                                this.UpdateModuleWithIsDeletedHandling(other, otherModule, local);

                                other.LocalId = local.TabModuleID;
                                otherModule.LocalId = localExpModule.ModuleID;
                                this.Repository.UpdateItem(otherModule);
                                allImportedIds.Add(local.ModuleID);

                                // this is not saved upon updating the module
                                if (isDeleted != other.IsDeleted && !otherTab.IsDeleted)
                                {
                                    local.IsDeleted = other.IsDeleted;
                                    if (other.IsDeleted)
                                    {
                                        this._moduleController.DeleteTabModule(local.TabID, local.ModuleID, true);
                                    }
                                    else
                                    {
                                        this._moduleController.RestoreModule(local);
                                    }
                                }
                            }

                            var createdBy = Util.GetUserIdByName(this._exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                            var modifiedBy = Util.GetUserIdByName(this._exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                            this.UpdateTabModuleChangers(local.TabModuleID, createdBy, modifiedBy);

                            createdBy = Util.GetUserIdByName(this._exportImportJob, otherModule.CreatedByUserID, otherModule.CreatedByUserName);
                            modifiedBy = Util.GetUserIdByName(this._exportImportJob, otherModule.LastModifiedByUserID, otherModule.LastModifiedByUserName);
                            this.UpdateModuleChangers(local.ModuleID, createdBy, modifiedBy);

                            this._totals.TotalTabModuleSettings += this.ImportTabModuleSettings(local, other, isNew);

                            this._totals.TotalModuleSettings += this.ImportModuleSettings(local, otherModule, isNew);
                            this._totals.TotalModulePermissions += this.ImportModulePermissions(local, otherModule, isNew);

                            if (this._exportDto.IncludeContent)
                            {
                                this._totals.TotalContents += this.ImportPortableContent(localTab.TabID, local, otherModule, isNew);
                            }

                            this.Result.AddLogEntry("Updated tab module", local.TabModuleID.ToString());
                            this.Result.AddLogEntry("Updated module", local.ModuleID.ToString());

                            count++;
                        }
                        catch (Exception ex)
                        {
                            this.Result.AddLogEntry("EXCEPTION importing tab module, Module ID=" + local.ModuleID, ex.Message, ReportLevel.Error);
                            Logger.Error(ex);
                        }
                    }
                }
            }

            if (!isNew && this._exportDto.ExportMode == ExportMode.Full &&
                this._importDto.CollisionResolution == CollisionResolution.Overwrite)
            {
                // delete left over tab modules for full import in an existing page
                var unimported = allExistingIds.Distinct().Except(allImportedIds);
                foreach (var moduleId in unimported)
                {
                    try
                    {
                        this._moduleController.DeleteTabModule(localTab.TabID, moduleId, false);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(new Exception($"Delete TabModule Failed: {moduleId}", ex));
                    }

                    this.Result.AddLogEntry("Removed existing tab module", "Module ID=" + moduleId);
                }
            }

            return count;
        }

        /*
            Update Modules.IsDeleted with ExportModule.IsDeleted and not ExportTabModule.IsDeleted.
            ExportTabModule.IsDeleted may different from ExportModule.IsDeleted when Module is deleted.
            Change ModuleInfo.IsDeleted to ExportModule.IsDeleted and reverting to ExportMabModule.IsDeleted after
            updating Modules.
        */
        private void UpdateModuleWithIsDeletedHandling(ExportTabModule exportTabModule, ExportModule exportModule, ModuleInfo importModule)
        {
            importModule.IsDeleted = exportModule.IsDeleted;
            this.ActionInWorkflowlessContext(importModule.TabID, () =>
            {
                this._moduleController.UpdateModule(importModule);
            });
            importModule.IsDeleted = exportTabModule.IsDeleted;
        }

        private bool ModuleOrderMatched(ModuleInfo module, ExportTabModule exportTabModule, IDictionary<int, int> localOrders, IDictionary<int, int> exportOrders)
        {
            return localOrders.ContainsKey(module.ModuleID)
                   && exportOrders.ContainsKey(exportTabModule.ModuleID)
                   && localOrders[module.ModuleID] == exportOrders[exportTabModule.ModuleID];
        }

        private IDictionary<int, int> BuildModuleOrders(IList<ModuleInfo> modules)
        {
            var moduleOrders = new Dictionary<int, int>();
            var moduleOrder = 1;
            Action resetModulOrder = () => { moduleOrder = 1; };
            var lastPane = string.Empty;
            var lastIsDeleted = false;
            foreach (var module in modules.OrderBy(m => m.PaneName.ToLowerInvariant()).ThenBy(m => m.IsDeleted))
            {
                var paneName = module.PaneName.ToLowerInvariant();
                var isDeleted = module.IsDeleted;
                if (paneName != lastPane || isDeleted != lastIsDeleted)
                {
                    resetModulOrder();
                }

                var currentOrder = moduleOrder + 2;

                if (!moduleOrders.ContainsKey(module.ModuleID))
                {
                    moduleOrders.Add(module.ModuleID, currentOrder);
                }

                moduleOrder = currentOrder;

                lastPane = paneName;
                lastIsDeleted = isDeleted;
            }

            return moduleOrders;
        }

        private IDictionary<int, int> BuildModuleOrders(IList<ExportTabModule> modules)
        {
            var moduleOrders = new Dictionary<int, int>();
            var moduleOrder = 1;
            Action resetModulOrder = () => { moduleOrder = 1; };
            var lastPane = string.Empty;
            var lastIsDeleted = false;
            foreach (var module in modules.OrderBy(m => m.PaneName.ToLowerInvariant()).ThenBy(m => m.IsDeleted))
            {
                var paneName = module.PaneName.ToLowerInvariant();
                var isDeleted = module.IsDeleted;
                if (paneName != lastPane || isDeleted != lastIsDeleted)
                {
                    resetModulOrder();
                }

                var currentOrder = moduleOrder + 2;

                if (!moduleOrders.ContainsKey(module.ModuleID))
                {
                    moduleOrders.Add(module.ModuleID, currentOrder);
                }

                moduleOrder = currentOrder;

                lastPane = paneName;
                lastIsDeleted = isDeleted;
            }

            return moduleOrders;
        }

        private int ImportModuleSettings(ModuleInfo localModule, ExportModule otherModule, bool isNew)
        {
            var count = 0;
            var moduleSettings = this.Repository.GetRelatedItems<ExportModuleSetting>(otherModule.Id).ToList();
            foreach (var other in moduleSettings)
            {
                var localValue = isNew ? string.Empty : Convert.ToString(localModule.ModuleSettings[other.SettingName]);
                if (string.IsNullOrEmpty(localValue))
                {
                    this._moduleController.UpdateModuleSetting(localModule.ModuleID, other.SettingName, other.SettingValue);
                    var createdBy = Util.GetUserIdByName(this._exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                    var modifiedBy = Util.GetUserIdByName(this._exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                    this._dataProvider.UpdateSettingRecordChangers("ModuleSettings", "ModuleID",
                        localModule.ModuleID, other.SettingName, createdBy, modifiedBy);
                    this.Result.AddLogEntry("Added module setting", $"{other.SettingName} - {other.ModuleID}");
                    count++;
                }
                else
                {
                    switch (this._importDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            if (localValue != other.SettingValue)
                            {
                                // the next will clear the cache
                                this._moduleController.UpdateModuleSetting(localModule.ModuleID, other.SettingName, other.SettingValue);
                                var createdBy = Util.GetUserIdByName(this._exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                                var modifiedBy = Util.GetUserIdByName(this._exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                                this._dataProvider.UpdateSettingRecordChangers("ModuleSettings", "ModuleID",
                                    localModule.ModuleID, other.SettingName, createdBy, modifiedBy);
                                this.Result.AddLogEntry("Updated module setting", $"{other.SettingName} - {other.ModuleID}");
                                count++;
                            }
                            else
                            {
                                goto case CollisionResolution.Ignore;
                            }

                            break;
                        case CollisionResolution.Ignore:
                            this.Result.AddLogEntry("Ignored module setting", other.SettingName);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(this._importDto.CollisionResolution.ToString());
                    }
                }
            }

            return count;
        }

        private int ImportModulePermissions(ModuleInfo localModule, ExportModule otherModule, bool isNew)
        {
            var count = 0;
            var noRole = Convert.ToInt32(Globals.glbRoleNothing);
            var modulePermissions = this.Repository.GetRelatedItems<ExportModulePermission>(otherModule.Id).ToList();
            var localModulePermissions = isNew
                ? new List<ModulePermissionInfo>()
                : localModule.ModulePermissions.OfType<ModulePermissionInfo>().ToList();
            foreach (var other in modulePermissions)
            {
                var userId = UserController.GetUserByName(this._importDto.PortalId, other.Username)?.UserID;
                var roleId = Util.GetRoleIdByName(this._importDto.PortalId, other.RoleID ?? noRole, other.RoleName);
                var permissionId = DataProvider.Instance().GetPermissionId(other.PermissionCode, other.PermissionKey, other.PermissionName);

                if (permissionId != null)
                {
                    var local = new ModulePermissionInfo
                    {
                        ModuleID = localModule.ModuleID,
                        UserID = Null.NullInteger,
                        RoleID = noRole,
                        RoleName = other.RoleName,
                        Username = other.Username,
                        PermissionKey = other.PermissionKey,
                        PermissionName = other.PermissionName,
                        AllowAccess = other.AllowAccess,
                        PermissionID = permissionId.Value,
                    };
                    if (other.UserID != null && other.UserID > 0 && !string.IsNullOrEmpty(other.Username))
                    {
                        if (userId == null)
                        {
                            continue;
                        }

                        local.UserID = userId.Value;
                    }

                    if (other.RoleID != null && other.RoleID > noRole && !string.IsNullOrEmpty(other.RoleName))
                    {
                        if (roleId == null)
                        {
                            continue;
                        }

                        local.RoleID = roleId.Value;
                    }

                    other.LocalId = localModule.ModulePermissions.Add(local, true);

                    this.Result.AddLogEntry("Added module permission", $"{other.PermissionKey} - {other.PermissionID}");
                    count++;
                }
            }

            if (count > 0)
            {
                ModulePermissionController.SaveModulePermissions(localModule);
            }

            return count;
        }

        private int ImportPortableContent(int tabId, ModuleInfo localModule, ExportModule otherModule, bool isNew)
        {
            var exportedContent = this.Repository.FindItems<ExportModuleContent>(m => m.ModuleID == otherModule.ModuleID).ToList();
            if (exportedContent.Count > 0)
            {
                var moduleDef = ModuleDefinitionController.GetModuleDefinitionByID(localModule.ModuleDefID);
                var desktopModuleInfo = DesktopModuleController.GetDesktopModule(moduleDef.DesktopModuleID, this._exportDto.PortalId);
                if (!string.IsNullOrEmpty(desktopModuleInfo?.BusinessControllerClass))
                {
                    try
                    {
                        var module = this._moduleController.GetModule(localModule.ModuleID, tabId, true);
                        if (!string.IsNullOrEmpty(module.DesktopModule.BusinessControllerClass) && module.DesktopModule.IsPortable)
                        {
                            var businessController = Reflection.CreateObject(module.DesktopModule.BusinessControllerClass, module.DesktopModule.BusinessControllerClass);
                            var controller = businessController as IPortable;
                            if (controller != null)
                            {
                                // Note: there is no chek whether the content exists or not to manage conflict resolution
                                if (isNew || this._importDto.CollisionResolution == CollisionResolution.Overwrite)
                                {
                                    var restoreCount = 0;
                                    var version = DotNetNukeContext.Current.Application.Version.ToString(3);

                                    this.ActionInWorkflowlessContext(tabId, () =>
                                    {
                                        foreach (var moduleContent in exportedContent)
                                        {
                                            if (!moduleContent.IsRestored
                                                || !this._importContentList.Any(i => i.ExportModuleId == otherModule.ModuleID && i.LocalModuleId == localModule.ModuleID))
                                            {
                                                try
                                                {
                                                    this._importContentList.Add(new ImportModuleMapping { ExportModuleId = otherModule.ModuleID, LocalModuleId = localModule.ModuleID });
                                                    var content = moduleContent.XmlContent;
                                                    if (content.IndexOf('\x03') >= 0)
                                                    {
                                                        // exported data contains this character sometimes
                                                        content = content.Replace('\x03', ' ');
                                                    }

                                                    controller.ImportModule(localModule.ModuleID, content, version, this._exportImportJob.CreatedByUserId);
                                                    moduleContent.IsRestored = true;
                                                    this.Repository.UpdateItem(moduleContent);
                                                    restoreCount++;
                                                }
                                                catch (Exception ex)
                                                {
                                                    this.Result.AddLogEntry("Error importing module data, Module ID=" + localModule.ModuleID, ex.Message, ReportLevel.Error);
                                                    Logger.ErrorFormat(
                                                        "ModuleContent: (Module ID={0}). Error: {1}{2}{3}",
                                                        localModule.ModuleID, ex, Environment.NewLine, moduleContent.XmlContent);
                                                }
                                            }
                                        }
                                    });

                                    if (restoreCount > 0)
                                    {
                                        this.Result.AddLogEntry("Added/Updated module content inside Tab ID=" + tabId, "Module ID=" + localModule.ModuleID);
                                        return restoreCount;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Result.AddLogEntry("Error cerating business class type", desktopModuleInfo.BusinessControllerClass, ReportLevel.Error);
                        Logger.Error("Error cerating business class type. " + ex);
                    }
                }
            }

            return 0;
        }

        private void ActionInWorkflowlessContext(int tabId, Action action)
        {
            bool versionEnabledPortalLevel, versionEnabledTabLevel, workflowEnabledPortalLevel, workflowEnabledTabLevel;
            this.DisableVersioning(tabId, out versionEnabledPortalLevel, out versionEnabledTabLevel, out workflowEnabledPortalLevel, out workflowEnabledTabLevel);

            try
            {
                action();
            }
            finally
            {
                this.RestoreVersioning(tabId, versionEnabledPortalLevel, versionEnabledTabLevel, workflowEnabledPortalLevel, workflowEnabledTabLevel);
            }
        }

        private void DisableVersioning(
            int tabId,
            out bool versionEnabledPortalLevel,
            out bool versionEnabledTabLevel,
            out bool workflowEnabledPortalLevel,
            out bool workflowEnabledTabLevel)
        {
            var portalId = this._importDto.PortalId;
            versionEnabledPortalLevel = TabVersionSettings.Instance.IsVersioningEnabled(portalId);
            versionEnabledTabLevel = TabVersionSettings.Instance.IsVersioningEnabled(portalId, tabId);
            TabVersionSettings.Instance.SetEnabledVersioningForPortal(portalId, false);
            TabVersionSettings.Instance.SetEnabledVersioningForTab(tabId, false);

            var workflowSettings = TabWorkflowSettings.Instance;
            workflowEnabledPortalLevel = workflowSettings.IsWorkflowEnabled(portalId);
            workflowEnabledTabLevel = workflowSettings.IsWorkflowEnabled(portalId, tabId);
            workflowSettings.SetWorkflowEnabled(portalId, tabId, false);
        }

        private void RestoreVersioning(
            int tabId,
            bool versionEnabledPortalLevel,
            bool versionEnabledTabLevel,
            bool workflowEnabledPortalLevel,
            bool workflowEnabledTabLevel)
        {
            var portalId = this._importDto.PortalId;
            TabVersionSettings.Instance.SetEnabledVersioningForPortal(portalId, versionEnabledPortalLevel);
            TabVersionSettings.Instance.SetEnabledVersioningForTab(tabId, versionEnabledTabLevel);
            TabWorkflowSettings.Instance.SetWorkflowEnabled(portalId, workflowEnabledPortalLevel);
            TabWorkflowSettings.Instance.SetWorkflowEnabled(portalId, tabId, workflowEnabledTabLevel);
        }

        private int ImportTabModuleSettings(ModuleInfo localTabModule, ExportTabModule otherTabModule, bool isNew)
        {
            var count = 0;
            var tabModuleSettings = this.Repository.GetRelatedItems<ExportTabModuleSetting>(otherTabModule.Id).ToList();
            foreach (var other in tabModuleSettings)
            {
                var localValue = isNew ? string.Empty : Convert.ToString(localTabModule.TabModuleSettings[other.SettingName]);
                if (string.IsNullOrEmpty(localValue))
                {
                    // the next will clear the cache
                    this._moduleController.UpdateTabModuleSetting(localTabModule.TabModuleID, other.SettingName, other.SettingValue);
                    var createdBy = Util.GetUserIdByName(this._exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                    var modifiedBy = Util.GetUserIdByName(this._exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                    this._dataProvider.UpdateSettingRecordChangers("TabModuleSettings", "TabModuleID",
                        localTabModule.TabModuleID, other.SettingName, createdBy, modifiedBy);
                    this.Result.AddLogEntry("Added tab module setting", $"{other.SettingName} - {other.TabModuleID}");
                    count++;
                }
                else
                {
                    switch (this._importDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            if (localValue != other.SettingValue)
                            {
                                // the next will clear the cache
                                this._moduleController.UpdateTabModuleSetting(localTabModule.TabModuleID, other.SettingName, other.SettingValue);
                                var createdBy = Util.GetUserIdByName(this._exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                                var modifiedBy = Util.GetUserIdByName(this._exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                                this._dataProvider.UpdateSettingRecordChangers("TabModuleSettings", "TabModuleID",
                                    localTabModule.TabModuleID, other.SettingName, createdBy, modifiedBy);
                                this.Result.AddLogEntry("Updated tab module setting", $"{other.SettingName} - {other.TabModuleID}");
                                count++;
                            }
                            else
                            {
                                goto case CollisionResolution.Ignore;
                            }

                            break;
                        case CollisionResolution.Ignore:
                            this.Result.AddLogEntry("Ignored module setting", other.SettingName);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(this._importDto.CollisionResolution.ToString());
                    }
                }
            }

            return count;
        }

        private void RepairReferenceTabs(IList<int> referenceTabs, IList<TabInfo> localTabs, IList<ExportTab> exportTabs)
        {
            foreach (var tabId in referenceTabs)
            {
                var localTab = localTabs.FirstOrDefault(t => t.TabID == tabId);
                if (localTab != null && int.TryParse(localTab.Url, out int urlTabId))
                {
                    var exportTab = exportTabs.FirstOrDefault(t => t.TabId == urlTabId);
                    if (exportTab != null && exportTab.LocalId.HasValue)
                    {
                        localTab.Url = exportTab.LocalId.ToString();
                        TabController.Instance.UpdateTab(localTab);
                    }
                }
            }
        }

        private void UpdateTabChangers(int tabId, int createdBy, int modifiedBy)
        {
            this._dataProvider.UpdateRecordChangers("Tabs", "TabID", tabId, createdBy, modifiedBy);
        } // ReSharper disable UnusedMember.Local
        private void UpdateTabPermissionChangers(int tabPermissionId, int createdBy, int modifiedBy)
        {
            this._dataProvider.UpdateRecordChangers("TabPermission", "TabPermissionID", tabPermissionId, createdBy, modifiedBy);
        }

        private void UpdateTabSettingChangers(int tabId, string settingName, int createdBy, int modifiedBy)
        {
            this._dataProvider.UpdateSettingRecordChangers("TabSettings", "TabID", tabId, settingName, createdBy, modifiedBy);
        }

        private void UpdateTabUrlChangers(int tabUrlId, int createdBy, int modifiedBy)
        {
            this._dataProvider.UpdateRecordChangers("TabUrls", "TabUrlID", tabUrlId, createdBy, modifiedBy);
        }

        private void UpdateTabModuleChangers(int tabModuleId, int createdBy, int modifiedBy)
        {
            this._dataProvider.UpdateRecordChangers("TabModules", "TabModuleID", tabModuleId, createdBy, modifiedBy);
        }

        private void UpdateTabModuleSettingsChangers(int tabModuleId, string settingName, int createdBy, int modifiedBy)
        {
            this._dataProvider.UpdateSettingRecordChangers("TabModuleSettings", "TabModuleID", tabModuleId, settingName, createdBy, modifiedBy);
        }

        private void UpdateModuleChangers(int moduleId, int createdBy, int modifiedBy)
        {
            this._dataProvider.UpdateRecordChangers("Modules", "ModuleID", moduleId, createdBy, modifiedBy);
        }

        private void UpdateModulePermissionChangers(int modulePermissionId, int createdBy, int modifiedBy)
        {
            this._dataProvider.UpdateRecordChangers("ModulePermission", "ModulePermissionID", modulePermissionId, createdBy, modifiedBy);
        }

        private void UpdateModuleSettingsChangers(int moduleId, string settingName, int createdBy, int modifiedBy)
        {
            this._dataProvider.UpdateSettingRecordChangers("ModuleSettings", "ModuleID", moduleId, settingName, createdBy, modifiedBy);
        }

        private void UpdateTabUniqueId(int tabId, Guid uniqueId)
        {
            this._dataProvider.UpdateUniqueId("Tabs", "TabID", tabId, uniqueId);
        }

        private void UpdateModuleUniqueId(int tabModuleId, Guid uniqueId)
        {
            this._dataProvider.UpdateUniqueId("TabModules", "TabModuleID", tabModuleId, uniqueId);
        }

        private void ProcessExportPages()
        {
            var selectedPages = this._exportDto.Pages;
            this._totals = string.IsNullOrEmpty(this.CheckPoint.StageData)
                ? new ProgressTotals()
                : JsonConvert.DeserializeObject<ProgressTotals>(this.CheckPoint.StageData);

            var portalId = this._exportImportJob.PortalId;

            var toDate = this._exportImportJob.CreatedOnDate.ToLocalTime();
            var fromDate = (this._exportDto.FromDateUtc ?? Constants.MinDbTime).ToLocalTime();
            var isAllIncluded =
                selectedPages.Any(p => p.TabId == -1 && p.CheckedState == TriCheckedState.CheckedWithAllChildren);

            var allTabs = EntitiesController.Instance.GetPortalTabs(
                portalId,
                this._exportDto.IncludeDeletions, this.IncludeSystem, toDate, fromDate) // ordered by TabID
                .OrderBy(tab => tab.TabPath).ToArray();

            // Update the total items count in the check points. This should be updated only once.
            this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? allTabs.Length : this.CheckPoint.TotalItems;
            if (this.CheckPointStageCallback(this))
            {
                return;
            }

            var progressStep = 100.0 / allTabs.Length;

            this.CheckPoint.TotalItems = this.IncludeSystem || isAllIncluded
                ? allTabs.Length
                : allTabs.Count(otherPg => IsTabIncluded(otherPg, allTabs, selectedPages));

            // Note: We assume no new tabs were added while running; otherwise, some tabs might get skipped.
            for (var index = 0; index < allTabs.Length; index++)
            {
                if (this.CheckCancelled(this._exportImportJob))
                {
                    break;
                }

                var otherPg = allTabs.ElementAt(index);
                if (this._totals.LastProcessedId > index)
                {
                    continue;
                }

                if (this.IncludeSystem || isAllIncluded || IsTabIncluded(otherPg, allTabs, selectedPages))
                {
                    var tab = this._tabController.GetTab(otherPg.TabID, portalId);

                    // Do not export tab which has never been published.
                    if (tab.HasBeenPublished)
                    {
                        var exportPage = this.SaveExportPage(tab);

                        this._totals.TotalTabSettings +=
                            this.ExportTabSettings(exportPage, toDate, fromDate);

                        this._totals.TotalTabPermissions +=
                            this.ExportTabPermissions(exportPage, toDate, fromDate);

                        this._totals.TotalTabUrls +=
                            this.ExportTabUrls(exportPage, toDate, fromDate);

                        this._totals.TotalModules +=
                            this.ExportTabModulesAndRelatedItems(exportPage, toDate, fromDate);

                        this._totals.TotalTabModules +=
                            this.ExportTabModules(exportPage, this._exportDto.IncludeDeletions, toDate, fromDate);

                        this._totals.TotalTabModuleSettings +=
                            this.ExportTabModuleSettings(exportPage, this._exportDto.IncludeDeletions, toDate, fromDate);
                        this._totals.TotalTabs++;
                    }

                    this._totals.LastProcessedId = index;
                }

                this.CheckPoint.Progress += progressStep;
                this.CheckPoint.ProcessedItems++;
                this.CheckPoint.StageData = JsonConvert.SerializeObject(this._totals);
                if (this.CheckPointStageCallback(this))
                {
                    break;
                }
            }

            this.ReportExportTotals();
            this.UpdateTotalProcessedPackages();
        }

        private int ExportTabSettings(ExportTab exportPage, DateTime toDate, DateTime? fromDate)
        {
            var tabSettings = EntitiesController.Instance.GetTabSettings(exportPage.TabId, toDate, fromDate);
            if (tabSettings.Count > 0)
            {
                this.Repository.CreateItems(tabSettings, exportPage.Id);
            }

            return tabSettings.Count;
        }

        private int ExportTabPermissions(ExportTab exportPage, DateTime toDate, DateTime? fromDate)
        {
            if (!this._exportDto.IncludePermissions)
            {
                return 0;
            }

            var tabPermissions = EntitiesController.Instance.GetTabPermissions(exportPage.TabId, toDate, fromDate);
            if (tabPermissions.Count > 0)
            {
                this.Repository.CreateItems(tabPermissions, exportPage.Id);
            }

            return tabPermissions.Count;
        }

        private int ExportTabUrls(ExportTab exportPage, DateTime toDate, DateTime? fromDate)
        {
            var tabUrls = EntitiesController.Instance.GetTabUrls(exportPage.TabId, toDate, fromDate);
            if (tabUrls.Count > 0)
            {
                this.Repository.CreateItems(tabUrls, exportPage.Id);
            }

            return tabUrls.Count;
        }

        private int ExportTabModules(ExportTab exportPage, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            var tabModules = EntitiesController.Instance.GetTabModules(exportPage.TabId, includeDeleted, toDate, fromDate);
            if (tabModules.Count > 0)
            {
                this.Repository.CreateItems(tabModules, exportPage.Id);
            }

            return tabModules.Count;
        }

        private int ExportTabModuleSettings(ExportTab exportPage, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            var tabModuleSettings = EntitiesController.Instance.GetTabModuleSettings(exportPage.TabId, includeDeleted, toDate, fromDate);
            if (tabModuleSettings.Count > 0)
            {
                this.Repository.CreateItems(tabModuleSettings, exportPage.Id);
            }

            return tabModuleSettings.Count;
        }

        private int ExportTabModulesAndRelatedItems(ExportTab exportPage, DateTime toDate, DateTime? fromDate)
        {
            var modules = EntitiesController.Instance.GetModules(exportPage.TabId, this._exportDto.IncludeDeletions, toDate, fromDate);
            if (modules.Count > 0)
            {
                this.Repository.CreateItems(modules, exportPage.Id);
                foreach (var exportModule in modules)
                {
                    this._totals.TotalModuleSettings +=
                        this.ExportModuleSettings(exportModule, toDate, fromDate);

                    this._totals.TotalModulePermissions +=
                        this.ExportModulePermissions(exportModule, toDate, fromDate);

                    if (this._exportDto.IncludeContent)
                    {
                        this._totals.TotalContents +=
                            this.ExportPortableContent(exportPage, exportModule, toDate, fromDate);
                    }

                    this._totals.TotalPackages +=
                        this.ExportModulePackage(exportModule);
                }
            }

            return modules.Count;
        }

        private int ExportModulePackage(ExportModule exportModule)
        {
            if (!this._exportedModuleDefinitions.Contains(exportModule.ModuleDefID) && this._exportDto.IncludeExtensions)
            {
                var packageZipFile = $"{Globals.ApplicationMapPath}{Constants.ExportFolder}{this._exportImportJob.Directory.TrimEnd('\\', '/')}\\{Constants.ExportZipPackages}";
                var moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(exportModule.ModuleDefID);
                var desktopModuleId = moduleDefinition.DesktopModuleID;
                var desktopModule = DesktopModuleController.GetDesktopModule(desktopModuleId, Null.NullInteger);
                var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == desktopModule.PackageID);

                var filePath = InstallerUtil.GetPackageBackupPath(package);
                if (File.Exists(filePath))
                {
                    try
                    {
                        var offset = Path.GetDirectoryName(filePath)?.Length + 1;
                        CompressionUtil.AddFileToArchive(filePath, packageZipFile, offset.GetValueOrDefault(0));

                        this.Repository.CreateItem(
                            new ExportPackage
                            {
                                PackageName = package.Name,
                                Version = package.Version,
                                PackageType = package.PackageType,
                                PackageFileName = InstallerUtil.GetPackageBackupName(package),
                            }, null);

                        this._exportedModuleDefinitions.Add(exportModule.ModuleDefID);
                        return 1;
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        return 0;
                    }
                }
            }

            return 0;
        }

        private int ExportModuleSettings(ExportModule exportModule, DateTime toDate, DateTime? fromDate)
        {
            var moduleSettings = EntitiesController.Instance.GetModuleSettings(exportModule.ModuleID, toDate, fromDate);
            if (moduleSettings.Count > 0)
            {
                this.Repository.CreateItems(moduleSettings, exportModule.Id);
            }

            return moduleSettings.Count;
        }

        private int ExportModulePermissions(ExportModule exportModule, DateTime toDate, DateTime? fromDate)
        {
            var modulePermission = EntitiesController.Instance.GetModulePermissions(exportModule.ModuleID, toDate, fromDate);
            if (modulePermission.Count > 0)
            {
                this.Repository.CreateItems(modulePermission, exportModule.Id);
            }

            return modulePermission.Count;
        }

        // Note: until now there is no use of time range for content
        // ReSharper disable UnusedParameter.Local
        private int ExportPortableContent(ExportTab exportPage, ExportModule exportModule, DateTime toDate, DateTime? fromDat)

        // ReSharper enable UnusedParameter.Local
        {
            // check if module's contnt was exported before
            var existingItems = this.Repository.FindItems<ExportModuleContent>(m => m.ModuleID == exportModule.ModuleID);
            if (!existingItems.Any())
            {
                var moduleDef = ModuleDefinitionController.GetModuleDefinitionByID(exportModule.ModuleDefID);
                var desktopModuleInfo = DesktopModuleController.GetDesktopModule(moduleDef.DesktopModuleID, this._exportDto.PortalId);
                if (!string.IsNullOrEmpty(desktopModuleInfo?.BusinessControllerClass))
                {
                    try
                    {
                        var module = this._moduleController.GetModule(exportModule.ModuleID, exportPage.TabId, true);
                        if (!string.IsNullOrEmpty(module.DesktopModule.BusinessControllerClass) && module.DesktopModule.IsPortable)
                        {
                            try
                            {
                                var businessController = Reflection.CreateObject(
                                    module.DesktopModule.BusinessControllerClass,
                                    module.DesktopModule.BusinessControllerClass);
                                var controller = businessController as IPortable;
                                var content = controller?.ExportModule(module.ModuleID);
                                if (!string.IsNullOrEmpty(content))
                                {
                                    var record = new ExportModuleContent
                                    {
                                        ModuleID = exportModule.ModuleID,
                                        ModuleDefID = exportModule.ModuleDefID,
                                        XmlContent = content,
                                    };

                                    this.Repository.CreateItem(record, exportModule.Id);
                                    return 1;
                                }
                            }
                            catch (Exception e)
                            {
                                this.Result.AddLogEntry("Error exporting module data, Module ID=" + exportModule.ModuleID, e.Message, ReportLevel.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        this.Result.AddLogEntry("Error cerating business class type", desktopModuleInfo.BusinessControllerClass, ReportLevel.Error);
                        Logger.Error("Error cerating business class type. " + ex);
                    }
                }
            }

            return 0;
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
                Tags = tab.GetTags(),
                Description = tab.Description,
                KeyWords = tab.KeyWords,
                IsDeleted = tab.IsDeleted,
                Url = tab.Url,
                SkinSrc = tab.SkinSrc,
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
                StateID = tab.StateID,
            };
            this.Repository.CreateItem(exportPage, null);
            this.Result.AddLogEntry("Exported page", tab.TabName + " (" + tab.TabPath + ")");
            return exportPage;
        }

        private void ReportExportTotals()
        {
            this.ReportTotals("Exported");
        }

        private void ReportImportTotals()
        {
            this.ReportTotals("Imported");
        }

        private void ReportTotals(string prefix)
        {
            this.Result.AddSummary(prefix + " Tabs", this._totals.TotalTabs.ToString());
            this.Result.AddLogEntry(prefix + " Tab Settings", this._totals.TotalTabSettings.ToString());
            this.Result.AddLogEntry(prefix + " Tab Permissions", this._totals.TotalTabPermissions.ToString());
            this.Result.AddLogEntry(prefix + " Tab Urls", this._totals.TotalTabUrls.ToString());
            this.Result.AddLogEntry(prefix + " Modules", this._totals.TotalModules.ToString());
            this.Result.AddLogEntry(prefix + " Module Settings", this._totals.TotalModuleSettings.ToString());
            this.Result.AddLogEntry(prefix + " Module Permissions", this._totals.TotalModulePermissions.ToString());
            this.Result.AddLogEntry(prefix + " Tab Modules", this._totals.TotalTabModules.ToString());
            this.Result.AddLogEntry(prefix + " Tab Module Settings", this._totals.TotalTabModuleSettings.ToString());
            this.Result.AddLogEntry(prefix + " Module Packages", this._totals.TotalPackages.ToString());
        }

        private void UpdateTotalProcessedPackages()
        {
            // HACK: get skin packages checkpoint and add "_totals.TotalPackages" to it
            var packagesCheckpoint = EntitiesController.Instance.GetJobChekpoints(this._exportImportJob.JobId).FirstOrDefault(
                cp => cp.Category == Constants.Category_Packages);
            if (packagesCheckpoint != null)
            {
                // Note: if restart of job occurs, these will report wrong values
                packagesCheckpoint.TotalItems += this._totals.TotalPackages;
                packagesCheckpoint.ProcessedItems += this._totals.TotalPackages;
                EntitiesController.Instance.UpdateJobChekpoint(packagesCheckpoint);
            }
        }

        private int GetLocalStateId(int exportedStateId)
        {
            var exportWorkflowState = this.Repository.GetItem<ExportWorkflowState>(item => item.StateID == exportedStateId);
            var stateId = exportWorkflowState?.LocalId ?? Null.NullInteger;
            if (stateId <= 0)
            {
                return stateId;
            }

            var state = WorkflowStateManager.Instance.GetWorkflowState(stateId);
            if (state == null)
            {
                return -1;
            }

            var workflow = WorkflowManager.Instance.GetWorkflow(state.WorkflowID);
            if (workflow == null)
            {
                return -1;
            }

            return workflow.FirstState.StateID;
        }

        private bool IsTabPublished(TabInfo tab)
        {
            var stateId = tab.StateID;
            if (stateId <= 0)
            {
                return true;
            }

            var state = WorkflowStateManager.Instance.GetWorkflowState(stateId);
            if (state == null)
            {
                return true;
            }

            var workflow = WorkflowManager.Instance.GetWorkflow(state.WorkflowID);
            if (workflow == null)
            {
                return true;
            }

            return workflow.LastState.StateID == stateId;
        }

        private bool IsParentTabPresentInExport(ExportTab exportedTab, IList<ExportTab> exportedTabs, IList<TabInfo> localTabs)
        {
            var isParentPresent = true;
            var parentId = exportedTab.ParentId.GetValueOrDefault(Null.NullInteger);
            int parentIdUrl;
            var isTabUrlParsed = int.TryParse(exportedTab.Url, out parentIdUrl);

            if (parentId != -1 || isTabUrlParsed)
            {
                if (parentId != -1)
                {
                    if (this.IsParentAlreadyCheck(parentId))
                    {
                        return true;
                    }

                    var localParentFound = localTabs.FirstOrDefault(t => t.TabID == parentId);

                    if (localParentFound == null)
                    {
                        var parentFound = exportedTabs.FirstOrDefault(t => t.TabId == parentId);
                        if (parentFound != null)
                        {
                            this.AddToParentSearched(parentFound.TabId, true);
                            isParentPresent = this.IsParentTabPresentInExport(parentFound, exportedTabs, localTabs);
                            return isParentPresent;
                        }
                        else
                        {
                            isParentPresent = false;
                        }
                    }
                    else
                    {
                        return isParentPresent;
                    }
                }

                if (isTabUrlParsed)
                {
                    if (this.IsParentAlreadyCheck(parentIdUrl))
                    {
                        return true;
                    }

                    var localParentFound = localTabs.FirstOrDefault(t => t.TabID == parentIdUrl);

                    if (localParentFound == null)
                    {
                        var parentFound = exportedTabs.FirstOrDefault(t => t.TabId == parentIdUrl);
                        if (parentFound != null)
                        {
                            this.AddToParentSearched(parentFound.TabId, false);
                            isParentPresent = this.IsParentTabPresentInExport(parentFound, exportedTabs, localTabs);
                        }
                        else
                        {
                            isParentPresent = false;
                        }
                    }
                }
            }

            return isParentPresent;
        }

        private bool IsParentAlreadyCheck(int parentId)
        {
            return this._searchedParentTabs.ContainsKey(parentId);
        }

        private void AddToParentSearched(int tabId, bool isParentId)
        {
            if (!this._searchedParentTabs.ContainsKey(tabId))
            {
                this._searchedParentTabs.Add(tabId, isParentId);
            }
        }

        private void UpdateParentInPartialImportTabs(TabInfo localTab, ExportTab parentExportedTab, int portalId, IList<ExportTab> exportTabs, IList<TabInfo> localTabs)
        {
            if (!this._searchedParentTabs.ContainsKey(parentExportedTab.TabId))
            {
                return;
            }

            var parentId = parentExportedTab.TabId;

            var tabsToUpdateGuids = this._partialImportedTabs.Where(t => t.Value == parentId).ToList();

            foreach (var tabGuid in tabsToUpdateGuids)
            {
                var localTabToUpdate = localTabs.FirstOrDefault(t => t.TabID == tabGuid.Key);

                if (localTabToUpdate != null)
                {
                    var tabWithoutParentId = this._tabController.GetTab(localTabToUpdate.TabID, portalId);

                    if (tabWithoutParentId != null)
                    {
                        if (this._searchedParentTabs[parentExportedTab.TabId])
                        {
                            tabWithoutParentId.ParentId = localTab.TabID;

                            var exportedTab = exportTabs.FirstOrDefault(t => t.LocalId == tabGuid.Key);
                            if (exportedTab != null)
                            {
                                tabWithoutParentId.IsVisible = exportedTab.IsVisible;
                            }
                        }
                        else
                        {
                            tabWithoutParentId.Url = localTab.TabID.ToString();
                        }

                        this._tabController.UpdateTab(tabWithoutParentId);
                        this._partialImportedTabs.Remove(tabGuid.Key);
                    }
                }
            }
        }

        private void SetPartialImportSettings(ExportTab exportedTab, TabInfo localTab)
        {
            if (exportedTab.LocalId != null && this._partialImportedTabs.ContainsKey(exportedTab.LocalId.GetValueOrDefault(Null.NullInteger)) && (exportedTab.ParentId.GetValueOrDefault(Null.NullInteger) != -1))
            {
                localTab.ParentId = -1;
                localTab.IsVisible = false;
            }
        }

        private void CheckForPartialImportedTabs(ExportTab tabToExport)
        {
            var exportTabParentId = tabToExport.ParentId.GetValueOrDefault(Null.NullInteger);

            if (exportTabParentId == -1)
            {
                if (int.TryParse(tabToExport.Url, out exportTabParentId))
                {
                    this.AddToPartialImportedTabs(tabToExport.LocalId.GetValueOrDefault(Null.NullInteger), exportTabParentId);
                }
            }
            else
            {
                this.AddToPartialImportedTabs(tabToExport.LocalId.GetValueOrDefault(Null.NullInteger), exportTabParentId);
            }
        }

        private void AddToPartialImportedTabs(int localTabId, int exportTabParentId)
        {
            if (!this._partialImportedTabs.ContainsKey(localTabId) && exportTabParentId != -1)
            {
                this._partialImportedTabs.Add(localTabId, exportTabParentId);
            }
        }

        [JsonObject]
        private class ProgressTotals
        {
            // for Export: this is the TabID
            // for Import: this is the exported DB row ID; not the TabID
            public int LastProcessedId { get; set; }

            public int TotalTabs { get; set; }

            public int TotalTabSettings { get; set; }

            public int TotalTabPermissions { get; set; }

            public int TotalTabUrls { get; set; }

            public int TotalModules { get; set; }

            public int TotalModulePermissions { get; set; }

            public int TotalModuleSettings { get; set; }

            public int TotalContents { get; set; }

            public int TotalPackages { get; set; }

            public int TotalTabModules { get; set; }

            public int TotalTabModuleSettings { get; set; }
        }

        private class ImportModuleMapping
        {
            public int ExportModuleId { get; set; }

            public int LocalModuleId { get; set; }
        }
    }
}
