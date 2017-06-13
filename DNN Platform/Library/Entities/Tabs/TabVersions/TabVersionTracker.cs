#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
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
using System.Linq;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    class TabVersionTracker : ServiceLocator<ITabChangeTracker, TabVersionTracker>, ITabChangeTracker
    {
        #region Public Methods

        /// <summary>
        /// Tracks a version detail when a module is added to a page
        /// </summary>
        /// <param name="module">Module which tracks the version detail</param>
        /// <param name="moduleVersion">Version number corresponding to the version detail</param>
        /// <param name="userId">User Id who provokes the version detail</param>  
        public void TrackModuleAddition(ModuleInfo module, int moduleVersion, int userId)
        {
            Requires.NotNull("module", module);

            try
            {
                var unPublishedVersion = GetOrCreateUnPublishedTabVersion(module.PortalID, module.TabID, userId);

                ProcessAdditionDetail(module, moduleVersion, userId, unPublishedVersion);
            }
            catch (Exception ex)
            {
                Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        /// <summary>
        /// Tracks a version detail when a module is modified on a page
        /// </summary>
        /// <param name="module">Module which tracks the version detail</param>
        /// <param name="moduleVersion">Version number corresponding to the version detail</param>
        /// <param name="userId">User Id who provokes the version detail</param>  
        public void TrackModuleModification(ModuleInfo module, int moduleVersion, int userId)
        {
            Requires.NotNull("module", module);

            try
            {
                if (IsHostModule(module))
                {
                    return;
                }

                var unPublishedVersion = GetOrCreateUnPublishedTabVersion(module.PortalID, module.TabID, userId);
                var tabVersionDetail = CreateNewTabVersionDetailObjectFromModule(unPublishedVersion.TabVersionId, module.ModuleID, module.PaneName, module.ModuleOrder, moduleVersion, TabVersionDetailAction.Modified);

                var existingTabVersionDetail = TabVersionDetailController.Instance.GetTabVersionDetails(unPublishedVersion.TabVersionId).SingleOrDefault(tvd => tvd.ModuleId == module.ModuleID);
                if (existingTabVersionDetail != null)
                {
                    tabVersionDetail.TabVersionDetailId = existingTabVersionDetail.TabVersionDetailId;
                    if (moduleVersion == Null.NullInteger)
                    {
                        tabVersionDetail.ModuleVersion = existingTabVersionDetail.ModuleVersion;
                    }

                    // If the operation is done over a just created module, the Added operation is kept.
                    if (existingTabVersionDetail.Action == TabVersionDetailAction.Added)
                    {
                        tabVersionDetail.Action = TabVersionDetailAction.Added;
                    }
                }

                TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail, userId);
            }
            catch (InvalidOperationException)
            {
                throw; 
            }
            catch (Exception ex)
            {
                Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        /// <summary>
        /// Tracks a version detail when a module is deleted from a page
        /// </summary>
        /// <param name="module">Module which tracks the version detail</param>
        /// <param name="moduleVersion">Version number corresponding to the version detail</param>
        /// <param name="userId">User Id who provokes the version detail</param>  
        public void TrackModuleDeletion(ModuleInfo module, int moduleVersion, int userId)
        {
            Requires.NotNull("module", module);

            try
            {
                var unPublishedVersion = GetOrCreateUnPublishedTabVersion(module.PortalID, module.TabID, userId);

                ProcessDeletionDetail(module, moduleVersion, userId, unPublishedVersion);
            }
            catch (Exception ex)
            {
                Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        /// <summary>
        /// Tracks a version detail when a module is copied from an exisitng page
        /// </summary>
        /// <param name="module">Module which tracks the version detail</param>
        /// <param name="moduleVersion">Version number corresponding to the version detail</param>
        /// <param name="originalTabId">Tab Id where the module originally is</param>
        /// /// <param name="userId">User Id who provokes the version detail</param>  
        public void TrackModuleCopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId)
        {
            Requires.NotNull("module", module);

            try
            {
                var targetVersion = module.IsDeleted ? GetOrCreateUnPublishedTabVersion(module.PortalID, module.TabID, userId)
                                        : (TabVersionBuilder.Instance.GetUnPublishedVersion(module.TabID) ??TabVersionBuilder.Instance.GetCurrentVersion(module.TabID));
                if (targetVersion == null)
                {
                    return;
                }

                ProcessAdditionDetail(module, moduleVersion, userId, targetVersion);
            }
            catch (Exception ex)
            {
                Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        /// <summary>
        /// Tracks a version detail when a copied module is deleted from an exisitng page
        /// </summary>
        /// <param name="module">Module which tracks the version detail</param>
        /// <param name="moduleVersion">Version number corresponding to the version detail</param>
        /// <param name="originalTabId">Tab Id where the module originally is</param>
        /// <param name="userId">User Id who provokes the version detail</param>  
        public void TrackModuleUncopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId)
        {
            Requires.NotNull("module", module);

            try
            {
                var targetVersion = TabVersionBuilder.Instance.GetUnPublishedVersion(module.TabID) ??
                                    TabVersionBuilder.Instance.GetCurrentVersion(module.TabID);

                if (targetVersion == null)
                {
                    return;
                }

                ProcessDeletionDetail(module, moduleVersion, userId, targetVersion);
            }
            catch (Exception ex)
            {
                Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        #endregion

        #region Private Statics Methods

        private static void ProcessAdditionDetail(ModuleInfo module, int moduleVersion, int userId, TabVersion targetVersion)
        {
            if (IsHostModule(module))
            {
                return;
            }
            
            //Module could be restored in the same version
            var existingTabDetails =
                TabVersionDetailController.Instance.GetTabVersionDetails(targetVersion.TabVersionId)
                    .Where(tvd => tvd.ModuleId == module.ModuleID);
            foreach (var existingTabDetail in existingTabDetails)
            {
                TabVersionDetailController.Instance.DeleteTabVersionDetail(existingTabDetail.TabVersionId,
                    existingTabDetail.TabVersionDetailId);
            }

            var tabVersionDetail = CreateNewTabVersionDetailObjectFromModule(targetVersion.TabVersionId, module,
                moduleVersion, TabVersionDetailAction.Added);
            TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail, userId);
        }

        private static void ProcessDeletionDetail(ModuleInfo module, int moduleVersion, int userId, TabVersion targetVersion)
        {
            if (IsHostModule(module))
            {
                return;
            }

            var existingTabDetail =
                TabVersionDetailController.Instance.GetTabVersionDetails(targetVersion.TabVersionId)
                    .SingleOrDefault(tvd => tvd.ModuleId == module.ModuleID);
            if (existingTabDetail != null)
            {
                TabVersionDetailController.Instance.DeleteTabVersionDetail(existingTabDetail.TabVersionId,
                    existingTabDetail.TabVersionDetailId);
                
                //When a module is added in the same version, then we should do nothing with it
                if (existingTabDetail.Action == TabVersionDetailAction.Added)
                {
                    return;
                }
            }

            //Do not add module to Tab Version Details if it has been hard deleted
            ModuleInfo moduleInfo = ModuleController.Instance.GetModule(module.ModuleID, module.TabID, false);
            if (moduleInfo != null)
            {
                var tabVersionDetail = CreateNewTabVersionDetailObjectFromModule(targetVersion.TabVersionId, module,
                    moduleVersion, TabVersionDetailAction.Deleted);
                TabVersionDetailController.Instance.SaveTabVersionDetail(tabVersionDetail, userId);
            }
        }

        private static bool IsHostModule(ModuleInfo module)
        {
            return module.PortalID == Null.NullInteger;
        }

        private static TabVersion GetOrCreateUnPublishedTabVersion(int portalId, int tabId, int createdByUserId)
        {
            var unPublishedVersion = TabVersionBuilder.Instance.GetUnPublishedVersion(tabId);
            return unPublishedVersion == null ?
                TabVersionBuilder.Instance.CreateNewVersion(portalId, tabId, createdByUserId) :
                TabVersionController.Instance.GetTabVersions(tabId).SingleOrDefault(tv => !tv.IsPublished);
        }

        private static TabVersionDetail CreateNewTabVersionDetailObjectFromModule(int tabVersionId, ModuleInfo module, int moduleVersion, TabVersionDetailAction action)
        {
            return new TabVersionDetail
            {
                TabVersionDetailId = 0,
                TabVersionId = tabVersionId,
                ModuleId = module.ModuleID,
                ModuleVersion = moduleVersion,
                ModuleOrder = module.ModuleOrder,
                PaneName = module.PaneName,
                Action = action              
            };
        }

        private static TabVersionDetail CreateNewTabVersionDetailObjectFromModule(int tabVersionId, int moduleId, string paneName, int moduleOrder, int moduleVersion, TabVersionDetailAction action)
        {
            return new TabVersionDetail
            {
                TabVersionDetailId = 0,
                TabVersionId = tabVersionId,
                ModuleId = moduleId,
                ModuleVersion = moduleVersion,
                ModuleOrder = moduleOrder,
                PaneName = paneName,
                Action = action
            };
        }
        #endregion

        #region Service Locator
        protected override Func<ITabChangeTracker> GetFactory()
        {
            return () => new TabVersionTracker();
        }
        #endregion
    }
}
