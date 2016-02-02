using System;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Web;
using System.Web.Http;
using DotNetNuke.Entities.Modules;

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
            var szRemoteAddr = request.UserHostAddress;
            var szXForwardedFor = request.ServerVariables["X_FORWARDED_FOR"];
            var szIP = string.Empty;

            if (szXForwardedFor == null)
            {
                szIP = szRemoteAddr;
            }
            else
            {
                szIP = szXForwardedFor;
                if (szIP.IndexOf(",", StringComparison.Ordinal) <= 0) return szIP;
                var arIPs = szIP.Split(',');
                foreach (var item in arIPs.Where(item => !IsPrivateIP(item)))
                {
                    return item;
                }
            }
            return szIP;
        }
        private static bool IsPrivateIP(string address)
        {
            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
            var ipAddress = new IPAddress(Encoding.UTF8.GetBytes(address));
            return interfaces.Select(iface => iface.GetIPProperties()).SelectMany(properties => properties.UnicastAddresses).Any(ifAddr => ifAddr.IPv4Mask != null && ifAddr.Address.AddressFamily == AddressFamily.InterNetwork && CheckMask(ifAddr.Address, ifAddr.IPv4Mask, ipAddress));
        }

        private static bool CheckMask(IPAddress address, IPAddress mask, IPAddress target)
        {
            if (mask == null)
                return false;

            var ba = address.GetAddressBytes();
            var bm = mask.GetAddressBytes();
            var bb = target.GetAddressBytes();

            if (ba.Length != bm.Length || bm.Length != bb.Length)
                return false;

            for (var i = 0; i < ba.Length; i++)
            {
                var m = bm[i];

                var a = ba[i] & m;
                var b = bb[i] & m;

                if (a != b)
                    return false;
            }

            return true;
        }
    }
}