// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Services
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Text.RegularExpressions;
    using System.Web.Http;

    using Dnn.PersonaBar.Extensions.Services.Dto;
    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class UpgradesController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UpgradesController));
        private readonly IApplicationStatusInfo applicationStatusInfo;

        public UpgradesController(IApplicationStatusInfo applicationStatusInfo)
        {
            this.applicationStatusInfo = applicationStatusInfo;
        }

        [HttpGet]
        public HttpResponseMessage List()
        {
            try
            {
                var res = new List<UpgradeFileDto>();

                var upgradePath = Path.Combine(this.applicationStatusInfo.ApplicationMapPath, "App_Data", "Upgrade");
                if (!Directory.Exists(upgradePath))
                {
                    return this.Request.CreateResponse(HttpStatusCode.OK, res);
                }

                foreach (var file in Directory.GetFiles(upgradePath))
                {
                    var fileName = Path.GetFileName(file);
                    var m = Regex.Match(fileName, @"(\d+\.\d+\.\d+)_(.+)\.zip$", RegexOptions.IgnoreCase);

                    if (m.Success)
                    {
                        var version = new Version(m.Groups[1].Value);
                        var fileDto = new UpgradeFileDto()
                        {
                            FileName = fileName,
                            Version = version,
                            IsObsolete = version <= this.applicationStatusInfo.DatabaseVersion,
                        };
                        res.Add(fileDto);
                    }
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, res);
            }
            catch (Exception ex)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }
    }
}
