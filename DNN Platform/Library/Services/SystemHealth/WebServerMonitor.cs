namespace DotNetNuke.Services.SystemHealth
{
    using System;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Scheduling;

    public class WebServerMonitor : SchedulerClient
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WebServerMonitor));

        /// <summary>
        /// Initializes a new instance of the <see cref="WebServerMonitor"/> class.
        /// Constructs a WebServerMonitor SchedulerClient.
        /// </summary>
        /// <param name="objScheduleHistoryItem">A SchedulerHistiryItem.</param>
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

        private void RemoveInActiveServers()
        {
            Logger.Info("Starting RemoveInActiveServers");

            foreach (var s in ServerController.GetInActiveServers(1440))
            {
                ServerController.DeleteServer(s.ServerID);
            }

            Logger.Info("Finished RemoveInActiveServers");
        }
    }
}
