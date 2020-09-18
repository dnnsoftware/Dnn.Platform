// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Common.Extensions
{
    using System.Web;

    using Microsoft.Extensions.DependencyInjection;

    public static class HttpContextDependencyInjectionExtensions
    {
        public static void SetScope(this HttpContextBase httpContext, IServiceScope scope)
        {
            httpContext.Items[typeof(IServiceScope)] = scope;
        }

        public static void SetScope(this HttpContext httpContext, IServiceScope scope)
        {
            httpContext.Items[typeof(IServiceScope)] = scope;
        }

        public static void ClearScope(this HttpContext httpContext)
        {
            httpContext.Items.Remove(typeof(IServiceScope));
        }

        public static IServiceScope GetScope(this HttpContextBase httpContext)
        {
            return GetScope(httpContext.Items);
        }

        public static IServiceScope GetScope(this HttpContext httpContext)
        {
            return GetScope(httpContext.Items);
        }

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
