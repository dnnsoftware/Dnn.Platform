// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users;

using System;
using System.Runtime.Serialization;

[Serializable]
public class UserRelationshipDoesNotExistException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="UserRelationshipDoesNotExistException"/> class.</summary>
    public UserRelationshipDoesNotExistException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="UserRelationshipDoesNotExistException"/> class.</summary>
    /// <param name="message"></param>
    public UserRelationshipDoesNotExistException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="UserRelationshipDoesNotExistException"/> class.</summary>
    /// <param name="message"></param>
    /// <param name="inner"></param>
    public UserRelationshipDoesNotExistException(string message, Exception inner)
        : base(message, inner)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="UserRelationshipDoesNotExistException"/> class.</summary>
    /// <param name="info"></param>
    /// <param name="context"></param>
    public UserRelationshipDoesNotExistException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
