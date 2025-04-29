// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Exceptions;

using System;

public class WorkflowException : ApplicationException
{
    /// <summary>Initializes a new instance of the <see cref="WorkflowException"/> class.</summary>
    public WorkflowException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="WorkflowException"/> class.</summary>
    /// <param name="message"></param>
    public WorkflowException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="WorkflowException"/> class.</summary>
    /// <param name="message"></param>
    /// <param name="innerException"></param>
    public WorkflowException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
