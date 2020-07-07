// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using System.Security.Permissions;

    public class PageLoadException : BasePortalException
    {
        // default constructor
        public PageLoadException()
        {
        }

        // constructor with exception message
        public PageLoadException(string message)
            : base(message)
        {
        }

        // constructor with message and inner exception
        public PageLoadException(string message, Exception inner)
            : base(message, inner)
        {
        }

        protected PageLoadException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
