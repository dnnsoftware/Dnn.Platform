// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.ExportImport.Components.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.ExportImport.Components.Common;
    using Dnn.ExportImport.Components.Dto;
    using Dnn.ExportImport.Components.Entities;
    using Dnn.ExportImport.Components.Providers;
    using Dnn.ExportImport.Dto.Taxonomy;
    using Dnn.ExportImport.Dto.Workflow;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Content.Workflow;
    using DotNetNuke.Entities.Content.Workflow.Entities;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Entities.Users;

    public class WorkflowsExportService : BasePortableService
    {
        public override string Category => Constants.Category_Workflows;

        public override string ParentCategory => null;

        public override uint Priority => 6;

        public override int GetImportTotal()
        {
            return this.Repository.GetCount<TaxonomyVocabulary>() + this.Repository.GetCount<TaxonomyTerm>();
        }

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            if (this.CheckPoint.Stage > 0)
            {
                return;
            }

            if (this.CheckCancelled(exportJob))
            {
                return;
            }

            var fromDate = (exportDto.FromDateUtc ?? Constants.MinDbTime).ToLocalTime();
            var toDate = exportDto.ToDateUtc.ToLocalTime();

            var contentWorkflows = GetWorkflows(exportDto.PortalId, exportDto.IncludeDeletions);
            if (contentWorkflows.Count > 0)
            {
                var defaultWorkflowId = TabWorkflowSettings.Instance.GetDefaultTabWorkflowId(exportDto.PortalId);
                var defaultWorkflow = contentWorkflows.FirstOrDefault(w => w.WorkflowID == defaultWorkflowId);
                if (defaultWorkflow != null)
                {
                    defaultWorkflow.IsDefault = true;
                }

                this.CheckPoint.TotalItems = contentWorkflows.Count;
                this.Repository.CreateItems(contentWorkflows);
                this.Result.AddLogEntry("Exported ContentWorkflows", contentWorkflows.Count.ToString());

                foreach (var workflow in contentWorkflows)
                {
                    var contentWorkflowStates = GetWorkflowStates(workflow.WorkflowID);
                    this.Repository.CreateItems(contentWorkflowStates, workflow.Id);

                    foreach (var workflowState in contentWorkflowStates)
                    {
                        var contentWorkflowStatePermissions = GetWorkflowStatePermissions(workflowState.StateID, toDate, fromDate);
                        this.Repository.CreateItems(contentWorkflowStatePermissions, workflowState.Id);
                    }
                }
            }

            this.CheckPoint.Progress = 100;
            this.CheckPoint.Completed = true;
            this.CheckPoint.Stage++;
            this.CheckPoint.StageData = null;
            this.CheckPointStageCallback(this);
        }

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (this.CheckCancelled(importJob) || this.CheckPoint.Stage >= 1 || this.CheckPoint.Completed || this.CheckPointStageCallback(this))
            {
                return;
            }

            var workflowManager = WorkflowManager.Instance;
            var workflowStateManager = WorkflowStateManager.Instance;
            var portalId = importJob.PortalId;
            var importWorkflows = this.Repository.GetAllItems<ExportWorkflow>().ToList();
            var existWorkflows = workflowManager.GetWorkflows(portalId).ToList();
            var defaultTabWorkflowId = importWorkflows.FirstOrDefault(w => w.IsDefault)?.WorkflowID ?? 1;
            this.CheckPoint.TotalItems = this.CheckPoint.TotalItems <= 0 ? importWorkflows.Count : this.CheckPoint.TotalItems;
            foreach (var importWorkflow in importWorkflows)
            {
                var workflow = existWorkflows.FirstOrDefault(w => w.WorkflowName == importWorkflow.WorkflowName);
                if (workflow != null)
                {
                    if (!importWorkflow.IsSystem && importDto.CollisionResolution == CollisionResolution.Overwrite)
                    {
                        if (workflow.Description != importWorkflow.Description ||
                            workflow.WorkflowKey != importWorkflow.WorkflowKey)
                        {
                            workflow.Description = importWorkflow.Description;
                            workflow.WorkflowKey = importWorkflow.WorkflowKey;
                            workflowManager.UpdateWorkflow(workflow);
                            this.Result.AddLogEntry("Updated workflow", workflow.WorkflowName);
                        }
                    }
                }
                else
                {
                    workflow = new Workflow
                    {
                        PortalID = portalId,
                        WorkflowName = importWorkflow.WorkflowName,
                        Description = importWorkflow.Description,
                        WorkflowKey = importWorkflow.WorkflowKey,
                    };

                    workflowManager.AddWorkflow(workflow);
                    this.Result.AddLogEntry("Added workflow", workflow.WorkflowName);

                    if (importWorkflow.WorkflowID == defaultTabWorkflowId)
                    {
                        TabWorkflowSettings.Instance.SetDefaultTabWorkflowId(portalId, workflow.WorkflowID);
                    }
                }

                importWorkflow.LocalId = workflow.WorkflowID;
                var importStates = this.Repository.GetRelatedItems<ExportWorkflowState>(importWorkflow.Id).ToList();
                foreach (var importState in importStates)
                {
                    var workflowState = workflow.States.FirstOrDefault(s => s.StateName == importState.StateName);
                    if (workflowState != null)
                    {
                        if (!workflowState.IsSystem)
                        {
                            workflowState.Order = importState.Order;
                            workflowState.IsSystem = false;
                            workflowState.SendNotification = importState.SendNotification;
                            workflowState.SendNotificationToAdministrators = importState.SendNotificationToAdministrators;
                            workflowStateManager.UpdateWorkflowState(workflowState);
                            this.Result.AddLogEntry("Updated workflow state", workflowState.StateID.ToString());
                        }
                    }
                    else
                    {
                        workflowState = new WorkflowState
                        {
                            StateName = importState.StateName,
                            WorkflowID = workflow.WorkflowID,
                            Order = importState.Order,
                            IsSystem = importState.IsSystem,
                            SendNotification = importState.SendNotification,
                            SendNotificationToAdministrators = importState.SendNotificationToAdministrators,
                        };
                        WorkflowStateManager.Instance.AddWorkflowState(workflowState);
                        this.Result.AddLogEntry("Added workflow state", workflowState.StateID.ToString());
                    }

                    importState.LocalId = workflowState.StateID;
                    if (!workflowState.IsSystem)
                    {
                        var importPermissions = this.Repository.GetRelatedItems<ExportWorkflowStatePermission>(importState.Id).ToList();
                        foreach (var importPermission in importPermissions)
                        {
                            var permissionId = DataProvider.Instance().GetPermissionId(
                                importPermission.PermissionCode, importPermission.PermissionKey, importPermission.PermissionName);

                            if (permissionId != null)
                            {
                                var noRole = Convert.ToInt32(Globals.glbRoleNothing);
                                var userId = UserController.GetUserByName(importDto.PortalId, importPermission.Username)?.UserID;
                                var roleId = Util.GetRoleIdByName(importDto.PortalId, importPermission.RoleID ?? noRole, importPermission.RoleName);

                                var permission = new WorkflowStatePermission
                                {
                                    PermissionID = permissionId ?? -1,
                                    StateID = workflowState.StateID,
                                    RoleID = noRole,
                                    UserID = -1,
                                    AllowAccess = importPermission.AllowAccess,
                                };

                                if (importPermission.UserID != null && importPermission.UserID > 0 && !string.IsNullOrEmpty(importPermission.Username))
                                {
                                    if (userId == null)
                                    {
                                        this.Result.AddLogEntry(
                                            "Couldn't add tab permission; User is undefined!",
                                            $"{importPermission.PermissionKey} - {importPermission.PermissionID}", ReportLevel.Warn);
                                        continue;
                                    }

                                    permission.UserID = userId.Value;
                                }

                                if (importPermission.RoleID != null && importPermission.RoleID > noRole && !string.IsNullOrEmpty(importPermission.RoleName))
                                {
                                    if (roleId == null)
                                    {
                                        this.Result.AddLogEntry(
                                            "Couldn't add tab permission; Role is undefined!",
                                            $"{importPermission.PermissionKey} - {importPermission.PermissionID}", ReportLevel.Warn);
                                        continue;
                                    }

                                    permission.RoleID = roleId.Value;
                                }

                                try
                                {
                                    var existingPermissions = workflowStateManager.GetWorkflowStatePermissionByState(workflowState.StateID);
                                    var local = existingPermissions.FirstOrDefault(
                                        x => x.PermissionCode == importPermission.PermissionCode && x.PermissionKey == importPermission.PermissionKey
                                        && x.PermissionName.Equals(importPermission.PermissionName, StringComparison.InvariantCultureIgnoreCase) &&
                                        x.RoleID == roleId && x.UserID == userId);

                                    if (local == null)
                                    {
                                        workflowStateManager.AddWorkflowStatePermission(permission, -1);
                                        importPermission.LocalId = permission.WorkflowStatePermissionID;
                                        this.Result.AddLogEntry(
                                            "Added workflow state permission",
                                            permission.WorkflowStatePermissionID.ToString());
                                    }
                                    else
                                    {
                                        importPermission.LocalId = local.WorkflowStatePermissionID;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    this.Result.AddLogEntry("Exception adding workflow state permission", ex.Message, ReportLevel.Error);
                                }
                            }
                        }
                    }
                }

                this.Repository.UpdateItems(importStates);
                this.Result.AddSummary("Imported Workflow", importWorkflows.Count.ToString());
                this.CheckPoint.ProcessedItems++;
                this.CheckPointStageCallback(this); // no need to return; very small amount of data processed
            }

            this.Repository.UpdateItems(importWorkflows);

            this.CheckPoint.Stage++;
            this.CheckPoint.StageData = null;
            this.CheckPoint.Progress = 100;
            this.CheckPoint.TotalItems = importWorkflows.Count;
            this.CheckPoint.ProcessedItems = importWorkflows.Count;
            this.CheckPointStageCallback(this);
        }

        private static List<ExportWorkflow> GetWorkflows(int portalId, bool includeDeletions)
        {
            return CBO.FillCollection<ExportWorkflow>(
                DataProvider.Instance().GetAllWorkflows(portalId, includeDeletions));
        }

        private static List<ExportWorkflowState> GetWorkflowStates(int workflowId)
        {
            return CBO.FillCollection<ExportWorkflowState>(
                DataProvider.Instance().GetAllWorkflowStates(workflowId));
        }

        private static List<ExportWorkflowStatePermission> GetWorkflowStatePermissions(
            int stateId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.FillCollection<ExportWorkflowStatePermission>(
                DataProvider.Instance().GetAllWorkflowStatePermissions(stateId, toDate, fromDate));
        }
    }
}
