// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Mvc.Framework.Controllers;

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    public class RequireHostAttribute : AuthorizeAttributeBase
    {
        private UserInfo _user;

        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            var principal = Thread.CurrentPrincipal;
            if (!principal.Identity.IsAuthenticated)
            {
                return false;
            }
            
            if (_user != null)
            {
                return _user.IsSuperUser;
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

            _user = controller.ModuleContext.PortalSettings.UserInfo;

            base.OnAuthorization(filterContext);
        }
    }
}
