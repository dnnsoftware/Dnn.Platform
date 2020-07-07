// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Tabs.Dto;
    using DotNetNuke.Entities.Tabs.TabVersions;
    using DotNetNuke.Framework;

    public class TabChangeSettings : ServiceLocator<ITabChangeSettings, TabChangeSettings>, ITabChangeSettings
    {
        public bool IsChangeControlEnabled(int portalId, int tabId)
        {
            if (portalId == Null.NullInteger)
            {
                return false;
            }

            var isVersioningEnabled = TabVersionSettings.Instance.IsVersioningEnabled(portalId, tabId);
            var isWorkflowEnable = TabWorkflowSettings.Instance.IsWorkflowEnabled(portalId, tabId);
            return isVersioningEnabled || isWorkflowEnable;
        }

        public ChangeControlState GetChangeControlState(int portalId, int tabId)
        {
            return new ChangeControlState
            {
                PortalId = portalId,
                TabId = tabId,
                IsVersioningEnabledForTab = TabVersionSettings.Instance.IsVersioningEnabled(portalId, tabId),
                IsWorkflowEnabledForTab = TabWorkflowSettings.Instance.IsWorkflowEnabled(portalId, tabId),
            };
        }

        protected override Func<ITabChangeSettings> GetFactory()
        {
            return () => new TabChangeSettings();
        }
    }
}
