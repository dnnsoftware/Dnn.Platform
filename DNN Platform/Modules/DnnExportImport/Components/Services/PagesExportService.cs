﻿#region Copyright
//
// DotNetNuke® - http://www.dnnsoftware.com
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
using DotNetNuke.Entities.Modules.Definitions;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Tabs.TabVersions;
using DotNetNuke.Entities.Users;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Services.Installer.Packages;
using Newtonsoft.Json;
using Util = Dnn.ExportImport.Components.Common.Util;
using InstallerUtil = DotNetNuke.Services.Installer.Util;

// ReSharper disable SuggestBaseTypeForParameter

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

        public virtual bool IncludeSystem { get; set; } = false;

        public virtual bool IgnoreParentMatch { get; set; } = false;

        protected ImportDto ImportDto => _importDto;

        private ProgressTotals _totals;
        private DataProvider _dataProvider;
        private ITabController _tabController;
        private IModuleController _moduleController;
        private ExportImportJob _exportImportJob;
        private ImportDto _importDto;
        private ExportDto _exportDto;

        private IList<int> _exportedModuleDefinitions = new List<int>();

        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ExportImportEngine));
        private Dictionary<Guid, int> _partialImportedTabs = new Dictionary<Guid, int>();
        private Dictionary<int, bool> _searchedParentTabs = new Dictionary<int, bool>();

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            if (CheckPoint.Stage > 0) return;
            if (CheckCancelled(exportJob)) return;

            var checkedPages = exportDto.Pages.Where(p => p.CheckedState == TriCheckedState.Checked || p.CheckedState == TriCheckedState.CheckedWithAllChildren);
            if (checkedPages.Any())
            {
                _exportImportJob = exportJob;
                _exportDto = exportDto;
                _tabController = TabController.Instance;
                _moduleController = ModuleController.Instance;
                ProcessExportPages();
            }

            CheckPoint.Progress = 100;
            CheckPoint.Completed = true;
            CheckPoint.Stage++;
            CheckPoint.StageData = null;
            CheckPointStageCallback(this);
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (CheckPoint.Stage > 0) return;
            if (CheckCancelled(importJob)) return;

            _exportImportJob = importJob;
            _importDto = importDto;
            _exportDto = importDto.ExportDto;
            _tabController = TabController.Instance;
            _moduleController = ModuleController.Instance;

            ProcessImportPages();

            CheckPoint.Progress = 100;
            CheckPoint.Completed = true;
            CheckPoint.Stage++;
            CheckPoint.StageData = null;
            CheckPointStageCallback(this);
        }

        public override int GetImportTotal()
        {
            return Repository.GetCount<ExportTab>(x => x.IsSystem == IncludeSystem);
        }

        #region import methods

        private void ProcessImportPages()
        {
            _dataProvider = DataProvider.Instance();
            _totals = string.IsNullOrEmpty(CheckPoint.StageData)
                ? new ProgressTotals()
                : JsonConvert.DeserializeObject<ProgressTotals>(CheckPoint.StageData);

            var portalId = _exportImportJob.PortalId;

            var localTabs = _tabController.GetTabsByPortal(portalId).Values.ToList();

            var exportedTabs = Repository.GetItems<ExportTab>(x => x.IsSystem == (Category == Constants.Category_Templates))
                .OrderBy(t => t.Level).ThenBy(t => t.ParentId).ThenBy(t => t.TabOrder).ToList(); 

            //Update the total items count in the check points. This should be updated only once.
            CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? exportedTabs.Count : CheckPoint.TotalItems;
            if (CheckPointStageCallback(this)) return;
            var progressStep = 100.0 / exportedTabs.OrderByDescending(x => x.Id).Count(x => x.Id < _totals.LastProcessedId);

            var index = 0;
            foreach (var otherTab in exportedTabs)
            {
                if (CheckCancelled(_exportImportJob)) break;
                if (_totals.LastProcessedId > index) continue; // this is the exported DB row ID; not the TabID

                ProcessImportPage(otherTab, exportedTabs, localTabs);

                CheckPoint.ProcessedItems++;
                CheckPoint.Progress += progressStep;
                if (CheckPointStageCallback(this)) break;

                _totals.LastProcessedId = index++;
                CheckPoint.StageData = JsonConvert.SerializeObject(_totals);
            }
            _searchedParentTabs.Clear();
            ReportImportTotals();
        }

        protected virtual void ProcessImportPage(ExportTab otherTab, IList<ExportTab> exportedTabs, IList<TabInfo> localTabs)
        {
            var portalId = _exportImportJob.PortalId;
            var createdBy = Util.GetUserIdByName(_exportImportJob, otherTab.CreatedByUserID, otherTab.CreatedByUserName);
            var modifiedBy = Util.GetUserIdByName(_exportImportJob, otherTab.LastModifiedByUserID, otherTab.LastModifiedByUserName);
            var localTab = localTabs.FirstOrDefault(t => otherTab.UniqueId.Equals(t.UniqueId)) ?? localTabs.FirstOrDefault(t =>
                  otherTab.TabPath.Equals(t.TabPath, StringComparison.InvariantCultureIgnoreCase)
                  && (t.CultureCode ?? "") == (otherTab.CultureCode ?? ""));

            var isParentPresent = IsParentTabPresentInExport(otherTab, exportedTabs, localTabs);
            
            if (localTab != null)
            {
                localTab.TabSettings.Remove("TabImported");
                otherTab.LocalId = localTab.TabID;
                switch (_importDto.CollisionResolution)
                {
                    case CollisionResolution.Ignore:
                        Result.AddLogEntry("Ignored Tab", $"{otherTab.TabName} ({otherTab.TabPath})");
                        break;
                    case CollisionResolution.Overwrite:
                        if (!IsTabPublished(localTab)) return;
                        SetTabData(localTab, otherTab);
                        localTab.StateID = GetLocalStateId(otherTab.StateID);
                        var parentId = IgnoreParentMatch ? otherTab.ParentId.GetValueOrDefault(Null.NullInteger) : TryFindLocalParentTabId(otherTab, exportedTabs, localTabs);
                        if (parentId == -1 && otherTab.ParentId > 0)
                        {
                            if (!isParentPresent)
                            {
                                Result.AddLogEntry("Importing existing tab skipped as its parent was not found", $"{otherTab.TabName} ({otherTab.TabPath})", ReportLevel.Warn);
                                return;
                            }
                            CheckForPartialImportedTabs(otherTab);
                        }
                        var urlTabId = -1;
                        var tabType = Globals.GetURLType(otherTab.Url);
                        if (tabType == TabType.Tab)
                        {
                            urlTabId = TryFindLocalUrlTabId(otherTab, exportedTabs, localTabs);
                            if (urlTabId == -1)
                            {
                                if (!isParentPresent)
                                {
                                    Result.AddLogEntry("Importing existing tab skipped as its referenced page was not found", $"{otherTab.TabName} ({otherTab.TabPath})", ReportLevel.Warn);
                                    return;
                                }
                                CheckForPartialImportedTabs(otherTab);
                            }
                        }

                        // this is not saved when adding the tab; so set it explicitly
                        localTab.IsVisible = otherTab.IsVisible;
                        EntitiesController.Instance.SetTabSpecificData(localTab.TabID, false, localTab.IsVisible);
                        //Try to set the unique id of existing page same as source page unique id, if possible. This will help for future updates etc.
                        if (localTab.UniqueId != otherTab.UniqueId && !DataProvider.Instance().CheckTabUniqueIdExists(otherTab.UniqueId))
                        {
                            localTab.UniqueId = otherTab.UniqueId;
                            UpdateTabUniqueId(localTab.TabID, localTab.UniqueId);
                        }
                        try
                        {
                            localTab.TabPermissions.Clear(); // without this the UpdateTab() could fail
                            localTab.ParentId = parentId;
                            if (tabType == TabType.Tab && urlTabId > -1)
                            {
                                localTab.Url = urlTabId.ToString();
                            }
                            else if (tabType == TabType.Url)
                            {
                                localTab.Url = otherTab.Url;
                            }
                            SetPartialImportSettings(otherTab, localTab);
                            _tabController.UpdateTab(localTab);
                        }
                        catch (Exception ex)
                        {
                            Result.AddLogEntry($"Importing tab '{otherTab.TabName}' exception", ex.Message, ReportLevel.Error);
                            return;
                        }

                        UpdateTabChangers(localTab.TabID, createdBy, modifiedBy);
                        UpdateDefaultLanguageGuid(portalId, localTab, otherTab, exportedTabs);
                        AddTabRelatedItems(localTab, otherTab, false);
                        TriggerImportEvent(localTab);
                        Result.AddLogEntry("Updated Tab", $"{otherTab.TabName} ({otherTab.TabPath})");
                        _totals.TotalTabs++;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(_importDto.CollisionResolution.ToString());
                }
            }
            else
            {
                localTab = new TabInfo { PortalID = portalId };
                SetTabData(localTab, otherTab);
                localTab.StateID = GetLocalStateId(otherTab.StateID);
                var parentId = IgnoreParentMatch ? otherTab.ParentId.GetValueOrDefault(Null.NullInteger) : TryFindLocalParentTabId(otherTab, exportedTabs, localTabs);
                if (parentId == -1 && otherTab.ParentId > 0)
                {
                    if (!isParentPresent)
                    {
                        Result.AddLogEntry("Importing new tab skipped as its parent was not found", $"{otherTab.TabName} ({otherTab.TabPath})", ReportLevel.Warn);
                        return;
                    }
                    CheckForPartialImportedTabs(otherTab);
                }
                var urlTabId = -1;
                var tabType = Globals.GetURLType(otherTab.Url);
                if (tabType == TabType.Tab)
                {
                    urlTabId = TryFindLocalUrlTabId(otherTab, exportedTabs, localTabs);
                    if (urlTabId == -1)
                    {
                        if (!isParentPresent)
                        {
                            Result.AddLogEntry("Importing existing tab skipped as its referenced page was not found", $"{otherTab.TabName} ({otherTab.TabPath})", ReportLevel.Warn);
                            return;
                        }
                        CheckForPartialImportedTabs(otherTab);
                    }
                }

                try
                {
                    localTab.ParentId = parentId;
                    if (tabType == TabType.Tab && urlTabId > -1)
                    {
                        localTab.Url = urlTabId.ToString();
                    }
                    else if (tabType == TabType.Url)
                    {
                        localTab.Url = otherTab.Url;
                    }
                    localTab.UniqueId = Guid.NewGuid();
                    SetPartialImportSettings(otherTab, localTab);
                    otherTab.LocalId = localTab.TabID = _tabController.AddTab(localTab);
                    localTabs.Add(localTab);
                }
                catch (Exception ex)
                {
                    Result.AddLogEntry($"Importing tab '{otherTab.TabName}' exception", ex.Message, ReportLevel.Error);
                    return;
                }

                UpdateTabChangers(localTab.TabID, createdBy, modifiedBy);

                // this is not saved upon updating the tab
                localTab.IsVisible = otherTab.IsVisible;
                EntitiesController.Instance.SetTabSpecificData(localTab.TabID, false, localTab.IsVisible);
                //_tabController.UpdateTab(localTab); // to clear cache
                //Try to set the unique id of existing page same as source page unique id, if possible. This will help for future updates etc.
                if (!DataProvider.Instance().CheckTabUniqueIdExists(otherTab.UniqueId))
                {
                    localTab.UniqueId = otherTab.UniqueId;
                    UpdateTabUniqueId(localTab.TabID, localTab.UniqueId);
                }

                Result.AddLogEntry("Added Tab", $"{otherTab.TabName} ({otherTab.TabPath})");
                _totals.TotalTabs++;
                UpdateDefaultLanguageGuid(portalId, localTab, otherTab, exportedTabs);
                AddTabRelatedItems(localTab, otherTab, true);

                TriggerImportEvent(localTab);
            }
            var portalSettings = new PortalSettings(portalId);

            if (otherTab.IsDeleted)
            {
                _tabController.SoftDeleteTab(localTab.TabID, portalSettings);
            }
            else
            {
                var tab = _tabController.GetTab(localTab.TabID, portalId);
                if (tab.IsDeleted)
                {
                    RestoreTab(tab, portalSettings);
                }
            }
            UpdateParentInPartialImportTabs(localTab, otherTab, portalId, exportedTabs, localTabs);
        }

        public void RestoreTab(TabInfo tab, PortalSettings portalSettings)
        {
            var changeControlStateForTab = TabChangeSettings.Instance.GetChangeControlState(tab.PortalID, tab.TabID);
            if (changeControlStateForTab.IsChangeControlEnabledForTab)
            {
                TabVersionSettings.Instance.SetEnabledVersioningForTab(tab.TabID, false);
                TabWorkflowSettings.Instance.SetWorkflowEnabled(tab.PortalID, tab.TabID, false);
            }

            _tabController.RestoreTab(tab, portalSettings);

            if (changeControlStateForTab.IsChangeControlEnabledForTab)
            {
                TabVersionSettings.Instance.SetEnabledVersioningForTab(tab.TabID, changeControlStateForTab.IsVersioningEnabledForTab);
                TabWorkflowSettings.Instance.SetWorkflowEnabled(tab.PortalID, tab.TabID, changeControlStateForTab.IsWorkflowEnabledForTab);
            }
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
                var defaultLanguagePageLocal = _tabController.GetTab(defaultLanguagePageToImport.LocalId.Value, portalId);

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
                //update tab with import flag, to trigger update event handler.
                if (localTab.TabSettings.ContainsKey("TabImported"))
                {
                    localTab.TabSettings["TabImported"] = "Y";
                }
                else
                {
                    localTab.TabSettings.Add("TabImported", "Y");
                }
                _tabController.UpdateTab(localTab);
                TabController.Instance.DeleteTabSetting(localTab.TabID, "TabImported");
            }
            catch (Exception)
            {
                TabController.Instance.DeleteTabSetting(localTab.TabID, "TabImported");
            }
        }

        private void AddTabRelatedItems(TabInfo localTab, ExportTab otherTab, bool isNew)
        {
            _totals.TotalTabSettings += ImportTabSettings(localTab, otherTab, isNew);
            _totals.TotalTabPermissions += ImportTabPermissions(localTab, otherTab, isNew);
            _totals.TotalTabUrls += ImportTabUrls(localTab, otherTab, isNew);
            _totals.TotalTabModules += ImportTabModulesAndRelatedItems(localTab, otherTab, isNew);
        }

        private int ImportTabSettings(TabInfo localTab, ExportTab otherTab, bool isNew)
        {
            var tabSettings = Repository.GetRelatedItems<ExportTabSetting>(otherTab.Id).ToList();
            foreach (var other in tabSettings)
            {
                var localValue = isNew ? string.Empty : Convert.ToString(localTab.TabSettings[other.SettingName]);
                if (string.IsNullOrEmpty(localValue))
                {
                    _tabController.UpdateTabSetting(localTab.TabID, other.SettingName, other.SettingValue);
                    var createdBy = Util.GetUserIdByName(_exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                    var modifiedBy = Util.GetUserIdByName(_exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                    _dataProvider.UpdateSettingRecordChangers("TabSettings", "TabID",
                        localTab.TabID, other.SettingName, createdBy, modifiedBy);
                    Result.AddLogEntry("Added tab setting", $"{other.SettingName} - {other.TabID}");
                }
                else
                {
                    switch (_importDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            if (localValue != other.SettingValue)
                            {
                                // the next will clear the cache
                                _tabController.UpdateTabSetting(localTab.TabID, other.SettingName, other.SettingValue);
                                var createdBy = Util.GetUserIdByName(_exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                                var modifiedBy = Util.GetUserIdByName(_exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                                _dataProvider.UpdateSettingRecordChangers("TabSettings", "TabID",
                                    localTab.TabID, other.SettingName, createdBy, modifiedBy);
                                Result.AddLogEntry("Updated tab setting", $"{other.SettingName} - {other.TabID}");
                            }
                            else
                            {
                                goto case CollisionResolution.Ignore;
                            }
                            break;
                        case CollisionResolution.Ignore:
                            Result.AddLogEntry("Ignored tab setting", other.SettingName);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(_importDto.CollisionResolution.ToString());
                    }
                }
            }

            return tabSettings.Count;
        }

        private int ImportTabPermissions(TabInfo localTab, ExportTab otherTab, bool isNew)
        {
            if (!_exportDto.IncludePermissions) return 0;
            var noRole = Convert.ToInt32(Globals.glbRoleNothing);
            var count = 0;
            var tabPermissions = Repository.GetRelatedItems<ExportTabPermission>(otherTab.Id).ToList();
            var localTabPermissions = localTab.TabPermissions.OfType<TabPermissionInfo>().ToList();
            foreach (var other in tabPermissions)
            {
                var roleId = Util.GetRoleIdByName(_importDto.PortalId, other.RoleID ?? noRole, other.RoleName);
                var userId = UserController.GetUserByName(_importDto.PortalId, other.Username)?.UserID;

                var local = isNew ? null : localTabPermissions.FirstOrDefault(
                    x => x.PermissionCode == other.PermissionCode && x.PermissionKey == other.PermissionKey
                    && x.PermissionName.Equals(other.PermissionName, StringComparison.InvariantCultureIgnoreCase) &&
                    x.RoleID == roleId && x.UserID == userId);
                var isUpdate = false;
                if (local != null)
                {
                    switch (_importDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            isUpdate = true;
                            break;
                        case CollisionResolution.Ignore:
                            Result.AddLogEntry("Ignored tab permission", other.PermissionKey);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(_importDto.CollisionResolution.ToString());
                    }
                }

                if (isUpdate)
                {
                    //UNDONE: Do we really need to update an existing permission? It won't do anything; permissions are immutable
                    //Result.AddLogEntry("Updated tab permission", other.PermissionKey);
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
                            PermissionID = permissionId.Value
                        };
                        if (other.UserID != null && other.UserID > 0 && !string.IsNullOrEmpty(other.Username))
                        {
                            if (userId == null)
                            {
                                Result.AddLogEntry("Couldn't add tab permission; User is undefined!",
                                    $"{other.PermissionKey} - {other.PermissionID}", ReportLevel.Warn);
                                continue;
                            }
                            local.UserID = userId.Value;
                        }
                        if (other.RoleID != null && other.RoleID > noRole && !string.IsNullOrEmpty(other.RoleName))
                        {
                            if (roleId == null)
                            {
                                Result.AddLogEntry("Couldn't add tab permission; Role is undefined!",
                                    $"{other.PermissionKey} - {other.PermissionID}", ReportLevel.Warn);
                                continue;
                            }
                            local.RoleID = roleId.Value;
                        }
                        localTab.TabPermissions.Add(local, true);
                        //UNDONE: none set; not possible until after saving all tab permissions as donbefore exiting this method
                        //var createdBy = Util.GetUserIdByName(_exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                        //var modifiedBy = Util.GetUserIdByName(_exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                        //UpdateTabPermissionChangers(local.TabPermissionID, createdBy, modifiedBy);
                        Result.AddLogEntry("Added tab permission", $"{other.PermissionKey} - {other.PermissionID}");
                        count++;
                    }
                    else
                    {
                        Result.AddLogEntry("Couldn't add tab permission; Permission is undefined!",
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
            var tabUrls = Repository.GetRelatedItems<ExportTabUrl>(otherTab.Id).ToList();
            var localUrls = localTab.TabUrls;
            foreach (var other in tabUrls)
            {
                var local = isNew ? null : localUrls.FirstOrDefault(url => url.SeqNum == other.SeqNum);
                if (local != null)
                {
                    switch (_importDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            try
                            {
                                local.Url = other.Url;
                                TabController.Instance.SaveTabUrl(local, _importDto.PortalId, true);
                                Result.AddLogEntry("Update Tab Url", other.Url);
                                count++;
                            }
                            catch (Exception ex)
                            {
                                Result.AddLogEntry("EXCEPTION updating tab, Tab ID=" + local.TabId, ex.Message, ReportLevel.Error);
                            }
                            break;
                        case CollisionResolution.Ignore:
                            Result.AddLogEntry("Ignored tab url", other.Url);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(_importDto.CollisionResolution.ToString());
                    }
                }
                else
                {
                    var alias = PortalAliasController.Instance.GetPortalAliasesByPortalId(_importDto.PortalId).FirstOrDefault(a => a.IsPrimary);
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
                        TabController.Instance.SaveTabUrl(local, _importDto.PortalId, true);

                        var createdBy = Util.GetUserIdByName(_exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                        var modifiedBy = Util.GetUserIdByName(_exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                        _dataProvider.UpdateTabUrlChangers(local.TabId, local.SeqNum, createdBy, modifiedBy);

                        Result.AddLogEntry("Added Tab Url", other.Url);
                        count++;
                    }
                    catch (Exception ex)
                    {
                        Result.AddLogEntry("EXCEPTION adding tab, Tab ID=" + local.TabId, ex.Message, ReportLevel.Error);
                    }
                }
            }

            return count;
        }

        private int ImportTabModulesAndRelatedItems(TabInfo localTab, ExportTab otherTab, bool isNew)
        {
            var count = 0;
            var exportedModules = Repository.GetRelatedItems<ExportModule>(otherTab.Id).ToList();
            var exportedTabModules = Repository.GetRelatedItems<ExportTabModule>(otherTab.Id).ToList();
            var localExportModules = isNew ? new List<ExportModule>()
                : EntitiesController.Instance.GetModules(localTab.TabID, true, Constants.MaxDbTime, null).ToList();
            var localTabModules = isNew ? new List<ModuleInfo>() : _moduleController.GetTabModules(localTab.TabID).Values.ToList();
            var allExistingIds = localTabModules.Select(l => l.ModuleID).ToList();
            var allImportedIds = new List<int>();

            foreach (var other in exportedTabModules)
            {
                var locals = localTabModules.Where(
                    m => m.UniqueId == other.UniqueId ||
                        (m.ModuleDefinition.FriendlyName == other.FriendlyName &&
                        m.PaneName == other.PaneName && m.ModuleOrder == other.ModuleOrder)).ToList();

                var otherModule = exportedModules.FirstOrDefault(m => m.ModuleID == other.ModuleID);
                if (otherModule == null) continue; // must not happen

                var moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByFriendlyName(other.FriendlyName);
                if (moduleDefinition == null)
                {
                    Result.AddLogEntry("Error adding tab module, ModuleDef=" + other.FriendlyName,
                        "The modue definition is not present in the system", ReportLevel.Error);
                    continue; // the module is not installed, therefore ignore it
                }

                var sharedModules = Repository.FindItems<ExportModule>(m => m.ModuleID == other.ModuleID);
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
                        //UniqueId = other.UniqueId,
                        UniqueId = DataProvider.Instance().CheckTabModuleUniqueIdExists(other.UniqueId) ? Guid.NewGuid() : other.UniqueId,
                        VersionGuid = other.VersionGuid,
                        DefaultLanguageGuid = other.DefaultLanguageGuid ?? Guid.Empty,
                        LocalizedVersionGuid = other.LocalizedVersionGuid,
                        InheritViewPermissions = other.InheritViewPermissions,
                        IsShareable = other.IsShareable,
                        IsShareableViewOnly = other.IsShareableViewOnly,
                        PortalID = _exportImportJob.PortalId
                    };

                    //Logger.Error($"Local Tab ID={local.TabID}, ModuleID={local.ModuleID}, ModuleDefID={local.ModuleDefID}");
                    try
                    {
                        //this will create up to 2 records:  Module (if it is not already there) and TabModule
                        otherModule.LocalId = _moduleController.AddModule(local);
                        other.LocalId = local.TabModuleID;
                        Repository.UpdateItem(otherModule);
                        allImportedIds.Add(local.ModuleID);

                        // this is not saved upon adding the module
                        if (other.IsDeleted && !otherTab.IsDeleted)
                        {
                            local.IsDeleted = other.IsDeleted;
                            _moduleController.DeleteTabModule(local.TabID, local.ModuleID, true);
                        }

                        var createdBy = Util.GetUserIdByName(_exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                        var modifiedBy = Util.GetUserIdByName(_exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                        UpdateTabModuleChangers(local.TabModuleID, createdBy, modifiedBy);

                        if (sharedModule == null)
                        {
                            createdBy = Util.GetUserIdByName(_exportImportJob, otherModule.CreatedByUserID, otherModule.CreatedByUserName);
                            modifiedBy = Util.GetUserIdByName(_exportImportJob, otherModule.LastModifiedByUserID, otherModule.LastModifiedByUserName);
                            UpdateModuleChangers(local.ModuleID, createdBy, modifiedBy);

                            _totals.TotalModuleSettings += ImportModuleSettings(local, otherModule, isNew);
                            _totals.TotalModulePermissions += ImportModulePermissions(local, otherModule, isNew);
                            _totals.TotalTabModuleSettings += ImportTabModuleSettings(local, other, isNew);

                            if (_exportDto.IncludeContent)
                            {
                                _totals.TotalContents += ImportPortableContent(localTab.TabID, local, otherModule, isNew);
                            }

                            Result.AddLogEntry("Added module", local.ModuleID.ToString());
                        }

                        Result.AddLogEntry("Added tab module", local.TabModuleID.ToString());
                        count++;
                    }
                    catch (Exception ex)
                    {
                        Result.AddLogEntry("EXCEPTION importing tab module, Module ID=" + local.ModuleID, ex.Message, ReportLevel.Error);
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
                                    //UniqueId = other.UniqueId,
                                    UniqueId = DataProvider.Instance().CheckTabModuleUniqueIdExists(other.UniqueId) ? Guid.NewGuid() : other.UniqueId,
                                    VersionGuid = other.VersionGuid,
                                    DefaultLanguageGuid = other.DefaultLanguageGuid ?? Guid.Empty,
                                    LocalizedVersionGuid = other.LocalizedVersionGuid,
                                    InheritViewPermissions = other.InheritViewPermissions,
                                    IsShareable = other.IsShareable,
                                    IsShareableViewOnly = other.IsShareableViewOnly,
                                    PortalID = _exportImportJob.PortalId
                                };

                                //this will create up to 2 records:  Module (if it is not already there) and TabModule
                                otherModule.LocalId = _moduleController.AddModule(local);
                                other.LocalId = local.TabModuleID;
                                Repository.UpdateItem(otherModule);
                                allImportedIds.Add(local.ModuleID);

                                // this is not saved upon updating the module
                                if (isDeleted != other.IsDeleted && !otherTab.IsDeleted)
                                {
                                    local.IsDeleted = other.IsDeleted;
                                    if (other.IsDeleted)
                                        _moduleController.DeleteTabModule(local.TabID, local.ModuleID, true);
                                    else
                                        _moduleController.RestoreModule(local);
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
                                local.IsDeleted = other.IsDeleted;
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
                                    UpdateModuleUniqueId(local.TabModuleID, other.UniqueId);
                                }

                                // updates both module and tab module db records
                                _moduleController.UpdateModule(local);
                                other.LocalId = local.TabModuleID;
                                otherModule.LocalId = localExpModule.ModuleID;
                                Repository.UpdateItem(otherModule);
                                allImportedIds.Add(local.ModuleID);

                                // this is not saved upon updating the module
                                if (isDeleted != other.IsDeleted && !otherTab.IsDeleted)
                                {
                                    local.IsDeleted = other.IsDeleted;
                                    if (other.IsDeleted)
                                        _moduleController.DeleteTabModule(local.TabID, local.ModuleID, true);
                                    else
                                        _moduleController.RestoreModule(local);
                                }

                            }

                            var createdBy = Util.GetUserIdByName(_exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                            var modifiedBy = Util.GetUserIdByName(_exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                            UpdateTabModuleChangers(local.TabModuleID, createdBy, modifiedBy);

                            createdBy = Util.GetUserIdByName(_exportImportJob, otherModule.CreatedByUserID, otherModule.CreatedByUserName);
                            modifiedBy = Util.GetUserIdByName(_exportImportJob, otherModule.LastModifiedByUserID, otherModule.LastModifiedByUserName);
                            UpdateModuleChangers(local.ModuleID, createdBy, modifiedBy);

                            _totals.TotalTabModuleSettings += ImportTabModuleSettings(local, other, isNew);

                            _totals.TotalModuleSettings += ImportModuleSettings(local, otherModule, isNew);
                            _totals.TotalModulePermissions += ImportModulePermissions(local, otherModule, isNew);

                            if (_exportDto.IncludeContent)
                            {
                                _totals.TotalContents += ImportPortableContent(localTab.TabID, local, otherModule, isNew);
                            }

                            Result.AddLogEntry("Updated tab module", local.TabModuleID.ToString());
                            Result.AddLogEntry("Updated module", local.ModuleID.ToString());

                            count++;
                        }
                        catch (Exception ex)
                        {
                            Result.AddLogEntry("EXCEPTION importing tab module, Module ID=" + local.ModuleID, ex.Message, ReportLevel.Error);
                            Logger.Error(ex);
                        }
                    }
                }
            }

            if (!isNew && _exportDto.ExportMode == ExportMode.Full &&
                _importDto.CollisionResolution == CollisionResolution.Overwrite)
            {
                // delete left over tab modules for full import in an existing page
                var unimported = allExistingIds.Distinct().Except(allImportedIds);
                foreach (var moduleId in unimported)
                {
                    _moduleController.DeleteTabModule(localTab.TabID, moduleId, false);
                    Result.AddLogEntry("Removed existing tab module", "Module ID=" + moduleId);
                }
            }

            return count;
        }

        private int ImportModuleSettings(ModuleInfo localModule, ExportModule otherModule, bool isNew)
        {
            var count = 0;
            var moduleSettings = Repository.GetRelatedItems<ExportModuleSetting>(otherModule.Id).ToList();
            foreach (var other in moduleSettings)
            {
                var localValue = isNew ? string.Empty : Convert.ToString(localModule.ModuleSettings[other.SettingName]);
                if (string.IsNullOrEmpty(localValue))
                {
                    _moduleController.UpdateModuleSetting(localModule.ModuleID, other.SettingName, other.SettingValue);
                    var createdBy = Util.GetUserIdByName(_exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                    var modifiedBy = Util.GetUserIdByName(_exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                    _dataProvider.UpdateSettingRecordChangers("ModuleSettings", "ModuleID",
                        localModule.ModuleID, other.SettingName, createdBy, modifiedBy);
                    Result.AddLogEntry("Added module setting", $"{other.SettingName} - {other.ModuleID}");
                    count++;
                }
                else
                {
                    switch (_importDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            if (localValue != other.SettingValue)
                            {
                                // the next will clear the cache
                                _moduleController.UpdateModuleSetting(localModule.ModuleID, other.SettingName, other.SettingValue);
                                var createdBy = Util.GetUserIdByName(_exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                                var modifiedBy = Util.GetUserIdByName(_exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                                _dataProvider.UpdateSettingRecordChangers("ModuleSettings", "ModuleID",
                                    localModule.ModuleID, other.SettingName, createdBy, modifiedBy);
                                Result.AddLogEntry("Updated module setting", $"{other.SettingName} - {other.ModuleID}");
                                count++;
                            }
                            else
                            {
                                goto case CollisionResolution.Ignore;
                            }
                            break;
                        case CollisionResolution.Ignore:
                            Result.AddLogEntry("Ignored module setting", other.SettingName);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(_importDto.CollisionResolution.ToString());
                    }
                }
            }

            return count;
        }

        private int ImportModulePermissions(ModuleInfo localModule, ExportModule otherModule, bool isNew)
        {
            var count = 0;
            var noRole = Convert.ToInt32(Globals.glbRoleNothing);
            var modulePermissions = Repository.GetRelatedItems<ExportModulePermission>(otherModule.Id).ToList();
            var localModulePermissions = isNew
                ? new List<ModulePermissionInfo>()
                : localModule.ModulePermissions.OfType<ModulePermissionInfo>().ToList();
            foreach (var other in modulePermissions)
            {
                var userId = UserController.GetUserByName(_importDto.PortalId, other.Username)?.UserID;
                var roleId = Util.GetRoleIdByName(_importDto.PortalId, other.RoleID ?? noRole, other.RoleName);
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
                        PermissionID = permissionId.Value
                    };
                    if (other.UserID != null && other.UserID > 0 && !string.IsNullOrEmpty(other.Username))
                    {
                        if (userId == null)
                            continue;
                        local.UserID = userId.Value;
                    }
                    if (other.RoleID != null && other.RoleID > noRole && !string.IsNullOrEmpty(other.RoleName))
                    {
                        if (roleId == null)
                            continue;
                        local.RoleID = roleId.Value;
                    }

                    other.LocalId = localModule.ModulePermissions.Add(local, true);

                    Result.AddLogEntry("Added module permission", $"{other.PermissionKey} - {other.PermissionID}");
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
            var exportedContent = Repository.FindItems<ExportModuleContent>(m => m.ModuleID == otherModule.ModuleID).ToList();
            if (exportedContent.Count > 0)
            {
                var moduleDef = ModuleDefinitionController.GetModuleDefinitionByID(localModule.ModuleDefID);
                var desktopModuleInfo = DesktopModuleController.GetDesktopModule(moduleDef.DesktopModuleID, _exportDto.PortalId);
                if (!string.IsNullOrEmpty(desktopModuleInfo?.BusinessControllerClass))
                {
                    try
                    {
                        var module = _moduleController.GetModule(localModule.ModuleID, tabId, true);
                        if (!string.IsNullOrEmpty(module.DesktopModule.BusinessControllerClass) && module.DesktopModule.IsPortable)
                        {
                            var businessController = Reflection.CreateObject(module.DesktopModule.BusinessControllerClass, module.DesktopModule.BusinessControllerClass);
                            var controller = businessController as IPortable;
                            if (controller != null)
                            {
                                //Note: there is no chek whether the content exists or not to manage conflict resolution
                                if (isNew || _importDto.CollisionResolution == CollisionResolution.Overwrite)
                                {
                                    var restoreCount = 0;
                                    var version = DotNetNukeContext.Current.Application.Version.ToString(3);

                                    bool tabVersionsEnabled;
                                    bool tabWorkflowEnabled;
                                    DisableVersioning(tabId, out tabVersionsEnabled, out tabWorkflowEnabled);

                                    try
                                    {
                                        foreach (var moduleContent in exportedContent)
                                        {
                                            if (!moduleContent.IsRestored)
                                            {
                                                try
                                                {
                                                    var content = moduleContent.XmlContent;
                                                    if (content.IndexOf('\x03') >= 0)
                                                    {
                                                        // exported data contains this character sometimes
                                                        content = content.Replace('\x03', ' ');
                                                    }

                                                    controller.ImportModule(localModule.ModuleID, content, version, _exportImportJob.CreatedByUserId);
                                                    moduleContent.IsRestored = true;
                                                    Repository.UpdateItem(moduleContent);
                                                    restoreCount++;
                                                }
                                                catch (Exception ex)
                                                {
                                                    Result.AddLogEntry("Error importing module data, Module ID=" + localModule.ModuleID, ex.Message, ReportLevel.Error);
                                                    Logger.ErrorFormat("ModuleContent: (Module ID={0}). Error: {1}{2}{3}",
                                                        localModule.ModuleID, ex, Environment.NewLine, moduleContent.XmlContent);
                                                }
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        RestoreVersioning(tabId, tabVersionsEnabled, tabWorkflowEnabled);
                                    }

                                    if (restoreCount > 0)
                                    {
                                        Result.AddLogEntry("Added/Updated module content inside Tab ID=" + tabId, "Module ID=" + localModule.ModuleID);
                                        return restoreCount;
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Result.AddLogEntry("Error cerating business class type", desktopModuleInfo.BusinessControllerClass, ReportLevel.Error);
                        Logger.Error("Error cerating business class type. " + ex);
                    }
                }
            }
            return 0;
        }

        private void DisableVersioning(int tabId, out bool tabVersionsEnabled, out bool tabWorkflowEnabled)
        {
            var portalId = _importDto.PortalId;
            tabWorkflowEnabled = TabVersionSettings.Instance.IsVersioningEnabled(portalId, tabId);
            TabVersionSettings.Instance.SetEnabledVersioningForPortal(portalId, false);
            TabVersionSettings.Instance.SetEnabledVersioningForTab(tabId, false);

            var workflowSettings = TabWorkflowSettings.Instance;
            tabVersionsEnabled = workflowSettings.IsWorkflowEnabled(portalId, tabId);
            workflowSettings.SetWorkflowEnabled(portalId, tabId, false);
        }

        private void RestoreVersioning(int tabId, bool tabVersionsEnabled, bool tabWorkflowEnabled)
        {
            var portalId = _importDto.PortalId;
            TabVersionSettings.Instance.SetEnabledVersioningForPortal(portalId, tabVersionsEnabled);
            TabVersionSettings.Instance.SetEnabledVersioningForTab(tabId, tabVersionsEnabled);
            TabWorkflowSettings.Instance.SetWorkflowEnabled(portalId, tabId, tabWorkflowEnabled);
        }

        private int ImportTabModuleSettings(ModuleInfo localTabModule, ExportTabModule otherTabModule, bool isNew)
        {
            var count = 0;
            var tabModuleSettings = Repository.GetRelatedItems<ExportTabModuleSetting>(otherTabModule.Id).ToList();
            foreach (var other in tabModuleSettings)
            {
                var localValue = isNew ? "" : Convert.ToString(localTabModule.TabModuleSettings[other.SettingName]);
                if (string.IsNullOrEmpty(localValue))
                {
                    // the next will clear the cache
                    _moduleController.UpdateTabModuleSetting(localTabModule.TabModuleID, other.SettingName, other.SettingValue);
                    var createdBy = Util.GetUserIdByName(_exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                    var modifiedBy = Util.GetUserIdByName(_exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                    _dataProvider.UpdateSettingRecordChangers("TabModuleSettings", "TabModuleID",
                        localTabModule.TabModuleID, other.SettingName, createdBy, modifiedBy);
                    Result.AddLogEntry("Added tab module setting", $"{other.SettingName} - {other.TabModuleID}");
                    count++;
                }
                else
                {
                    switch (_importDto.CollisionResolution)
                    {
                        case CollisionResolution.Overwrite:
                            if (localValue != other.SettingValue)
                            {
                                // the next will clear the cache
                                _moduleController.UpdateTabModuleSetting(localTabModule.TabModuleID, other.SettingName, other.SettingValue);
                                var createdBy = Util.GetUserIdByName(_exportImportJob, other.CreatedByUserID, other.CreatedByUserName);
                                var modifiedBy = Util.GetUserIdByName(_exportImportJob, other.LastModifiedByUserID, other.LastModifiedByUserName);
                                _dataProvider.UpdateSettingRecordChangers("TabModuleSettings", "TabModuleID",
                                    localTabModule.TabModuleID, other.SettingName, createdBy, modifiedBy);
                                Result.AddLogEntry("Updated tab module setting", $"{other.SettingName} - {other.TabModuleID}");
                                count++;
                            }
                            else
                            {
                                goto case CollisionResolution.Ignore;
                            }
                            break;
                        case CollisionResolution.Ignore:
                            Result.AddLogEntry("Ignored module setting", other.SettingName);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException(_importDto.CollisionResolution.ToString());
                    }
                }
            }

            return count;
        }

        private static int TryFindLocalUrlTabId(ExportTab exportedTab, IEnumerable<ExportTab> exportedTabs, IEnumerable<TabInfo> localTabs)
        {
            int urlTabId;
            if (int.TryParse(exportedTab.Url, out urlTabId))
            {
                return TryFindLocalTabId(exportedTab, exportedTabs, localTabs, urlTabId);
            }
            return -1;
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
                            return localTab.TabID;
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
                            && (t.CultureCode ?? "") == (exportedTab.CultureCode ?? ""));
                        if (localTab != null)
                            return localTab.TabID;
                    }
                }
            }

            return -1;
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
            //localTab.IsDeleted = otherTab.IsDeleted; // DO NOT enable this; leave this to other logic
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
            //localTab.UniqueId = otherTab.UniqueId;
            localTab.VersionGuid = otherTab.VersionGuid;
            localTab.LocalizedVersionGuid = otherTab.LocalizedVersionGuid;
            localTab.Level = otherTab.Level;
            localTab.TabPath = otherTab.TabPath;
            localTab.HasBeenPublished = otherTab.HasBeenPublished;
            localTab.IsSystem = otherTab.IsSystem;
        }

        #region Methods for updating CreatedBy and ModifiedBy of various tables

        private void UpdateTabChangers(int tabId, int createdBy, int modifiedBy)
        {
            _dataProvider.UpdateRecordChangers("Tabs", "TabID", tabId, createdBy, modifiedBy);
        }

        // ReSharper disable UnusedMember.Local
        private void UpdateTabPermissionChangers(int tabPermissionId, int createdBy, int modifiedBy)
        {
            _dataProvider.UpdateRecordChangers("TabPermission", "TabPermissionID", tabPermissionId, createdBy, modifiedBy);
        }

        private void UpdateTabSettingChangers(int tabId, string settingName, int createdBy, int modifiedBy)
        {
            _dataProvider.UpdateSettingRecordChangers("TabSettings", "TabID", tabId, settingName, createdBy, modifiedBy);
        }

        private void UpdateTabUrlChangers(int tabUrlId, int createdBy, int modifiedBy)
        {
            _dataProvider.UpdateRecordChangers("TabUrls", "TabUrlID", tabUrlId, createdBy, modifiedBy);
        }

        private void UpdateTabModuleChangers(int tabModuleId, int createdBy, int modifiedBy)
        {
            _dataProvider.UpdateRecordChangers("TabModules", "TabModuleID", tabModuleId, createdBy, modifiedBy);
        }

        private void UpdateTabModuleSettingsChangers(int tabModuleId, string settingName, int createdBy, int modifiedBy)
        {
            _dataProvider.UpdateSettingRecordChangers("TabModuleSettings", "TabModuleID", tabModuleId, settingName, createdBy, modifiedBy);
        }

        private void UpdateModuleChangers(int moduleId, int createdBy, int modifiedBy)
        {
            _dataProvider.UpdateRecordChangers("Modules", "ModuleID", moduleId, createdBy, modifiedBy);
        }

        private void UpdateModulePermissionChangers(int modulePermissionId, int createdBy, int modifiedBy)
        {
            _dataProvider.UpdateRecordChangers("ModulePermission", "ModulePermissionID", modulePermissionId, createdBy, modifiedBy);
        }

        private void UpdateModuleSettingsChangers(int moduleId, string settingName, int createdBy, int modifiedBy)
        {
            _dataProvider.UpdateSettingRecordChangers("ModuleSettings", "ModuleID", moduleId, settingName, createdBy, modifiedBy);
        }

        private void UpdateTabUniqueId(int tabId, Guid uniqueId)
        {
            _dataProvider.UpdateUniqueId("Tabs", "TabID", tabId, uniqueId);
        }

        private void UpdateModuleUniqueId(int tabModuleId, Guid uniqueId)
        {
            _dataProvider.UpdateUniqueId("TabModules", "TabModuleID", tabModuleId, uniqueId);
        }


        #endregion

        #endregion

        #region export methods

        private void ProcessExportPages()
        {
            var selectedPages = _exportDto.Pages;
            _totals = string.IsNullOrEmpty(CheckPoint.StageData)
                ? new ProgressTotals()
                : JsonConvert.DeserializeObject<ProgressTotals>(CheckPoint.StageData);

            var portalId = _exportImportJob.PortalId;

            var toDate = _exportImportJob.CreatedOnDate.ToLocalTime();
            var fromDate = (_exportDto.FromDateUtc ?? Constants.MinDbTime).ToLocalTime();
            var isAllIncluded =
                selectedPages.Any(p => p.TabId == -1 && p.CheckedState == TriCheckedState.CheckedWithAllChildren);

            var allTabs = EntitiesController.Instance.GetPortalTabs(portalId,
                    _exportDto.IncludeDeletions, IncludeSystem, toDate, fromDate) // ordered by TabID
                .OrderBy(tab => tab.TabPath).ToArray();

            //Update the total items count in the check points. This should be updated only once.
            CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? allTabs.Length : CheckPoint.TotalItems;
            if (CheckPointStageCallback(this)) return;
            var progressStep = 100.0 / allTabs.Length;

            CheckPoint.TotalItems = IncludeSystem || isAllIncluded
                ? allTabs.Length
                : allTabs.Count(otherPg => IsTabIncluded(otherPg, allTabs, selectedPages));

            //Note: We assume no new tabs were added while running; otherwise, some tabs might get skipped.
            for (var index = 0; index < allTabs.Length; index++)
            {
                if (CheckCancelled(_exportImportJob)) break;

                var otherPg = allTabs.ElementAt(index);
                if (_totals.LastProcessedId > index) continue;

                if (IncludeSystem || isAllIncluded || IsTabIncluded(otherPg, allTabs, selectedPages))
                {
                    var tab = _tabController.GetTab(otherPg.TabID, portalId);
                    //Do not export tab which has never been published.
                    if (tab.HasBeenPublished)
                    {
                        var exportPage = SaveExportPage(tab);

                        _totals.TotalTabSettings +=
                            ExportTabSettings(exportPage, toDate, fromDate);

                        _totals.TotalTabPermissions +=
                            ExportTabPermissions(exportPage, toDate, fromDate);

                        _totals.TotalTabUrls +=
                            ExportTabUrls(exportPage, toDate, fromDate);

                        _totals.TotalModules +=
                            ExportTabModulesAndRelatedItems(exportPage, toDate, fromDate);

                        _totals.TotalTabModules +=
                            ExportTabModules(exportPage, _exportDto.IncludeDeletions, toDate, fromDate);

                        _totals.TotalTabModuleSettings +=
                            ExportTabModuleSettings(exportPage, _exportDto.IncludeDeletions,  toDate, fromDate);
                        _totals.TotalTabs++;
                    }
                    _totals.LastProcessedId = index;
                }

                CheckPoint.Progress += progressStep;
                CheckPoint.ProcessedItems++;
                CheckPoint.StageData = JsonConvert.SerializeObject(_totals);
                if (CheckPointStageCallback(this)) break;
            }

            ReportExportTotals();
            UpdateTotalProcessedPackages();
        }

        private int ExportTabSettings(ExportTab exportPage, DateTime toDate, DateTime? fromDate)
        {
            var tabSettings = EntitiesController.Instance.GetTabSettings(exportPage.TabId, toDate, fromDate);
            if (tabSettings.Count > 0)
                Repository.CreateItems(tabSettings, exportPage.Id);
            return tabSettings.Count;
        }

        private int ExportTabPermissions(ExportTab exportPage, DateTime toDate, DateTime? fromDate)
        {
            if (!_exportDto.IncludePermissions) return 0;

            var tabPermissions = EntitiesController.Instance.GetTabPermissions(exportPage.TabId, toDate, fromDate);
            if (tabPermissions.Count > 0)
                Repository.CreateItems(tabPermissions, exportPage.Id);
            return tabPermissions.Count;
        }

        private int ExportTabUrls(ExportTab exportPage, DateTime toDate, DateTime? fromDate)
        {
            var tabUrls = EntitiesController.Instance.GetTabUrls(exportPage.TabId, toDate, fromDate);
            if (tabUrls.Count > 0)
                Repository.CreateItems(tabUrls, exportPage.Id);
            return tabUrls.Count;
        }

        private int ExportTabModules(ExportTab exportPage, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            var tabModules = EntitiesController.Instance.GetTabModules(exportPage.TabId, includeDeleted, toDate, fromDate);
            if (tabModules.Count > 0)
                Repository.CreateItems(tabModules, exportPage.Id);
            return tabModules.Count;
        }

        private int ExportTabModuleSettings(ExportTab exportPage, bool includeDeleted, DateTime toDate, DateTime? fromDate)
        {
            var tabModuleSettings = EntitiesController.Instance.GetTabModuleSettings(exportPage.TabId, includeDeleted, toDate, fromDate);
            if (tabModuleSettings.Count > 0)
                Repository.CreateItems(tabModuleSettings, exportPage.Id);
            return tabModuleSettings.Count;
        }

        private int ExportTabModulesAndRelatedItems(ExportTab exportPage, DateTime toDate, DateTime? fromDate)
        {
            var modules = EntitiesController.Instance.GetModules(exportPage.TabId, _exportDto.IncludeDeletions, toDate, fromDate);
            if (modules.Count > 0)
            {
                Repository.CreateItems(modules, exportPage.Id);
                foreach (var exportModule in modules)
                {
                    _totals.TotalModuleSettings +=
                        ExportModuleSettings(exportModule, toDate, fromDate);

                    _totals.TotalModulePermissions +=
                        ExportModulePermissions(exportModule, toDate, fromDate);

                    if (_exportDto.IncludeContent)
                    {
                        _totals.TotalContents +=
                            ExportPortableContent(exportPage, exportModule, toDate, fromDate);
                    }

                    _totals.TotalPackages +=
                        ExportModulePackage(exportModule);
                }
            }

            return modules.Count;
        }

        private int ExportModulePackage(ExportModule exportModule)
        {
            if (!_exportedModuleDefinitions.Contains(exportModule.ModuleDefID))
            {
                var packageZipFile = $"{Globals.ApplicationMapPath}{Constants.ExportFolder}{_exportImportJob.Directory.TrimEnd('\\', '/')}\\{Constants.ExportZipPackages}";
                var moduleDefinition = ModuleDefinitionController.GetModuleDefinitionByID(exportModule.ModuleDefID);
                var desktopModuleId = moduleDefinition.DesktopModuleID;
                var desktopModule = DesktopModuleController.GetDesktopModule(desktopModuleId, Null.NullInteger);
                var package = PackageController.Instance.GetExtensionPackage(Null.NullInteger, p => p.PackageID == desktopModule.PackageID);

                var filePath = InstallerUtil.GetPackageBackupPath(package);
                if (File.Exists(filePath))
                {
                    var offset = Path.GetDirectoryName(filePath)?.Length + 1;
                    CompressionUtil.AddFileToArchive(filePath, packageZipFile, offset.GetValueOrDefault(0));

                    Repository.CreateItem(new ExportPackage
                    {
                        PackageName = package.Name,
                        Version = package.Version,
                        PackageType = package.PackageType,
                        PackageFileName = InstallerUtil.GetPackageBackupName(package)
                    }, null);

                    _exportedModuleDefinitions.Add(exportModule.ModuleDefID);
                    return 1;
                }
            }
            return 0;
        }

        private int ExportModuleSettings(ExportModule exportModule, DateTime toDate, DateTime? fromDate)
        {
            var moduleSettings = EntitiesController.Instance.GetModuleSettings(exportModule.ModuleID, toDate, fromDate);
            if (moduleSettings.Count > 0)
                Repository.CreateItems(moduleSettings, exportModule.Id);
            return moduleSettings.Count;
        }

        private int ExportModulePermissions(ExportModule exportModule, DateTime toDate, DateTime? fromDate)
        {
            var modulePermission = EntitiesController.Instance.GetModulePermissions(exportModule.ModuleID, toDate, fromDate);
            if (modulePermission.Count > 0)
                Repository.CreateItems(modulePermission, exportModule.Id);
            return modulePermission.Count;
        }

        // Note: until now there is no use of time range for content
        // ReSharper disable UnusedParameter.Local
        private int ExportPortableContent(ExportTab exportPage, ExportModule exportModule, DateTime toDate, DateTime? fromDat)
        // ReSharper enable UnusedParameter.Local
        {
            // check if module's contnt was exported before
            var existingItems = Repository.FindItems<ExportModuleContent>(m => m.ModuleID == exportModule.ModuleID);
            if (!existingItems.Any())
            {
                var moduleDef = ModuleDefinitionController.GetModuleDefinitionByID(exportModule.ModuleDefID);
                var desktopModuleInfo = DesktopModuleController.GetDesktopModule(moduleDef.DesktopModuleID, _exportDto.PortalId);
                if (!string.IsNullOrEmpty(desktopModuleInfo?.BusinessControllerClass))
                {
                    try
                    {
                        var module = _moduleController.GetModule(exportModule.ModuleID, exportPage.TabId, true);
                        if (!string.IsNullOrEmpty(module.DesktopModule.BusinessControllerClass) && module.DesktopModule.IsPortable)
                        {
                            try
                            {
                                var businessController = Reflection.CreateObject(module.DesktopModule.BusinessControllerClass,
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

                                    Repository.CreateItem(record, exportModule.Id);
                                    return 1;
                                }
                            }
                            catch (Exception e)
                            {
                                Result.AddLogEntry("Error exporting module data, Module ID=" + exportModule.ModuleID, e.Message, ReportLevel.Error);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Result.AddLogEntry("Error cerating business class type", desktopModuleInfo.BusinessControllerClass, ReportLevel.Error);
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
                StateID = tab.StateID
            };
            Repository.CreateItem(exportPage, null);
            Result.AddLogEntry("Exported page", tab.TabName + " (" + tab.TabPath + ")");
            return exportPage;
        }

        #endregion

        #region helper methods

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

        private void ReportExportTotals()
        {
            ReportTotals("Exported");
        }

        private void ReportImportTotals()
        {
            ReportTotals("Imported");
        }

        private void ReportTotals(string prefix)
        {
            Result.AddSummary(prefix + " Tabs", _totals.TotalTabs.ToString());
            Result.AddLogEntry(prefix + " Tab Settings", _totals.TotalTabSettings.ToString());
            Result.AddLogEntry(prefix + " Tab Permissions", _totals.TotalTabPermissions.ToString());
            Result.AddLogEntry(prefix + " Tab Urls", _totals.TotalTabUrls.ToString());
            Result.AddLogEntry(prefix + " Modules", _totals.TotalModules.ToString());
            Result.AddLogEntry(prefix + " Module Settings", _totals.TotalModuleSettings.ToString());
            Result.AddLogEntry(prefix + " Module Permissions", _totals.TotalModulePermissions.ToString());
            Result.AddLogEntry(prefix + " Tab Modules", _totals.TotalTabModules.ToString());
            Result.AddLogEntry(prefix + " Tab Module Settings", _totals.TotalTabModuleSettings.ToString());
            Result.AddLogEntry(prefix + " Module Packages", _totals.TotalPackages.ToString());
        }

        private void UpdateTotalProcessedPackages()
        {
            //HACK: get skin packages checkpoint and add "_totals.TotalPackages" to it
            var packagesCheckpoint = EntitiesController.Instance.GetJobChekpoints(_exportImportJob.JobId).FirstOrDefault(
                cp => cp.Category == Constants.Category_Packages);
            if (packagesCheckpoint != null)
            {
                //Note: if restart of job occurs, these will report wrong values
                packagesCheckpoint.TotalItems += _totals.TotalPackages;
                packagesCheckpoint.ProcessedItems += _totals.TotalPackages;
                EntitiesController.Instance.UpdateJobChekpoint(packagesCheckpoint);
            }
        }

        private int GetLocalStateId(int exportedStateId)
        {
            var exportWorkflowState = Repository.GetItem<ExportWorkflowState>(item => item.StateID == exportedStateId);
            var stateId = exportWorkflowState?.LocalId ?? Null.NullInteger;
            if (stateId <= 0) return stateId;
            var state = WorkflowStateManager.Instance.GetWorkflowState(stateId);
            if (state == null) return -1;
            var workflow = WorkflowManager.Instance.GetWorkflow(state.WorkflowID);
            if (workflow == null) return -1;
            return workflow.FirstState.StateID;
        }

        private bool IsTabPublished(TabInfo tab)
        {
            var stateId = tab.StateID;
            if (stateId <= 0) return true;
            var state = WorkflowStateManager.Instance.GetWorkflowState(stateId);
            if (state == null) return true;
            var workflow = WorkflowManager.Instance.GetWorkflow(state.WorkflowID);
            if (workflow == null) return true;
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
                    if (IsParentAlreadyCheck(parentId))
                    {
                        return true;
                    }

                    var localParentFound = localTabs.FirstOrDefault(t => t.TabID == parentId);

                    if (localParentFound == null)
                    {
                        var parentFound = exportedTabs.FirstOrDefault(t => t.TabId == parentId);
                        if (parentFound != null)
                        {
                            AddToParentSearched(parentFound.TabId, true);
                            isParentPresent = IsParentTabPresentInExport(parentFound, exportedTabs, localTabs);
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
                    if (IsParentAlreadyCheck(parentIdUrl))
                    {
                        return true;
                    }

                    var localParentFound = localTabs.FirstOrDefault(t => t.TabID == parentIdUrl);

                    if (localParentFound == null)
                    {
                        var parentFound = exportedTabs.FirstOrDefault(t => t.TabId == parentIdUrl);
                        if (parentFound != null)
                        {
                            AddToParentSearched(parentFound.TabId, false);
                            isParentPresent = IsParentTabPresentInExport(parentFound, exportedTabs, localTabs);
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
            return _searchedParentTabs.ContainsKey(parentId);
        }

        private void AddToParentSearched(int tabId, bool isParentId)
        {
            if (!_searchedParentTabs.ContainsKey(tabId))
            {
                _searchedParentTabs.Add(tabId, isParentId);
            }
        }

        private void UpdateParentInPartialImportTabs(TabInfo localTab, ExportTab parentExportedTab, int portalId, IList<ExportTab> exportTabs, IList<TabInfo> localTabs)
        {
            if (!_searchedParentTabs.ContainsKey(parentExportedTab.TabId))
            {
                return;
            }

            var parentId = parentExportedTab.TabId;

            var tabsToUpdateGuids = _partialImportedTabs.Where(t => t.Value == parentId).ToList();

            foreach (var tabGuid in tabsToUpdateGuids)
            {
                var localTabToUpdate = localTabs.FirstOrDefault(t => t.LocalizedVersionGuid == tabGuid.Key);

                if (localTabToUpdate != null)
                {
                    var tabWithoutParentId = _tabController.GetTab(localTabToUpdate.TabID, portalId);

                    if (tabWithoutParentId != null)
                    {
                        if (_searchedParentTabs[parentExportedTab.TabId])
                        {
                            tabWithoutParentId.ParentId = localTab.TabID;

                            var exportedTab = exportTabs.FirstOrDefault(t => t.LocalizedVersionGuid == tabGuid.Key);
                            if (exportedTab != null)
                            {
                                tabWithoutParentId.IsVisible = exportedTab.IsVisible;
                            }
                        }
                        else
                        {
                            tabWithoutParentId.Url = localTab.TabID.ToString();
                        }

                        _tabController.UpdateTab(tabWithoutParentId);
                        _partialImportedTabs.Remove(tabGuid.Key);
                    }
                }
            }
        }

        private void SetPartialImportSettings(ExportTab exportedTab, TabInfo localTab)
        {
            if (_partialImportedTabs.ContainsKey(exportedTab.LocalizedVersionGuid) && (exportedTab.ParentId.GetValueOrDefault(Null.NullInteger) != -1))
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
                    AddToPartialImportedTabs(tabToExport.LocalizedVersionGuid, exportTabParentId);
                }
            }
            else
            {
                AddToPartialImportedTabs(tabToExport.LocalizedVersionGuid, exportTabParentId);
            }
        }

        private void AddToPartialImportedTabs(Guid localizedVersionGuid, int exportTabParentId)
        {
            if (!_partialImportedTabs.ContainsKey(localizedVersionGuid) && exportTabParentId != -1)
            {
                _partialImportedTabs.Add(localizedVersionGuid, exportTabParentId);
            }
        }

        #endregion

        #region private classes

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
        #endregion
    }
}