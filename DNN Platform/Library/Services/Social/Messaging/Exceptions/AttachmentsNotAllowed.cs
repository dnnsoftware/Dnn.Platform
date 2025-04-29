// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Social.Messaging.Exceptions;

using System;
using System.Runtime.Serialization;

[Serializable]
public class AttachmentsNotAllowed : Exception
{
    /// <summary>Initializes a new instance of the <see cref="AttachmentsNotAllowed"/> class.</summary>
    public AttachmentsNotAllowed()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="AttachmentsNotAllowed"/> class.</summary>
    /// <param name="message"></param>
    public AttachmentsNotAllowed(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="AttachmentsNotAllowed"/> class.</summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public AttachmentsNotAllowed(string message, Exception inner)
        : base(message, inner)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="AttachmentsNotAllowed"/> class.</summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public AttachmentsNotAllowed(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
