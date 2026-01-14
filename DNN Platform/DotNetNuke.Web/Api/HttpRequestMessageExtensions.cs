// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

#nullable enable
namespace DotNetNuke.Web.Api
{
    using System;
    using System.Net.Http;
    using System.Web;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Services.UserRequest;

    /// <summary>Extension methods for <see cref="HttpRequestMessage"/>.</summary>
    public static class HttpRequestMessageExtensions
    {
        private delegate bool TryMethod<T>(ITabAndModuleInfoProvider provider, HttpRequestMessage request, out T output);

        /// <summary>Gets the Tab ID associated with the <paramref name="request"/>.</summary>
        /// <param name="request">The web API request.</param>
        /// <returns>The tab ID or <c>-1</c>.</returns>
        public static int FindTabId(this HttpRequestMessage request)
        {
            return IterateTabAndModuleInfoProviders(request, TryFindTabId, -1);
        }

        /// <summary>Gets the module associated with the <paramref name="request"/>.</summary>
        /// <param name="request">The web API request.</param>
        /// <returns>The <see cref="ModuleInfo"/> instance or <see langword="null"/>.</returns>
        public static ModuleInfo? FindModuleInfo(this HttpRequestMessage request)
        {
            return IterateTabAndModuleInfoProviders<ModuleInfo>(request, TryFindModuleInfo, null);
        }

        /// <summary>Gets the module ID associated with the <paramref name="request"/>.</summary>
        /// <param name="request">The web API request.</param>
        /// <returns>The module ID or <c>-1</c>.</returns>
        public static int FindModuleId(this HttpRequestMessage request)
        {
            return IterateTabAndModuleInfoProviders(request, TryFindModuleId, -1);
        }

        /// <summary>Gets the <see cref="HttpContextBase"/> connected to the <paramref name="request"/>.</summary>
        /// <param name="request">The web API request.</param>
        /// <returns>The <see cref="HttpContextBase"/> or <see langword="null"/>.</returns>
        public static HttpContextBase? GetHttpContext(this HttpRequestMessage request)
        {
            if (!request.Properties.TryGetValue("MS_HttpContext", out var context))
            {
                return null;
            }

            return context as HttpContextBase;
        }

        /// <summary>Get the IP address of the request.</summary>
        /// <param name="request">The web API request.</param>
        /// <returns>The IPv4 address or <see cref="string.Empty"/> if the IP address of the request is not available.</returns>
        public static string GetIPAddress(this HttpRequestMessage request)
        {
            var context = GetHttpContext(request);
            if (context is null)
            {
                throw new InvalidOperationException("Request does not have an associated HTTP Context, cannot retrieve IP address");
            }

            return UserRequestIPAddressController.Instance.GetUserRequestIPAddress(context.Request);
        }

        private static bool TryFindTabId(ITabAndModuleInfoProvider provider, HttpRequestMessage request, out int output)
        {
            return provider.TryFindTabId(request, out output);
        }

        private static bool TryFindModuleInfo(ITabAndModuleInfoProvider provider, HttpRequestMessage request, out ModuleInfo? output)
        {
            return provider.TryFindModuleInfo(request, out output);
        }

        private static bool TryFindModuleId(ITabAndModuleInfoProvider provider, HttpRequestMessage request, out int output)
        {
            return provider.TryFindModuleId(request, out output);
        }

        private static T? IterateTabAndModuleInfoProviders<T>(HttpRequestMessage request, TryMethod<T?> func, T? fallback)
        {
            var providers = request.GetConfiguration().GetTabAndModuleInfoProviders();

            foreach (var provider in providers)
            {
                if (func(provider, request, out var output))
                {
                    return output;
                }
            }

            return fallback;
        }
    }
}
