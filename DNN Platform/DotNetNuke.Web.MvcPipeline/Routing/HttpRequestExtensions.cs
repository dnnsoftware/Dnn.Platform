// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Routing
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.NetworkInformation;
    using System.Net.Sockets;
    using System.Text;
    using System.Web;
    using System.Web.Http;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.UserRequest;

    /// <summary>
    /// Extension methods for <see cref="HttpRequestBase"/> to find DNN tab and module information.
    /// </summary>
    internal static class HttpRequestExtensions
    {
        private delegate bool TryMethod<T>(ITabAndModuleInfoProvider provider, HttpRequestBase request, out T output);

        /// <summary>
        /// Finds the tab identifier associated with the current request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The tab identifier, or -1 if not found.</returns>
        public static int FindTabId(this HttpRequestBase request)
        {
            return IterateTabAndModuleInfoProviders(request, TryFindTabId, -1);
        }

        /// <summary>
        /// Finds the module information associated with the current request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The <see cref="ModuleInfo"/> instance, or <c>null</c> if not found.</returns>
        public static ModuleInfo FindModuleInfo(this HttpRequestBase request)
        {
            return IterateTabAndModuleInfoProviders<ModuleInfo>(request, TryFindModuleInfo, null);
        }

        /// <summary>
        /// Finds the module identifier associated with the current request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The module identifier, or -1 if not found.</returns>
        public static int FindModuleId(this HttpRequestBase request)
        {
            return IterateTabAndModuleInfoProviders(request, TryFindModuleId, -1);
        }

        /// <summary>
        /// Gets the client IP address for the current request.
        /// </summary>
        /// <param name="request">The HTTP request.</param>
        /// <returns>The client IP address.</returns>
        public static string GetIPAddress(HttpRequestBase request)
        {
            return UserRequestIPAddressController.Instance.GetUserRequestIPAddress(request);
        }

        private static bool TryFindTabId(ITabAndModuleInfoProvider provider, HttpRequestBase request, out int output)
        {
            return provider.TryFindTabId(request, out output);
        }

        private static bool TryFindModuleInfo(ITabAndModuleInfoProvider provider, HttpRequestBase request, out ModuleInfo output)
        {
            return provider.TryFindModuleInfo(request, out output);
        }

        private static bool TryFindModuleId(ITabAndModuleInfoProvider provider, HttpRequestBase request, out int output)
        {
            return provider.TryFindModuleId(request, out output);
        }

        /// <summary>
        /// Iterates over the registered <see cref="ITabAndModuleInfoProvider"/> instances to locate tab or module information.
        /// </summary>
        /// <typeparam name="T">The type of value to locate.</typeparam>
        /// <param name="request">The HTTP request.</param>
        /// <param name="func">The delegate used to attempt to find the value.</param>
        /// <param name="fallback">The fallback value if nothing is found.</param>
        /// <returns>The located value or the fallback.</returns>
        private static T IterateTabAndModuleInfoProviders<T>(HttpRequestBase request, TryMethod<T> func, T fallback)
        {
            var providers = GlobalConfiguration.Configuration.GetTabAndModuleInfoProviders();

            foreach (var provider in providers)
            {
                T output;
                if (func(provider, request, out output))
                {
                    return output;
                }
            }

            return fallback;
        }
    }
}
