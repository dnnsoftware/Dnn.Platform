// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
// ReSharper disable CheckNamespace
namespace DotNetNuke.Entities.Content.Workflow
// ReSharper enable CheckNamespace
{
    /// <summary>
    /// This enum represents the possible list of WorkflowLogType
    /// </summary>
    [Obsolete("Deprecated in Platform 7.4.0. Scheduled removal in v10.0.0.")]   
    public enum ContentWorkflowLogType
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
