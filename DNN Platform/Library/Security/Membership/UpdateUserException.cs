// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Security.Membership;

using System;
using System.Runtime.Serialization;

/// <summary>An exception throw when there's an error updating a user.</summary>
[Serializable]
public class UpdateUserException : Exception
{
    /// <inheritdoc />
    public UpdateUserException()
    {
    }

    /// <inheritdoc />
    public UpdateUserException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public UpdateUserException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected UpdateUserException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
