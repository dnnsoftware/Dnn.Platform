#region Copyright
// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2018
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

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