// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Providers.Caching.SimpleWebFarmCachingProvider;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when there's an issue with the simple web farm cache sync.</summary>
[Serializable]
public class SyncException : ApplicationException
{
    /// <inheritdoc />
    public SyncException()
    {
    }

    /// <inheritdoc />
    public SyncException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public SyncException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected SyncException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
