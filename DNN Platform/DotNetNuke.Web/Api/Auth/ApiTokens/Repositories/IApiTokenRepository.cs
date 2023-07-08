// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace DotNetNuke.Web.Api.Auth.ApiTokens.Repositories
{
    using System.Collections.Generic;

    using DotNetNuke.Collections;
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
        ApiTokenBase GetApiToken(int portalId, string tokenHash);

        /// <summary>
        /// Retrieves an existing API token instance using its hash and portal identifier.
        /// </summary>
        /// <param name="apiTokenId">The identifier of the token.</param>
        /// <returns>An instance of the `ApiToken` class if it exists.</returns>
        ApiToken GetApiToken(int apiTokenId);

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
        /// <param name="apiKeys">Comma separated list of API keys for this token.</param>
        /// <param name="userId">User ID of the user creating the token.</param>
        /// <returns>The same instance.</returns>
        ApiTokenBase AddApiToken(ApiTokenBase apiToken, string apiKeys, int userId);

        /// <summary>
        /// Revokes an API token instance.
        /// </summary>
        /// <param name="apiToken">The `ApiToken` instance to revoke.</param>
        void RevokeApiToken(ApiTokenBase apiToken);

        /// <summary>
        /// Retrieves a paged list of API tokens in the database.
        /// </summary>
        /// <param name="scope">The scope of the API tokens to retrieve.</param>
        /// <param name="includeNarrowerScopes">Include scopes narrower than the supplied scope.</param>
        /// <param name="portalId">The identifier of the portal where the API token was generated.</param>
        /// <param name="userId">The unique identifier of the user that generated the token.</param>
        /// <param name="filter">Value indicating which tokens to return based on status.</param>
        /// <param name="apiKey">API key to filter the results by.</param>
        /// <param name="pageIndex">The index of the starting page.</param>
        /// <param name="pageSize">The maximum number of records to return in a single page.</param>
        /// <returns>A paged list of `ApiToken` objects.</returns>
        public IPagedList<ApiToken> GetApiTokens(ApiTokenScope scope, bool includeNarrowerScopes, int portalId, int userId, ApiTokenFilter filter, string apiKey, int pageIndex, int pageSize);
    }
}
