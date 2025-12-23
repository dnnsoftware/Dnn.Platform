// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    /// <summary>
    /// Base exception type for MVC page errors within the MVC pipeline.
    /// </summary>
    public class MvcPageException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MvcPageException"/> class.
        /// </summary>
        public MvcPageException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MvcPageException"/> class with a specified error message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public MvcPageException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MvcPageException"/> class with a specified error message and redirect URL.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="redirectUrl">The URL to which the user should be redirected.</param>
        public MvcPageException(string message, string redirectUrl)
            : base(message)
        {
            this.RedirectUrl = redirectUrl;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MvcPageException"/> class with a specified error message and a reference to the inner exception that is the cause of this exception.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The inner exception.</param>
        public MvcPageException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MvcPageException"/> class with serialized data.
        /// </summary>
        /// <param name="info">The serialization information.</param>
        /// <param name="context">The streaming context.</param>
        protected MvcPageException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        /// Gets the URL to which the user should be redirected.
        /// </summary>
        public string RedirectUrl { get; private set; }
    }
}
