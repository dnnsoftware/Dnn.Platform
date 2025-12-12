// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.HttpModules.RequestFilter;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when there's an issue processing request filters.</summary>
[Serializable]
public class RequestFilterException : Exception
{
    /// <inheritdoc />
    public RequestFilterException()
    {
    }

    /// <inheritdoc />
    public RequestFilterException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public RequestFilterException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected RequestFilterException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
