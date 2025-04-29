// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users;

using System;

/// <summary>An exception class which should be used to throw exceptions when password validations fail.</summary>
[Serializable]
public class InvalidPasswordException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="InvalidPasswordException"/> class.</summary>
    public InvalidPasswordException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="InvalidPasswordException"/> class with a message.</summary>
    /// <param name="message">Exception message.</param>
    public InvalidPasswordException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance of the <see cref="InvalidPasswordException"/> class with a message and an inner exception.</summary>
    /// <param name="message">Exception message.</param>
    /// <param name="innerException">Inner exception to wrap.</param>
    public InvalidPasswordException(
        string message,
        Exception innerException)
        : base(message, innerException)
    {
    }
}
