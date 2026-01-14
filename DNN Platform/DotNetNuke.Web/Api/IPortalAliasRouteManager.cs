// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System.Collections.Generic;
    using System.Web.Http.Routing;

    using DotNetNuke.Entities.Portals;

    /// <summary>A contract specifying the ability to manage routes for a portal alias.</summary>
    internal interface IPortalAliasRouteManager
    {
        /// <summary>Gets the prefix counts.</summary>
        /// <returns>A sequence of counts.</returns>
        IEnumerable<int> GetRoutePrefixCounts();

        /// <summary>Gets the route name.</summary>
        /// <param name="moduleFolderName">The module folder name.</param>
        /// <param name="routeName">The route name.</param>
        /// <param name="count">The prefix count.</param>
        /// <returns>The complete route name.</returns>
        string GetRouteName(string moduleFolderName, string routeName, int count);

        /// <summary>Gets the route name.</summary>
        /// <param name="moduleFolderName">The module folder name.</param>
        /// <param name="routeName">The route name.</param>
        /// <param name="portalAlias">The portal alias.</param>
        /// <returns>The complete route name.</returns>
        string GetRouteName(string moduleFolderName, string routeName, PortalAliasInfo portalAlias);

        /// <summary>Adds prefix route values to the <paramref name="routeValues"/>.</summary>
        /// <param name="portalAliasInfo">The portal alias.</param>
        /// <param name="routeValues">The route values.</param>
        /// <returns>A new <see cref="HttpRouteValueDictionary"/> with prefixes added.</returns>
        HttpRouteValueDictionary GetAllRouteValues(PortalAliasInfo portalAliasInfo, object routeValues);

        /// <summary>Get the route URL.</summary>
        /// <param name="moduleFolderName">The module folder name.</param>
        /// <param name="url">The URL.</param>
        /// <param name="count">The prefix count.</param>
        /// <returns>The prefixed URL.</returns>
        string GetRouteUrl(string moduleFolderName, string url, int count);

        /// <summary>Clears cached route data.</summary>
        void ClearCachedData();
    }
}
