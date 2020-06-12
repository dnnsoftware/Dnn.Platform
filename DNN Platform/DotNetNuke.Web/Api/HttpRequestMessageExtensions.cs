// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System.Net.Http;
using System.Web;

using DotNetNuke.Entities.Modules;
using DotNetNuke.Services.UserRequest;

namespace DotNetNuke.Web.Api
{
    public static class HttpRequestMessageExtensions
    {
        public static int FindTabId(this HttpRequestMessage request)
        {
            return IterateTabAndModuleInfoProviders(request, TryFindTabId, -1);
        }

        private static bool TryFindTabId(ITabAndModuleInfoProvider provider, HttpRequestMessage request, out int output)
        {
            return provider.TryFindTabId(request, out output);
        }

        public static ModuleInfo FindModuleInfo(this HttpRequestMessage request)
        {
            return IterateTabAndModuleInfoProviders<ModuleInfo>(request, TryFindModuleInfo, null);
        }

        private static bool TryFindModuleInfo(ITabAndModuleInfoProvider provider, HttpRequestMessage request, out ModuleInfo output)
        {
            return provider.TryFindModuleInfo(request, out output);
        }

        public static int FindModuleId(this HttpRequestMessage request)
        {
            return IterateTabAndModuleInfoProviders(request, TryFindModuleId, -1);
        }

        private static bool TryFindModuleId(ITabAndModuleInfoProvider provider, HttpRequestMessage request, out int output)
        {
            return provider.TryFindModuleId(request, out output);
        }

        private delegate bool TryMethod<T>(ITabAndModuleInfoProvider provider, HttpRequestMessage request, out T output);

        private static T IterateTabAndModuleInfoProviders<T>(HttpRequestMessage request, TryMethod<T> func, T fallback)
        {
            var providers = request.GetConfiguration().GetTabAndModuleInfoProviders();

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

        public static HttpContextBase GetHttpContext(this HttpRequestMessage request)
        {
            object context;
            request.Properties.TryGetValue("MS_HttpContext", out context);

            return context as HttpContextBase;
        }

        public static string GetIPAddress(this HttpRequestMessage request)
        {
            return UserRequestIPAddressController.Instance.GetUserRequestIPAddress(GetHttpContext(request).Request);
        }
    }
}
