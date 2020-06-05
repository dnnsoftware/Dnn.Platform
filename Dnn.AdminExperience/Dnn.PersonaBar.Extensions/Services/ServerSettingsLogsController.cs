﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

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

namespace Dnn.PersonaBar.Servers.Services
{
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

                this.ValidateFilePath(logFilePath);

                using (var reader = File.OpenText(logFilePath))
                {
                    var logText = reader.ReadToEnd();
                    return this.Request.CreateResponse(HttpStatusCode.OK, logText);
                }
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
                var upgradeText = string.Empty;
                var providerPath = DataProvider.Instance().GetProviderPath();
                var logFilePath = Path.Combine(providerPath, logName + ".log.resources");

                this.ValidateFilePath(logFilePath);

                if (File.Exists(logFilePath))
                {
                    using (var reader = File.OpenText(logFilePath))
                    {
                        upgradeText = reader.ReadToEnd();
                        upgradeText = upgradeText.Replace("\n", "<br>");
                        reader.Close();
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, upgradeText);
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

        private void ValidateFilePath(string physicalPath)
        {
            var fileInfo = new FileInfo(physicalPath);
            if (!fileInfo.DirectoryName.StartsWith(Globals.ApplicationMapPath, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new ArgumentException("Invalid File Path");
            }
        }
    }
}
