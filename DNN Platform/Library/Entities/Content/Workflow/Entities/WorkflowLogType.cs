// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
namespace DotNetNuke.Entities.Content.Workflow.Entities
{
    /// <summary>
    /// This enum represents the possible list of WorkflowLogType
    /// </summary>
    public enum WorkflowLogType
    {
        WorkflowStarted = 0,
        StateCompleted = 1,
        DraftCompleted = 2,
        StateDiscarded = 3,
        StateInitiated = 4,
        WorkflowApproved = 5,
        WorkflowDiscarded = 6,
        CommentProvided = 10,
        WorkflowError = 500
    }
}
