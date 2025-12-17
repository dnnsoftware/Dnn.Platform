// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.GeneratedImage.ImageQuantization;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when there's an issue in a quantizer.</summary>
[Serializable]
public class QuantizerException : Exception
{
    /// <inheritdoc />
    public QuantizerException()
    {
    }

    /// <inheritdoc />
    public QuantizerException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public QuantizerException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected QuantizerException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
