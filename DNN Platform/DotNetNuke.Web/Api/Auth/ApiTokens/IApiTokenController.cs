// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens
{
    using System.Net.Http;

    using DotNetNuke.Entities.Users;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;

    /// <summary>
    /// Interface for managing API tokens for authentication schemes.
    /// </summary>
    internal interface IApiTokenController
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
        (ApiToken, UserInfo) ValidateToken(HttpRequestMessage request);

        /// <summary>
        /// Sets the API token of the current thread.
        /// </summary>
        /// <param name="token">The `ApiToken` instance to set.</param>
        void SetCurrentThreadApiToken(ApiToken token);

        /// <summary>
        /// Gets the API token of the current thread.
        /// </summary>
        /// <returns>An `ApiToken` object.</returns>
        ApiToken GetCurrentThreadApiToken();
    }
}
