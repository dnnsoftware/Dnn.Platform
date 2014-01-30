using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
