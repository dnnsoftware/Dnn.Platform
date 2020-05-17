﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Content;
using DotNetNuke.Entities.Content.Workflow;
using DotNetNuke.Entities.Content.Workflow.Entities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Exceptions;

namespace DotNetNuke.Entities.Tabs
{
    class TabWorkflowTracker : ServiceLocator<ITabChangeTracker, TabWorkflowTracker>, ITabChangeTracker
    {
        private static readonly DnnLogger Logger = DnnLogger.GetClassLogger(typeof(TabWorkflowTracker));
       
        #region Members
        private readonly ITabController _tabController;
        private readonly IWorkflowEngine _workflowEngine;
        private readonly IWorkflowManager _workflowManager;
        private readonly ITabWorkflowSettings _tabWorkflowSettings;
        #endregion
        
        public TabWorkflowTracker()
        {
            _tabController = TabController.Instance;
            _workflowEngine = WorkflowEngine.Instance;
            _workflowManager = WorkflowManager.Instance;
            _tabWorkflowSettings = TabWorkflowSettings.Instance;
        }

        protected override Func<ITabChangeTracker> GetFactory()
        {
            return () => new TabWorkflowTracker();
        }


        /// <summary>
        /// Tracks a workflow instance when a module is added to a page
        /// </summary>
        /// <param name="module">Module which tracks the workflow instance</param>
        /// <param name="moduleVersion">Version number corresponding to the module</param>
        /// <param name="userId">User Id related with the workflow instance</param>  
        public void TrackModuleAddition(ModuleInfo module, int moduleVersion, int userId)
        {
            NotifyWorkflowAboutChanges(module.PortalID, module.TabID, userId);
        }
        
        /// <summary>
        /// Tracks a workflow instance when a module is modified on a page
        /// </summary>
        /// <param name="module">Module which tracks the workflow instance</param>
        /// <param name="moduleVersion">Version number corresponding to the module</param>
        /// <param name="userId">User Id related with the workflow instance</param>  
        public void TrackModuleModification(ModuleInfo module, int moduleVersion, int userId)
        {
            NotifyWorkflowAboutChanges(module.PortalID, module.TabID, userId);
        }


        /// <summary>
        /// Tracks a workflow instance when a module is deleted from a page
        /// </summary>
        /// <param name="module">Module which tracks the workflow instance</param>
        /// <param name="moduleVersion">Version number corresponding to the module</param>
        /// <param name="userId">User Id related with the workflow instance</param>  
        public void TrackModuleDeletion(ModuleInfo module, int moduleVersion, int userId)
        {
            NotifyWorkflowAboutChanges(module.PortalID, module.TabID, userId);
        }

        /// <summary>
        /// Tracks a workflow instance when a module is copied from an exisitng page
        /// </summary>
        /// <param name="module">Module which tracks the workflow instance</param>
        /// <param name="moduleVersion">Version number corresponding to the module</param>
        /// <param name="originalTabId">Tab Id where the module originally is</param>
        /// <param name="userId">User Id related with the workflow instance</param>  
        public void TrackModuleCopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId)
        {
            TrackModuleAddition(module, moduleVersion, userId);
        }


        /// <summary>
        /// Tracks a workflow instance when a copied module is deleted from an exisitng page
        /// </summary>
        /// <param name="module">Module which tracks the workflow instance</param>
        /// <param name="moduleVersion">Version number corresponding to the module</param>
        /// <param name="originalTabId">Tab Id where the module originally is</param>
        /// <param name="userId">User Id related with the workflow instance</param> 
        public void TrackModuleUncopy(ModuleInfo module, int moduleVersion, int originalTabId, int userId)
        {
            TrackModuleDeletion(module, moduleVersion, userId);
        }

        #region Private Statics Methods
        private void NotifyWorkflowAboutChanges(int portalId, int tabId, int userId)
        {
            try
            {
                var tabInfo = _tabController.GetTab(tabId, portalId);
				if (tabInfo!= null && !tabInfo.IsDeleted && _workflowEngine.IsWorkflowCompleted(tabInfo))
                {
                    var workflow = GetCurrentOrDefaultWorkflow(tabInfo, portalId);
                    if (workflow == null)
                    {
                        Logger.Warn("Current Workflow and Default workflow are not found on NotifyWorkflowAboutChanges");
                        return;
                    }

                    _workflowEngine.StartWorkflow(workflow.WorkflowID, tabInfo.ContentItemId, userId);
                    _tabController.RefreshCache(portalId, tabId);
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
                return _workflowManager.GetWorkflow(item);
            }

            var defaultWorkflow = _tabWorkflowSettings.GetDefaultTabWorkflowId(portalId);
            return _workflowManager.GetWorkflow(defaultWorkflow);
        }
        #endregion
    }
}
