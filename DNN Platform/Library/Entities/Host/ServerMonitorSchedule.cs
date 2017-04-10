﻿#region Copyright
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2016
// by DotNetNuke Corporation
// All Rights Reserved
#endregion

using System;
using System.Linq;
using System.Net;
using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.Services.Log.EventLog;
using DotNetNuke.Services.Scheduling;

namespace DotNetNuke.Entities.Host
{
    public class ServerMonitorSchedule : SchedulerClient
    {
        #region Consts

        private const int MaxFailureCount = 3;
        private const string PingPath = "/KeepAlive.aspx";

        #endregion

        #region Constructors

        public ServerMonitorSchedule(ScheduleHistoryItem scheduleHistoryItem)
        {
            ScheduleHistoryItem = scheduleHistoryItem;
        }

        #endregion

        #region ScheduleClient Implements

        public override void DoWork()
        {
            try
            {
                //notification that the event is progressing
                Progressing(); //OPTIONAL
                CheckServersStatus();
                ScheduleHistoryItem.Succeeded = true; //REQUIRED
            }
            catch (Exception exc) //REQUIRED
            {
                ScheduleHistoryItem.Succeeded = false; //REQUIRED
                ScheduleHistoryItem.AddLogNote("EXCEPTION: " + exc); //OPTIONAL
                Errored(ref exc); //REQUIRED
                //log the exception
                Exceptions.LogException(exc); //OPTIONAL
            }
        }

        #endregion

        #region Private Methods

        private void CheckServersStatus()
        {
            var servers = ServerController.GetServers().Where(s => s.Enabled 
                                                                    && (s.ServerName != Globals.ServerName || s.IISAppName != Globals.IISAppName)
                                                                    && !string.IsNullOrEmpty(s.Url)).ToList();

            if (!servers.Any())
            {
                ScheduleHistoryItem.AddLogNote("<p>No server available to detect.<p>");
            }

            foreach (var server in servers)
            {
                TryConnect(server, (statusCode) =>
                                       {
                                           if (statusCode == HttpStatusCode.OK)
                                           {
                                               UpdateAvailableServer(server);
                                           }
                                           else
                                           {
                                               UpdateUnavailableServer(server);
                                           }
                                       });
            }
        }

        private void TryConnect(ServerInfo server, Action<HttpStatusCode> connectCompleteCallback)
        {
            var connectUrl = $"{Globals.AddHTTP(server.Url)}{PingPath}";
            try
            {
                var serverRequestAdpater = ServerController.GetServerWebRequestAdapter();
                var request = Globals.GetExternalRequest(connectUrl);
                if (request.CookieContainer == null)
                {
                    request.CookieContainer = new CookieContainer();
                }

                //call web request adapter to process the request before send.
                serverRequestAdpater?.ProcessRequest(request, server);

                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    var statusCode = response.StatusCode;

                    //call web request adapter ro check the response.
                    serverRequestAdpater?.CheckResponse(response, server, ref statusCode);

                    connectCompleteCallback(statusCode);
                }
            }
            catch (Exception)
            {
                connectCompleteCallback(HttpStatusCode.BadRequest);
            }
        }

        private void UpdateAvailableServer(ServerInfo server)
        {
            if (server.PingFailureCount > 0)
            {
                server.PingFailureCount = 0;
                server.LastActivityDate = DateTime.Now;

                ServerController.UpdateServerActivity(server);
            }
            ScheduleHistoryItem.AddLogNote($"<p>Server \"{server.ServerName}-{server.IISAppName}\" Connected Successful.</p>");
        }

        private void UpdateUnavailableServer(ServerInfo server)
        {
            server.PingFailureCount++;
            ServerController.UpdateServerActivity(server);

            AddLogEvent(server, EventLogController.EventLogType.WEBSERVER_PINGFAILED.ToString());
            ScheduleHistoryItem.AddLogNote($"<p>Server \"{server.ServerName}-{server.IISAppName}\" Connected Failed in {server.PingFailureCount} time(s).</p>");

            if (server.PingFailureCount == MaxFailureCount)
            {
                server.Enabled = false;
                ServerController.UpdateServer(server);

                AddLogEvent(server, EventLogController.EventLogType.WEBSERVER_DISABLED.ToString());
                ScheduleHistoryItem.AddLogNote($"<p>Server \"{server.ServerName}-{server.IISAppName}\" Failed times reach max failure times, disable the server. </p>");
            }
        }

        private void AddLogEvent(ServerInfo server, string eventLogType)
        {
            var log = new LogInfo {LogTypeKey = eventLogType};
            log.AddProperty("Server", server.ServerName);
            log.AddProperty("IISAppName", server.IISAppName);
            log.AddProperty("Last Activity Date", server.LastActivityDate.ToString("G"));
            LogController.Instance.AddLog(log);
        }
            
        #endregion
    }
}