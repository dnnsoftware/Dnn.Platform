#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Web;
using System.Xml;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Host;
using DotNetNuke.Framework.Providers;
using DotNetNuke.Services.Cache;

namespace Dnn.PersonaBar.Servers.Components.WebServer
{
    public class WebServersController
    {
        public List<string> GetWebRequestAdapters()
        {
            var webRequestAdapters = new List<string>();
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    webRequestAdapters.AddRange(from t in assembly.GetTypes()
                                                where t.GetInterface("IServerWebRequestAdapter") != null
                                                select $"{t.FullName}, {assembly.GetName().Name}");
                }
                catch (Exception)
                {
                    //do nothing but just ignore the error.
                }
            }
            return webRequestAdapters;
        }

        public object GetCachingInfo()
        {
            return new
            {
                CachingProviders = ProviderConfiguration.GetProviderConfiguration("caching").Providers.Keys.Cast<string>().ToList(),
                UseIISAppName = ServerController.UseAppName,
                WebFarmEnabled = CachingProvider.Instance().IsWebFarm(),
                UseSSL = HostController.Instance.GetBoolean(Constants.UseSSLKey, false),
                DefaultCachingProvider = ProviderConfiguration.GetProviderConfiguration("caching").DefaultProvider
            };
        }

        public object GetMemoryUsage()
        {
            return new
            {
                Globals.ServerName,
                HttpContext.Current.Cache.Count,
                PrivateBytes = GetEffectivePrivateBytesLimit(),
                Limit = HttpContext.Current.Cache.EffectivePercentagePhysicalMemoryLimit + "%"
            };
        }

        public object GetServers()
        {
            return ServerController.GetServers().Select(s => new
            {
                s.ServerID,
                s.ServerName,
                s.IISAppName,
                s.CreatedDate,
                s.LastActivityDate,
                s.Enabled,
                s.ServerGroup,
                s.Url
            }).ToList();
        }

        public object GetCacheItem(string cacheKey)
        {
            return new
            {
                key = cacheKey,
                value = GetCacheItemValue(cacheKey),
                size = GetCacheItemSize(cacheKey)
            };
        }

        public void SaveWebServerSettings(string url, int serverId, bool enabled, string serverGroup)
        {
            if (!string.IsNullOrEmpty(url) && url.Contains("://"))
            {
                url = url.Substring(url.IndexOf("://") + 3);
            }
            var webServer = ServerController.GetServers().First(s => s.ServerID == serverId);
            webServer.Enabled = enabled;
            webServer.Url = url;
            webServer.ServerGroup = serverGroup;
            ServerController.UpdateServer(webServer);
        }

        private string GetEffectivePrivateBytesLimit()
        {
            var effectivePrivateBytesLimit = string.Empty;
            if (HttpContext.Current.Cache.EffectivePrivateBytesLimit > 1024 * 1024 * 1024)
            {
                effectivePrivateBytesLimit = (HttpContext.Current.Cache.EffectivePrivateBytesLimit / (1024.0 * 1024.0 * 1024.0)).ToString("N2") + " GB";
            }
            else if (HttpContext.Current.Cache.EffectivePrivateBytesLimit > 1024 * 1024)
            {
                effectivePrivateBytesLimit = (HttpContext.Current.Cache.EffectivePrivateBytesLimit / (1024.0 * 1024.0)).ToString("N2") + " MB";
            }
            else
            {
                effectivePrivateBytesLimit = Math.Round(HttpContext.Current.Cache.EffectivePrivateBytesLimit / 1024.0) + " KB";
            }
            return effectivePrivateBytesLimit;
        }

        private string GetCacheItemValue(string key)
        {
            try
            {
                var sb = new StringBuilder();
                CBO.SerializeObject(HttpContext.Current.Cache[key], XmlWriter.Create(sb, XmlUtils.GetXmlWriterSettings(ConformanceLevel.Document)));
                return HttpUtility.HtmlEncode(sb.ToString());
            }
            catch
            {
                return Constants.ValueCannotSerialize;
            }
        }

        private string GetCacheItemSize(string key)
        {
            try
            {
                using (var ms = new MemoryStream())
                {
                    var bf = new BinaryFormatter();
                    bf.Serialize(ms, HttpContext.Current.Cache[key]);
                    return ms.Length.ToString();
                }
            }
            catch
            {
                return Constants.SizeUnknown;
            }
        }
    }
}