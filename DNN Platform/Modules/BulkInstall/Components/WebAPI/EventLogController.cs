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

    [RequireHost]
    [ValidateAntiForgeryToken]
    [InWhitelist]
    public class EventLogController : DnnApiController
    {
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
            IEnumerable<EventLog> eventLogs = EventLogManager.Browse(pageIndex, pageSize, eventType, actualSeverity);

            // Work out pagination details.
            int rowCount = EventLogManager.BrowseCount(pageIndex, pageSize, eventType, actualSeverity);
            int pageCount = (int)Math.Ceiling(rowCount / (double)pageSize);

            // Build navigation.
            Dictionary<string, string> navigation = new Dictionary<string, string>();

            // Parameters passed in not changed by pagination.
            string fixedParams = "";

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

        [HttpGet]
        public HttpResponseMessage Count()
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, EventLogManager.EventCount());
        }

        [HttpGet]
        public HttpResponseMessage EventTypes()
        {
            List<string> eventTypes = EventLogManager.GetEventTypes().ToList();

            return this.Request.CreateResponse(HttpStatusCode.OK, eventTypes);
        }
    }
}
