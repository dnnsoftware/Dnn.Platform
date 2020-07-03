// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Framework;
    using DotNetNuke.Services.Localization;

    public class TabChangeTracker : ServiceLocator<ITabChangeTracker, TabChangeTracker>, ITabChangeTracker
    {
        public void TrackModuleAddition(ModuleInfo module, int moduleVersion, int userId)
        {
            var unPublishedVersion = TabVersionBuilder.Instance.GetUnPublishedVersion(module.TabID);
            if (TabChangeSettings.Instance.IsChangeControlEnabled(module.PortalID, module.TabID))
            {
                TabVersionTracker.Instance.TrackModuleAddition(module, moduleVersion, userId);
            }

            if (TabWorkflowSettings.Instance.IsWorkflowEnabled(module.PortalID, module.TabID) && unPublishedVersion == null)
            {
                TabWorkflowTracker.Instance.TrackModuleAddition(module, moduleVersion, userId);
            }
        }

        public void TrackModuleModification(ModuleInfo module, int moduleVersion, int userId)
        {
            if (ModuleController.Instance.IsSharedModule(module) && moduleVersion != Null.NullInteger)
            {
                throw new InvalidOperationException(Localization.GetExceptionMessage(
                    "ModuleDoesNotBelongToPage",
                    "This module does not belong to the page. Please, move to its master page to change the module"));
            }

            var unPublishedVersion = TabVersionBuilder.Instance.GetUnPublishedVersion(module.TabID);
            if (TabChangeSettings.Instance.IsChangeControlEnabled(module.PortalID, module.TabID))
            {
                TabVersionTracker.Instance.TrackModuleModification(module, moduleVersion, userId);
            }

            if (TabWorkflowSettings.Instance.IsWorkflowEnabled(module.PortalID, module.TabID) && unPublishedVersion == null)
            {
                TabWorkflowTracker.Instance.TrackModuleModification(module, moduleVersion, userId);
            }
        }

        public void TrackModuleDeletion(ModuleInfo module, int moduleVersion, int userId)
        {
            var unPublishedVersion = TabVersionBuilder.Instance.GetUnPublishedVersion(module.TabID);
            if (TabChangeSettings.Instance.IsChangeControlEnabled(module.PortalID, module.TabID))
            {
                TabVersionTracker.Instance.TrackModuleDeletion(module, moduleVersion, userId);
            }

            if (TabWorkflowSettings.Instance.IsWorkflowEnabled(module.PortalID, module.TabID) && unPublishedVersion == null)
            {
                TabWorkflowTracker.Instance.TrackModuleDeletion(module, moduleVersion, userId);
            }
        }

        public void TrackModuleUncopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId)
        {
            if (module != null && TabChangeSettings.Instance.IsChangeControlEnabled(module.PortalID, module.TabID))
            {
                TabVersionTracker.Instance.TrackModuleUncopy(module, moduleVersion, originalTabId, userId);
            }
        }

        public void TrackModuleCopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId)
        {
            if (TabChangeSettings.Instance.IsChangeControlEnabled(module.PortalID, module.TabID))
            {
                TabVersionTracker.Instance.TrackModuleCopy(module, moduleVersion, originalTabId, userId);
            }
        }

        protected override Func<ITabChangeTracker> GetFactory()
        {
            return () => new TabChangeTracker();
        }
    }
}
