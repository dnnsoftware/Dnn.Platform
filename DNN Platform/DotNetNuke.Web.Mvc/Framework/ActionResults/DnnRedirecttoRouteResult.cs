// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.Common;
using DotNetNuke.Abstractions;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Helpers;

namespace DotNetNuke.Web.Mvc.Framework.ActionResults
{
    internal class DnnRedirecttoRouteResult : RedirectToRouteResult
    {
        protected INavigationManager NavigationManager { get; }
        public DnnRedirecttoRouteResult(string actionName, string controllerName, string routeName, RouteValueDictionary routeValues, bool permanent)
            : base(routeName, routeValues, permanent)
        {
            this.NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
            this.ActionName = actionName;
            this.ControllerName = controllerName;
        }

        public DnnRedirecttoRouteResult(string actionName, string controllerName, string routeName, RouteValueDictionary routeValues, bool permanent, DnnUrlHelper url)
            : this(actionName, controllerName, routeName, routeValues, permanent)
        {
            this.Url = url;
        }

        public DnnUrlHelper Url { get; private set; }

        public string ActionName { get; private set; }

        public string ControllerName { get; private set; }

        public override void ExecuteResult(ControllerContext context)
        {
            Requires.NotNull("context", context);

            Guard.Against(context.IsChildAction, "Cannot Redirect In Child Action");

            string url;
            if (this.Url != null && context.Controller is IDnnController)
            {
                url = this.Url.Action(this.ActionName, this.ControllerName);
            }
            else
            {
                // TODO - match other actions
                url = this.NavigationManager.NavigateURL();
            }

            if (this.Permanent)
            {
                context.HttpContext.Response.RedirectPermanent(url, true);
            }
            else
            {
                context.HttpContext.Response.Redirect(url, true);
            }


        }
    }
}
