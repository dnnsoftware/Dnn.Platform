// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Entities.Host
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Web.Caching;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Data;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.UI.WebControls;

    public class ServerController
    {
        public const string DefaultUrlAdapter = "DotNetNuke.Entities.Host.ServerWebRequestAdapter, DotNetNuke";

        private const string CacheKey = "WebServers";
        private const int CacheTimeout = 20;
        private const CacheItemPriority CachePriority = CacheItemPriority.High;
        private static readonly DataProvider DataProvider = DataProvider.Instance();
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
            DataCache.RemoveCache(CacheKey);
        }

        public static void DeleteServer(int serverID)
        {
            DataProvider.Instance().DeleteServer(serverID);
            ClearCachedServers();
        }

        public static List<ServerInfo> GetEnabledServers()
        {
            return GetServers()?.Where(i => i.Enabled).ToList() ?? new List<ServerInfo>();
        }

        /// <summary>
        /// Gets the servers, order by last activity date in descending order.
        /// </summary>
        /// <param name="lastMinutes">The number of recent minutes activity had to occur.</param>
        /// <returns>A list of servers with activity within the specified minutes.</returns>
        public static List<ServerInfo> GetEnabledServersWithActivity(int lastMinutes = 10)
        {
            return GetServersNoCache()
                .Where(i => i.Enabled == true && DateTime.Now.Subtract(i.LastActivityDate).TotalMinutes <= lastMinutes)
                .OrderByDescending(i => i.LastActivityDate)
                .ToList();
        }

        /// <summary>
        /// Gets the servers, that have no activtiy in the specified time frame.
        /// </summary>
        /// <param name="lastMinutes">The number of recent minutes activity had to occur.</param>
        /// <returns>A list of servers with no activity for the specified minutes. Defaults to 24 hours.</returns>
        public static List<ServerInfo> GetInActiveServers(int lastMinutes = 1440)
        {
            return GetServersNoCache()
                .Where(i => DateTime.Now.Subtract(i.LastActivityDate).TotalMinutes > lastMinutes && i.ServerName != Environment.MachineName)
                .OrderByDescending(i => i.LastActivityDate)
                .ToList();
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
            var servers = CBO.GetCachedObject<List<ServerInfo>>(new CacheItemArgs(CacheKey, CacheTimeout, CachePriority), GetServersCallBack);
            return servers;
        }

        public static void UpdateServer(ServerInfo server)
        {
            DataProvider.Instance().UpdateServer(server.ServerID, server.Url, server.UniqueId, server.Enabled, server.ServerGroup);
            ClearCachedServers();
        }

        public static void UpdateServerActivity(ServerInfo server)
        {
            var allServers = GetServers();
            var existServer = allServers.FirstOrDefault(s => s.ServerName == server.ServerName && s.IISAppName == server.IISAppName);
            var serverId = DataProvider.Instance().UpdateServerActivity(server.ServerName, server.IISAppName, server.CreatedDate, server.LastActivityDate, server.PingFailureCount, server.Enabled);

            server.ServerID = serverId;
            if (existServer == null
                || string.IsNullOrEmpty(existServer.Url)
                || (string.IsNullOrEmpty(existServer.UniqueId) && !string.IsNullOrEmpty(GetServerUniqueId())))
            {
                // try to detect the server url from url adapter.
                server.Url = existServer == null || string.IsNullOrEmpty(existServer.Url) ? GetServerUrl() : existServer.Url;

                // try to detect the server unique id from url adapter.
                server.UniqueId = existServer == null || string.IsNullOrEmpty(existServer.UniqueId) ? GetServerUniqueId() : existServer.UniqueId;

                UpdateServer(server);
                ClearCachedServers(); // Only clear the cache if we added a server
            }
            else
            {
                // Just update the existing item in the cache
                existServer.LastActivityDate = server.LastActivityDate;
                existServer.Enabled = server.Enabled;
                DataCache.SetCache(ServerController.CacheKey, allServers);
            }

            // log the server info
            var log = new LogInfo();
            log.AddProperty(existServer != null ? "Server Updated" : "Add New Server", server.ServerName);
            log.AddProperty("IISAppName", server.IISAppName);
            log.AddProperty("Last Activity Date", server.LastActivityDate.ToString(CultureInfo.InvariantCulture));
            log.LogTypeKey = existServer != null ? nameof(EventLogType.WEBSERVER_UPDATED)
                                        : nameof(EventLogType.WEBSERVER_CREATED);
            LogController.Instance.AddLog(log);
        }

        public static IServerWebRequestAdapter GetServerWebRequestAdapter()
        {
            var adapterConfig = HostController.Instance.GetString("WebServer_ServerRequestAdapter", DefaultUrlAdapter);
            var adapterType = Reflection.CreateType(adapterConfig);
            return Reflection.CreateInstance(adapterType) as IServerWebRequestAdapter;
        }

        private static List<ServerInfo> GetServersNoCache()
        {
            return CBO.FillCollection<ServerInfo>(DataProvider.GetServers());
        }

        private static object GetServersCallBack(CacheItemArgs cacheItemArgs)
        {
            return CBO.FillCollection<ServerInfo>(DataProvider.GetServers());
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
