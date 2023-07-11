// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Web.Api.Auth.ApiTokens;
    using DotNetNuke.Web.Api.Auth.ApiTokens.Models;

    /// <summary>
    /// Attribute to authorize apis based on the api token used. 
    /// </summary>
    public class ApiTokenAuthorizeAttribute : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApiTokenAuthorizeAttribute"/> class.
        /// </summary>
        /// <param name="key">The Key to authenticate api.</param>
        /// <param name="resourceFile">The resource file for the token api.</param>
        /// <param name="scope">The required api token scope.</param>
        public ApiTokenAuthorizeAttribute(string key, string resourceFile, ApiTokenScope scope)
        {
            this.Key = key;
            this.ResourceFile = resourceFile;
            this.Scope = scope;
        }

        /// <summary>
        /// Gets or sets the Key for the api token.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the resource file for the api token.
        /// </summary>
        public string ResourceFile { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the required api token scope.
        /// </summary>
        public ApiTokenScope Scope { get; set; } = ApiTokenScope.User;

        /// <summary>
        /// Check if the request is authorized.
        /// </summary>
        /// <param name="context">The authentication filter context.</param>
        /// <returns>True if authorized, false otherwise.</returns>
        public override bool IsAuthorized(AuthFilterContext context)
        {
            var token = ApiTokenController.Instance.GetCurrentThreadApiToken();
            if (token == null)
            {
                return false;
            }

            if (token.Scope != ApiTokenScope.Host)
            {
                var settings = ApiTokenSettings.GetSettings(PortalController.Instance.GetCurrentSettings().PortalId);
                if (!settings.AllowApiTokens)
                {
                    return false;
                }
            }

            var scopeMatch = false;
            switch (this.Scope)
            {
                case ApiTokenScope.Host:
                    scopeMatch = token.Scope == ApiTokenScope.Host;
                    break;
                case ApiTokenScope.Portal:
                    scopeMatch = token.Scope == ApiTokenScope.Portal || token.Scope == ApiTokenScope.Host;
                    break;
                case ApiTokenScope.User:
                    scopeMatch = token.Scope == ApiTokenScope.User && UserController.Instance.GetCurrentUserInfo() != null;
                    break;
            }

            if (scopeMatch)
            {
                return token.TokenKeys.Contains(this.Key.ToLowerInvariant());
            }

            return false;
        }
    }
}
