// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail.OAuth;

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

using DotNetNuke.Common.Utilities;

/// <summary>A contract specifying the ability to authenticate an SMTP client with an OAuth 2 endpoint.</summary>
public interface ISmtpOAuthProvider
{
    /// <summary>Gets provider name.</summary>
    string Name { get; }

    /// <summary>Gets the localized name.</summary>
    string LocalizedName { get; }

    /// <summary>Whether the provider has completed the authorization process for the portal.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns><see langword="true"/> if the authorization has been completed, otherwise <see langword="false"/>..</returns>
    bool IsAuthorized(int portalId);

    /// <summary>Whether the provider has completed the authorization process for the portal.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns><see langword="true"/> if the authorization has been completed, otherwise <see langword="false"/>..</returns>
    Task<bool> IsAuthorizedAsync(int portalId, CancellationToken cancellationToken = default);

    /// <summary>Get the authorize URL.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <returns>The URL.</returns>
    string GetAuthorizeUrl(int portalId);

    /// <summary>Get the provider parameters.</summary>
    /// <param name="portalId">the portal ID of the setting, pass <see cref="Null.NullInteger"/> if it's a global setting.</param>
    /// <returns>The list of settings.</returns>
    IList<SmtpOAuthSetting> GetSettings(int portalId);

    /// <summary>Update provider settings.</summary>
    /// <param name="portalId">The portal ID of the setting, pass <see cref="Null.NullInteger"/> if it's a global setting.</param>
    /// <param name="settings">The settings.</param>
    /// <param name="errorMessages">The errors.</param>
    /// <returns>Whether the settings changed.</returns>
    bool UpdateSettings(int portalId, IDictionary<string, string> settings, out IList<string> errorMessages);

    /// <summary>Authorize the SMTP client.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="smtpClient">The SMTP client.</param>
    void Authorize(int portalId, IOAuth2SmtpClient smtpClient);

    /// <summary>Authorize the SMTP client.</summary>
    /// <param name="portalId">The portal ID.</param>
    /// <param name="smtpClient">The SMTP client.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> indicating completion.</returns>
    Task AuthorizeAsync(int portalId, IOAuth2SmtpClient smtpClient, CancellationToken cancellationToken = default);
}
