// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Internal;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when an auth handler is invalid.</summary>
[Serializable]
public class InvalidAuthHandlerException : Exception
{
    /// <inheritdoc />
    public InvalidAuthHandlerException()
    {
    }

    /// <inheritdoc />
    public InvalidAuthHandlerException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public InvalidAuthHandlerException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected InvalidAuthHandlerException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
