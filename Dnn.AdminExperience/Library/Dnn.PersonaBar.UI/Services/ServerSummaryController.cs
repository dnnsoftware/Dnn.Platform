// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Services
{
    using System;
    using System.IO;
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.UI.Services.DTO;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Exceptions;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A Persona Bar API controller for the server summary.</summary>
    [MenuPermission(Scope = ServiceScope.Regular)]
    public class ServerSummaryController : PersonaBarApiController
    {
        private readonly IHostSettings hostSettings;

        /// <summary>Initializes a new instance of the <see cref="ServerSummaryController"/> class.</summary>
        public ServerSummaryController()
            : this(null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ServerSummaryController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        public ServerSummaryController(IHostSettings hostSettings)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
        }

        /// <summary>Return server info.</summary>
        /// <returns>A response with server info.</returns>
        [HttpGet]
        public HttpResponseMessage GetServerInfo()
        {
            try
            {
                var isHost = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
                var response = new
                {
                    ProductName = DotNetNukeContext.Current.Application.Description,
                    ProductVersion = "v. " + Globals.FormatVersion(DotNetNukeContext.Current.Application.Version, true),
                    FrameworkVersion = isHost ? Globals.FormattedNetFrameworkVersion : string.Empty,
                    ServerName = isHost ? Globals.ServerName : string.Empty,
                    LicenseVisible = isHost && this.GetVisibleSetting("LicenseVisible"),
                    DocCenterVisible = this.GetVisibleSetting("DocCenterVisible"),
                    Update = this.UpdateInfo(),
                };

                return this.Request.CreateResponse(HttpStatusCode.OK, response);
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
                return this.Request.CreateErrorResponse(HttpStatusCode.NotFound, ex.Message);
            }
        }

        /// <summary>Returns update information about current framework version.</summary>
        /// <returns>A serialized FrameworkQueryDTO object.</returns>
        [HttpGet]
        public HttpResponseMessage GetUpdateInfo()
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, this.UpdateInfo());
        }

        private bool GetVisibleSetting(string settingName)
        {
            var portalSettings = PortalController.Instance.GetPortalSettings(this.PortalId);
            return !portalSettings.ContainsKey(settingName)
                   || string.IsNullOrEmpty(portalSettings[settingName])
                   || portalSettings[settingName] == "true";
        }

        private FrameworkQueryDTO UpdateInfo()
        {
            if (HttpContextSource.Current == null || !this.hostSettings.CheckUpgrade || !this.UserInfo.IsSuperUser)
            {
                return new FrameworkQueryDTO();
            }

            return CBO.GetCachedObject<FrameworkQueryDTO>(this.hostSettings, new CacheItemArgs("DnnUpdateUrl"), this.RetrieveUpdateUrl);
        }

        private FrameworkQueryDTO RetrieveUpdateUrl(CacheItemArgs args)
        {
            try
            {
                var url = $"{DotNetNukeContext.Current.Application.UpgradeUrl}/Update/FrameworkStatus";
                url += "?core=" + Globals.FormatVersion(DotNetNukeContext.Current.Application.Version, "00", 3, string.Empty);
                url += "&type=" + DotNetNukeContext.Current.Application.Type;
                url += "&name=" + DotNetNukeContext.Current.Application.Name;
                url += "&id=" + this.hostSettings.Guid;
                url += "&no=" + PortalController.Instance.GetPortals().Count;
                url += "&os=" + Globals.FormatVersion(Globals.OperatingSystemVersion, "00", 2, string.Empty);
                url += "&net=" + Globals.FormatVersion(Globals.NETFrameworkVersion, "00", 2, string.Empty);
                url += "&db=" + Globals.FormatVersion(Globals.DatabaseEngineVersion, "00", 2, string.Empty);
                var response = this.GetJsonObject<FrameworkQueryDTO>(url);
                if (response.Version.Length == 6)
                {
                    response.Version = $"v. {response.Version.Substring(0, 2)}.{response.Version.Substring(2, 2)}.{response.Version.Substring(4, 2)}";
                }

                return response;
            }
            catch (Exception ex)
            {
                Exceptions.LogException(ex);
            }

            return new FrameworkQueryDTO();
        }

        private T GetJsonObject<T>(string url)
        {
            var request = Globals.GetExternalRequest(this.hostSettings, url);
            using (var response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    var dataStream = response.GetResponseStream();
                    var reader = new StreamReader(dataStream);
                    var responseFromServer = reader.ReadToEnd();
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(responseFromServer);
                }
            }

            return default(T);
        }
    }
}
