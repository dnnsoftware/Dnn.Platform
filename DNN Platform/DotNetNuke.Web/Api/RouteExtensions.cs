// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api
{
    using System.Web.Http.Routing;
    using System.Web.Routing;

    /// <summary>Extension methods for <see cref="Route"/> and <see cref="IHttpRoute"/>.</summary>
    public static class RouteExtensions
    {
        private const string NamespaceKey = "namespaces";
        private const string NameKey = "name";

        /// <summary>Get the name of the route.</summary>
        /// <param name="route">The route.</param>
        /// <returns>Route name.</returns>
        public static string GetName(this Route route)
        {
            return (string)route.DataTokens[NameKey];
        }

        /// <summary>Get the name of the route.</summary>
        /// <param name="route">The route.</param>
        /// <returns>Route name.</returns>
        public static string GetName(this IHttpRoute route)
        {
            return (string)route.DataTokens[NameKey];
        }

        /// <summary>Set the namespaces for the route.</summary>
        /// <param name="route">The route.</param>
        /// <param name="namespaces">The namespaces.</param>
        internal static void SetNameSpaces(this Route route, string[] namespaces)
        {
            route.DataTokens[NamespaceKey] = namespaces;
        }

        /// <summary>Set the namespaces for the route.</summary>
        /// <param name="route">The route.</param>
        /// <param name="namespaces">The namespaces.</param>
        internal static void SetNameSpaces(this IHttpRoute route, string[] namespaces)
        {
            route.DataTokens[NamespaceKey] = namespaces;
        }

        /// <summary>Get Namespaces that are searched for controllers for this route.</summary>
        /// <param name="route">The route.</param>
        /// <returns>Namespaces.</returns>
        internal static string[] GetNameSpaces(this Route route)
        {
            return (string[])route.DataTokens[NamespaceKey];
        }

        /// <summary>Get Namespaces that are searched for controllers for this route.</summary>
        /// <param name="route">The route.</param>
        /// <returns>Namespaces.</returns>
        internal static string[] GetNameSpaces(this IHttpRoute route)
        {
            return (string[])route.DataTokens[NamespaceKey];
        }

        /// <summary>Set the name for the route.</summary>
        /// <param name="route">The route.</param>
        /// <param name="name">The name.</param>
        internal static void SetName(this Route route, string name)
        {
            route.DataTokens[NameKey] = name;
        }

        /// <summary>Set the name for the route.</summary>
        /// <param name="route">The route.</param>
        /// <param name="name">The name.</param>
        internal static void SetName(this IHttpRoute route, string name)
        {
            route.DataTokens[NameKey] = name;
        }
    }
}
