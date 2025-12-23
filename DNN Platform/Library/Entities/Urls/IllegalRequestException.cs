// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when processing a request that is illegal (e.g. it matches multiple pages).</summary>
[Serializable]
public class IllegalRequestException : Exception
{
    /// <inheritdoc />
    public IllegalRequestException()
    {
    }

    /// <inheritdoc />
    public IllegalRequestException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public IllegalRequestException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected IllegalRequestException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
