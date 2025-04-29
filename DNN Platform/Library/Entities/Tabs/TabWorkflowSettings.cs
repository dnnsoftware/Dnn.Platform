// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs;

using System;
using System.Globalization;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content.Workflow;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;

public class TabWorkflowSettings : ServiceLocator<ITabWorkflowSettings, TabWorkflowSettings>, ITabWorkflowSettings
{
    private const string DefaultTabWorkflowKey = "DefaultTabWorkflowKey";
    private const string TabWorkflowEnableKey = "TabWorkflowEnabledKey";
    private readonly ITabController tabController;
    private readonly ISystemWorkflowManager systemWorkflowManager;

    /// <summary>Initializes a new instance of the <see cref="TabWorkflowSettings"/> class.</summary>
    public TabWorkflowSettings()
    {
        this.tabController = TabController.Instance;
        this.systemWorkflowManager = SystemWorkflowManager.Instance;
    }

    /// <inheritdoc/>
    public int GetDefaultTabWorkflowId(int portalId)
    {
        var workflowId = PortalController.GetPortalSettingAsInteger(DefaultTabWorkflowKey, portalId, Null.NullInteger);
        if (workflowId == Null.NullInteger)
        {
            var workflow = this.systemWorkflowManager.GetDirectPublishWorkflow(portalId);
            workflowId = (workflow != null) ? workflow.WorkflowID : Null.NullInteger;
            if (workflowId != Null.NullInteger)
            {
                PortalController.UpdatePortalSetting(portalId, DefaultTabWorkflowKey, workflowId.ToString(CultureInfo.InvariantCulture), true);
            }
        }

        return workflowId;
    }

    /// <inheritdoc/>
    public void SetDefaultTabWorkflowId(int portalId, int workflowId)
    {
        PortalController.UpdatePortalSetting(portalId, DefaultTabWorkflowKey, workflowId.ToString(CultureInfo.InvariantCulture), true);
    }

    /// <inheritdoc/>
    public void SetWorkflowEnabled(int portalId, bool enabled)
    {
        Requires.NotNegative("portalId", portalId);

        PortalController.UpdatePortalSetting(portalId, TabWorkflowEnableKey, enabled.ToString(CultureInfo.InvariantCulture), true);
    }

    /// <inheritdoc/>
    public void SetWorkflowEnabled(int portalId, int tabId, bool enabled)
    {
        Requires.NotNegative("tabId", tabId);

        this.tabController.UpdateTabSetting(tabId, TabWorkflowEnableKey, enabled.ToString(CultureInfo.InvariantCulture));
    }

    /// <inheritdoc/>
    public bool IsWorkflowEnabled(int portalId, int tabId)
    {
        if (!this.IsWorkflowEnabled(portalId))
        {
            return false;
        }

        var tabInfo = this.tabController.GetTab(tabId, portalId);
        var settings = this.tabController.GetTabSettings(tabId);

        return !this.tabController.IsHostOrAdminPage(tabInfo) && (settings[TabWorkflowEnableKey] == null || Convert.ToBoolean(settings[TabWorkflowEnableKey]));
    }

    /// <inheritdoc/>
    public bool IsWorkflowEnabled(int portalId)
    {
        if (portalId == Null.NullInteger)
        {
            return false;
        }

        return Convert.ToBoolean(PortalController.GetPortalSetting(TabWorkflowEnableKey, portalId, bool.FalseString));
    }

    /// <inheritdoc/>
    protected override Func<ITabWorkflowSettings> GetFactory()
    {
        return () => new TabWorkflowSettings();
    }
}
