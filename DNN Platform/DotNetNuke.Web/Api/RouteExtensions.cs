#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
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