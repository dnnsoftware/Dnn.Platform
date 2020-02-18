// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
                return permissionKey;
            }
            set
            {
                permissionKey = value;
            }
        }
        public override bool IsAuthorized(AuthFilterContext context)
        {
            Requires.NotNull("context", context);

            return PagePermissionsAttributesHelper.HasTabPermission(PermissionKey);
        }
    }
}
