// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Web;
using System.Web.Mvc;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Web.Mvc.Framework.Controllers;

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    public class DnnModuleAuthorizeAttribute : AuthorizeAttributeBase
    {
        private ModuleInfo _module;

        public DnnModuleAuthorizeAttribute()
        {
            AccessLevel = SecurityAccessLevel.Host;
        }

        public SecurityAccessLevel AccessLevel { get; set; }

        public string PermissionKey { get; set; }

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (_module != null)
            {
                return HasModuleAccess();
            }

            return false;
        }

        public override void OnAuthorization(AuthorizationContext filterContext)
        {
            var controller = filterContext.Controller as IDnnController;

            if (controller == null)
            {
                throw new InvalidOperationException("This attribute can only be applied to Controllers that implement IDnnController");
            }

            _module = controller.ModuleContext.Configuration;

            base.OnAuthorization(filterContext);
        }

        protected virtual bool HasModuleAccess()
        {
            return ModulePermissionController.HasModuleAccess(AccessLevel, PermissionKey, _module);
        }
    }
}
