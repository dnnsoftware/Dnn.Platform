// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Extensions
{
    using System.Web;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>Dependency injection extensions for HttpContext.</summary>
    public static class HttpContextDependencyInjectionExtensions
    {
        /// <summary>Sets the service scope for the http context base.</summary>
        /// <param name="httpContext">The http context base.</param>
        /// <param name="scope">The service scope.</param>
        public static void SetScope(this HttpContextBase httpContext, IServiceScope scope)
        {
            httpContext.Items[typeof(IServiceScope)] = scope;
        }

        /// <summary>Sets the service scope for the http context.</summary>
        /// <param name="httpContext">The http context.</param>
        /// <param name="scope">The service scope.</param>
        public static void SetScope(this HttpContext httpContext, IServiceScope scope)
        {
            httpContext.Items[typeof(IServiceScope)] = scope;
        }

        /// <summary>Clears the service scope for the http context.</summary>
        /// <param name="httpContext">The http context on which to clear the service scope.</param>
        public static void ClearScope(this HttpContext httpContext)
        {
            httpContext.Items.Remove(typeof(IServiceScope));
        }

        /// <summary>Gets the http context base service scope.</summary>
        /// <param name="httpContext">The http context base from which to get the scope from.</param>
        /// <returns>A service scope.</returns>
        public static IServiceScope GetScope(this HttpContextBase httpContext)
        {
            return GetScope(httpContext.Items);
        }

        /// <summary>Gets the http context service scope.</summary>
        /// <param name="httpContext">The http context from which to get the scope from.</param>
        /// <returns>A service scope.</returns>
        public static IServiceScope GetScope(this HttpContext httpContext)
        {
            return GetScope(httpContext.Items);
        }

        /// <summary>Gets the service scope from a collection of context items.</summary>
        /// <param name="contextItems">A dictionary of context items.</param>
        /// <returns>The found service scope.</returns>
        internal static IServiceScope GetScope(System.Collections.IDictionary contextItems)
        {
            if (!contextItems.Contains(typeof(IServiceScope)))
            {
                return null;
            }

            return contextItems[typeof(IServiceScope)] is IServiceScope scope ? scope : null;
        }
    }
}
