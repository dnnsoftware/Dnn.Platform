// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api.Internal
{
    using DotNetNuke.Common;

    /// <summary>Requires the user to have access to the page.</summary>
    public sealed class DnnPagePermissionAttribute : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
        /// <summary>Gets or sets the permission key for the access.</summary>
        public string PermissionKey { get; set; } = "EDIT";

        /// <inheritdoc />
        public override bool IsAuthorized(AuthFilterContext context)
        {
            Requires.NotNull("context", context);

            return PagePermissionsAttributesHelper.HasTabPermission(this.PermissionKey);
        }
    }
}
