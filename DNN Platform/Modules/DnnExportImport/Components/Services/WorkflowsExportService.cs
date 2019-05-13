#region Copyright
//
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2018
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

namespace Dnn.ExportImport.Components.Services
{
    public class WorkflowsExportService : BasePortableService
    {
        public override string Category => Constants.Category_Workflows;

        public override string ParentCategory => null;

        public override uint Priority => 6;

        public override int GetImportTotal()
        {
            return Repository.GetCount<TaxonomyVocabulary>() + Repository.GetCount<TaxonomyTerm>();
        }

        #region Exporting

        public override void ExportData(ExportImportJob exportJob, ExportDto exportDto)
        {
            if (CheckPoint.Stage > 0) return;
            if (CheckCancelled(exportJob)) return;

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

                CheckPoint.TotalItems = contentWorkflows.Count;
                Repository.CreateItems(contentWorkflows);
                Result.AddLogEntry("Exported ContentWorkflows", contentWorkflows.Count.ToString());

                foreach (var workflow in contentWorkflows)
                {
                    var contentWorkflowStates = GetWorkflowStates(workflow.WorkflowID);
                    Repository.CreateItems(contentWorkflowStates, workflow.Id);

                    foreach (var workflowState in contentWorkflowStates)
                    {
                        var contentWorkflowStatePermissions = GetWorkflowStatePermissions(workflowState.StateID, toDate, fromDate);
                        Repository.CreateItems(contentWorkflowStatePermissions, workflowState.Id);
                    }
                }
            }

            CheckPoint.Progress = 100;
            CheckPoint.Completed = true;
            CheckPoint.Stage++;
            CheckPoint.StageData = null;
            CheckPointStageCallback(this);
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

        #endregion

        #region Importing

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
            if (CheckCancelled(importJob) || CheckPoint.Stage >= 1 || CheckPoint.Completed || CheckPointStageCallback(this))
            {
                return;
            }

            var workflowManager = WorkflowManager.Instance;
            var workflowStateManager = WorkflowStateManager.Instance;
            var portalId = importJob.PortalId;
            var importWorkflows = Repository.GetAllItems<ExportWorkflow>().ToList();
            var existWorkflows = workflowManager.GetWorkflows(portalId).ToList();
            var defaultTabWorkflowId = importWorkflows.FirstOrDefault(w => w.IsDefault)?.WorkflowID ?? 1;
            CheckPoint.TotalItems = CheckPoint.TotalItems <= 0 ? importWorkflows.Count : CheckPoint.TotalItems;

            #region importing workflows

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
                            Result.AddLogEntry("Updated workflow", workflow.WorkflowName);
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
                    Result.AddLogEntry("Added workflow", workflow.WorkflowName);

                    if (importWorkflow.WorkflowID == defaultTabWorkflowId)
                    {
                        TabWorkflowSettings.Instance.SetDefaultTabWorkflowId(portalId, workflow.WorkflowID);
                    }
                }

                importWorkflow.LocalId = workflow.WorkflowID;

                #region importing workflow states

                var importStates = Repository.GetRelatedItems<ExportWorkflowState>(importWorkflow.Id).ToList();
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
                            Result.AddLogEntry("Updated workflow state", workflowState.StateID.ToString());
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
                            SendNotificationToAdministrators = importState.SendNotificationToAdministrators
                        };
                        WorkflowStateManager.Instance.AddWorkflowState(workflowState);
                        Result.AddLogEntry("Added workflow state", workflowState.StateID.ToString());
                    }
                    importState.LocalId = workflowState.StateID;

                    #region importin permissions

                    if (!workflowState.IsSystem)
                    {
                        var importPermissions = Repository.GetRelatedItems<ExportWorkflowStatePermission>(importState.Id).ToList();
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
                                        Result.AddLogEntry("Couldn't add tab permission; User is undefined!",
                                            $"{importPermission.PermissionKey} - {importPermission.PermissionID}", ReportLevel.Warn);
                                        continue;
                                    }
                                    permission.UserID = userId.Value;
                                }

                                if (importPermission.RoleID != null && importPermission.RoleID > noRole && !string.IsNullOrEmpty(importPermission.RoleName))
                                {
                                    if (roleId == null)
                                    {
                                        Result.AddLogEntry("Couldn't add tab permission; Role is undefined!",
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
                                        Result.AddLogEntry("Added workflow state permission",
                                            permission.WorkflowStatePermissionID.ToString());
                                    }
                                    else
                                    {
                                        importPermission.LocalId = local.WorkflowStatePermissionID;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Result.AddLogEntry("Exception adding workflow state permission", ex.Message, ReportLevel.Error);
                                }
                            }
                        }
                    }

                    #endregion
                }

                #endregion

                Repository.UpdateItems(importStates);
                Result.AddSummary("Imported Workflow", importWorkflows.Count.ToString());
                CheckPoint.ProcessedItems++;
                CheckPointStageCallback(this); // no need to return; very small amount of data processed
            }

            #endregion

            Repository.UpdateItems(importWorkflows);

            CheckPoint.Stage++;
            CheckPoint.StageData = null;
            CheckPoint.Progress = 100;
            CheckPoint.TotalItems = importWorkflows.Count;
            CheckPoint.ProcessedItems = importWorkflows.Count;
            CheckPointStageCallback(this);
        }

        #endregion
    }
}