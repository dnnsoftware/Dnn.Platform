// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Exceptions;

public class WorkflowInvalidOperationException : WorkflowException
{
    /// <summary>Initializes a new instance of the <see cref="WorkflowInvalidOperationException"/> class.</summary>
    /// <param name="message"></param>
    public WorkflowInvalidOperationException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="WorkflowInvalidOperationException"/> class.</summary>
    public WorkflowInvalidOperationException()
    {
    }
}
