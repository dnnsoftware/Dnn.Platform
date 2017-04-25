#region Copyright
//
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2017
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
using Dnn.ExportImport.Components.Common;
using Dnn.ExportImport.Components.Dto;
using Dnn.ExportImport.Components.Entities;
using Dnn.ExportImport.Components.Providers;
using Dnn.ExportImport.Dto.Taxonomy;
using Dnn.ExportImport.Dto.Workflow;
using DotNetNuke.Common.Utilities;

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
                CheckPoint.TotalItems = contentWorkflows.Count;
                Repository.CreateItems(contentWorkflows);
                Result.AddLogEntry("Exported ContentWorkflows", contentWorkflows.Count.ToString());

                foreach (var workflow in contentWorkflows)
                {
                    var contentWorkflowSources = GetWorkflowSources(workflow.WorkflowID);
                    Repository.CreateItems(contentWorkflowSources, workflow.Id);
                    CheckPoint.ProcessedItems++;

                    var contentWorkflowStates = GetWorkflowStates(workflow.WorkflowID);
                    Repository.CreateItems(contentWorkflowStates, workflow.Id);

                    foreach (var workflowState in contentWorkflowStates)
                    {
                        var contentWorkflowStatePermissions = GetWorkflowStatePermissions(workflowState.Id, toDate, fromDate);
                        try
                        {
                            //TODO: check why this throws exception
                            Repository.CreateItems(contentWorkflowStatePermissions, workflowState.Id);

                        }
                        catch (Exception e)
                        {
                            Console.WriteLine(e);
                        }
                    }
                }
            }

            CheckPoint.Progress = 100;
            CheckPoint.Completed = true;
            CheckPoint.Stage++;
            CheckPoint.StageData = null;
            CheckPointStageCallback(this);
        }

        private static List<ExportContentWorkflow> GetWorkflows(int portalId, bool includeDeletions)
        {
            return CBO.FillCollection<ExportContentWorkflow>(
                DataProvider.Instance().GetAllWorkflows(portalId, includeDeletions));
        }

        private static List<ExportContentWorkflowSource> GetWorkflowSources(int workflowId)
        {
            return CBO.FillCollection<ExportContentWorkflowSource>(
                DataProvider.Instance().GetAllWorkflowSources(workflowId));
        }

        private static List<ExportContentWorkflowState> GetWorkflowStates(int workflowId)
        {
            return CBO.FillCollection<ExportContentWorkflowState>(
                DataProvider.Instance().GetAllWorkflowStates(workflowId));
        }

        private static List<ExportContentWorkflowStatePermission> GetWorkflowStatePermissions(
            int stateId, DateTime toDate, DateTime? fromDate)
        {
            return CBO.FillCollection<ExportContentWorkflowStatePermission>(
                DataProvider.Instance().GetAllWorkflowStatePermissions(stateId, toDate, fromDate));
        }

        #endregion

        #region Importing

        public override void ImportData(ExportImportJob importJob, ImportDto importDto)
        {
        }

        #endregion
    }
}