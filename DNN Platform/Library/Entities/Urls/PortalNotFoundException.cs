// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Urls;

using System;
using System.Runtime.Serialization;

/// <summary>An exception throw when a portal is requested that could not be found.</summary>
[Serializable]
public class PortalNotFoundException : Exception
{
    /// <inheritdoc />
    public PortalNotFoundException()
    {
    }

    /// <inheritdoc />
    public PortalNotFoundException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public PortalNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected PortalNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
