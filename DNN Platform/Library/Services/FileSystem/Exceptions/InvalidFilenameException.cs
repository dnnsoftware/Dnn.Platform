// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.FileSystem;

using System;
using System.Runtime.Serialization;

[Serializable]
public class InvalidFilenameException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="InvalidFilenameException"/> class.</summary>
    public InvalidFilenameException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="InvalidFilenameException"/> class.</summary>
    /// <param name="message"></param>
    public InvalidFilenameException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="InvalidFilenameException"/> class.</summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public InvalidFilenameException(string message, Exception inner)
        : base(message, inner)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="InvalidFilenameException"/> class.</summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public InvalidFilenameException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
