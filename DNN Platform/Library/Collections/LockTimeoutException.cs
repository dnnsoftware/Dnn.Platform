// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Collections.Internal;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when the operation of obtaining a lock times out.</summary>
[Serializable]
public class LockTimeoutException : ApplicationException
{
    /// <inheritdoc />
    public LockTimeoutException()
    {
    }

    /// <inheritdoc />
    public LockTimeoutException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public LockTimeoutException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected LockTimeoutException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
