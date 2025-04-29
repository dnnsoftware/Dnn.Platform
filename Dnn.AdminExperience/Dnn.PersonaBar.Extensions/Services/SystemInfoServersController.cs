// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Servers.Services;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

using Dnn.PersonaBar.Extensions.Services.Dto;
using Dnn.PersonaBar.Library;
using Dnn.PersonaBar.Library.Attributes;
using DotNetNuke.Instrumentation;

[MenuPermission(Scope = ServiceScope.Host)]
public class SystemInfoServersController : PersonaBarApiController
{
    private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(SystemInfoServersController));

    [HttpGet]

    public HttpResponseMessage GetServers()
    {
        try
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, this.GetServerList());
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
        }
    }

    [HttpPost]
    public HttpResponseMessage DeleteServer(DeleteServerDTO data)
    {
        try
        {
            DotNetNuke.Entities.Host.ServerController.DeleteServer(data.ServerId);

            return this.Request.CreateResponse(HttpStatusCode.OK, true);
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
        }
    }

    [HttpPost]

    public HttpResponseMessage EditServerUrl(EditServerUrlDTO data)
    {
        try
        {
            var server = DotNetNuke.Entities.Host.ServerController.GetServers().FirstOrDefault(s => s.ServerID == data.ServerId);
            server.Url = data.NewUrl;
            DotNetNuke.Entities.Host.ServerController.UpdateServer(server);

            return this.Request.CreateResponse(HttpStatusCode.OK, true);
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
        }
    }

    [HttpPost]

    public HttpResponseMessage DeleteNonActiveServers()
    {
        try
        {
            foreach (var s in DotNetNuke.Entities.Host.ServerController.GetInActiveServers(1440))
            {
                DotNetNuke.Entities.Host.ServerController.DeleteServer(s.ServerID);
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, true);
        }
        catch (Exception exc)
        {
            Logger.Error(exc);
            return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
        }
    }

    private IEnumerable<WebServer> GetServerList()
    {
        return DotNetNuke.Entities.Host.ServerController.GetServers().Select(s => new WebServer()
        {
            ServerId = s.ServerID,
            ServerGroup = s.ServerGroup,
            ServerName = s.ServerName,
            Url = s.Url,
            LastActivityDate = s.LastActivityDate,
            IsActive = s.LastActivityDate.AddDays(1) >= DateTime.Now,
            IsCurrent = s.ServerName == Environment.MachineName,
        });
    }
}
