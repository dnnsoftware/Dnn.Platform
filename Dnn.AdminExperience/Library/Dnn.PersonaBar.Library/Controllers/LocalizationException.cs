// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Library.Controllers;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when issues are encountered with localization.</summary>
[Serializable]
public class LocalizationException : ApplicationException
{
    /// <inheritdoc />
    public LocalizationException()
    {
    }

    /// <inheritdoc />
    public LocalizationException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public LocalizationException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected LocalizationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
