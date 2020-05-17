// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Web.Http.Routing;
using System.Web.Routing;

namespace DotNetNuke.Web.Api
{
    public static class RouteExtensions
    {
        private const string NamespaceKey = "namespaces";
        private const string NameKey = "name";

        internal static void SetNameSpaces(this Route route, string[] namespaces)
        {
            route.DataTokens[NamespaceKey] = namespaces;
        }

        internal static void SetNameSpaces(this IHttpRoute route, string[] namespaces)
        {
            route.DataTokens[NamespaceKey] = namespaces;
        }

        /// <summary>
        /// Get Namespaces that are searched for controllers for this route
        /// </summary>
        /// <returns>Namespaces</returns>
        internal static string[] GetNameSpaces(this Route route)
        {
            return (string[]) route.DataTokens[NamespaceKey];
        }

        /// <summary>
        /// Get Namespaces that are searched for controllers for this route
        /// </summary>
        /// <returns>Namespaces</returns>
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

        /// <summary>
        /// Get the name of the route
        /// </summary>
        /// <returns>Route name</returns>
        public static string GetName(this Route route)
        {
            return (string) route.DataTokens[NameKey];
        }

        /// <summary>
        /// Get the name of the route
        /// </summary>
        /// <returns>Route name</returns>
        public static string GetName(this IHttpRoute route)
        {
            return (string)route.DataTokens[NameKey];
        }
    }
}
