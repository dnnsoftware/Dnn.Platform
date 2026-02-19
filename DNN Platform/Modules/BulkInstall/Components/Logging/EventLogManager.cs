// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.Logging
{
    using System;
    using System.Collections.Generic;

    using Dnn.Modules.BulkInstall.Components.DataAccess.DataControllers;
    using Dnn.Modules.BulkInstall.Components.DataAccess.Models;

    /// <summary>A manager for <see cref="EventLog"/>.</summary>
    /// <param name="dataController">The data controller.</param>
    public sealed class EventLogManager(EventLogDataController dataController)
    {
        private readonly EventLogDataController dataController = dataController;

        /// <summary>Gets a page of <see cref="EventLog"/> entries.</summary>
        /// <param name="pageIndex">The 0-based page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="eventType">The event type to filter by, or <see langword="null"/>.</param>
        /// <param name="severity">The severity to filter by, or <see langword="null"/>.</param>
        /// <returns>A sequence of <see cref="EventLog"/>.</returns>
        public IEnumerable<EventLog> Browse(int pageIndex, int pageSize, string eventType, EventLogSeverity? severity)
        {
            return this.dataController.Browse(pageIndex, pageSize, eventType, severity);
        }

        /// <summary>Gets the total count of event logs for the given filters.</summary>
        /// <param name="pageIndex">Page index is not used.</param>
        /// <param name="pageSize">Page size is not used.</param>
        /// <param name="eventType">The event type or <see langword="null"/>.</param>
        /// <param name="severity">The severity or <see langword="null"/>.</param>
        /// <returns>The total count.</returns>
        public int BrowseCount(int pageIndex, int pageSize, string eventType, EventLogSeverity? severity)
        {
            return this.dataController.BrowseCount(pageIndex, pageSize, eventType, severity);
        }

        /// <summary>Gets all the event types.</summary>
        /// <returns>A sequence of <see cref="string"/> values.</returns>
        public IEnumerable<string> GetEventTypes()
        {
            return this.dataController.GetEventTypes();
        }

        /// <summary>Gets the total count of <see cref="EventLog"/> rows.</summary>
        /// <returns>The count.</returns>
        public int EventCount()
        {
            return this.dataController.EventCount();
        }

        /// <summary>Creates a new <see cref="EventLog"/>.</summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="message">The message.</param>
        public void Log(string eventType, EventLogSeverity severity, string message)
            => this.Log(eventType, severity, message, null);

        /// <summary>Creates a new <see cref="EventLog"/>.</summary>
        /// <param name="eventType">The event type.</param>
        /// <param name="severity">The severity.</param>
        /// <param name="ex">An exception.</param>
        public void Log(string eventType, EventLogSeverity severity, Exception ex)
            => this.Log(eventType, severity, null, ex);

        private void Log(string eventType, EventLogSeverity severity, string message = null, Exception ex = null)
        {
            // TODO: Internal logging switched on?
            this.LogInternal(eventType, severity, message, ex);

            // TODO: DNN logging switched on?
            // Log to DNN event log.
        }

        private void LogInternal(string eventType, EventLogSeverity severity, string message, Exception ex)
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

            this.dataController.Create(eventLog);
        }
    }
}
