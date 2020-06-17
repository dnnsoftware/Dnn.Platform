// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System;
    using System.Web.Http.Routing;
    using System.Web.Routing;

    public static class RouteExtensions
    {
        private const string NamespaceKey = "namespaces";
        private const string NameKey = "name";

        /// <summary>
        /// Get the name of the route.
        /// </summary>
        /// <returns>Route name.</returns>
        public static string GetName(this Route route)
        {
            return (string)route.DataTokens[NameKey];
        }

        /// <summary>
        /// Get the name of the route.
        /// </summary>
        /// <returns>Route name.</returns>
        public static string GetName(this IHttpRoute route)
        {
            return (string)route.DataTokens[NameKey];
        }

        internal static void SetNameSpaces(this Route route, string[] namespaces)
        {
            route.DataTokens[NamespaceKey] = namespaces;
        }

        internal static void SetNameSpaces(this IHttpRoute route, string[] namespaces)
        {
            route.DataTokens[NamespaceKey] = namespaces;
        }

        /// <summary>
        /// Get Namespaces that are searched for controllers for this route.
        /// </summary>
        /// <returns>Namespaces.</returns>
        internal static string[] GetNameSpaces(this Route route)
        {
            return (string[])route.DataTokens[NamespaceKey];
        }

        /// <summary>
        /// Get Namespaces that are searched for controllers for this route.
        /// </summary>
        /// <returns>Namespaces.</returns>
        internal static string[] GetNameSpaces(this IHttpRoute route)
        {
            return (string[])route.DataTokens[NamespaceKey];
        }

        internal static void SetName(this Route route, string name)
        {
            route.DataTokens[NameKey] = name;
        }

        internal static void SetName(this IHttpRoute route, string name)
        {
            route.DataTokens[NameKey] = name;
        }
    }
}
