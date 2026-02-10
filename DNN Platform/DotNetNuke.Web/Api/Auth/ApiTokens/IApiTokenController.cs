// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Net.Http;

    using DotNetNuke.Collections;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;

    /// <summary>Interface for managing API tokens for authentication schemes.</summary>
    public interface IApiTokenController
    {
        /// <summary>Gets the name of the authentication scheme type.</summary>
        string SchemeType { get; }

        /// <summary>Validates an API token from an HTTP request message.</summary>
        /// <param name="request">The `HttpRequestMessage` instance that contains the token.</param>
        /// <returns>A tuple object containing an `ApiToken` object and a `UserInfo` object.</returns>
        (ApiToken Token, UserInfo User) ValidateToken(HttpRequestMessage request);

        /// <summary>Sets the API token of the current thread.</summary>
        /// <param name="token">The `ApiToken` instance to set.</param>
        void SetApiTokenForRequest(ApiToken token);

        /// <summary>Gets the API token of the current thread.</summary>
        /// <returns>An `ApiToken` object.</returns>
        ApiToken GetCurrentThreadApiToken();

        /// <summary>Retrieves a sorted dictionary of token keys and their values based on the provided API token scope and locale.</summary>
        /// <param name="scope">The API token scope to retrieve token keys for.</param>
        /// <param name="locale">The locale to retrieve API token keys for.</param>
        /// <returns>A sorted dictionary of token keys.</returns>
        SortedDictionary<string, ApiTokenAttribute> ApiTokenKeyList(ApiTokenScope scope, string locale);

        /// <summary>Retrieves a sorted dictionary of token keys and their values based on the provided API token scope and locale.</summary>
        /// <param name="scope">The API token scope to retrieve tokens for.</param>
        /// <param name="includeNarrowerScopes">Include scopes narrower than the supplied scope.</param>
        /// <param name="portalId">The portal to retrieve tokens for.</param>
        /// <param name="userId">The user id used to retrieve personal tokens for. Used only for User scope retrieval.</param>
        /// <param name="filter">Value indicating which tokens to return based on status.</param>
        /// <param name="apiKey">API key to filter the results by.</param>
        /// <param name="pageIndex">Page number of data to retrieve.</param>
        /// <param name="pageSize">Page size of data to retrieve.</param>
        /// <returns>An IPagedList of type ApiToken.</returns>
        IPagedList<ApiToken> GetApiTokens(ApiTokenScope scope, bool includeNarrowerScopes, int portalId, int userId, ApiTokenFilter filter, string apiKey, int pageIndex, int pageSize);

        /// <summary>Creates a new <see cref="ApiToken"/> object and returns it.</summary>
        /// <param name="portalId">The ID of the portal to create the token for.</param>
        /// <param name="tokenName">The name of the token.</param>
        /// <param name="scope">The scope of the token.</param>
        /// <param name="expiresOn">The date and time the token should expire.</param>
        /// <param name="apiKeys">The API keys associated with the token.</param>
        /// <param name="userId">The ID of the user that the token belongs to.</param>
        /// <returns>The string token to use for authentication.</returns>
        string CreateApiToken(int portalId, string tokenName, ApiTokenScope scope, DateTime expiresOn, string apiKeys, int userId);

        /// <summary>Retrieves an API token by its ID.</summary>
        /// <param name="apiTokenId">The ID of the API token to retrieve.</param>
        /// <returns>The specified API token.</returns>
        ApiToken GetApiToken(int apiTokenId);

        /// <summary>Revokes or deletes the specified API token of the user.</summary>
        /// <param name="token">The `ApiTokenBase` object to revoke or delete.</param>
        /// <param name="delete">A boolean value indicating whether to delete or revoke the token.</param>
        /// <param name="userId">The id of the user that revokes the token if being revoked.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", Justification = "Breaking change")]
        void RevokeOrDeleteApiToken(ApiToken token, bool delete, int userId);

        /// <summary>Deletes expired and revoked API tokens.</summary>
        /// <param name="portalId">The identifier of the portal. Use -1 for all portals.</param>
        /// <param name="userId">The identifier of the user whose tokens should be deleted. Use -1 for admin and host.</param>
        void DeleteExpiredAndRevokedApiTokens(int portalId, int userId);
    }
}
