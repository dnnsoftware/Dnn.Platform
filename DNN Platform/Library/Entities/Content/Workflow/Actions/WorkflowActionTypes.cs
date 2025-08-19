// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Actions
{
    /// <summary>This enum represents the workflow action types.</summary>
    public enum WorkflowActionTypes
    {
        /// <summary>Discards all edits in the workflow.</summary>
        DiscardWorkflow = 0,

        /// <summary>Completes the workflow.</summary>
        CompleteWorkflow = 1,

        /// <summary>Discards the current state of the workflow.</summary>
        DiscardState = 2,

        /// <summary>Completes the current state of the workflow.</summary>
        CompleteState = 3,

        /// <summary>Starts the workflow.</summary>
        StartWorkflow = 4,
    }
}
