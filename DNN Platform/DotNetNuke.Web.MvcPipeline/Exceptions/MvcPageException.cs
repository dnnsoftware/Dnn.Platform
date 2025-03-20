// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    public class MvcPageException : Exception
    {
        public MvcPageException()
        {
        }

        public MvcPageException(string message)
            : base(message)
        {
        }

        public MvcPageException(string message, string redirectUrl)
            : base(message)
        {
            this.RedirectUrl = redirectUrl;
        }

        public MvcPageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected MvcPageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public string RedirectUrl { get; private set; }
    }
}
