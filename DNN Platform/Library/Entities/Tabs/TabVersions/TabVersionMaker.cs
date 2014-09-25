#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2014
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
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    public class TabVersionMaker : ServiceLocator<ITabVersionMaker, TabVersionMaker>, ITabVersionMaker
    {
        #region Public Methods
        public void Publish(int portalId, int tabId, int createdByUserID)
        {
            CheckVersioningEnabled();

            var tabVersion = GetUnPublishedVersion(tabId);            
            if (tabVersion == null)
            {                
                throw new InvalidOperationException(String.Format(Localization.GetString("TabHasNotAnUnpublishedVersion", Localization.ExceptionsResourceFile), tabId));
            }
            if (tabVersion.IsPublished)
            {
                throw new InvalidOperationException(String.Format(Localization.GetString("TabVersionAlreadyPublished", Localization.ExceptionsResourceFile), tabId, tabVersion.Version));
            }
            PublishVersion(portalId, tabId, createdByUserID, tabVersion);
        }

        public void Discard(int tabId, int createdByUserID)
        {
            CheckVersioningEnabled();

            var tabVersion = GetUnPublishedVersion(tabId);            
            if (tabVersion == null)
            {
                throw new InvalidOperationException(String.Format(Localization.GetString("TabHasNotAnUnpublishedVersion", Localization.ExceptionsResourceFile), tabId));
            }
            if (tabVersion.IsPublished)
            {
                throw new InvalidOperationException(String.Format(Localization.GetString("TabVersionAlreadyPublished", Localization.ExceptionsResourceFile), tabId, tabVersion.Version));
            }
            if (TabVersionController.Instance.GetTabVersions(tabId).Count() == 1)
            {
                throw new InvalidOperationException(String.Format(Localization.GetString("TabVersionCannotBeDiscarded_OnlyOneVersion", Localization.ExceptionsResourceFile), tabId, tabVersion.Version));
            }
            DiscardVersion(tabId, createdByUserID, tabVersion);
        }

        public void DiscardVersion(int tabId, int createdByUserID, TabVersion tabVersion)
        {
            var unPublishedDetails = TabVersionDetailController.Instance.GetTabVersionDetails(tabVersion.TabVersionId);
            var publishedChanges = GetVersionModulesDetails(tabId, GetCurrentVersion(tabId).Version).ToArray();
            foreach (var unPublishedDetail in unPublishedDetails)
            {
                if (unPublishedDetail.Action == TabVersionDetailAction.Deleted)
                {
                    var restoredModuleDetail = publishedChanges.SingleOrDefault(tv => tv.ModuleId == unPublishedDetail.ModuleId);
                    RestoreModuleInfo(tabId, restoredModuleDetail);                    
                    continue;
                }
                
                if (publishedChanges.All(tv => tv.ModuleId != unPublishedDetail.ModuleId))
                {
                    ModuleController.Instance.DeleteTabModule(tabId, unPublishedDetail.ModuleId, true);
                    continue;
                }
                
                if (unPublishedDetail.ModuleVersion != Null.NullInteger)
                {
                    DiscardDetail(tabId, unPublishedDetail);
                }
            }

            TabVersionController.Instance.DeleteTabVersion(tabId, tabVersion.TabVersionId);
        }

        public void DeleteVersion(int tabId, int createdByUserID, int version)
        {
            CheckVersioningEnabled();

            if (GetUnPublishedVersion(tabId) != null)
            {                
                throw new InvalidOperationException(String.Format(Localization.GetString("TabVersionCannotBeDeleted_UnpublishedVersionExists", Localization.ExceptionsResourceFile), tabId, version));
            }

            var tabVersions = TabVersionController.Instance.GetTabVersions(tabId).OrderByDescending(tv => tv.Version);
            if (tabVersions.FirstOrDefault().Version == version)
            {
                var tabVersion = tabVersions.FirstOrDefault();
                var unPublishedDetails = TabVersionDetailController.Instance.GetTabVersionDetails(tabVersion.TabVersionId);
                foreach (var unPublishedDetail in unPublishedDetails)
                {
                    if (unPublishedDetail.Action == TabVersionDetailAction.Added)
                    {
                        ModuleController.Instance.DeleteTabModule(tabId, unPublishedDetail.ModuleId, true);
                        continue;
                    }
                    if (unPublishedDetail.ModuleVersion != Null.NullInteger && unPublishedDetail.Action == TabVersionDetailAction.Modified)
                    {
                        DiscardDetail(tabId, unPublishedDetail);
                    }    
                }
                TabVersionController.Instance.DeleteTabVersion(tabId, tabVersion.TabVersionId);
            }
            else
            {
                for (int i = 1; i < tabVersions.Count(); i++)
                {
                    if (tabVersions.ElementAtOrDefault(i).Version == version)
                    {
                        CreateSnapshotOverVersion(tabId, tabVersions.ElementAtOrDefault(i-1));
                        TabVersionController.Instance.DeleteTabVersion(tabId, tabVersions.ElementAtOrDefault(i).TabVersionId);
                        return;
                    }
                }
            }
        }

        public TabVersion RollBackVesion(int tabId, int createdByUserID, int version)
        {
            CheckVersioningEnabled();

            if (GetUnPublishedVersion(tabId) != null)
            {                
                throw new InvalidOperationException(String.Format(Localization.GetString("TabVersionCannotBeRolledBack_UnpublishedVersionExists", Localization.ExceptionsResourceFile), tabId, version));
            }

            var tabVersion = TabVersionController.Instance.GetTabVersions(tabId).OrderByDescending(tv => tv.Version).FirstOrDefault();
            var publishedDetails = GetVersionModulesDetails(tabId, tabVersion.Version);

            var newVersion = CreateNewVersion(tabId, createdByUserID);
            var rollbackDetails = CopyVersionDetails(GetVersionModulesDetails(tabId, version));
            
            //Save Reset detail
            TabVersionDetailController.Instance.SaveTabVersionDetail(GetResetTabVersionDetail(newVersion), createdByUserID);
            
            foreach (var rollbackDetail in rollbackDetails)
            {
                rollbackDetail.TabVersionId = newVersion.TabVersionId;
                rollbackDetail.ModuleVersion = RollBackDetail(tabId, rollbackDetail);
                TabVersionDetailController.Instance.SaveTabVersionDetail(rollbackDetail, createdByUserID);

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
                ModuleController.Instance.DeleteTabModule(tabId, publishedDetail.ModuleId, true);
            }
            
            return newVersion;
        }

        public TabVersion CreateNewVersion(int tabId, int createdByUserID)
        {
            CheckVersioningEnabled();

            DeleteOldestVersionIfTabHasMaxNumberOfVersions(tabId);

            return TabVersionController.Instance.CreateTabVersion(tabId, createdByUserID);
        }

        public IEnumerable<ModuleInfo> GetUnPublishedVersionModules(int tabId)
        {
            var unPublishedVersion = GetUnPublishedVersion(tabId);
            if (unPublishedVersion == null)
            {
                return CBO.FillCollection<ModuleInfo>(DataProvider.Instance().GetTabModules(tabId)).Select(t => t.Clone());
            }

            return GetVersionModules(tabId, unPublishedVersion.TabVersionId);
        }

        public TabVersion GetCurrentVersion(int tabId, bool ignoreCache = false)
        {
            return TabVersionController.Instance.GetTabVersions(tabId, ignoreCache)
                .Where(tv => tv.IsPublished).OrderByDescending(tv => tv.CreatedOnDate).FirstOrDefault();
        }

        public TabVersion GetUnPublishedVersion(int tabId)
        {
            return TabVersionController.Instance.GetTabVersions(tabId, true)
                .SingleOrDefault(tv => !tv.IsPublished);
        }

        public IEnumerable<ModuleInfo> GetCurrentModules(int tabId)
        {
            var currentVersion = GetCurrentVersion(tabId);

            if (currentVersion == null) //Only when a tab is on a first version and it is not published, the currentVersion object can be null
            {
                return CBO.FillCollection<ModuleInfo>(DataProvider.Instance().GetTabModules(tabId)).Select(t => t.Clone());
            }

            return GetVersionModules(tabId, currentVersion.Version);
        }
        
        public IEnumerable<ModuleInfo> GetVersionModules(int tabId, int version)
        {
            return ConvertToModuleInfo(GetVersionModulesDetails(tabId, version));
        }
        #endregion

        #region Private Methods
        private void DeleteOldestVersionIfTabHasMaxNumberOfVersions(int tabId)
        {
            var maxVersionsAllowed = GetMaxNumberOfVersions();
            var tabVersionsOrdered = TabVersionController.Instance.GetTabVersions(tabId).OrderByDescending(tv => tv.Version);

            if (tabVersionsOrdered.Count() < maxVersionsAllowed) return;

            //The last existing version is going to be deleted, therefore we need to add the snapshot to the previous one
            var snapShotTabVersion = tabVersionsOrdered.ElementAtOrDefault(maxVersionsAllowed - 2);
            CreateSnapshotOverVersion(tabId, snapShotTabVersion);
            DeleteOldVersions(tabVersionsOrdered, snapShotTabVersion);
        }

        private static int GetMaxNumberOfVersions()
        {
            var portalId = GetCurrentPortalId();
            return TabVersionSettings.Instance.GetMaxNumberOfVersions(portalId);
        }

        private void UpdateModuleOrder(int tabId, TabVersionDetail detailToRestore)
        {
            var restoredModule = ModuleController.Instance.GetModule(detailToRestore.ModuleId, tabId, true);            
            UpdateModuleInfoOrder(restoredModule, detailToRestore);
        }

        private void UpdateModuleInfoOrder(ModuleInfo module, TabVersionDetail detailToRestore)
        {
            module.PaneName = detailToRestore.PaneName;
            module.ModuleOrder = detailToRestore.ModuleOrder;
            ModuleController.Instance.UpdateModule(module);
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
            var restoredModule = ModuleController.Instance.GetModule(detailsToRestore.ModuleId, tabId, true);
            ModuleController.Instance.RestoreModule(restoredModule);            
            UpdateModuleInfoOrder(restoredModule, detailsToRestore);                  
        }

        private IEnumerable<TabVersionDetail> GetVersionModulesDetails(int tabId, int version)
        {
            var tabVersionDetails = TabVersionDetailController.Instance.GetVersionHistory(tabId, version);
            return GetSnapShot(tabVersionDetails);
        }

        private void PublishVersion(int portalId, int tabId, int createdByUserID, TabVersion tabVersion)
        {
            var unPublishedDetails = TabVersionDetailController.Instance.GetTabVersionDetails(tabVersion.TabVersionId);
            foreach (var unPublishedDetail in unPublishedDetails)
            {
                if (unPublishedDetail.ModuleVersion != Null.NullInteger)
                {
                    PublishDetail(tabId, unPublishedDetail);
                }
            }

            tabVersion.IsPublished = true;
            TabVersionController.Instance.SaveTabVersion(tabVersion, tabVersion.CreatedByUserID, createdByUserID);
            var tab = TabController.Instance.GetTab(tabId, portalId);
            if (!tab.HasBeenPublished)
            {
                TabController.Instance.MarkAsPublished(tab);
            }
            ModuleController.Instance.ClearCache(tabId);
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

        private static void CheckVersioningEnabled()
        {
            var portalId = GetCurrentPortalId();
            if (portalId == Null.NullInteger || !TabVersionSettings.Instance.IsVersioningEnabled(portalId))
            {
                throw new InvalidOperationException(Localization.GetString("TabVersioningNotEnabled", Localization.ExceptionsResourceFile));
            }
        }

        private static int GetCurrentPortalId()
        {
            return PortalSettings.Current == null ? Null.NullInteger : PortalSettings.Current.PortalId;
        }

        private void CreateSnapshotOverVersion(int tabId, TabVersion snapshoTabVersion)
        {
            var snapShotTabVersionDetails = GetVersionModulesDetails(tabId, snapshoTabVersion.Version).ToArray();
            var existingTabVersionDetails = TabVersionDetailController.Instance.GetTabVersionDetails(snapshoTabVersion.TabVersionId).ToArray();

            for (var i = existingTabVersionDetails.Count(); i > 0; i--)
            {
                var existingDetail = existingTabVersionDetails.ElementAtOrDefault(i - 1);
                if (snapShotTabVersionDetails.All(tvd => tvd.TabVersionDetailId != existingDetail.TabVersionDetailId))
                {
                    TabVersionDetailController.Instance.DeleteTabVersionDetail(existingDetail.TabVersionId, existingDetail.TabVersionDetailId);
                }
            }

            foreach (var tabVersionDetail in snapShotTabVersionDetails)
            {
                tabVersionDetail.TabVersionId = snapshoTabVersion.TabVersionId;
                TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail);
            }
        }

        private void DeleteOldVersions(IEnumerable<TabVersion> tabVersionsOrdered, TabVersion snapShotTabVersion)
        {
            var oldVersions = tabVersionsOrdered.Where(tv => tv.Version < snapShotTabVersion.Version).ToArray();
            for (var i = oldVersions.Count(); i > 0; i--)
            {
                var oldVersion = oldVersions.ElementAtOrDefault(i - 1);
                var oldVersionDetails = TabVersionDetailController.Instance.GetTabVersionDetails(oldVersion.TabVersionId).ToArray();
                for (var j = oldVersionDetails.Count(); j > 0; j--)
                {
                    var oldVersionDetail = oldVersionDetails.ElementAtOrDefault(j - 1);
                    TabVersionDetailController.Instance.DeleteTabVersionDetail(oldVersionDetail.TabVersionId, oldVersionDetail.TabVersionDetailId);
                }
                TabVersionController.Instance.DeleteTabVersion(oldVersion.TabId, oldVersion.TabVersionId);
            }
        }

        private static IEnumerable<ModuleInfo> ConvertToModuleInfo(IEnumerable<TabVersionDetail> details)
        {
            var modules = new List<ModuleInfo>();
            foreach (var detail in details)
            {
                var module = ModuleController.Instance.GetModule(detail.ModuleId, Null.NullInteger, false);
                if (module == null)
                {
                    continue;
                }

                var cloneModule = module.Clone();
                cloneModule.ModuleVersion = detail.ModuleVersion;
                cloneModule.PaneName = detail.PaneName;
                cloneModule.ModuleOrder = detail.ModuleOrder;
                modules.Add(cloneModule);
            };

            return modules;
        }
        
        private int RollBackDetail(int tabId, TabVersionDetail unPublishedDetail)
        {
            var moduleInfo = ModuleController.Instance.GetModule(unPublishedDetail.ModuleId, tabId, true);

            var versionableController = GetVersionableController(moduleInfo);
            if (versionableController == null) return Null.NullInteger;
            
            return versionableController.RollBackVersion(unPublishedDetail.ModuleId, unPublishedDetail.ModuleVersion);
        }

        private void PublishDetail(int tabId, TabVersionDetail unPublishedDetail)
        {
            var moduleInfo = ModuleController.Instance.GetModule(unPublishedDetail.ModuleId, tabId, true);

            var versionableController = GetVersionableController(moduleInfo);
            if (versionableController != null)
            {
                versionableController.PublishVersion(unPublishedDetail.ModuleId, unPublishedDetail.ModuleVersion);
            }
        }

        private void DiscardDetail(int tabId, TabVersionDetail unPublishedDetail)
        {
            var moduleInfo = ModuleController.Instance.GetModule(unPublishedDetail.ModuleId, tabId, true);

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
        #endregion

        protected override Func<ITabVersionMaker> GetFactory()
        {
            return () => new TabVersionMaker();
        }
    }
}
