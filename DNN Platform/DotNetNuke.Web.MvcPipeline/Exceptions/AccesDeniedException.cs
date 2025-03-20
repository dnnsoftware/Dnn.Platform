// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class AccesDeniedException : MvcPageException
    {
        public AccesDeniedException()
        {
        }

        public AccesDeniedException(string message, string redirectUrl)
            : base(message, redirectUrl)
        {
        }

        public AccesDeniedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        protected AccesDeniedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
