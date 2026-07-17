// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Exception thrown when access to an MVC page or resource is denied.
    /// </summary>
    [Serializable]
    public class AccesDeniedException : MvcPageException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AccesDeniedException"/> class.
        /// </summary>
        public AccesDeniedException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccesDeniedException"/> class with a specified error message and redirect URL.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="redirectUrl">The URL to which the user should be redirected.</param>
        public AccesDeniedException(string message, string redirectUrl)
            : base(message, redirectUrl)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccesDeniedException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public AccesDeniedException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="AccesDeniedException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The serialization information.</param>
        /// <param name="context">The streaming context.</param>
        protected AccesDeniedException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
