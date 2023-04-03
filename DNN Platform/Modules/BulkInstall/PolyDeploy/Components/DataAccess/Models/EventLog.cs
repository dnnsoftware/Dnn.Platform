using Cantarus.Modules.PolyDeploy.Components.Logging;
using DotNetNuke.ComponentModel.DataAnnotations;
using System;

namespace Cantarus.Modules.PolyDeploy.Components.DataAccess.Models
{
    [TableName("Cantarus_PolyDeploy_EventLogs")]
    [PrimaryKey("EventLogID")]
    public class EventLog
    {
        public int EventLogID { get; set; }
        public DateTime Date { get; set; }
        private string _EventType;
        public string EventType {
            get
            {
                if (!string.IsNullOrEmpty(_EventType))
                {
                    return _EventType.ToUpper();
                }

                return _EventType;
            }

            set
            {
                _EventType = value.ToUpper();
            }
        }
        public EventLogSeverity Severity { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }

        public EventLog() { }

        public EventLog(string eventType, EventLogSeverity severity, string message)
        {
            EventType = eventType;
            Severity = severity;
            Message = message;
        }

        public EventLog(string eventType, EventLogSeverity severity, Exception ex)
        {
            EventType = eventType;
            Severity = severity;
            Message = ex.Message;
            StackTrace = ex.StackTrace;
        }
    }
}
