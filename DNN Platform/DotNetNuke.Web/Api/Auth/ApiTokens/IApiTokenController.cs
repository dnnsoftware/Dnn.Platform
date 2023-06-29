// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens
{
    using System.Collections.Generic;
    using System.Net.Http;
  using DotNetNuke.Collections;
  using DotNetNuke.Entities.Users;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;

    /// <summary>
    /// Interface for managing API tokens for authentication schemes.
    /// </summary>
    public interface IApiTokenController
    {
        /// <summary>
        /// Gets the name of the authentication scheme type.
        /// </summary>
        string SchemeType { get; }

        /// <summary>
        /// Validates an API token from a HTTP request message.
        /// </summary>
        /// <param name="request">The `HttpRequestMessage` instance that contains the token.</param>
        /// <returns>A tuple object containing an `ApiToken` object and a `UserInfo` object.</returns>
        (ApiTokenBase, UserInfo) ValidateToken(HttpRequestMessage request);

        /// <summary>
        /// Sets the API token of the current thread.
        /// </summary>
        /// <param name="token">The `ApiToken` instance to set.</param>
        void SetCurrentThreadApiToken(ApiTokenBase token);

        /// <summary>
        /// Gets the API token of the current thread.
        /// </summary>
        /// <returns>An `ApiToken` object.</returns>
        ApiTokenBase GetCurrentThreadApiToken();

        /// <summary>
        /// Retrieves a sorted dictionary of token keys and their values based on the provided API token scope and locale.
        /// </summary>
        /// <param name="scope">The API token scope to retrieve token keys for.</param>
        /// <param name="locale">The locale to retrieve API token keys for.</param>
        /// <returns>A sorted dictionary of token keys.</returns>
        SortedDictionary<string, ApiTokenAttribute> ApiTokenKeyList(ApiTokenScope scope, string locale);

        /// <summary>
        /// Retrieves a sorted dictionary of token keys and their values based on the provided API token scope and locale.
        /// </summary>
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
    }
}
