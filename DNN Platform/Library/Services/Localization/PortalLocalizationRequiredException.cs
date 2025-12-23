// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Localization;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when an action would result in a portal without any localization entries.</summary>
[Serializable]
public class PortalLocalizationRequiredException : Exception
{
    /// <inheritdoc />
    public PortalLocalizationRequiredException()
    {
    }

    /// <inheritdoc />
    public PortalLocalizationRequiredException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public PortalLocalizationRequiredException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected PortalLocalizationRequiredException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
