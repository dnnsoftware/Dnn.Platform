// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Portals;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when there's an error while creating a portal.</summary>
[Serializable]
public class CreatePortalException : Exception
{
    /// <inheritdoc />
    public CreatePortalException()
    {
    }

    /// <inheritdoc />
    public CreatePortalException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public CreatePortalException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected CreatePortalException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
