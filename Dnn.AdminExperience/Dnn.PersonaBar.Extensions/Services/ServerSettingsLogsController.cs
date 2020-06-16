// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Services
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.Servers.Components.Log;
    using DotNetNuke.Common;
    using DotNetNuke.Data;
    using DotNetNuke.Instrumentation;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class ServerSettingsLogsController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServerSettingsLogsController));
        private readonly LogController _logController = new LogController();

        [HttpGet]
        public HttpResponseMessage GetLogs()
        {
            try
            {
                var response = new
                {
                    Success = true,
                    Results = new
                    {
                        LogList = this._logController.GetLogFilesList(),
                        UpgradeLogList = this._logController.GetUpgradeLogList()
                    },
                    TotalResults = 1
                };
                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetLogFile(string fileName)
        {
            try
            {
                var logFilePath = Path.Combine(Globals.ApplicationMapPath, @"portals\_default\logs", fileName);
                return CreateLogFileResponse(logFilePath);
            }
            catch (ArgumentException exc)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc.Message);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetUpgradeLogFile(string logName)
        {
            try
            {
                var providerPath = DataProvider.Instance().GetProviderPath();
                var logFilePath = Path.Combine(providerPath, logName);
                return CreateLogFileResponse(logFilePath);
            }
            catch (ArgumentException exc)
            {
                return this.Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc.Message);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [NonAction]
        private static void ValidateFilePath(string physicalPath)
        {
            var fileInfo = new FileInfo(physicalPath);
            if (!fileInfo.DirectoryName.StartsWith(Globals.ApplicationMapPath, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException("Invalid File Path");
            }
        }

        [NonAction]
        private HttpResponseMessage CreateLogFileResponse(string logFilePath)
        {
            ValidateFilePath(logFilePath);
            if (!File.Exists(logFilePath))
            {
                return Request.CreateResponse(HttpStatusCode.NotFound);

            }

            using (var reader = File.OpenText(logFilePath))
            {
                var logText = reader.ReadToEnd();
                return Request.CreateResponse(HttpStatusCode.OK, logText);
            }
        }
    }
}
