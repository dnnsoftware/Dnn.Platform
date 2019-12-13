﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Entities.Content.Workflow.Actions
{
    /// <summary>
    /// This class is responsible to manage Workflow Actions
    /// </summary>
    public interface IWorkflowActionManager
    {
        /// <summary>
        /// This method gets an instance of the IWorkflowAction associated to the content type and action type
        /// </summary>
        /// <param name="contentTypeId">Content Item Id</param>
        /// <param name="actionType">Action type</param>
        /// <returns>IWorkflowAction instance</returns>
        IWorkflowAction GetWorkflowActionInstance(int contentTypeId, WorkflowActionTypes actionType);

        /// <summary>
        /// This method registers a new workflow action
        /// </summary>
        /// <remarks>This method checks that the WorkflowAction Source implements the IWorkflowAction interface before register it</remarks>
        /// <param name="workflowAction">Workflow action entity</param>
        /// <exception cref="ArgumentException">Thrown if the ActionSource does not implement the IWorkflowAction interface</exception>
        void RegisterWorkflowAction(WorkflowAction workflowAction);
    }
}
