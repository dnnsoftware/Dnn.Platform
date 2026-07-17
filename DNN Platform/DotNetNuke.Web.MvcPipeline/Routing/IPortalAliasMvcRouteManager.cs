// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Routing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Routing;

    using DotNetNuke.Abstractions.Portals;
    using DotNetNuke.Entities.Portals;

    /// <summary>
    /// Provides methods to manage MVC route mappings that depend on portal aliases.
    /// </summary>
    internal interface IPortalAliasMvcRouteManager
    {
        /// <summary>
        /// Gets all distinct route prefix counts for the current portals and aliases.
        /// </summary>
        /// <returns>A collection of prefix segment counts.</returns>
        IEnumerable<int> GetRoutePrefixCounts();

        /// <summary>
        /// Gets a route name for the specified module folder, route name, and prefix count.
        /// </summary>
        /// <param name="moduleFolderName">The module folder name.</param>
        /// <param name="routeName">The base route name.</param>
        /// <param name="count">The prefix count.</param>
        /// <returns>The generated route name.</returns>
        string GetRouteName(string moduleFolderName, string routeName, int count);

        /// <summary>
        /// Gets all route values for a specific portal alias by expanding its prefix segments.
        /// </summary>
        /// <param name="portalAliasInfo">The portal alias information.</param>
        /// <param name="routeValues">The base route values.</param>
        /// <returns>A route value dictionary including prefix segments.</returns>
        RouteValueDictionary GetAllRouteValues(IPortalAliasInfo portalAliasInfo, object routeValues);

        /// <summary>
        /// Gets the full route URL for the specified module folder, relative URL, and prefix count.
        /// </summary>
        /// <param name="moduleFolderName">The module folder name.</param>
        /// <param name="url">The relative URL.</param>
        /// <param name="count">The prefix count.</param>
        /// <returns>The full route URL.</returns>
        string GetRouteUrl(string moduleFolderName, string url, int count);

        /// <summary>
        /// Clears any cached data used for route generation.
        /// </summary>
        void ClearCachedData();
    }
}
