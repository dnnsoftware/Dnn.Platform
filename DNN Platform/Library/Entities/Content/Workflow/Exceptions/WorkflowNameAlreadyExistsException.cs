// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using DotNetNuke.Services.Localization;

namespace DotNetNuke.Entities.Content.Workflow.Exceptions
{
    public class WorkflowNameAlreadyExistsException : WorkflowException
    {
        public WorkflowNameAlreadyExistsException()
            : base(Localization.GetString("WorkflowNameAlreadyExistsException", Localization.ExceptionsResourceFile))
        {
            
        }
    }
}
