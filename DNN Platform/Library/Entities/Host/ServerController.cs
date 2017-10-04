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
#region Usings

using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Caching;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Data;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Framework;
using DotNetNuke.Instrumentation;
using DotNetNuke.Services.Log.EventLog;

#endregion

namespace DotNetNuke.Entities.Host
{
    public class ServerController
    {

        public const string DefaultUrlAdapter = "DotNetNuke.Entities.Host.ServerWebRequestAdapter, DotNetNuke";

        private const string cacheKey = "WebServers";
        private const int cacheTimeout = 20;
        private const CacheItemPriority cachePriority = CacheItemPriority.High;
        private static readonly DataProvider dataProvider = DataProvider.Instance();
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServerController));

        public static bool UseAppName
        {
            get
            {
                var uniqueServers = new Dictionary<string, string>();
                foreach (ServerInfo server in GetEnabledServers())
                {
                    uniqueServers[server.ServerName] = server.IISAppName;
                }
                return uniqueServers.Count < GetEnabledServers().Count;
            }
        }

        public static void ClearCachedServers()
        {
            DataCache.RemoveCache(cacheKey);
        }

        public static void DeleteServer(int serverID)
        {
            DataProvider.Instance().DeleteServer(serverID);
            ClearCachedServers();
        }

        public static List<ServerInfo> GetEnabledServers()
        {
            var servers = new List<ServerInfo>();
            var storedServers = GetServers();
            if (storedServers != null)
            {
                foreach (ServerInfo server in storedServers)
                {
                    if (server.Enabled)
                    {
                        servers.Add(server);
                    }
                }
            }
            return servers;
        }

        public static string GetExecutingServerName()
        {
            string executingServerName = Globals.ServerName;
            if (UseAppName)
            {
                executingServerName += "-" + Globals.IISAppName;
            }
            Logger.Debug("GetExecutingServerName:" + executingServerName);
            return executingServerName;
        }

        public static string GetServerName(ServerInfo webServer)
        {
            string serverName = webServer.ServerName;
            if (UseAppName)
            {
                serverName += "-" + webServer.IISAppName;
            }
            Logger.Debug("GetServerName:" + serverName);
            return serverName;
        }

        public static List<ServerInfo> GetServers()
        {
            var servers = CBO.GetCachedObject<List<ServerInfo>>(new CacheItemArgs(cacheKey, cacheTimeout, cachePriority), GetServersCallBack);
            return servers;
        }

        public static void UpdateServer(ServerInfo server)
        {
            DataProvider.Instance().UpdateServer(server.ServerID, server.Url, server.UniqueId, server.Enabled, server.ServerGroup);
            ClearCachedServers();
        }

        public static void UpdateServerActivity(ServerInfo server)
        {
            var existServer = GetServers().FirstOrDefault(s => s.ServerName == server.ServerName && s.IISAppName == server.IISAppName);
            var serverId = DataProvider.Instance().UpdateServerActivity(server.ServerName, server.IISAppName, server.CreatedDate, server.LastActivityDate, server.PingFailureCount, server.Enabled);
            
            server.ServerID = serverId;
            if (existServer == null
                || string.IsNullOrEmpty(existServer.Url)
                || (string.IsNullOrEmpty(existServer.UniqueId) && !string.IsNullOrEmpty(GetServerUniqueId())))
            {
                //try to detect the server url from url adapter.
                server.Url = existServer == null || string.IsNullOrEmpty(existServer.Url) ? GetServerUrl() : existServer.Url;
                //try to detect the server unique id from url adapter.
                server.UniqueId = existServer == null || string.IsNullOrEmpty(existServer.UniqueId) ? GetServerUniqueId() : existServer.UniqueId;

                UpdateServer(server);
            }

            
            //log the server info
            var log = new LogInfo();
            log.AddProperty(existServer != null ? "Server Updated" : "Add New Server", server.ServerName);
            log.AddProperty("IISAppName", server.IISAppName);
            log.AddProperty("Last Activity Date", server.LastActivityDate.ToString());
            log.LogTypeKey = existServer != null ? EventLogController.EventLogType.WEBSERVER_UPDATED.ToString() 
                                        : EventLogController.EventLogType.WEBSERVER_CREATED.ToString();
            LogController.Instance.AddLog(log);

            ClearCachedServers();
        }

        public static IServerWebRequestAdapter GetServerWebRequestAdapter()
        {
            var adapterConfig = HostController.Instance.GetString("WebServer_ServerRequestAdapter", DefaultUrlAdapter);
            var adapterType = Reflection.CreateType(adapterConfig);
            return Reflection.CreateInstance(adapterType) as IServerWebRequestAdapter;
        }

        private static object GetServersCallBack(CacheItemArgs cacheItemArgs)
        {
            return CBO.FillCollection<ServerInfo>(dataProvider.GetServers());
        }

        private static string GetServerUrl()
        {
            try
            {
                var adpapter = GetServerWebRequestAdapter();
                if (adpapter == null)
                {
                    return string.Empty;
                }

                return adpapter.GetServerUrl();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return string.Empty;
            }
        }

        private static string GetServerUniqueId()
        {
            try
            {
                var adpapter = GetServerWebRequestAdapter();
                if (adpapter == null)
                {
                    return string.Empty;
                }

                return adpapter.GetServerUniqueId();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return string.Empty;
            }
        }
    }
}