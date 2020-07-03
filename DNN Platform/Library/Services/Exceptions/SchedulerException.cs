// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    public class SchedulerException : BasePortalException
    {
        // default constructor
        public SchedulerException()
        {
        }

        // constructor with exception message
        public SchedulerException(string message)
            : base(message)
        {
        }

        // constructor with message and inner exception
        public SchedulerException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected SchedulerException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
