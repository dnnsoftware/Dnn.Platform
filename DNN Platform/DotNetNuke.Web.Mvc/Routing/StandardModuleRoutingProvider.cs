// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using Microsoft.Extensions.DependencyInjection;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Abstractions;
using DotNetNuke.Entities.Modules;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Web.Mvc.Routing
{
    public class StandardModuleRoutingProvider : ModuleRoutingProvider
    {
        protected INavigationManager NavigationManager { get; }
        private const string ExcludedQueryStringParams = "tabid,mid,ctl,language,popup,action,controller";
        private const string ExcludedRouteValues = "mid,ctl,popup";
        public StandardModuleRoutingProvider()
        {
            NavigationManager = Globals.DependencyProvider.GetRequiredService<INavigationManager>();
        }


        public override string GenerateUrl(RouteValueDictionary routeValues, ModuleInstanceContext moduleContext)
        {
            //Look for a module control
            string controlKey = (routeValues.ContainsKey("ctl")) ? (string)routeValues["ctl"] : String.Empty;

            List<string> additionalParams = (from routeValue in routeValues
                                             where !ExcludedRouteValues.Split(',').ToList().Contains(routeValue.Key.ToLowerInvariant())
                                             select routeValue.Key + "=" + routeValue.Value)
                                             .ToList();

            string url;
            if (String.IsNullOrEmpty(controlKey))
            {
                additionalParams.Insert(0, "moduleId=" + moduleContext.Configuration.ModuleID);
                url = NavigationManager.NavigateURL("", additionalParams.ToArray());
            }
            else
            {
                url = moduleContext.EditUrl(String.Empty, String.Empty, controlKey, additionalParams.ToArray());
            }

            return url;
        }

        public override RouteData GetRouteData(HttpContextBase httpContext, ModuleControlInfo moduleControl)
        {
            var segments = moduleControl.ControlSrc.Replace(".mvc", "").Split('/');
            string routeNamespace = String.Empty;
            string routeControllerName;
            string routeActionName;
            if (segments.Length == 3)
            {
                routeNamespace = segments[0];
                routeControllerName = segments[1];
                routeActionName = segments[2];
            }
            else
            {
                routeControllerName = segments[0];
                routeActionName = segments[1];
            }

            var actionName = (httpContext == null) ? routeActionName : httpContext.Request.QueryString.GetValueOrDefault("action", routeActionName);
            var controllerName = (httpContext == null) ? routeControllerName : httpContext.Request.QueryString.GetValueOrDefault("controller", routeControllerName);

            var routeData = new RouteData();
            routeData.Values.Add("controller", controllerName);
            routeData.Values.Add("action", actionName);

            if (httpContext != null)
            {
                foreach (var param in httpContext.Request.QueryString.AllKeys)
                {
                    if (!ExcludedQueryStringParams.Split(',').ToList().Contains(param.ToLowerInvariant()))
                    {
                        routeData.Values.Add(param, httpContext.Request.QueryString[param]);
                    }
                }
            }
            if (!String.IsNullOrEmpty(routeNamespace))
            {
                routeData.DataTokens.Add("namespaces", new string[] { routeNamespace });
            }

            return routeData;
        }
    }
}
