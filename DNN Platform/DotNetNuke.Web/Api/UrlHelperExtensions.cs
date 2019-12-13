﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System.Web.Http.Routing;
using DotNetNuke.Entities.Portals;

namespace DotNetNuke.Web.Api
{
    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Generate WebAPI Links compatible with DNN Services Framework
        /// </summary>
        /// <param name="urlHelper">The UrlHelper</param>
        /// <param name="moduleFolderName">ModuleFolderName for the route</param>
        /// <param name="routeName">RouteName for the route</param>
        /// <param name="routeValues">Values to be passed to the route</param>
        /// <returns>a url</returns>
        public static string DnnLink(this UrlHelper urlHelper, string moduleFolderName, string routeName, object routeValues)
        {
            return DnnLink(urlHelper, moduleFolderName, routeName, routeValues, PortalController.Instance.GetCurrentPortalSettings().PortalAlias);
        }

        /// <summary>
        /// Generate WebAPI Links compatible with DNN Services Framework
        /// </summary>
        /// <param name="urlHelper">The UrlHelper</param>
        /// <param name="moduleFolderName">ModuleFolderName for the route</param>
        /// <param name="routeName">RouteName for the route</param>
        /// <param name="routeValues">Values to be passed to the route</param>
        /// <param name="portalAliasInfo">The PortalAlias to use in the Url </param>
        /// <returns>a url</returns>
        public static string DnnLink(this UrlHelper urlHelper, string moduleFolderName, string routeName, object routeValues, PortalAliasInfo portalAliasInfo)
        {
            var parm = new PortalAliasRouteManager();
            var fullName = parm.GetRouteName(moduleFolderName, routeName, PortalController.Instance.GetCurrentPortalSettings().PortalAlias);
            HttpRouteValueDictionary allRouteValues = parm.GetAllRouteValues(portalAliasInfo, routeValues);

            return urlHelper.Link(fullName, allRouteValues);
        }
    }
}
