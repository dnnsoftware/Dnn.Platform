// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Internal;

/// <summary>A contract specifying the ability to validate an anti-forgery cookie.</summary>
public interface IAntiForgery
{
    /// <summary>Gets the cookie name.</summary>
    string CookieName { get; }

    /// <summary>Validates that the cookie token matches the header token.</summary>
    /// <param name="cookieToken">The CSRF token from the cookie.</param>
    /// <param name="headerToken">The CSRF token from the header.</param>
    void Validate(string cookieToken, string headerToken);
}
