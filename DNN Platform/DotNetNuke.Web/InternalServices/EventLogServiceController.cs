// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Web;
    using System.Web.Http;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Web.Api;

    [DnnAuthorize]
    public class EventLogServiceController : DnnApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(EventLogServiceController));

        [HttpGet]
        [DnnAuthorize(StaticRoles = "Administrators")]
        public HttpResponseMessage GetLogDetails(string guid)
        {
            Guid logId;
            if (string.IsNullOrEmpty(guid) || !Guid.TryParse(guid, out logId))
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            try
            {
                var logInfo = new LogInfo { LogGUID = guid };
                logInfo = EventLogController.Instance.GetSingleLog(logInfo, LoggingProvider.ReturnType.LogInfoObjects) as LogInfo;
                if (logInfo == null)
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest);
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    Title = Localization.GetSafeJSString("CriticalError.Error", Localization.SharedResourceFile),
                    Content = this.GetPropertiesText(logInfo),
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateResponse(HttpStatusCode.BadRequest);
            }
        }

        private string GetPropertiesText(LogInfo logInfo)
        {
            var objLogProperties = logInfo.LogProperties;
            var str = new StringBuilder();
            int i;
            for (i = 0; i <= objLogProperties.Count - 1; i++)
            {
                // display the values in the Panel child controls.
                var ldi = (LogDetailInfo)objLogProperties[i];
                if (ldi.PropertyName == "Message")
                {
                    str.Append("<p><strong>" + ldi.PropertyName + "</strong>:</br><pre>" + HttpUtility.HtmlEncode(ldi.PropertyValue) + "</pre></p>");
                }
                else
                {
                    str.Append("<p><strong>" + ldi.PropertyName + "</strong>:" + HttpUtility.HtmlEncode(ldi.PropertyValue) + "</p>");
                }
            }

            str.Append("<p><b>Server Name</b>: " + HttpUtility.HtmlEncode(logInfo.LogServerName) + "</p>");
            return str.ToString();
        }
    }
}
