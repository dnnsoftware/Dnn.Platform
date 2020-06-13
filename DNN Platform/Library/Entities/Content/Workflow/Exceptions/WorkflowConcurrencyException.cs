// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content.Workflow.Exceptions
{
    using DotNetNuke.Services.Localization;

    public class WorkflowConcurrencyException : WorkflowException
    {
        public WorkflowConcurrencyException()
            : base(Localization.GetString("WorkflowConcurrencyException", Localization.ExceptionsResourceFile))
        {
        }
    }
}
