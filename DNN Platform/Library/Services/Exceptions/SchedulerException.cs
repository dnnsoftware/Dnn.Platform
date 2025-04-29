// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Exceptions;

using System;
using System.Runtime.Serialization;

public class SchedulerException : BasePortalException
{
    // default constructor

    /// <summary>Initializes a new instance of the <see cref="SchedulerException"/> class.</summary>
    public SchedulerException()
    {
    }

    // constructor with exception message

    /// <summary>Initializes a new instance of the <see cref="SchedulerException"/> class.</summary>
    /// <param name="message"></param>
    public SchedulerException(string message)
        : base(message)
    {
    }

    // constructor with message and inner exception

    /// <summary>Initializes a new instance of the <see cref="SchedulerException"/> class.</summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public SchedulerException(string message, Exception inner)
        : base(message, inner)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="SchedulerException"/> class.</summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    protected SchedulerException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
