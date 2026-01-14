// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Auth.ApiTokens.Models
{
    /// <summary>
    /// Enum that lists available scopes of an Api token.
    /// </summary>
    public enum ApiTokenScope
    {
        /// <summary>
        /// User scope. This means the token is tied to the user referenced in the createdby field and portal id of the token.
        /// </summary>
        User = 0,

        /// <summary>
        /// Portal scope. The token is anonymous, but should not be able to access host or user scope endpoints.
        /// </summary>
        Portal = 1,

        /// <summary>
        /// Host scope. The token can access both host scope endpoints as well as portal scope endpoints.
        /// </summary>
        Host = 2,
    }
}
