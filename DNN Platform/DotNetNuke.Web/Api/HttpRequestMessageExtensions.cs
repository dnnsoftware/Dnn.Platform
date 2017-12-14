#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
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