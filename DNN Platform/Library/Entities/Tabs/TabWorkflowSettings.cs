// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Globalization;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Workflow;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;

namespace DotNetNuke.Entities.Tabs
{
    public class TabWorkflowSettings : ServiceLocator<ITabWorkflowSettings, TabWorkflowSettings>, ITabWorkflowSettings
    {
        #region Constants
        private const string DefaultTabWorkflowKey = "DefaultTabWorkflowKey";
        private const string TabWorkflowEnableKey = "TabWorkflowEnabledKey";
        #endregion

        #region Private Members
        private readonly ITabController _tabController;
        private readonly ISystemWorkflowManager _systemWorkflowManager;
        #endregion

        #region Constructor

        public TabWorkflowSettings()
        {
            _tabController = TabController.Instance;
            _systemWorkflowManager = SystemWorkflowManager.Instance;
        }
        #endregion

        #region Public Methods
        public int GetDefaultTabWorkflowId(int portalId)
        {
            var workflowId = PortalController.GetPortalSettingAsInteger(DefaultTabWorkflowKey, portalId, Null.NullInteger);
            if (workflowId == Null.NullInteger)
            {
                var workflow = _systemWorkflowManager.GetDirectPublishWorkflow(portalId);
                workflowId = (workflow != null) ? workflow.WorkflowID : Null.NullInteger;
                if (workflowId != Null.NullInteger)
                {
                    PortalController.UpdatePortalSetting(portalId, DefaultTabWorkflowKey, workflowId.ToString(CultureInfo.InvariantCulture), true);                    
                }
            }
            return workflowId;
        }

        public void SetDefaultTabWorkflowId(int portalId, int workflowId)
        {
            PortalController.UpdatePortalSetting(portalId, DefaultTabWorkflowKey, workflowId.ToString(CultureInfo.InvariantCulture), true);
        }

        public void SetWorkflowEnabled(int portalId, bool enabled)
        {
            Requires.NotNegative("portalId", portalId);

            PortalController.UpdatePortalSetting(portalId, TabWorkflowEnableKey, enabled.ToString(CultureInfo.InvariantCulture), true);
        }

        public void SetWorkflowEnabled(int portalId, int tabId, bool enabled)
        {
            Requires.NotNegative("tabId", tabId);

            _tabController.UpdateTabSetting(tabId, TabWorkflowEnableKey, enabled.ToString(CultureInfo.InvariantCulture));
        }

        public bool IsWorkflowEnabled(int portalId, int tabId)
        {
            if (!IsWorkflowEnabled(portalId))
            {
                return false;
            }

            var tabInfo = _tabController.GetTab(tabId, portalId);
            var settings = _tabController.GetTabSettings(tabId);

            return !_tabController.IsHostOrAdminPage(tabInfo) && (settings[TabWorkflowEnableKey] == null || Convert.ToBoolean(settings[TabWorkflowEnableKey]));
        }

        public bool IsWorkflowEnabled(int portalId)
        {
            if (portalId == Null.NullInteger)
            {
                return false;
            }

            return Convert.ToBoolean(PortalController.GetPortalSetting(TabWorkflowEnableKey, portalId, Boolean.FalseString));
        }
        #endregion

        #region Service Locator
        protected override Func<ITabWorkflowSettings> GetFactory()
        {
            return () => new TabWorkflowSettings();
        }
        #endregion
    }
}
