// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Security.Services.Dto
{
    using DotNetNuke.Web.Api.Auth.ApiTokens;

    public class UpdateApiTokenSettingsRequest
    {
        /// <summary>
        /// Gets or sets the timespan of a token for end users.
        /// </summary>
        public int UserTokenTimespan { get; set; }

        /// <summary>
        /// Gets or sets the maximum timespan of a token for site admins.
        /// </summary>
        public int MaximumSiteTimespan { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether API tokens are allowed.
        /// </summary>
        public bool AllowApiTokens { get; set; }
    }
}
