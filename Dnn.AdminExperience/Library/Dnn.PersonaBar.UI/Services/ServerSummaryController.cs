// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.UI.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using Dnn.PersonaBar.UI.Services.DTO;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Users;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.Services.Installer;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>A Persona Bar API controller for the server summary.</summary>
    [MenuPermission(Scope = ServiceScope.Regular)]
    public class ServerSummaryController : PersonaBarApiController
    {
        private readonly IHostSettings hostSettings;
        private readonly IApplicationInfo application;
        private readonly ILocalUpgradeService localUpgradeService;

        /// <summary>Initializes a new instance of the <see cref="ServerSummaryController"/> class.</summary>
        [Obsolete("Deprecated in DotNetNuke 10.0.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public ServerSummaryController()
            : this(null, null, null)
        {
        }

        /// <summary>Initializes a new instance of the <see cref="ServerSummaryController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="application">The application.</param>
        /// <param name="localUpgradeService">The local upgrade service.</param>
        public ServerSummaryController(IHostSettings hostSettings, IApplicationInfo application, ILocalUpgradeService localUpgradeService)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
            this.application = application ?? Globals.GetCurrentServiceProvider().GetRequiredService<IApplicationInfo>();
            this.localUpgradeService = localUpgradeService ?? Globals.GetCurrentServiceProvider().GetRequiredService<ILocalUpgradeService>();
        }

        /// <summary>Return server info.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A response with server info.</returns>
        [HttpGet]
        public async Task<HttpResponseMessage> GetServerInfo(CancellationToken cancellationToken)
        {
            try
            {
                var isHost = UserController.Instance.GetCurrentUserInfo().IsSuperUser;
                var response = new
                {
                    ProductName = this.application.Description,
                    ProductVersion = "v. " + Globals.FormatVersion(this.application.Version, true),
                    FrameworkVersion = isHost ? Globals.FormattedNetFrameworkVersion : string.Empty,
                    ServerName = isHost ? Globals.ServerName : string.Empty,
                    LicenseVisible = isHost && this.GetVisibleSetting("LicenseVisible"),
                    DocCenterVisible = this.GetVisibleSetting("DocCenterVisible"),
                    Update = await this.UpdateInfo(cancellationToken),
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
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A serialized <see cref="FrameworkQueryDTO"/> object.</returns>
        [HttpGet]
        public async Task<HttpResponseMessage> GetUpdateInfo(CancellationToken cancellationToken)
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, await this.UpdateInfo(cancellationToken));
        }

        /// <summary>Starts a local upgrade.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An empty success response.</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> StartLocalUpgrade(CancellationToken cancellationToken)
        {
            if (!this.UserInfo.IsSuperUser)
            {
                return this.Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var upgrades = await this.localUpgradeService.GetLocalUpgrades(cancellationToken);
            if (!upgrades.Any(u => u.IsValid && !u.IsOutdated))
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = "There are no upgrades to apply", });
            }

            await this.localUpgradeService.StartLocalUpgrade(upgrades, cancellationToken);

            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }

        private bool GetVisibleSetting(string settingName)
        {
            var portalSettings = PortalController.Instance.GetPortalSettings(this.PortalId);
            return !portalSettings.ContainsKey(settingName)
                   || string.IsNullOrEmpty(portalSettings[settingName])
                   || portalSettings[settingName] == "true";
        }

        private async Task<FrameworkQueryDTO> UpdateInfo(CancellationToken cancellationToken)
        {
            var updateInfo = new FrameworkQueryDTO();
            if (HttpContextSource.Current == null || !this.UserInfo.IsSuperUser)
            {
                return updateInfo;
            }

            var localUpgrades = await this.localUpgradeService.GetLocalUpgrades(cancellationToken);
            updateInfo.LocalUpgrades = localUpgrades.Select(info => LocalUpgradeDto.FromLocalUpgradeInfo(info)).ToList();

            if (!this.hostSettings.CheckUpgrade)
            {
                return updateInfo;
            }

            return CBO.GetCachedObject<FrameworkQueryDTO>(
                this.hostSettings,
                new CacheItemArgs("DnnUpdateUrl") { ParamList = { updateInfo, }, },
                this.RetrieveUpdateUrl);
        }

        private FrameworkQueryDTO RetrieveUpdateUrl(CacheItemArgs args)
        {
            try
            {
                var coreVersion = Globals.FormatVersion(this.application.Version, "00", 3, string.Empty);
                var portalCount = PortalController.Instance.GetPortals().Count;
                var osVersion = Globals.FormatVersion(Globals.OperatingSystemVersion, "00", 2, string.Empty);
                var netVersion = Globals.FormatVersion(Globals.NETFrameworkVersion, "00", 2, string.Empty);
                var dbVersion = Globals.FormatVersion(Globals.DatabaseEngineVersion, "00", 2, string.Empty);
                var url = $"{this.application.UpgradeUrl}/Update/FrameworkStatus?core={coreVersion}&type={this.application.Type}&name={this.application.Name}&id={this.hostSettings.Guid}&no={portalCount}&os={osVersion}&net={netVersion}&db={dbVersion}";
                var response = this.GetJsonObject((FrameworkQueryDTO)args.ParamList[0], url);
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

        private T GetJsonObject<T>(T initial, string url)
        {
            var request = Globals.GetExternalRequest(this.hostSettings, url);
            using var response = (HttpWebResponse)request.GetResponse();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                return initial;
            }

            using var dataStream = response.GetResponseStream();
            using var reader = new StreamReader(dataStream);
            var responseFromServer = reader.ReadToEnd();
            Newtonsoft.Json.JsonConvert.PopulateObject(responseFromServer, initial);
            return initial;
        }
    }
}
