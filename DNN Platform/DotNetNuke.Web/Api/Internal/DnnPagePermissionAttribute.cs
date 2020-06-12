// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Common;

namespace DotNetNuke.Web.Api.Internal
{
    public sealed class DnnPagePermissionAttribute : AuthorizeAttributeBase, IOverrideDefaultAuthLevel
    {
        private string permissionKey = "EDIT";

        public string PermissionKey
        {
            get
            {
                return this.permissionKey;
            }

            set
            {
                this.permissionKey = value;
            }
        }

        public override bool IsAuthorized(AuthFilterContext context)
        {
            Requires.NotNull("context", context);

            return PagePermissionsAttributesHelper.HasTabPermission(this.PermissionKey);
        }
    }
}
