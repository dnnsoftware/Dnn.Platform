// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System.Web.Http.Routing;

    using DotNetNuke.Entities.Portals;

    public static class UrlHelperExtensions
    {
        /// <summary>
        /// Generate WebAPI Links compatible with DNN Services Framework.
        /// </summary>
        /// <param name="urlHelper">The UrlHelper.</param>
        /// <param name="moduleFolderName">ModuleFolderName for the route.</param>
        /// <param name="routeName">RouteName for the route.</param>
        /// <param name="routeValues">Values to be passed to the route.</param>
        /// <returns>a url.</returns>
        public static string DnnLink(this UrlHelper urlHelper, string moduleFolderName, string routeName, object routeValues)
        {
            return DnnLink(urlHelper, moduleFolderName, routeName, routeValues, PortalController.Instance.GetCurrentPortalSettings().PortalAlias);
        }

        /// <summary>
        /// Generate WebAPI Links compatible with DNN Services Framework.
        /// </summary>
        /// <param name="urlHelper">The UrlHelper.</param>
        /// <param name="moduleFolderName">ModuleFolderName for the route.</param>
        /// <param name="routeName">RouteName for the route.</param>
        /// <param name="routeValues">Values to be passed to the route.</param>
        /// <param name="portalAliasInfo">The PortalAlias to use in the Url. </param>
        /// <returns>a url.</returns>
        public static string DnnLink(this UrlHelper urlHelper, string moduleFolderName, string routeName, object routeValues, PortalAliasInfo portalAliasInfo)
        {
            var parm = new PortalAliasRouteManager();
            var fullName = parm.GetRouteName(moduleFolderName, routeName, PortalController.Instance.GetCurrentPortalSettings().PortalAlias);
            HttpRouteValueDictionary allRouteValues = parm.GetAllRouteValues(portalAliasInfo, routeValues);

            return urlHelper.Link(fullName, allRouteValues);
        }
    }
}
