// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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
