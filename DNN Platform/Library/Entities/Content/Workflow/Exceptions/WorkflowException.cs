// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;

namespace DotNetNuke.Entities.Content.Workflow.Exceptions
{
    public class WorkflowException : ApplicationException
    {
        public WorkflowException()
        {
            
        }

        public WorkflowException(string message) : base(message)
        {
            
        }

        public WorkflowException(string message, Exception innerException) : base(message, innerException)
        {

        }
    }
}
