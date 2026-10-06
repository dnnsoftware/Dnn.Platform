// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.BulkInstall.DataAccess.Models
{
    using System;

    using Dnn.PersonaBar.Extensions.Components.BulkInstall.Logging;
    using DotNetNuke.ComponentModel.DataAnnotations;

    /// <summary>A database entity representing an event log entry.</summary>
    [TableName("BulkInstall_EventLogs")]
    [PrimaryKey("EventLogID")]
    public class EventLog
    {
        private string eventType;

        /// <summary>Initializes a new instance of the <see cref="EventLog"/> class.</summary>
        public EventLog()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="EventLog"/> class.</summary>
        /// <param name="eventType">The type of event.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="message">The event message.</param>
        public EventLog(string eventType, EventLogSeverity severity, string message)
        {
            this.EventType = eventType;
            this.Severity = severity;
            this.Message = message;
        }

        /// <summary>Initializes a new instance of the <see cref="EventLog"/> class.</summary>
        /// <param name="eventType">The type of event.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="ex">The exception.</param>
        public EventLog(string eventType, EventLogSeverity severity, Exception ex)
        {
            this.EventType = eventType;
            this.Severity = severity;
            this.Message = ex.Message;
            this.StackTrace = ex.StackTrace;
        }

        /// <summary>Gets or sets the ID.</summary>
        [ColumnName("EventLogID")]
        public int EventLogId { get; set; }

        /// <summary>Gets or sets the date/time.</summary>
        public DateTime Date { get; set; }

        /// <summary>Gets or sets the event type.</summary>
        public string EventType
        {
            get
            {
                if (!string.IsNullOrEmpty(this.eventType))
                {
                    return this.eventType.ToUpperInvariant();
                }

                return this.eventType;
            }

            set
            {
                this.eventType = value.ToUpperInvariant();
            }
        }

        /// <summary>Gets or sets the severity.</summary>
        public EventLogSeverity Severity { get; set; }

        /// <summary>Gets or sets the message.</summary>
        public string Message { get; set; }

        /// <summary>Gets or sets the exception stack trace, if any.</summary>
        public string StackTrace { get; set; }
    }
}
