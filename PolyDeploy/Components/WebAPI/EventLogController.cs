using Cantarus.Modules.PolyDeploy.Components.DataAccess.Models;
using Cantarus.Modules.PolyDeploy.Components.Logging;
using Cantarus.Modules.PolyDeploy.Components.WebAPI.ActionFilters;
using DotNetNuke.Web.Api;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

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

            List<EventLog> eventLogs = EventLogManager.Browse(pageIndex, pageSize, eventType, actualSeverity).ToList();

            return Request.CreateResponse(HttpStatusCode.OK, eventLogs);
        }

        [HttpGet]
        public HttpResponseMessage EventTypes()
        {
            List<string> eventTypes = EventLogManager.GetEventTypes().ToList();

            return Request.CreateResponse(HttpStatusCode.OK, eventTypes);
        }
    }
}
