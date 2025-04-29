// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Entities.Users;

using System;

[Serializable]
public class InvalidUserRegisterException : Exception
{
    /// <summary>Initializes a new instance of the <see cref="InvalidUserRegisterException"/> class.</summary>
    public InvalidUserRegisterException()
    {
    }

    /// <summary>Initializes a new instance of the <see cref="InvalidUserRegisterException"/> class.</summary>
    /// <param name="message"></param>
    public InvalidUserRegisterException(string message)
        : base(message)
    {
    }
}
