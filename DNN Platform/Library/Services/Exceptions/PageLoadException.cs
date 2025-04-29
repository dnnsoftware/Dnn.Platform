// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions;

using System;
using System.Runtime.Serialization;

public class PageLoadException : BasePortalException
{
    // default constructor

    /// <summary>Initializes a new instance of the <see cref="PageLoadException"/> class.</summary>
    public PageLoadException()
    {
    }

    // constructor with exception message

    /// <summary>Initializes a new instance of the <see cref="PageLoadException"/> class.</summary>
    /// <param name="message"></param>
    public PageLoadException(string message)
        : base(message)
    {
    }

    // constructor with message and inner exception

    /// <summary>Initializes a new instance of the <see cref="PageLoadException"/> class.</summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public PageLoadException(string message, Exception inner)
        : base(message, inner)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="PageLoadException"/> class.</summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected PageLoadException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
