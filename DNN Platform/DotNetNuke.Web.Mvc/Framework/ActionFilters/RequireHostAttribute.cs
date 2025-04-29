// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc.Framework.ActionFilters;

using System;
using System.Threading;
using System.Web;
using System.Web.Mvc;

using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Mvc.Framework.Controllers;

public class RequireHostAttribute : AuthorizeAttributeBase
{
    private UserInfo user;

    /// <inheritdoc/>
    public override void OnAuthorization(AuthorizationContext filterContext)
    {
        var controller = filterContext.Controller as IDnnController;

        if (controller == null)
        {
            throw new InvalidOperationException("This attribute can only be applied to Controllers that implement IDnnController");
        }

        this.user = controller.ModuleContext.PortalSettings.UserInfo;

        base.OnAuthorization(filterContext);
    }

    /// <inheritdoc/>
    protected override bool AuthorizeCore(HttpContextBase httpContext)
    {
        var principal = Thread.CurrentPrincipal;
        if (!principal.Identity.IsAuthenticated)
        {
            return false;
        }

        if (this.user != null)
        {
            return this.user.IsSuperUser;
        }

        return false;
    }
}
