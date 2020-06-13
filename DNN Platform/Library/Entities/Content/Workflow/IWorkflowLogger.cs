// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow
{
    using System.Collections.Generic;

    using DotNetNuke.Entities.Content.Workflow.Entities;

    /// <summary>
    /// This class is responsible to manage the workflows logs.
    /// It provides addition and get operation methods.
    /// </summary>
    public interface IWorkflowLogger
    {
        /// <summary>
        /// Adds a log comment regarding a specific workflow.
        /// </summary>
        /// <param name="contentItemId">Content item Id related with the log.</param>
        /// <param name="workflowId">Workflow Id owner of the log.</param>
        /// <param name="type">Log Type.</param>
        /// <param name="comment">Comment to be added.</param>
        /// <param name="userId">User Id who adds the log.</param>
        void AddWorkflowLog(int contentItemId, int workflowId, WorkflowLogType type, string comment, int userId);

        /// <summary>
        /// Adds a log comment regarding a specific workflow.
        /// </summary>
        /// <param name="contentItemId">Content item Id related with the log.</param>
        /// <param name="workflowId">Workflow Id owner of the log.</param>
        /// <param name="action">Custom action related with the log.</param>
        /// <param name="comment">Comment to be added.</param>
        /// <param name="userId">User Id who adds the log.</param>
        void AddWorkflowLog(int contentItemId, int workflowId, string action, string comment, int userId);

        /// <summary>
        /// Gets all logs regarding a specific workflow.
        /// </summary>
        /// <param name="contentItemId">Content item Id related with the logs.</param>
        /// <param name="workflowId">Workflow Id owner of logs.</param>
        /// <returns></returns>
        IEnumerable<WorkflowLog> GetWorkflowLogs(int contentItemId, int workflowId);
    }
}
