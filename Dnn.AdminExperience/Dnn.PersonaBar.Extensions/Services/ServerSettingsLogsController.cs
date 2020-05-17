﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
                        LogList = _logController.GetLogFilesList(),
                        UpgradeLogList = _logController.GetUpgradeLogList()
                    },
                    TotalResults = 1
                };
                return Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpGet]
        public HttpResponseMessage GetLogFile(string fileName)
        {
            try
            {
                var logFilePath = Path.Combine(Globals.ApplicationMapPath, @"portals\_default\logs", fileName);

                ValidateFilePath(logFilePath);

                using (var reader = File.OpenText(logFilePath))
                {
                    var logText = reader.ReadToEnd();
                    return Request.CreateResponse(HttpStatusCode.OK, logText);
                }
            }
            catch (ArgumentException exc)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc.Message);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
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

                ValidateFilePath(logFilePath);

                if (File.Exists(logFilePath))
                {
                    using (var reader = File.OpenText(logFilePath))
                    {
                        upgradeText = reader.ReadToEnd();
                        upgradeText = upgradeText.Replace("\n", "<br>");
                        reader.Close();
                    }
                }

                return Request.CreateResponse(HttpStatusCode.OK, upgradeText);
            }
            catch (ArgumentException exc)
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, exc.Message);
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
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
