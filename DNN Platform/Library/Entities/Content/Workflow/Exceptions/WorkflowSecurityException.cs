// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Exceptions;

public class WorkflowSecurityException : WorkflowException
{
    /// <summary>Initializes a new instance of the <see cref="WorkflowSecurityException"/> class.</summary>
    /// <param name="message"></param>
    public WorkflowSecurityException(string message)
        : base(message)
    {
    }
}
