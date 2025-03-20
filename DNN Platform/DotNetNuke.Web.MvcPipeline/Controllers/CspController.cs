// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Controllers
{
    using System;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Log.EventLog;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;

    public class CspController : Controller, IMvcController
    {
        private readonly ILog logger = LoggerSource.Instance.GetLogger(typeof(CspController));
        private readonly IEventLogger eventLogger;

        public CspController(IEventLogger eventLogger)
        {
            this.eventLogger = eventLogger;
        }

        [HttpPost]
        public ActionResult Report()
        {
            var cspFileLogging = true;
            try
            {
                // Lire le rapport JSON du corps de la requête
                string jsonContent;
                using (var reader = new StreamReader(this.Request.InputStream))
                {
                    jsonContent = reader.ReadToEnd();
                }

                var cspReport = JsonConvert.DeserializeObject<CSPReport>(jsonContent);

                // Écrire le log
                var logEntry = "CSP Report : " +
                             $"Document URI: {cspReport?.CspReport?.DocumentUri}, " +
                             $"Violated Directive: {cspReport?.CspReport?.ViolatedDirective}, " +
                             $"Blocked URI: {cspReport?.CspReport?.BlockedUri}, " +
                             $"User Agent: {this.Request.UserAgent}";

                if (cspFileLogging)
                {
                    // Logger.Error("CspReport : " + JsonConvert.SerializeObject(request, Formatting.Indented));
                    this.logger.Error(logEntry);
                }

                ILogInfo log = new LogInfo { LogTypeKey = EventLogType.ADMIN_ALERT.ToString() };
                log.LogPortalId = PortalSettings.Current.PortalId;
                log.LogPortalName = PortalSettings.Current.PortalName;
                log.LogUserId = -1;
                log.LogProperties.Add(new LogDetailInfo("Content Security Policy", "Report"));
                log.LogProperties.Add(new LogDetailInfo("Document URI", cspReport?.CspReport?.DocumentUri));
                log.LogProperties.Add(new LogDetailInfo("Document URI", cspReport?.CspReport?.DocumentUri));
                log.LogProperties.Add(new LogDetailInfo("Violated Directive", cspReport?.CspReport?.ViolatedDirective));
                log.LogProperties.Add(new LogDetailInfo("Blocked URI", cspReport?.CspReport?.BlockedUri));
                log.LogProperties.Add(new LogDetailInfo("User Agent", this.Request.UserAgent));

                this.eventLogger.AddLog(log);

                /*
                if (cspEmailNotification)
                {
                    var cspEmail = Host.HostEmail;
                    if (this.Request != null)
                    {
                        // SendMail(Host.HostEmail, cspEmail, "", "", "", "Csp Report", JsonConvert.SerializeObject(request, Formatting.Indented));
                        SendMail(Host.HostEmail, cspEmail, string.Empty, string.Empty, string.Empty, "Csp Report", logEntry);
                    }
                    else
                    {
                        SendMail(Host.HostEmail, cspEmail, string.Empty, string.Empty, string.Empty, "Csp Report", "no report");
                    }
                }
                */
                return new HttpStatusCodeResult(201, "Report recorded");
            }
            catch (Exception ex)
            {
                this.logger.Error("CSP Report : " + ex.Message, ex);
                return new HttpStatusCodeResult(500, "Error processing report");
            }
        }
    }
}
