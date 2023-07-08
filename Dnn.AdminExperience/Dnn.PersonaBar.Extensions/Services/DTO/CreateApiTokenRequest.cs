// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Services.Dto
{
    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;

    /// <summary>
    /// Represents a request to create a new <see cref="ApiToken"/> object.
    /// </summary>
    public class CreateApiTokenRequest
    {
        /// <summary>
        /// Gets or sets the scope of the token.
        /// </summary>
        public int Scope { get; set; }

        /// <summary>
        /// Gets or sets the expiration date and time of the token.
        /// </summary>
        public string ExpiresOn { get; set; }

        /// <summary>
        /// Gets or sets the API keys associated with the token.
        /// </summary>
        public string ApiKeys { get; set; }
    }
}
