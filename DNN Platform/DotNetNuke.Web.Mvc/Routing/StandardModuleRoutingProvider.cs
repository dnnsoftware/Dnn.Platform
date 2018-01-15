#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2018
// by DNN Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;
using DotNetNuke.Collections;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.UI.Modules;

namespace DotNetNuke.Web.Mvc.Routing
{
    public class StandardModuleRoutingProvider : ModuleRoutingProvider
    {
        private const string ExcludedQueryStringParams = "tabid,mid,ctl,language,popup,action,controller";
        private const string ExcludedRouteValues = "mid,ctl,popup";

        public override string GenerateUrl(RouteValueDictionary routeValues, ModuleInstanceContext moduleContext)
        {
            //Look for a module control
            string controlKey = (routeValues.ContainsKey("ctl")) ? (string)routeValues["ctl"] : String.Empty;

            List<string> additionalParams = (from routeValue in routeValues 
                                             where !ExcludedRouteValues.Split(',').ToList().Contains(routeValue.Key.ToLower()) 
                                             select routeValue.Key + "=" + routeValue.Value)
                                             .ToList();

            string url;
            if (String.IsNullOrEmpty(controlKey))
            {
                additionalParams.Insert(0, "moduleId=" + moduleContext.Configuration.ModuleID);
                url = Globals.NavigateURL("", additionalParams.ToArray());
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
                    if (!ExcludedQueryStringParams.Split(',').ToList().Contains(param.ToLower()))
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
