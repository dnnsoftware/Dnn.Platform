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
    using DotNetNuke.Abstractions;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Client.ClientResourceManagement;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class ServerController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ServerController));

        public ServerController(INavigationManager navigationManager)
        {
            this.NavigationManager = navigationManager;
        }

        internal static string LocalResourceFile => Path.Combine("~/DesktopModules/admin/Dnn.PersonaBar/Modules/Dnn.Servers/App_LocalResources/Servers.resx");
        protected INavigationManager NavigationManager { get; }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage RestartApplication()
        {
            try
            {
                var log = new LogInfo { BypassBuffering = true, LogTypeKey = EventLogController.EventLogType.HOST_ALERT.ToString() };
                log.AddProperty("Message", Localization.GetString("UserRestart", LocalResourceFile));
                LogController.Instance.AddLog(log);
                Config.Touch();
                return this.Request.CreateResponse(HttpStatusCode.OK, new { url = this.NavigationManager.NavigateURL() });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public HttpResponseMessage ClearCache()
        {
            try
            {
                DataCache.ClearCache();
                ClientResourceManager.ClearCache();
                return this.Request.CreateResponse(HttpStatusCode.OK, new { url = this.NavigationManager.NavigateURL() });
            }
            catch (Exception exc)
            {
                Logger.Error(exc);
                return this.Request.CreateErrorResponse(HttpStatusCode.InternalServerError, exc);
            }
        }
    }
}
