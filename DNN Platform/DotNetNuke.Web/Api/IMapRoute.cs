// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System;
    using System.Collections.Generic;
    using System.Web.Routing;

    public interface IMapRoute
    {
        /// <summary>
        /// Sets up the route(s) for DotNetNuke services.
        /// </summary>
        /// <param name="moduleFolderName">The name of the folder under DesktopModules in which your module resides.</param>
        /// <param name="routeName">A unique name for the route.</param>
        /// <param name="url">The parameterized portion of the route.</param>
        /// <param name="defaults">Default values for the route parameters.</param>
        /// <param name="constraints">The constraints.</param>
        /// <param name="namespaces">The namespace(s) in which to search for the controllers for this route.</param>
        /// <returns>A list of all routes that were registered.</returns>
        /// <remarks>The combination of moduleFolderName and routeName must be unique for each route.</remarks>
        IList<Route> MapHttpRoute(string moduleFolderName, string routeName, string url, object defaults, object constraints, string[] namespaces);

        /// <summary>
        /// Sets up the route(s) for DotNetNuke services.
        /// </summary>
        /// <param name="moduleFolderName">The name of the folder under DesktopModules in which your module resides.</param>
        /// <param name="routeName">A unique name for the route.</param>
        /// <param name="url">The parameterized portion of the route.</param>
        /// <param name="defaults">Default values for the route parameters.</param>
        /// <param name="namespaces">The namespace(s) in which to search for the controllers for this route.</param>
        /// <returns>A list of all routes that were registered.</returns>
        /// <remarks>The combination of moduleFolderName and routeName must be unique for each route.</remarks>
        IList<Route> MapHttpRoute(string moduleFolderName, string routeName, string url, object defaults, string[] namespaces);

        /// <summary>
        /// Sets up the route(s) for DotNetNuke services.
        /// </summary>
        /// <param name="moduleFolderName">The name of the folder under DesktopModules in which your module resides.</param>
        /// <param name="routeName">A unique name for the route.</param>
        /// <param name="url">The parameterized portion of the route.</param>
        /// <param name="namespaces">The namespace(s) in which to search for the controllers for this route.</param>
        /// <returns>A list of all routes that were registered.</returns>
        /// <remarks>The combination of moduleFolderName and routeName must be unique for each route.</remarks>
        IList<Route> MapHttpRoute(string moduleFolderName, string routeName, string url, string[] namespaces);
    }
}
