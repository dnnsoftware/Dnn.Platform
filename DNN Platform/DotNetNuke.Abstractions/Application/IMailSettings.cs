// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Abstractions.Application;

using System;

/// <summary>Settings related to sending emails.</summary>
public interface IMailSettings
{
    /// <summary>Gets a value indicating whether secure connection are enabled for mail.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>Whether secure connections are enabled for mail in the given portal.</returns>
    public bool GetSecureConnectionEnabled(int portalId);

    /// <summary>Gets the currently configured mail OAuth provider if existing, for the current portal if portal mail enabled, otherwise for the installation.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The name of the mail OAuth provider.</returns>
    public string GetAuthProvider(int portalId);

    /// <summary>Gets the mail Authentication type.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The authentication type indicator, <c>"1"</c> for basic auth, <c>"2"</c> for NTLM auth, <c>"3"</c> for OAuth, any other value for anonymous auth.</returns>
    public string GetAuthentication(int portalId);

    /// <summary>Gets the mail Password.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The mail password.</returns>
    public string GetPassword(int portalId);

    /// <summary>Gets the mail Server.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The mail server.</returns>
    public string GetServer(int portalId);

    /// <summary>Gets the mail Username.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The mail username.</returns>
    public string GetUsername(int portalId);

    /// <summary>Gets the mail Connection Limit.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The mail connection limit.</returns>
    public int GetConnectionLimit(int portalId);

    /// <summary>Gets the mail MaxIdleTime.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The mail maximum idle time.</returns>
    public TimeSpan GetMaxIdleTime(int portalId);

    /// <summary>Gets a value indicating whether mail information is stored at the portal level.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>Whether mail information is stored per portal.</returns>
    public bool IsPortalEnabled(int portalId);
}
