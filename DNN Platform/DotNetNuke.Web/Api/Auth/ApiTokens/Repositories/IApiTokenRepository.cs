// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace DotNetNuke.Web.Api.Auth.ApiTokens.Repositories
{
    using System.Collections.Generic;

    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;

    /// <summary>
    /// Repository to manage API tokens.
    /// </summary>
    internal interface IApiTokenRepository
    {
        /// <summary>
        /// Retrieves an existing API token instance using its hash and portal identifier.
        /// </summary>
        /// <param name="portalId">The identifier of the portal where the token was generated.</param>
        /// <param name="tokenHash">The hash code of the API token.</param>
        /// <returns>An instance of the `ApiToken` class if it exists.</returns>
        ApiToken GetApiToken(int portalId, string tokenHash);

        /// <summary>
        /// Retrieves a list of token keys generated for the provided API token ID.
        /// </summary>
        /// <param name="apiTokenId">The unique ID of an API token instance.</param>
        /// <returns>A list of `string` objects containing token keys.</returns>
        List<string> GetApiTokenKeys(int apiTokenId);

        /// <summary>
        /// Adds a new API token instance to the database.
        /// </summary>
        /// <param name="apiToken">An `ApiToken` instance to add.</param>
        /// <returns>The same instance.</returns>
        ApiToken AddApiToken(ApiToken apiToken);

        /// <summary>
        /// Revokes an API token instance.
        /// </summary>
        /// <param name="apiToken">The `ApiToken` instance to revoke.</param>
        void RevokeApiToken(ApiToken apiToken);
    }
}
