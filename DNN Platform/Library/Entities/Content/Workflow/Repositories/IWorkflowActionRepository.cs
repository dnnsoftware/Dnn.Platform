// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Repositories
{
    using DotNetNuke.Entities.Content.Workflow.Actions;

    /// <summary>
    /// This class is responsible to persist and retrieve workflow action entity.
    /// </summary>
    internal interface IWorkflowActionRepository
    {
        /// <summary>
        /// This method gets the workflow action of a content type Id and action type.
        /// </summary>
        /// <param name="contentTypeId">Content Item Id.</param>
        /// <param name="actionType">Action type.</param>
        /// <returns>Workflow action entity.</returns>
        WorkflowAction GetWorkflowAction(int contentTypeId, string actionType);

        /// <summary>
        /// This method persists a new workflow action.
        /// </summary>
        /// <param name="workflowAction">Workflow action entity.</param>
        void AddWorkflowAction(WorkflowAction workflowAction);
    }
}
