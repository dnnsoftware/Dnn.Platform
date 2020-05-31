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
            NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
            ActionName = actionName;
            ControllerName = controllerName;
        }

        public DnnRedirecttoRouteResult(string actionName, string controllerName, string routeName, RouteValueDictionary routeValues, bool permanent, DnnUrlHelper url)
            : this(actionName, controllerName, routeName, routeValues, permanent)
        {
            Url = url;
        }

        public DnnUrlHelper Url { get; private set; }

        public string ActionName { get; private set; }

        public string ControllerName { get; private set; }

        public override void ExecuteResult(ControllerContext context)
        {
            Requires.NotNull("context", context);

            Guard.Against(context.IsChildAction, "Cannot Redirect In Child Action");

            string url;
            if (Url != null && context.Controller is IDnnController)
            {
                url = Url.Action(ActionName, ControllerName);
            }
            else
            {
                //TODO - match other actions
                url = NavigationManager.NavigateURL();
            }

            if (Permanent)
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
