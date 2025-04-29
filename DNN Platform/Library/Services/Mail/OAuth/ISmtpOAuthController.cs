// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.Mail.OAuth;

using System.Collections.Generic;

/// <summary>A contract specifying the ability to retrieve the SMTP OAuth providers.</summary>
public interface ISmtpOAuthController
{
    /// <summary>Get all the OAuth providers.</summary>
    /// <returns>OAuth providers list.</returns>
    IReadOnlyCollection<ISmtpOAuthProvider> GetOAuthProviders();

    /// <summary>Get an OAuth provider by name.</summary>
    /// <param name="name">The provider name.</param>
    /// <returns>The OAuth provider or <see langword="null"/>.</returns>
    ISmtpOAuthProvider GetOAuthProvider(string name);
}
