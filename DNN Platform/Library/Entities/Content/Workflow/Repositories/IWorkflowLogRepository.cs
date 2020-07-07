// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    using System.Collections.Generic;

    using DotNetNuke.Entities.Content.Workflow.Entities;

    /// <summary>
    /// This class is responsible to persist and retrieve workflow log entity.
    /// </summary>
    internal interface IWorkflowLogRepository
    {
        /// <summary>
        /// This method hard delete all the workflow log of a content items.
        /// </summary>
        /// <param name="contentItemId">Content item Id.</param>
        /// <param name="workflowId">Workflow Id.</param>
        void DeleteWorkflowLogs(int contentItemId, int workflowId);

        /// <summary>
        /// This method persists a workflow log entity.
        /// </summary>
        /// <param name="workflowLog">WorkflowLog entity.</param>
        void AddWorkflowLog(WorkflowLog workflowLog);

        /// <summary>
        /// This method gets all the Content workflow logs.
        /// </summary>
        /// <param name="contentItemId">Content item Id.</param>
        /// <param name="workflowId">Workflow Id.</param>
        /// <returns></returns>
        IEnumerable<WorkflowLog> GetWorkflowLogs(int contentItemId, int workflowId);
    }
}
