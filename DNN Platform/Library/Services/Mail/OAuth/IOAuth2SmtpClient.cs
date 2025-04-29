// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail.OAuth;

using System.Threading;
using System.Threading.Tasks;

/// <summary>A contract specifying the ability to authenticate an SMTP client with an OAuth 2 endpoint.</summary>
public interface IOAuth2SmtpClient
{
    /// <summary>Authenticates this SMTP client with the given OAuth credentials.</summary>
    /// <param name="username">The username.</param>
    /// <param name="token">The auth token.</param>
    void Authenticate(string username, string token);

    /// <summary>Authenticates this SMTP client with the given OAuth credentials.</summary>
    /// <param name="username">The username.</param>
    /// <param name="token">The auth token.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A <see cref="Task"/> indicating completion.</returns>
    Task AuthenticateAsync(string username, string token, CancellationToken cancellationToken = default(CancellationToken));
}
