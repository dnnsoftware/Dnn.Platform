// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Application;

/// <summary>Provides enumerated values that are used to set the <c>Cache-Control</c> HTTP header.</summary>
/// <remarks>This is a copy of <c>System.Web.HttpCacheability</c>, which is defined in System.Web (and therefore not available in .NET Standard 2.0).</remarks>
/// <seealso href="https://learn.microsoft.com/en-us/dotnet/api/system.web.httpcacheability"/>
public enum CacheControlHeader
{
    /// <summary>A header value that is unknown.</summary>
    Unknown = 0,

    /// <summary>Sets the <c>Cache-Control: no-cache</c> header. Without a field name, the directive applies to the entire request and a shared (proxy server) cache must force a successful revalidation with the origin Web server before satisfying the request. With a field name, the directive applies only to the named field; the rest of the response may be supplied from a shared cache.</summary>
    NoCache = 1,

    /// <summary>Default value. Sets <c>Cache-Control: private</c> to specify that the response is cacheable only on the client and not by shared (proxy server) caches.</summary>
    Private = 2,

    /// <summary>Applies the settings of both Server and NoCache to indicate that the content is cached at the server but all others are explicitly denied the ability to cache the response.</summary>
    ServerAndNoCache = 3,

    /// <summary>Sets <c>Cache-Control: public</c> to specify that the response is cacheable by clients and shared (proxy) caches.</summary>
    Public = 4,

    /// <summary>Indicates that the response is cached at the server and at the client but nowhere else. Proxy servers are not allowed to cache the response.</summary>
    ServerAndPrivate = 5,
}
