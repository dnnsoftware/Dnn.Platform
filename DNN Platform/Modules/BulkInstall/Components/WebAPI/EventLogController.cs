// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.Modules.BulkInstall.Components.WebAPI
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.Modules.BulkInstall.Components.DataAccess.Models;
    using Dnn.Modules.BulkInstall.Components.Logging;
    using Dnn.Modules.BulkInstall.Components.WebAPI.ActionFilters;
    using DotNetNuke.Web.Api;

    /// <summary>A web API controller for <see cref="EventLog"/>.</summary>
    /// <param name="eventLogManager">The event log manager.</param>
    [RequireHost]
    [ValidateAntiForgeryToken]
    [InWhitelist]
    public class EventLogController(EventLogManager eventLogManager) : DnnApiController
    {
        private readonly EventLogManager eventLogManager = eventLogManager;

        /// <summary>Gets a page of <see cref="EventLog"/> instances.</summary>
        /// <param name="pageIndex">The 0-based page index.</param>
        /// <param name="pageSize">The page size.</param>
        /// <param name="eventType">An event type to filter by, or <see langword="null"/>.</param>
        /// <param name="severity">A <see cref="EventLogSeverity"/> to filter by, or <c>-1</c>.</param>
        /// <returns>A response with a list of <see cref="EventLog"/> and pagination data.</returns>
        [HttpGet]
        public HttpResponseMessage Browse(int pageIndex = 0, int pageSize = 30, string eventType = null, int severity = -1)
        {
            EventLogSeverity? actualSeverity = null;

            // Is there a severity set?
            if (severity >= 0)
            {
                actualSeverity = (EventLogSeverity)severity;
            }

            // Get event logs.
            IEnumerable<EventLog> eventLogs = this.eventLogManager.Browse(pageIndex, pageSize, eventType, actualSeverity);

            // Work out pagination details.
            int rowCount = this.eventLogManager.BrowseCount(pageIndex, pageSize, eventType, actualSeverity);
            int pageCount = (int)Math.Ceiling(rowCount / (double)pageSize);

            // Build navigation.
            Dictionary<string, string> navigation = new Dictionary<string, string>();

            // Parameters passed in not changed by pagination.
            string fixedParams = string.Empty;

            // Page size.
            if (pageSize != 30)
            {
                fixedParams += $"pageSize={pageSize}";
            }

            // Event type.
            if (eventType != null)
            {
                fixedParams += $"eventType={eventType}";
            }

            // Severity.
            if (severity != -1)
            {
                fixedParams += $"eventType={severity}";
            }

            // Is there a next page?
            if (pageIndex < pageCount)
            {
                string nextLink = $"Browse?pageIndex={pageIndex + 1}";

                if (!string.IsNullOrEmpty(fixedParams))
                {
                    nextLink = $"{nextLink}&{fixedParams}";
                }

                navigation.Add("Next", nextLink);
            }

            // Is there a previous page?
            if (pageIndex > 0)
            {
                string prevLink = $"Browse?pageIndex={pageIndex - 1}";

                if (!string.IsNullOrEmpty(fixedParams))
                {
                    prevLink = $"{prevLink}&{fixedParams}";
                }

                navigation.Add("Previous", prevLink);
            }

            var pagination = new { Records = rowCount, Pages = pageCount, CurrentPage = pageIndex, Navigation = navigation, };
            return this.Request.CreateResponse(HttpStatusCode.OK, new { Data = eventLogs, Pagination = pagination, });
        }

        /// <summary>Gets the total count of events.</summary>
        /// <returns>A response with the total count.</returns>
        [HttpGet]
        public HttpResponseMessage Count()
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, this.eventLogManager.EventCount());
        }

        /// <summary>Gets the event types.</summary>
        /// <returns>A response with a list of <see cref="string"/> values.</returns>
        [HttpGet]
        public HttpResponseMessage EventTypes()
        {
            List<string> eventTypes = this.eventLogManager.GetEventTypes().ToList();

            return this.Request.CreateResponse(HttpStatusCode.OK, eventTypes);
        }
    }
}
