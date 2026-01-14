// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.HttpModules.Exceptions;

using System;
using System.Runtime.Serialization;

/// <summary>An exception which wraps an exception that was not handled.</summary>
public class UnhandledException : Exception
{
    /// <inheritdoc />
    public UnhandledException()
    {
    }

    /// <inheritdoc />
    public UnhandledException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public UnhandledException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected UnhandledException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
