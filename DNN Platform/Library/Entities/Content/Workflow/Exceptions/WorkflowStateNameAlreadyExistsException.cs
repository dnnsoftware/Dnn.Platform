// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Exceptions;

using DotNetNuke.Services.Localization;

public class WorkflowStateNameAlreadyExistsException : WorkflowException
{
    /// <summary>Initializes a new instance of the <see cref="WorkflowStateNameAlreadyExistsException"/> class.</summary>
    public WorkflowStateNameAlreadyExistsException()
        : base(Localization.GetString("WorkflowStateNameAlreadyExistsException", Localization.ExceptionsResourceFile))
    {
    }
}
