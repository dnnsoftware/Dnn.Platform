// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Services.Sitemap;

using System;
using System.Runtime.Serialization;

/// <summary>An exception thrown when there's an issue generating a site map.</summary>
[Serializable]
public class SitemapException : Exception
{
    /// <inheritdoc />
    public SitemapException()
    {
    }

    /// <inheritdoc />
    public SitemapException(string message)
        : base(message)
    {
    }

    /// <inheritdoc />
    public SitemapException(string message, Exception innerException)
        : base(message, innerException)
    {
    }

    /// <inheritdoc />
    protected SitemapException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
