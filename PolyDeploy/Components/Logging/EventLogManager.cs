using Cantarus.Modules.PolyDeploy.Components.DataAccess.DataControllers;
using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using System;

namespace Cantarus.Modules.PolyDeploy.Components.Logging
{
    internal static class EventLogManager
    {
        private static EventLogDataController LogDC = new EventLogDataController();

        public static void Log(string eventType, EventLogSeverity severity, string message = null, Exception ex = null)
        {
            // TODO: Internal logging switched on?
            LogInternal(eventType, severity, message, ex);

            // TODO: DNN logging switched on?
            // Log to DNN event log.
        }

        private static void LogInternal(string eventType, EventLogSeverity severity, string message, Exception ex)
        {
            EventLog eventLog;

            if (ex != null)
            {
                eventLog = new EventLog(eventType, severity, ex);
            }
            else
            {
                eventLog = new EventLog(eventType, severity, message);
            }

            LogDC.Create(eventLog);
        }
    }
}
