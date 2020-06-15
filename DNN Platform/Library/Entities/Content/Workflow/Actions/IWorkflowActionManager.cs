// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Actions
{
    using System;

    /// <summary>
    /// This class is responsible to manage Workflow Actions.
    /// </summary>
    public interface IWorkflowActionManager
    {
        /// <summary>
        /// This method gets an instance of the IWorkflowAction associated to the content type and action type.
        /// </summary>
        /// <param name="contentTypeId">Content Item Id.</param>
        /// <param name="actionType">Action type.</param>
        /// <returns>IWorkflowAction instance.</returns>
        IWorkflowAction GetWorkflowActionInstance(int contentTypeId, WorkflowActionTypes actionType);

        /// <summary>
        /// This method registers a new workflow action.
        /// </summary>
        /// <remarks>This method checks that the WorkflowAction Source implements the IWorkflowAction interface before register it.</remarks>
        /// <param name="workflowAction">Workflow action entity.</param>
        /// <exception cref="ArgumentException">Thrown if the ActionSource does not implement the IWorkflowAction interface.</exception>
        void RegisterWorkflowAction(WorkflowAction workflowAction);
    }
}
