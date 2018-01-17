using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using Cantarus.Modules.PolyDeploy.Components.Logging;
using Cantarus.Modules.PolyDeploy.Components.WebAPI.ActionFilters;
using DotNetNuke.Web.Api;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Web.Http;
using System.Web.Script.Serialization;

namespace Cantarus.Modules.PolyDeploy.Components.WebAPI
{
    //[RequireHost]
    //[ValidateAntiForgeryToken]
    //[InWhitelist]
    [AllowAnonymous]
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
            List<EventLog> eventLogs = EventLogManager.Browse(pageIndex, pageSize, eventType, actualSeverity).ToList();

            // Work out pagination details.
            int rowCount = EventLogManager.EventCount();
            int pageCount = (int)Math.Ceiling((double)(rowCount / pageSize));

            // Start building meta.
            Dictionary<string, dynamic> pagination = new Dictionary<string, dynamic>();

            // Add basics.
            pagination.Add("Records", rowCount);
            pagination.Add("Pages", pageCount);
            pagination.Add("CurrentPage", pageIndex);

            // Build navigation.
            Dictionary<string, string> navigation = new Dictionary<string, string>();

            // Parameters passed in not changed by pagination.
            string fixedParams = "";

            // Page size.
            if (pageSize != 30)
            {
                fixedParams += string.Format("pageSize={0}", pageSize);
            }

            // Event type.
            if (eventType != null)
            {
                fixedParams += string.Format("eventType={0}", eventType);
            }

            // Severity.
            if (severity != -1)
            {
                fixedParams += string.Format("eventType={0}", severity);
            }

            // Is there a next page?
            if (pageIndex < pageCount)
            {
                string nextLink = string.Format("Browse?pageIndex={0}", pageIndex + 1);

                if (!string.IsNullOrEmpty(fixedParams))
                {
                    nextLink = string.Format("{0}&{1}", nextLink, fixedParams);
                }

                navigation.Add("Next", nextLink);
            }
            
            // Is there a previous page?
            if (pageIndex > 0)
            {
                string prevLink = string.Format("Browse?pageIndex={0}", pageIndex - 1);

                if (!string.IsNullOrEmpty(fixedParams))
                {
                    prevLink = string.Format("{0}&{1}", prevLink, fixedParams);
                }

                navigation.Add("Previous", prevLink);
            }

            // Add navigation.
            pagination.Add("Navigation", navigation);

            Dictionary<string, dynamic> payload = new Dictionary<string, dynamic>();

            payload.Add("Data", eventLogs);
            payload.Add("Pagination", pagination);

            // Serialise.
            JavaScriptSerializer js = new JavaScriptSerializer();

            string json = js.Serialize(payload);

            HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

            response.Content = new StringContent(json, Encoding.UTF8, "application/json");

            return response;
        }

        [HttpGet]
        public HttpResponseMessage Count()
        {
            return Request.CreateResponse(HttpStatusCode.OK, EventLogManager.EventCount());
        }

        [HttpGet]
        public HttpResponseMessage EventTypes()
        {
            List<string> eventTypes = EventLogManager.GetEventTypes().ToList();

            return Request.CreateResponse(HttpStatusCode.OK, eventTypes);
        }
    }
}
