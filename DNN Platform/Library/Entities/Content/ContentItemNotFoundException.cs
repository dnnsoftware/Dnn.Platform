// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Content;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when a content item is requested that does not exist.</summary>
[Serializable]
public class ContentItemNotFoundException : ApplicationException
{
    /// <inheritdoc />
    public ContentItemNotFoundException()
    {
    }

    /// <inheritdoc />
    public ContentItemNotFoundException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public ContentItemNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected ContentItemNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
