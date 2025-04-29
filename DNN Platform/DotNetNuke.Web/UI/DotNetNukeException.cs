// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.UI;

using System;
using System.Runtime.Serialization;

public class DotNetNukeException : Exception
{
    private readonly DotNetNukeErrorCode errorCode = DotNetNukeErrorCode.NotSet;

    /// <summary>Initializes a new instance of the <see cref="DotNetNukeException"/> class.</summary>
    public DotNetNukeException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="DotNetNukeException"/> class.</summary>
    /// <param name="message">The message.</param>
    public DotNetNukeException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="DotNetNukeException"/> class.</summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    public DotNetNukeException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="DotNetNukeException"/> class.</summary>
    /// <param name="message">The message.</param>
    /// <param name="errorCode">The error code.</param>
    public DotNetNukeException(string message, DotNetNukeErrorCode errorCode)
        : base(message)
    {
        this.errorCode = errorCode;
    }

    /// <summary>Initializes a new instance of the <see cref="DotNetNukeException"/> class.</summary>
    /// <param name="message">The message.</param>
    /// <param name="innerException">The inner exception.</param>
    /// <param name="errorCode">The error code.</param>
    public DotNetNukeException(string message, Exception innerException, DotNetNukeErrorCode errorCode)
        : base(message, innerException)
    {
        this.errorCode = errorCode;
    }

    /// <summary>Initializes a new instance of the <see cref="DotNetNukeException"/> class.</summary>
    /// <param name="info">The serialization info.</param>
    /// <param name="context">The streaming context.</param>
    public DotNetNukeException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }

    public DotNetNukeErrorCode ErrorCode
    {
        get
        {
            return this.errorCode;
        }
    }
}
