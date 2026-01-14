// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens
{
    /// <summary>
    /// Enum representing a filter for API tokens.
    /// </summary>
    public enum ApiTokenFilter
    {
        /// <summary>
        /// Retrieve all API tokens.
        /// </summary>
        All = 0,

        /// <summary>
        /// Retrieve only active API tokens.
        /// </summary>
        Active = 1,

        /// <summary>
        /// Retrieve only revoked API tokens.
        /// </summary>
        Revoked = 2,

        /// <summary>
        /// Retrieve only expired API tokens.
        /// </summary>
        Expired = 3,

        /// <summary>
        /// Retrieve API tokens that have been either revoked or expired.
        /// </summary>
        RevokedOrExpired = 4,
    }
}
