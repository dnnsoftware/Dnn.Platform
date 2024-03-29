﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Actions
{
    /// <summary>This enum represents the workflow action types.</summary>
    public enum WorkflowActionTypes
    {
        DiscardWorkflow = 0,
        CompleteWorkflow = 1,
        DiscardState = 2,
        CompleteState = 3,
        StartWorkflow = 4,
    }
}
