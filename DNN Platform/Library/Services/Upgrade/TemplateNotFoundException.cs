// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Upgrade;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when a template is requested that cannot be found.</summary>
[Serializable]
public class TemplateNotFoundException : Exception
{
    /// <inheritdoc />
    public TemplateNotFoundException()
    {
    }

    /// <inheritdoc />
    public TemplateNotFoundException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public TemplateNotFoundException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected TemplateNotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
