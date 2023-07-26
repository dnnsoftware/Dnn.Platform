// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Services.SystemHealth
{
    using System;
    using System.Linq;

    using DotNetNuke.Entities.Host;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Scheduling;

    /// <summary>
    /// When run on each server it updates the last activity date for the server and removes any servers that haven't been seen in 24 hours.
    /// </summary>
    public class WebServerMonitor : SchedulerClient
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WebServerMonitor));

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServerMonitor"/> class.
        /// Constructs a WebServerMonitor SchedulerClient.
        /// </summary>
        /// <param name="objScheduleHistoryItem">A SchedulerHistoryItem.</param>
        /// <remarks>
        /// This must be run on all servers.
        /// </remarks>
        public WebServerMonitor(ScheduleHistoryItem objScheduleHistoryItem)
        {
            this.ScheduleHistoryItem = objScheduleHistoryItem;
        }

        /// <summary>
        /// Runs on the active server and updates the last activity date for the current server.
        /// </summary>
        public override void DoWork()
        {
            try
            {
                Logger.Info("Starting WebServerMonitor");

                this.UpdateCurrentServerActivity();
                this.DisableServersWithoutRecentActivity();
                this.RemoveInActiveServers();

                Logger.Info("Finished WebServerMonitor");
                this.ScheduleHistoryItem.Succeeded = true;
            }
            catch (Exception exc)
            {
                this.ScheduleHistoryItem.Succeeded = false;
                this.ScheduleHistoryItem.AddLogNote(string.Format("Updating server health failed: {0}.", exc.ToString()));
                this.Errored(ref exc);
                Logger.ErrorFormat("Error in WebServerMonitor: {0}. {1}", exc.Message, exc.StackTrace);
                Exceptions.LogException(exc);
            }
        }

        private void UpdateCurrentServerActivity()
        {
            Logger.Info("Starting UpdateCurrentServerActivity");

            // Creating a new ServerInfo object, by default points it at the current server
            var currentServer = new ServerInfo();
            ServerController.UpdateServerActivity(currentServer);

            Logger.Info("Finished UpdateCurrentServerActivity");
        }

        private void DisableServersWithoutRecentActivity()
        {
            var serversWithActivity = ServerController.GetEnabledServersWithActivity();
            var newServer = serversWithActivity.FirstOrDefault();

            foreach (var s in ServerController.GetInActiveServers(10))
            {
                s.Enabled = false;
                ServerController.UpdateServerActivity(s);

                // Update the schedules that were running on that server, that may never have been loaded
                if (newServer != null)
                {
                    SchedulingController.ReplaceServer(s, newServer);
                }
            }
        }

        private void RemoveInActiveServers()
        {
            Logger.Info("Starting RemoveInActiveServers");
            var serversWithActivity = ServerController.GetEnabledServersWithActivity();
            var newServer = serversWithActivity.FirstOrDefault();

            foreach (var s in ServerController.GetInActiveServers(1440))
            {
                ServerController.DeleteServer(s.ServerID);

                // Update the schedules that were running on that server, that may never have been loaded
                if (newServer != null)
                {
                    SchedulingController.ReplaceServer(s, newServer);
                }
            }

            Logger.Info("Finished RemoveInActiveServers");
        }
    }
}
