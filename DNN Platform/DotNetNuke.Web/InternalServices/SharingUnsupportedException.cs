// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when attempting to share a module that does not support sharing.</summary>
[Serializable]
public class SharingUnsupportedException : Exception
{
    /// <inheritdoc />
    public SharingUnsupportedException()
    {
    }

    /// <inheritdoc />
    public SharingUnsupportedException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public SharingUnsupportedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected SharingUnsupportedException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
