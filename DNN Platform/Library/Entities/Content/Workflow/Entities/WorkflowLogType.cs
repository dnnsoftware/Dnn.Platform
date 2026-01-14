// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Entities
{
    /// <summary>This enum represents the possible types of workflow logs.</summary>
    public enum WorkflowLogType
    {
        /// <summary>The workflow was started.</summary>
        WorkflowStarted = 0,

        /// <summary>The workflow state was completed.</summary>
        StateCompleted = 1,

        /// <summary>A draft was completed.</summary>
        DraftCompleted = 2,

        /// <summary>A workflow state was discarded.</summary>
        StateDiscarded = 3,

        /// <summary>A workflow state was initiated.</summary>
        StateInitiated = 4,

        /// <summary>A workflow was approved.</summary>
        WorkflowApproved = 5,

        /// <summary>An entire workflow was discarded.</summary>
        WorkflowDiscarded = 6,

        /// <summary>A comment was provided.</summary>
        CommentProvided = 10,

        /// <summary>There was an error with a workflow.</summary>
        WorkflowError = 500,
    }
}
