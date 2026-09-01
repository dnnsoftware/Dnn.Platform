// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Components.BulkInstall.DataAccess.DataControllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Dnn.PersonaBar.Extensions.Components.BulkInstall.DataAccess.Models;
    using Dnn.PersonaBar.Extensions.Components.BulkInstall.Logging;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Data;

    /// <summary>The data controller for <see cref="EventLog"/>.</summary>
    /// <param name="hostSettings">The host settings.</param>
    public sealed class EventLogDataController(IHostSettings hostSettings)
    {
        private readonly IHostSettings hostSettings = hostSettings;

        /// <summary>Creates an <see cref="EventLog"/>.</summary>
        /// <param name="eventLog">The event log to create.</param>
        public void Create(EventLog eventLog)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<EventLog>();

            eventLog.Date = DateTime.Now;

            repo.Insert(eventLog);
        }

        /// <summary>Gets all <see cref="EventLog"/> instances.</summary>
        /// <returns>A sequence of <see cref="EventLog"/>.</returns>
        public IEnumerable<EventLog> Get()
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            var repo = context.GetRepository<EventLog>();

            return repo.Get();
        }

        /// <summary>Gets a page of <see cref="EventLog"/> entries.</summary>
        /// <param name="pageIndex">The 0-based page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="eventType">The event type to filter by, or <see langword="null"/>.</param>
        /// <param name="severity">The severity to filter by, or <see langword="null"/>.</param>
        /// <returns>A sequence of <see cref="EventLog"/>.</returns>
        public IEnumerable<EventLog> Browse(int pageIndex, int pageSize, string eventType, EventLogSeverity? severity)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            return context.ExecuteQuery<EventLog>(
                System.Data.CommandType.StoredProcedure,
                "{databaseOwner}[{objectQualifier}BulkInstall_GetEventLogsPage]",
                pageIndex,
                pageSize,
                eventType,
                severity);
        }

        /// <summary>Gets the total count of event logs for the given filters.</summary>
        /// <param name="pageIndex">Page index is not used.</param>
        /// <param name="pageSize">Page size is not used.</param>
        /// <param name="eventType">The event type or <see langword="null"/>.</param>
        /// <param name="severity">The severity or <see langword="null"/>.</param>
        /// <returns>The count.</returns>
        public int BrowseCount(int pageIndex, int pageSize, string eventType, EventLogSeverity? severity)
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            return context.ExecuteQuery<int>(
                System.Data.CommandType.StoredProcedure,
                "{databaseOwner}[{objectQualifier}BulkInstall_GetEventLogsPageTotal]",
                pageIndex,
                pageSize,
                eventType,
                severity)
            .FirstOrDefault();
        }

        /// <summary>Gets all the event types.</summary>
        /// <returns>A sequence of <see cref="string"/> values.</returns>
        public IEnumerable<string> GetEventTypes()
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            return context.ExecuteQuery<string>(System.Data.CommandType.Text, "SELECT DISTINCT [EventType] FROM [dbo].[BulkInstall_EventLogs]", null);
        }

        /// <summary>Gets the total count of <see cref="EventLog"/> rows.</summary>
        /// <returns>The count.</returns>
        public int EventCount()
        {
            using IDataContext context = DataContext.Instance(this.hostSettings);
            return context.ExecuteQuery<int>(System.Data.CommandType.Text, "SELECT COUNT(*) FROM [dbo].[BulkInstall_EventLogs]", null).FirstOrDefault();
        }
    }
}
