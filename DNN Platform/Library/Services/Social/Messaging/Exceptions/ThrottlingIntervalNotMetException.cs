// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Messaging.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ThrottlingIntervalNotMetException : Exception
    {
        /// <summary>Initializes a new instance of the <see cref="ThrottlingIntervalNotMetException"/> class.</summary>
        public ThrottlingIntervalNotMetException()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ThrottlingIntervalNotMetException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        public ThrottlingIntervalNotMetException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ThrottlingIntervalNotMetException"/> class.</summary>
        /// <param name="message">The message that describes the error.</param>
        /// <param name="inner">The exception that is the cause of the current exception. If the <paramref name="inner"/> is not a <see langword="null" /> reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public ThrottlingIntervalNotMetException(string message, Exception inner)
            : base(message, inner)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ThrottlingIntervalNotMetException"/> class.</summary>
        /// <param name="info">The serialization info.</param>
        /// <param name="context">The streaming context.</param>
        public ThrottlingIntervalNotMetException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
