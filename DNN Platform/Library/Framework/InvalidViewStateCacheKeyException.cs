// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Framework;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when the view state cache key is missing or invalid.</summary>
[Serializable]
public class InvalidViewStateCacheKeyException : ApplicationException
{
    /// <inheritdoc />
    public InvalidViewStateCacheKeyException()
    {
    }

    /// <inheritdoc />
    public InvalidViewStateCacheKeyException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public InvalidViewStateCacheKeyException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected InvalidViewStateCacheKeyException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
