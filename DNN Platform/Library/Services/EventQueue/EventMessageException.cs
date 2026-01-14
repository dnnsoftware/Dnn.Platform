// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.EventQueue;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when there's an error processing an <see cref="EventMessage"/>.</summary>
[Serializable]
public class EventMessageException : Exception
{
    /// <inheritdoc />
    public EventMessageException()
    {
    }

    /// <inheritdoc />
    public EventMessageException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public EventMessageException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected EventMessageException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
