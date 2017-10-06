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

namespace DotNetNuke.Web.Mvc.Routing
{
    internal static class HttpRequestExtensions
    {
        public static int FindTabId(this HttpRequestBase request)
        {
            return IterateTabAndModuleInfoProviders(request, TryFindTabId, -1);
        }

        private static bool TryFindTabId(ITabAndModuleInfoProvider provider, HttpRequestBase request, out int output)
        {
            return provider.TryFindTabId(request, out output);
        }

        public static ModuleInfo FindModuleInfo(this HttpRequestBase request)
        {
            return IterateTabAndModuleInfoProviders<ModuleInfo>(request, TryFindModuleInfo, null);
        }

        private static bool TryFindModuleInfo(ITabAndModuleInfoProvider provider, HttpRequestBase request, out ModuleInfo output)
        {
            return provider.TryFindModuleInfo(request, out output);
        }

        public static int FindModuleId(this HttpRequestBase request)
        {
            return IterateTabAndModuleInfoProviders(request, TryFindModuleId, -1);
        }

        private static bool TryFindModuleId(ITabAndModuleInfoProvider provider, HttpRequestBase request, out int output)
        {
            return provider.TryFindModuleId(request, out output);
        }

        private delegate bool TryMethod<T>(ITabAndModuleInfoProvider provider, HttpRequestBase request, out T output);

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

        public static string GetIPAddress(HttpRequestBase request)
        {
            return UserRequestIPAddressController.Instance.GetUserRequestIPAddress(request);
        }        
    }
}
