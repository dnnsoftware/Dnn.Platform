// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Tabs
{
    using System;

    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Exceptions;

    internal class TabWorkflowTracker : ServiceLocator<ITabChangeTracker, TabWorkflowTracker>, ITabChangeTracker
    {
        private static readonly DnnLogger Logger = DnnLogger.GetClassLogger(typeof(TabWorkflowTracker));
        private readonly ITabController _tabController;
        private readonly IWorkflowEngine _workflowEngine;
        private readonly IWorkflowManager _workflowManager;
        private readonly ITabWorkflowSettings _tabWorkflowSettings;

        public TabWorkflowTracker()
        {
            this._tabController = TabController.Instance;
            this._workflowEngine = WorkflowEngine.Instance;
            this._workflowManager = WorkflowManager.Instance;
            this._tabWorkflowSettings = TabWorkflowSettings.Instance;
        }

        /// <summary>
        /// Tracks a workflow instance when a module is added to a page.
        /// </summary>
        /// <param name="module">Module which tracks the workflow instance.</param>
        /// <param name="moduleVersion">Version number corresponding to the module.</param>
        /// <param name="userId">User Id related with the workflow instance.</param>
        public void TrackModuleAddition(ModuleInfo module, int moduleVersion, int userId)
        {
            this.NotifyWorkflowAboutChanges(module.PortalID, module.TabID, userId);
        }

        /// <summary>
        /// Tracks a workflow instance when a module is modified on a page.
        /// </summary>
        /// <param name="module">Module which tracks the workflow instance.</param>
        /// <param name="moduleVersion">Version number corresponding to the module.</param>
        /// <param name="userId">User Id related with the workflow instance.</param>
        public void TrackModuleModification(ModuleInfo module, int moduleVersion, int userId)
        {
            this.NotifyWorkflowAboutChanges(module.PortalID, module.TabID, userId);
        }

        /// <summary>
        /// Tracks a workflow instance when a module is deleted from a page.
        /// </summary>
        /// <param name="module">Module which tracks the workflow instance.</param>
        /// <param name="moduleVersion">Version number corresponding to the module.</param>
        /// <param name="userId">User Id related with the workflow instance.</param>
        public void TrackModuleDeletion(ModuleInfo module, int moduleVersion, int userId)
        {
            this.NotifyWorkflowAboutChanges(module.PortalID, module.TabID, userId);
        }

        /// <summary>
        /// Tracks a workflow instance when a module is copied from an exisitng page.
        /// </summary>
        /// <param name="module">Module which tracks the workflow instance.</param>
        /// <param name="moduleVersion">Version number corresponding to the module.</param>
        /// <param name="originalTabId">Tab Id where the module originally is.</param>
        /// <param name="userId">User Id related with the workflow instance.</param>
        public void TrackModuleCopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId)
        {
            this.TrackModuleAddition(module, moduleVersion, userId);
        }

        /// <summary>
        /// Tracks a workflow instance when a copied module is deleted from an exisitng page.
        /// </summary>
        /// <param name="module">Module which tracks the workflow instance.</param>
        /// <param name="moduleVersion">Version number corresponding to the module.</param>
        /// <param name="originalTabId">Tab Id where the module originally is.</param>
        /// <param name="userId">User Id related with the workflow instance.</param>
        public void TrackModuleUncopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId)
        {
            this.TrackModuleDeletion(module, moduleVersion, userId);
        }

        protected override Func<ITabChangeTracker> GetFactory()
        {
            return () => new TabWorkflowTracker();
        }

        private void NotifyWorkflowAboutChanges(int portalId, int tabId, int userId)
        {
            try
            {
                var tabInfo = this._tabController.GetTab(tabId, portalId);
                if (tabInfo != null && !tabInfo.IsDeleted && this._workflowEngine.IsWorkflowCompleted(tabInfo))
                {
                    var workflow = this.GetCurrentOrDefaultWorkflow(tabInfo, portalId);
                    if (workflow == null)
                    {
                        Logger.Warn("Current Workflow and Default workflow are not found on NotifyWorkflowAboutChanges");
                        return;
                    }

                    this._workflowEngine.StartWorkflow(workflow.WorkflowID, tabInfo.ContentItemId, userId);
                    this._tabController.RefreshCache(portalId, tabId);
                }
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }
        }

        private Workflow GetCurrentOrDefaultWorkflow(ContentItem item, int portalId)
        {
            if (item.StateID != Null.NullInteger)
            {
                return this._workflowManager.GetWorkflow(item);
            }

            var defaultWorkflow = this._tabWorkflowSettings.GetDefaultTabWorkflowId(portalId);
            return this._workflowManager.GetWorkflow(defaultWorkflow);
        }
    }
}
