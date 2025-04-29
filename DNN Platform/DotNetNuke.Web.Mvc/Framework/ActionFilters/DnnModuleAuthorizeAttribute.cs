// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc.Framework.ActionFilters;

using System;
using System.Web;
using System.Web.Mvc;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Security;
using DotNetNuke.Security.Permissions;
using DotNetNuke.Web.Mvc.Framework.Controllers;

public class DnnModuleAuthorizeAttribute : AuthorizeAttributeBase
{
    private ModuleInfo module;

    public DnnModuleAuthorizeAttribute()
    {
        this.AccessLevel = SecurityAccessLevel.Host;
    }

    public SecurityAccessLevel AccessLevel { get; set; }

    public string PermissionKey { get; set; }

    /// <inheritdoc/>
    public override void OnAuthorization(AuthorizationContext filterContext)
    {
        var controller = filterContext.Controller as IDnnController;

        if (controller == null)
        {
            throw new InvalidOperationException("This attribute can only be applied to Controllers that implement IDnnController");
        }

        this.module = controller.ModuleContext.Configuration;

        base.OnAuthorization(filterContext);
    }

    /// <inheritdoc/>
    protected override bool AuthorizeCore(HttpContextBase httpContext)
    {
        if (this.module != null)
        {
            return this.HasModuleAccess();
        }

        return false;
    }

    protected virtual bool HasModuleAccess()
    {
        return ModulePermissionController.HasModuleAccess(this.AccessLevel, this.PermissionKey, this.module);
    }
}
