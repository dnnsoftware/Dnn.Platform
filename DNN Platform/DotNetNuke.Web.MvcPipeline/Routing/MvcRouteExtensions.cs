// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Routing
{
    using System.Web.Routing;

    /// <summary>
    /// Extension methods for working with MVC <see cref="Route"/> instances.
    /// </summary>
    public static class MvcRouteExtensions
    {
        private const string NamespaceKey = "namespaces";
        private const string NameKey = "name";

        /// <summary>Gets the name of the route.</summary>
        /// <param name="route">The route instance.</param>
        /// <returns>The route name.</returns>
        public static string GetName(this Route route)
        {
            return (string)route.DataTokens[NameKey];
        }

        /// <summary>
        /// Sets the namespaces that are searched for controllers for this route.
        /// </summary>
        /// <param name="route">The route instance.</param>
        /// <param name="namespaces">The controller namespaces.</param>
        internal static void SetNameSpaces(this Route route, string[] namespaces)
        {
            route.DataTokens[NamespaceKey] = namespaces;
        }

        /// <summary>Gets namespaces that are searched for controllers for this route.</summary>
        /// <param name="route">The route instance.</param>
        /// <returns>The controller namespaces.</returns>
        internal static string[] GetNameSpaces(this Route route)
        {
            return (string[])route.DataTokens[NamespaceKey];
        }

        /// <summary>
        /// Sets the name of the route.
        /// </summary>
        /// <param name="route">The route instance.</param>
        /// <param name="name">The route name.</param>
        internal static void SetName(this Route route, string name)
        {
            route.DataTokens[NameKey] = name;
        }
    }
}
