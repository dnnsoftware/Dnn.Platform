// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Entities
{
    /// <summary>
    /// This enum represents the possible list of WorkflowLogType.
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
        WorkflowError = 500,
    }
}
