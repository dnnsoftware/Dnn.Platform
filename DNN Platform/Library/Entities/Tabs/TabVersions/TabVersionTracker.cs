// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs.TabVersions
{
    using System;
    using System.Linq;

    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;

    internal class TabVersionTracker : ServiceLocator<ITabChangeTracker, TabVersionTracker>, ITabChangeTracker
    {
        /// <summary>
        /// Tracks a version detail when a module is added to a page.
        /// </summary>
        /// <param name="module">Module which tracks the version detail.</param>
        /// <param name="moduleVersion">Version number corresponding to the version detail.</param>
        /// <param name="userId">User Id who provokes the version detail.</param>
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
        /// Tracks a version detail when a module is modified on a page.
        /// </summary>
        /// <param name="module">Module which tracks the version detail.</param>
        /// <param name="moduleVersion">Version number corresponding to the version detail.</param>
        /// <param name="userId">User Id who provokes the version detail.</param>
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
        /// Tracks a version detail when a module is deleted from a page.
        /// </summary>
        /// <param name="module">Module which tracks the version detail.</param>
        /// <param name="moduleVersion">Version number corresponding to the version detail.</param>
        /// <param name="userId">User Id who provokes the version detail.</param>
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
        /// Tracks a version detail when a module is copied from an exisitng page.
        /// </summary>
        /// <param name="module">Module which tracks the version detail.</param>
        /// <param name="moduleVersion">Version number corresponding to the version detail.</param>
        /// <param name="originalTabId">Tab Id where the module originally is.</param>
        /// /// <param name="userId">User Id who provokes the version detail.</param>
        public void TrackModuleCopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId)
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

                ProcessAdditionDetail(module, moduleVersion, userId, targetVersion);
            }
            catch (Exception ex)
            {
                Services.Exceptions.Exceptions.LogException(ex);
            }
        }

        /// <summary>
        /// Tracks a version detail when a copied module is deleted from an exisitng page.
        /// </summary>
        /// <param name="module">Module which tracks the version detail.</param>
        /// <param name="moduleVersion">Version number corresponding to the version detail.</param>
        /// <param name="originalTabId">Tab Id where the module originally is.</param>
        /// <param name="userId">User Id who provokes the version detail.</param>
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

        protected override Func<ITabChangeTracker> GetFactory()
        {
            return () => new TabVersionTracker();
        }

        private static void ProcessAdditionDetail(ModuleInfo module, int moduleVersion, int userId, TabVersion targetVersion)
        {
            if (IsHostModule(module))
            {
                return;
            }

            // Module could be restored in the same version
            var existingTabDetails =
                TabVersionDetailController.Instance.GetTabVersionDetails(targetVersion.TabVersionId)
                    .Where(tvd => tvd.ModuleId == module.ModuleID);
            foreach (var existingTabDetail in existingTabDetails)
            {
                TabVersionDetailController.Instance.DeleteTabVersionDetail(
                    existingTabDetail.TabVersionId,
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
                TabVersionDetailController.Instance.DeleteTabVersionDetail(
                    existingTabDetail.TabVersionId,
                    existingTabDetail.TabVersionDetailId);

                // When a module is added in the same version, then we should do nothing with it
                if (existingTabDetail.Action == TabVersionDetailAction.Added)
                {
                    return;
                }
            }

            // Do not add module to Tab Version Details if it has been hard deleted
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
                Action = action,
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
                Action = action,
            };
        }
    }
}
