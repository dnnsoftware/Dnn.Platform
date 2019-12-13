// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Content.Workflow.Exceptions
{
    public class WorkflowConcurrencyException : WorkflowException
    {
        public WorkflowConcurrencyException()
            : base(Localization.GetString("WorkflowConcurrencyException", Localization.ExceptionsResourceFile))
        {

        }
    }
}
