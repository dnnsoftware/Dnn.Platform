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

using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs.TabVersions.Exceptions;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    public class TabVersionBuilder : ServiceLocator<ITabVersionBuilder, TabVersionBuilder>, ITabVersionBuilder
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(TabVersionBuilder));
        private const int DefaultVersionNumber = 1;

        #region Members
        private readonly ITabController _tabController;
        private readonly IModuleController _moduleController;
        private readonly ITabVersionSettings _tabVersionSettings;
        private readonly ITabVersionController _tabVersionController;
        private readonly ITabVersionDetailController _tabVersionDetailController;
        private readonly PortalSettings _portalSettings;
        #endregion

        #region Constructor
        public TabVersionBuilder()
        {
            _tabController = TabController.Instance;
            _moduleController = ModuleController.Instance;
            _tabVersionSettings = TabVersionSettings.Instance;
            _tabVersionController = TabVersionController.Instance;
            _tabVersionDetailController = TabVersionDetailController.Instance;
            _portalSettings = PortalSettings.Current;
        }
        #endregion

        #region Public Methods

        public void SetupFirstVersionForExistingTab(int portalId, int tabId)
        {
            if (!_tabVersionSettings.IsVersioningEnabled(portalId, tabId))
            {
                return;
            }

            // Check if already exist at least one version for the tab
            if (_tabVersionController.GetTabVersions(tabId).Any())
            {
                return;
            }

            var tab = _tabController.GetTab(tabId, portalId);
            var modules = _moduleController.GetTabModules(tabId).Where(m => m.Value.IsDeleted == false).Select(m => m.Value).ToArray();
            
            // Check if the page has modules
            if (!modules.Any())
            {
                return;
            }

            CreateFirstTabVersion(tabId, tab, modules);
        }
        
        public void Publish(int portalId, int tabId, int createdByUserId)
        {
            var tabVersion = GetUnPublishedVersion(tabId);            
            if (tabVersion == null)
            {                
                throw new InvalidOperationException(String.Format(Localization.GetString("TabHasNotAnUnpublishedVersion", Localization.ExceptionsResourceFile), tabId));
            }
            if (tabVersion.IsPublished)
            {
                throw new InvalidOperationException(String.Format(Localization.GetString("TabVersionAlreadyPublished", Localization.ExceptionsResourceFile), tabId, tabVersion.Version));
            }

            var previousPublishVersion = GetCurrentVersion(tabId);
            PublishVersion(portalId, tabId, createdByUserId, tabVersion);

            if (!_tabVersionSettings.IsVersioningEnabled(portalId, tabId)
                && previousPublishVersion != null)
            {
                ForceDeleteVersion(tabId, previousPublishVersion.Version);
            }
        }

        public void Discard(int tabId, int createdByUserId)
        {
            var tabVersion = GetUnPublishedVersion(tabId);            
            if (tabVersion == null)
            {
                throw new InvalidOperationException(String.Format(Localization.GetString("TabHasNotAnUnpublishedVersion", Localization.ExceptionsResourceFile), tabId));
            }
            if (tabVersion.IsPublished)
            {
                throw new InvalidOperationException(String.Format(Localization.GetString("TabVersionAlreadyPublished", Localization.ExceptionsResourceFile), tabId, tabVersion.Version));
            }
            DiscardVersion(tabId, tabVersion);
        }

        private void DiscardVersion(int tabId, TabVersion tabVersion)
        {
            var unPublishedDetails = _tabVersionDetailController.GetTabVersionDetails(tabVersion.TabVersionId);

            var currentPublishedVersion = GetCurrentVersion(tabId);
            TabVersionDetail[] publishedChanges = null;

            if (currentPublishedVersion != null)
            {
                publishedChanges = GetVersionModulesDetails(tabId, GetCurrentVersion(tabId).Version).ToArray();
            }
            
            foreach (var unPublishedDetail in unPublishedDetails)
            {
                if (publishedChanges == null)
                {
                    DiscardDetailWithoutPublishedTabVersions(tabId, unPublishedDetail);
                }
                else
                {
                    DiscardDetailWithPublishedTabVersions(tabId, unPublishedDetail, publishedChanges);
                }
            }

            _tabVersionController.DeleteTabVersion(tabId, tabVersion.TabVersionId);
        }

        public void DeleteVersion(int tabId, int createdByUserId, int version)
        {
            CheckVersioningEnabled(tabId);

            ForceDeleteVersion(tabId, version);
        }

        
        public TabVersion RollBackVesion(int tabId, int createdByUserId, int version)
        {
            CheckVersioningEnabled(tabId);

            if (GetUnPublishedVersion(tabId) != null)
            {                
                throw new InvalidOperationException(String.Format(Localization.GetString("TabVersionCannotBeRolledBack_UnpublishedVersionExists", Localization.ExceptionsResourceFile), tabId, version));
            }

            var lastTabVersion = _tabVersionController.GetTabVersions(tabId).OrderByDescending(tv => tv.Version).FirstOrDefault();
            if (lastTabVersion == null || lastTabVersion.Version == version)
            {
                throw new InvalidOperationException(String.Format(Localization.GetString("TabVersionCannotBeRolledBack_LastVersion", Localization.ExceptionsResourceFile), tabId, version));
            }

            var publishedDetails = GetVersionModulesDetails(tabId, lastTabVersion.Version).ToArray();

            var rollbackDetails = CopyVersionDetails(GetVersionModulesDetails(tabId, version)).ToArray();
            var newVersion = CreateNewVersion(tabId, createdByUserId);
            
            //Save Reset detail
            _tabVersionDetailController.SaveTabVersionDetail(GetResetTabVersionDetail(newVersion), createdByUserId);
            
            foreach (var rollbackDetail in rollbackDetails)
            {
                rollbackDetail.TabVersionId = newVersion.TabVersionId;
                try
                {
                    rollbackDetail.ModuleVersion = RollBackDetail(tabId, rollbackDetail);
                }
                catch (DnnTabVersionException e)
                {
                    Logger.Error(string.Format("There was a problem making rollbak of the module {0}. Message: {1}.", rollbackDetail.ModuleId, e.Message));
                    continue;
                }
                _tabVersionDetailController.SaveTabVersionDetail(rollbackDetail, createdByUserId);

                //Check if restoring version contains modules to restore
                if (publishedDetails.All(tv => tv.ModuleId != rollbackDetail.ModuleId))
                {
                    RestoreModuleInfo(tabId, rollbackDetail);
                }
                else
                {
                    UpdateModuleOrder(tabId, rollbackDetail);
                }               
            }
            
            //Check if current version contains modules not existing in restoring version 
            foreach (var publishedDetail in publishedDetails.Where(publishedDetail => rollbackDetails.All(tvd => tvd.ModuleId != publishedDetail.ModuleId)))
            {
                _moduleController.DeleteTabModule(tabId, publishedDetail.ModuleId, true);
            }
            
            // Publish Version
            return PublishVersion(GetCurrentPortalId(), tabId, createdByUserId, newVersion);
        }

        public TabVersion CreateNewVersion(int tabId, int createdByUserId)
        {
            return CreateNewVersion(GetCurrentPortalId(), tabId, createdByUserId);
        }

        public TabVersion CreateNewVersion(int portalid, int tabId, int createdByUserId)
        {
            if (portalid == Null.NullInteger)
            {
                throw new InvalidOperationException(Localization.GetString("TabVersioningNotEnabled", Localization.ExceptionsResourceFile));
            }

            SetupFirstVersionForExistingTab(portalid, tabId);

            DeleteOldestVersionIfTabHasMaxNumberOfVersions(portalid, tabId);
            try
            {
                return _tabVersionController.CreateTabVersion(tabId, createdByUserId);
            }
            catch (InvalidOperationException e)
            {
                Services.Exceptions.Exceptions.LogException(e);
                throw new InvalidOperationException(String.Format(Localization.GetString("TabVersionCannotBeCreated_UnpublishedVersionAlreadyExistsConcurrencyProblem", Localization.ExceptionsResourceFile), tabId), e);
            }
            catch (SqlException sqlException)
            {
                Services.Exceptions.Exceptions.LogException(sqlException);
                throw new InvalidOperationException(String.Format(Localization.GetString("TabVersionCannotBeCreated_UnpublishedVersionAlreadyExistsConcurrencyProblem", Localization.ExceptionsResourceFile), tabId));
            }
        }

        public IEnumerable<ModuleInfo> GetUnPublishedVersionModules(int tabId)
        {
            var unPublishedVersion = GetUnPublishedVersion(tabId);
            if (unPublishedVersion == null)
            {
                return CBO.FillCollection<ModuleInfo>(DataProvider.Instance().GetTabModules(tabId));
            }

            return GetVersionModules(tabId, unPublishedVersion.TabVersionId);
        }

        public TabVersion GetCurrentVersion(int tabId, bool ignoreCache = false)
        {
            return _tabVersionController.GetTabVersions(tabId, ignoreCache)
                .Where(tv => tv.IsPublished).OrderByDescending(tv => tv.CreatedOnDate).FirstOrDefault();
        }

        public TabVersion GetUnPublishedVersion(int tabId)
        {
            return _tabVersionController.GetTabVersions(tabId, true)
                .SingleOrDefault(tv => !tv.IsPublished);
        }

        public IEnumerable<ModuleInfo> GetCurrentModules(int tabId)
        {
            var cacheKey = string.Format(DataCache.PublishedTabModuleCacheKey, tabId);
            return CBO.GetCachedObject<IEnumerable<ModuleInfo>>(new CacheItemArgs(cacheKey,
                                                                    DataCache.PublishedTabModuleCacheTimeOut,
                                                                    DataCache.PublishedTabModuleCachePriority),
                                                                    c => GetCurrentModulesInternal(tabId));
        }
        
        public IEnumerable<ModuleInfo> GetVersionModules(int tabId, int version)
        {
            return ConvertToModuleInfo(GetVersionModulesDetails(tabId, version), tabId);
        }

        public int GetModuleContentLatestVersion(ModuleInfo module)
        {
            var versionableController = GetVersionableController(module);
            return versionableController != null ? versionableController.GetLatestVersion(module.ModuleID) : DefaultVersionNumber;
        }
        #endregion

        #region Private Methods
        private IEnumerable<ModuleInfo> GetCurrentModulesInternal(int tabId)
        {
            var versioningEnabled = _portalSettings != null &&
                                    _tabVersionSettings.IsVersioningEnabled(_portalSettings.PortalId, tabId);
            if (!versioningEnabled)
            {
                return CBO.FillCollection<ModuleInfo>(DataProvider.Instance().GetTabModules(tabId));
            }
            
            // If versionins is enabled but the tab doesn't have versions history, 
            // then it's a tab never edited after version enabling.
            var tabWithoutVersions = !_tabVersionController.GetTabVersions(tabId).Any();
            if (tabWithoutVersions)
            {
                return CBO.FillCollection<ModuleInfo>(DataProvider.Instance().GetTabModules(tabId));
            }

            var currentVersion = GetCurrentVersion(tabId);
            if (currentVersion == null)
            {
                //Only when a tab is on a first version and it is not published, the currentVersion object can be null
                return new List<ModuleInfo>();
            }

            return GetVersionModules(tabId, currentVersion.Version);
        }

        private void DiscardDetailWithoutPublishedTabVersions(int tabId, TabVersionDetail unPublishedDetail)
        {
            if (unPublishedDetail.ModuleVersion != Null.NullInteger)
            {
                DiscardDetail(tabId, unPublishedDetail);
            }
            _moduleController.DeleteTabModule(tabId, unPublishedDetail.ModuleId, true);
        }

        private void DiscardDetailWithPublishedTabVersions(int tabId, TabVersionDetail unPublishedDetail,
            TabVersionDetail[] publishedChanges)
        {
            if (unPublishedDetail.Action == TabVersionDetailAction.Deleted)
            {
                var restoredModuleDetail = publishedChanges.SingleOrDefault(tv => tv.ModuleId == unPublishedDetail.ModuleId);
                if (restoredModuleDetail != null)
                {
                    RestoreModuleInfo(tabId, restoredModuleDetail);
                }
                return;
            }

            if (publishedChanges.All(tv => tv.ModuleId != unPublishedDetail.ModuleId))
            {
                _moduleController.DeleteTabModule(tabId, unPublishedDetail.ModuleId, true);
                return;
            }

            if (unPublishedDetail.Action == TabVersionDetailAction.Modified)
            {
                var publishDetail = publishedChanges.SingleOrDefault(tv => tv.ModuleId == unPublishedDetail.ModuleId);
                if (publishDetail.PaneName != unPublishedDetail.PaneName ||
                    publishDetail.ModuleOrder != unPublishedDetail.ModuleOrder)
                {
                    _moduleController.UpdateModuleOrder(tabId, publishDetail.ModuleId, publishDetail.ModuleOrder,
                        publishDetail.PaneName);
                }

                if (unPublishedDetail.ModuleVersion != Null.NullInteger)
                {
                    DiscardDetail(tabId, unPublishedDetail);
                }
            }
        }

        private void ForceDeleteVersion(int tabId, int version)
        {
            var unpublishedVersion = GetUnPublishedVersion(tabId);
            if (unpublishedVersion != null 
                && unpublishedVersion.Version == version)
            {
                throw new InvalidOperationException(
                    String.Format(
                        Localization.GetString("TabVersionCannotBeDeleted_UnpublishedVersion",
                            Localization.ExceptionsResourceFile), tabId, version));
            }

            var tabVersions = _tabVersionController.GetTabVersions(tabId).OrderByDescending(tv => tv.Version);
            if (tabVersions.Count() <= 1)
            {
                throw new InvalidOperationException(
                    String.Format(
                        Localization.GetString("TabVersionCannotBeDiscarded_OnlyOneVersion", Localization.ExceptionsResourceFile),
                        tabId, version));
            }

            var versionToDelete = tabVersions.ElementAt(0);

            // check if the version to delete if the latest published one
            if (versionToDelete.Version == version)
            {
                var restoreMaxNumberOfVersions = false;
                var portalId = _portalSettings.PortalId;
                var maxNumberOfVersions = _tabVersionSettings.GetMaxNumberOfVersions(portalId);

                // If we already have reached the maxNumberOfVersions we need to extend to 1 this limit to allow the tmp version
                if (tabVersions.Count() == maxNumberOfVersions)
                {
                    _tabVersionSettings.SetMaxNumberOfVersions(portalId, maxNumberOfVersions + 1);
                    restoreMaxNumberOfVersions = true;
                }

                try
                {
                    var previousVersion = tabVersions.ElementAt(1);
                    var previousVersionDetails = GetVersionModulesDetails(tabId, previousVersion.Version).ToArray();
                    var versionToDeleteDetails =
                        _tabVersionDetailController.GetTabVersionDetails(versionToDelete.TabVersionId);

                    foreach (var versionToDeleteDetail in versionToDeleteDetails)
                    {
                        switch (versionToDeleteDetail.Action)
                        {
                            case TabVersionDetailAction.Added:
                                _moduleController.DeleteTabModule(tabId, versionToDeleteDetail.ModuleId, true);
                                break;
                            case TabVersionDetailAction.Modified:
                                var peviousVersionDetail =
                                    previousVersionDetails.SingleOrDefault(tv => tv.ModuleId == versionToDeleteDetail.ModuleId);
                                if (peviousVersionDetail != null &&
                                    (peviousVersionDetail.PaneName != versionToDeleteDetail.PaneName ||
                                      peviousVersionDetail.ModuleOrder != versionToDeleteDetail.ModuleOrder))
                                {
                                    _moduleController.UpdateModuleOrder(tabId, peviousVersionDetail.ModuleId,
                                        peviousVersionDetail.ModuleOrder, peviousVersionDetail.PaneName);
                                }

                                if (versionToDeleteDetail.ModuleVersion != Null.NullInteger)
                                {
                                    DiscardDetail(tabId, versionToDeleteDetail);
                                }
                                break;
                        }
                    }
                    DeleteTmpVersionIfExists(tabId, versionToDelete);
                    _tabVersionController.DeleteTabVersion(tabId, versionToDelete.TabVersionId);
                    ManageModulesToBeRestored(tabId, previousVersionDetails);
                    _moduleController.ClearCache(tabId);
                }
                finally
                {
                    if (restoreMaxNumberOfVersions)
                    {
                        _tabVersionSettings.SetMaxNumberOfVersions(portalId, maxNumberOfVersions);
                    }
                }
            }
            else
            {
                for (var i = 1; i < tabVersions.Count(); i++)
                {
                    if (tabVersions.ElementAt(i).Version == version)
                    {
                        CreateSnapshotOverVersion(tabId, tabVersions.ElementAtOrDefault(i - 1), tabVersions.ElementAt(i));
                        _tabVersionController.DeleteTabVersion(tabId, tabVersions.ElementAt(i).TabVersionId);
                        break;
                    }
                }
            }
        }

        private void ManageModulesToBeRestored(int tabId, TabVersionDetail[] versionDetails)
        {
            foreach (var detail in versionDetails)
            {
                var module = _moduleController.GetModule(detail.ModuleId, tabId, true);
                if (module.IsDeleted)
                {
                    _moduleController.RestoreModule(module);    
                }
            }
        }

        private void DeleteTmpVersionIfExists(int tabId, TabVersion versionToDelete)
        {
            var tmpVersion = _tabVersionController.GetTabVersions(tabId).OrderByDescending(tv => tv.Version).FirstOrDefault();
            if (tmpVersion != null && tmpVersion.Version > versionToDelete.Version)
            {
                _tabVersionController.DeleteTabVersion(tabId, tmpVersion.TabVersionId);
            }
        }
        
        private void DeleteOldestVersionIfTabHasMaxNumberOfVersions(int portalId, int tabId)
        {
            var maxVersionsAllowed = GetMaxNumberOfVersions(portalId);
            var tabVersionsOrdered = _tabVersionController.GetTabVersions(tabId).OrderByDescending(tv => tv.Version);

            if (tabVersionsOrdered.Count() < maxVersionsAllowed) return;

            //The last existing version is going to be deleted, therefore we need to add the snapshot to the previous one
            var snapShotTabVersion = tabVersionsOrdered.ElementAtOrDefault(maxVersionsAllowed - 2);
            CreateSnapshotOverVersion(tabId, snapShotTabVersion);
            DeleteOldVersions(tabVersionsOrdered, snapShotTabVersion);
        }
      
        private int GetMaxNumberOfVersions(int portalId)
        {            
            return _tabVersionSettings.GetMaxNumberOfVersions(portalId);
        }

        private void UpdateModuleOrder(int tabId, TabVersionDetail detailToRestore)
        {
            var restoredModule = _moduleController.GetModule(detailToRestore.ModuleId, tabId, true);
            if (restoredModule != null)
            {
                UpdateModuleInfoOrder(restoredModule, detailToRestore);
            }
        }

        private void UpdateModuleInfoOrder(ModuleInfo module, TabVersionDetail detailToRestore)
        {
            module.PaneName = detailToRestore.PaneName;
            module.ModuleOrder = detailToRestore.ModuleOrder;
            _moduleController.UpdateModule(module);
        }

        private TabVersionDetail GetResetTabVersionDetail(TabVersion tabVersion)
        {
            return new TabVersionDetail
            {
                PaneName = "none_resetAction",
                TabVersionId = tabVersion.TabVersionId,
                Action = TabVersionDetailAction.Reset,
                ModuleId = Null.NullInteger,
                ModuleVersion = Null.NullInteger
            };
        }

        private void RestoreModuleInfo(int tabId, TabVersionDetail detailsToRestore )
        {
            var restoredModule = _moduleController.GetModule(detailsToRestore.ModuleId, tabId, true);
            if (restoredModule != null)
            {
                _moduleController.RestoreModule(restoredModule);
                UpdateModuleInfoOrder(restoredModule, detailsToRestore);
            }
        }

        private IEnumerable<TabVersionDetail> GetVersionModulesDetails(int tabId, int version)
        {
            var tabVersionDetails = _tabVersionDetailController.GetVersionHistory(tabId, version);
            return GetSnapShot(tabVersionDetails);
        }

        private TabVersion PublishVersion(int portalId, int tabId, int createdByUserID, TabVersion tabVersion)
        {
            var unPublishedDetails = _tabVersionDetailController.GetTabVersionDetails(tabVersion.TabVersionId);
            foreach (var unPublishedDetail in unPublishedDetails)
            {
                if (unPublishedDetail.ModuleVersion != Null.NullInteger)
                {
                    PublishDetail(tabId, unPublishedDetail);
                }
            }

            tabVersion.IsPublished = true;
            _tabVersionController.SaveTabVersion(tabVersion, tabVersion.CreatedByUserID, createdByUserID);
            var tab = TabController.Instance.GetTab(tabId, portalId);
            if (!tab.HasBeenPublished)
            {
                TabController.Instance.MarkAsPublished(tab);
            }
            _moduleController.ClearCache(tabId);
            return tabVersion;
        }

        private IEnumerable<TabVersionDetail> CopyVersionDetails(IEnumerable<TabVersionDetail> tabVersionDetails)
        {
            return tabVersionDetails.Select(tabVersionDetail => new TabVersionDetail
                                                                {
                                                                    ModuleId = tabVersionDetail.ModuleId, 
                                                                    ModuleOrder = tabVersionDetail.ModuleOrder, 
                                                                    ModuleVersion = tabVersionDetail.ModuleVersion, 
                                                                    PaneName = tabVersionDetail.PaneName, 
                                                                    Action = tabVersionDetail.Action
                                                                }).ToList();
        }

        private void CheckVersioningEnabled(int tabId)
        {
            CheckVersioningEnabled(GetCurrentPortalId(), tabId);
        }

        private void CheckVersioningEnabled(int portalId, int tabId)
        {            
            if (portalId == Null.NullInteger || !_tabVersionSettings.IsVersioningEnabled(portalId, tabId))
            {
                throw new InvalidOperationException(Localization.GetString("TabVersioningNotEnabled", Localization.ExceptionsResourceFile));
            }
        }

        private int GetCurrentPortalId()
        {
            return _portalSettings == null ? Null.NullInteger : _portalSettings.PortalId;
        }

        private void CreateSnapshotOverVersion(int tabId, TabVersion snapshotTabVersion, TabVersion deletedTabVersion = null)
        {
            var snapShotTabVersionDetails = GetVersionModulesDetails(tabId, snapshotTabVersion.Version).ToArray();
            var existingTabVersionDetails = _tabVersionDetailController.GetTabVersionDetails(snapshotTabVersion.TabVersionId).ToArray();
            
            for (var i = existingTabVersionDetails.Count(); i > 0; i--)
            {
                var existingDetail = existingTabVersionDetails.ElementAtOrDefault(i - 1);

                if (deletedTabVersion == null)
                {
                    if (snapShotTabVersionDetails.All(tvd => tvd.TabVersionDetailId != existingDetail.TabVersionDetailId))
                    {
                        _tabVersionDetailController.DeleteTabVersionDetail(existingDetail.TabVersionId,
                            existingDetail.TabVersionDetailId);
                    }
                }
                else if (existingDetail.Action == TabVersionDetailAction.Deleted) 
                {
                    IEnumerable<TabVersionDetail> deletedTabVersionDetails = _tabVersionDetailController.GetTabVersionDetails(deletedTabVersion.TabVersionId);
                    var moduleAddedAndDeleted = deletedTabVersionDetails.Any(
                        deleteDetail =>
                            deleteDetail.ModuleId == existingDetail.ModuleId &&
                            deleteDetail.Action == TabVersionDetailAction.Added);
                    if (moduleAddedAndDeleted)
                    {
                        _tabVersionDetailController.DeleteTabVersionDetail(existingDetail.TabVersionId,
                            existingDetail.TabVersionDetailId);
                    }
                }
            }

            UpdateDeletedTabDetails(snapshotTabVersion, deletedTabVersion, snapShotTabVersionDetails);
        }

        private void UpdateDeletedTabDetails(TabVersion snapshotTabVersion, TabVersion deletedTabVersion,
            TabVersionDetail[] snapShotTabVersionDetails)
        {
            var tabVersionDetailsToBeUpdated = deletedTabVersion != null ? _tabVersionDetailController.GetTabVersionDetails(deletedTabVersion.TabVersionId).ToArray() 
                                                                                : snapShotTabVersionDetails;

            foreach (var tabVersionDetail in tabVersionDetailsToBeUpdated)
            {
                var detailInSnapshot =
                    snapShotTabVersionDetails.Any(
                        snapshotDetail => snapshotDetail.TabVersionDetailId == tabVersionDetail.TabVersionDetailId);
                var deleteOrResetAction = tabVersionDetail.Action == TabVersionDetailAction.Deleted || tabVersionDetail.Action == TabVersionDetailAction.Reset;
                if (detailInSnapshot
                    || deleteOrResetAction)
                {
                    var previousTabVersionId = tabVersionDetail.TabVersionId;
                    tabVersionDetail.TabVersionId = snapshotTabVersion.TabVersionId;
                    _tabVersionDetailController.SaveTabVersionDetail(tabVersionDetail);
                    _tabVersionDetailController.ClearCache(previousTabVersionId);
                }
                
            }
        }

        private void DeleteOldVersions(IEnumerable<TabVersion> tabVersionsOrdered, TabVersion snapShotTabVersion)
        {
            var oldVersions = tabVersionsOrdered.Where(tv => tv.Version < snapShotTabVersion.Version).ToArray();
            for (var i = oldVersions.Count(); i > 0; i--)
            {
                var oldVersion = oldVersions.ElementAtOrDefault(i - 1);
                var oldVersionDetails = _tabVersionDetailController.GetTabVersionDetails(oldVersion.TabVersionId).ToArray();
                for (var j = oldVersionDetails.Count(); j > 0; j--)
                {
                    var oldVersionDetail = oldVersionDetails.ElementAtOrDefault(j - 1);
                    _tabVersionDetailController.DeleteTabVersionDetail(oldVersionDetail.TabVersionId, oldVersionDetail.TabVersionDetailId);
                }
                _tabVersionController.DeleteTabVersion(oldVersion.TabId, oldVersion.TabVersionId);
            }
        }

        private  IEnumerable<ModuleInfo> ConvertToModuleInfo(IEnumerable<TabVersionDetail> details, int tabId)
        {
            var modules = new List<ModuleInfo>();
            try
            {
                foreach (var detail in details)
                {
                    var module = _moduleController.GetModule(detail.ModuleId, tabId, false);
                    if (module == null)
                    {
                        continue;
                    }
                    var moduleVersion = _moduleController.IsSharedModule(module)
                        ? Null.NullInteger
                        : detail.ModuleVersion;
                    var cloneModule = module.Clone();
                    cloneModule.UniqueId = module.UniqueId;
                    cloneModule.VersionGuid = module.VersionGuid;
                    cloneModule.IsDeleted = false;
                    cloneModule.ModuleVersion = moduleVersion;
                    cloneModule.PaneName = detail.PaneName;
                    cloneModule.ModuleOrder = detail.ModuleOrder;
                    modules.Add(cloneModule);
                }

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
            
            return modules;
        }
        
        private int RollBackDetail(int tabId, TabVersionDetail unPublishedDetail)
        {
            var moduleInfo = _moduleController.GetModule(unPublishedDetail.ModuleId, tabId, true);
            if (moduleInfo == null) return Null.NullInteger;

            var versionableController = GetVersionableController(moduleInfo);
            if (versionableController == null) return Null.NullInteger;

            if (_moduleController.IsSharedModule(moduleInfo))
            {
                return versionableController.GetPublishedVersion(moduleInfo.ModuleID);
            }

            return versionableController.RollBackVersion(unPublishedDetail.ModuleId, unPublishedDetail.ModuleVersion);
        }

        private void PublishDetail(int tabId, TabVersionDetail unPublishedDetail)
        {
            var moduleInfo = _moduleController.GetModule(unPublishedDetail.ModuleId, tabId, true);

            if (moduleInfo == null || _moduleController.IsSharedModule(moduleInfo))
            {
                return;
            }

            var versionableController = GetVersionableController(moduleInfo);
            if (versionableController != null)
            {
                versionableController.PublishVersion(unPublishedDetail.ModuleId, unPublishedDetail.ModuleVersion);
            }
        }

        private void DiscardDetail(int tabId, TabVersionDetail unPublishedDetail)
        {
            var moduleInfo = _moduleController.GetModule(unPublishedDetail.ModuleId, tabId, true);

            if (_moduleController.IsSharedModule(moduleInfo))
            {
                return;
            }

            var versionableController = GetVersionableController(moduleInfo);
            if (versionableController != null)
            {
                versionableController.DeleteVersion(unPublishedDetail.ModuleId, unPublishedDetail.ModuleVersion);                
            }
        }

        private IVersionable GetVersionableController(ModuleInfo moduleInfo)
        {
            if (String.IsNullOrEmpty(moduleInfo.DesktopModule.BusinessControllerClass))
            {
                return null;
            }
            
            object controller = Reflection.CreateObject(moduleInfo.DesktopModule.BusinessControllerClass, "");
            if (controller is IVersionable)
            {
                return controller as IVersionable;
            }
            return null;
        }

        private static IEnumerable<TabVersionDetail> GetSnapShot(IEnumerable<TabVersionDetail> tabVersionDetails)
        {
            var versionModules = new Dictionary<int, TabVersionDetail>();
            foreach (var tabVersionDetail in tabVersionDetails)
            {
                switch (tabVersionDetail.Action)
                {
                    case TabVersionDetailAction.Added:
                    case TabVersionDetailAction.Modified:
                        if (versionModules.ContainsKey(tabVersionDetail.ModuleId))
                        {
                            versionModules[tabVersionDetail.ModuleId] = JoinVersionDetails(versionModules[tabVersionDetail.ModuleId], tabVersionDetail);
                        }
                        else
                        {
                            versionModules.Add(tabVersionDetail.ModuleId, tabVersionDetail);
                        }
                        break;
                    case TabVersionDetailAction.Deleted:
                        if (versionModules.ContainsKey(tabVersionDetail.ModuleId))
                        {
                            versionModules.Remove(tabVersionDetail.ModuleId);
                        }
                        break;
                    case TabVersionDetailAction.Reset:
                        versionModules.Clear();
                        break;
                }
            }

            // Return Snapshot ordering by PaneName and ModuleOrder (this is required as Skin.cs does not order by these fields)
            return versionModules.Values
                .OrderBy(m => m.PaneName)
                .ThenBy(m => m.ModuleOrder)
                .ToList();
        }

        private static TabVersionDetail JoinVersionDetails(TabVersionDetail tabVersionDetail, TabVersionDetail newVersionDetail)
        {
            // Movement changes have not ModuleVersion
            if (newVersionDetail.ModuleVersion == Null.NullInteger)
            {
                newVersionDetail.ModuleVersion = tabVersionDetail.ModuleVersion;
            }

            return newVersionDetail;
        }

        private void CreateFirstTabVersion(int tabId, TabInfo tab, IEnumerable<ModuleInfo> modules)
        {
            var tabVersion = _tabVersionController.CreateTabVersion(tabId, tab.CreatedByUserID, true);
            foreach (var module in modules)
            {
                var moduleVersion = GetModuleContentPublishedVersion(module);
                _tabVersionDetailController.SaveTabVersionDetail(new TabVersionDetail
                {
                    Action = TabVersionDetailAction.Added,
                    ModuleId = module.ModuleID,
                    ModuleOrder = module.ModuleOrder,
                    ModuleVersion = moduleVersion,
                    PaneName = module.PaneName,
                    TabVersionId = tabVersion.TabVersionId
                }, module.CreatedByUserID);
            }
        }

        private int GetModuleContentPublishedVersion(ModuleInfo module)
        {
            var versionableController = GetVersionableController(module);
            return versionableController != null ? versionableController.GetPublishedVersion(module.ModuleID) : Null.NullInteger;
        }
        #endregion

        protected override Func<ITabVersionBuilder> GetFactory()
        {
            return () => new TabVersionBuilder();
        }
    }
}
