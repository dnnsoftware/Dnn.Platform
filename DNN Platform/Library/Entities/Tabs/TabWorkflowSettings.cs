// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
            this._tabController = TabController.Instance;
            this._systemWorkflowManager = SystemWorkflowManager.Instance;
        }
        #endregion

        #region Public Methods
        public int GetDefaultTabWorkflowId(int portalId)
        {
            var workflowId = PortalController.GetPortalSettingAsInteger(DefaultTabWorkflowKey, portalId, Null.NullInteger);
            if (workflowId == Null.NullInteger)
            {
                var workflow = this._systemWorkflowManager.GetDirectPublishWorkflow(portalId);
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

            this._tabController.UpdateTabSetting(tabId, TabWorkflowEnableKey, enabled.ToString(CultureInfo.InvariantCulture));
        }

        public bool IsWorkflowEnabled(int portalId, int tabId)
        {
            if (!this.IsWorkflowEnabled(portalId))
            {
                return false;
            }

            var tabInfo = this._tabController.GetTab(tabId, portalId);
            var settings = this._tabController.GetTabSettings(tabId);

            return !this._tabController.IsHostOrAdminPage(tabInfo) && (settings[TabWorkflowEnableKey] == null || Convert.ToBoolean(settings[TabWorkflowEnableKey]));
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
