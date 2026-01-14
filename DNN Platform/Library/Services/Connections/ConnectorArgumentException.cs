// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Connections
{
    using System;

    public class ConnectorArgumentException : ApplicationException
    {
        /// <summary>Initializes a new instance of the <see cref="ConnectorArgumentException"/> class.</summary>
        public ConnectorArgumentException()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ConnectorArgumentException"/> class.</summary>
        /// <param name="message">A message that describes the error.</param>
        public ConnectorArgumentException(string message)
            : base(message)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ConnectorArgumentException"/> class.</summary>
        /// <param name="message">A message that describes the error.</param>
        /// <param name="innerException">The exception that is the cause of the current exception. If the <paramref name="innerException"/> is not a <see langword="null" /> reference, the current exception is raised in a catch block that handles the inner exception.</param>
        public ConnectorArgumentException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
