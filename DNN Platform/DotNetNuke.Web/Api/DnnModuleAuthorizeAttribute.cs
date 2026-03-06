// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System.Net.Http;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Security;
    using DotNetNuke.Security.Permissions;

    /// <summary>Determines web API authorization based on module permissions.</summary>
    public class DnnModuleAuthorizeAttribute : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
        /// <summary>Initializes a new instance of the <see cref="DnnModuleAuthorizeAttribute"/> class.</summary>
        public DnnModuleAuthorizeAttribute()
        {
            this.AccessLevel = SecurityAccessLevel.Host;
        }

        /// <summary>Gets or sets a custom permission key to check for access.</summary>
        public string PermissionKey { get; set; }

        /// <summary>Gets or sets an access level to verify the user has.</summary>
        public SecurityAccessLevel AccessLevel { get; set; }

        /// <inheritdoc />
        public override bool IsAuthorized(AuthFilterContext context)
        {
            var activeModule = this.FindModuleInfo(context.ActionContext.Request);

            if (activeModule != null)
            {
                return ModulePermissionController.HasModuleAccess(this.AccessLevel, this.PermissionKey, activeModule);
            }

            return false;
        }

        /// <summary>Finds the module associated with the request.</summary>
        /// <param name="request">The web API request.</param>
        /// <returns>A <see cref="ModuleInfo"/> instance or <see langword="null"/>.</returns>
        protected virtual ModuleInfo FindModuleInfo(HttpRequestMessage request)
        {
            return request.FindModuleInfo();
        }
    }
}
