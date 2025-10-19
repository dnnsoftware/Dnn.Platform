// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Services
{
    using System;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Dnn.PersonaBar.Extensions.Common;
    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class UpgradesController : PersonaBarApiController
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UpgradesController));
        private readonly IApplicationStatusInfo applicationStatusInfo;
        private readonly ILocalUpgradeService localUpgradeService;

        public UpgradesController(IApplicationStatusInfo applicationStatusInfo, ILocalUpgradeService localUpgradeService)
        {
            this.applicationStatusInfo = applicationStatusInfo;
            this.localUpgradeService = localUpgradeService;
        }

        [HttpGet]
        public async Task<HttpResponseMessage> List(CancellationToken cancellationToken)
        {
            try
            {
                var upgrades = await this.localUpgradeService.GetLocalUpgrades(cancellationToken);

                return this.Request.CreateResponse(HttpStatusCode.OK, upgrades);
            }
            catch (Exception ex)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }

        /// <summary>Starts a local upgrade.</summary>
        /// <param name="version">The version to upgrade to.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An empty success response.</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> StartUpgrade(string version, CancellationToken cancellationToken)
        {
            var upgrades = await this.localUpgradeService.GetLocalUpgrades(cancellationToken);
            if (!upgrades.Any(u => u.IsValid && !u.IsOutdated))
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = "There are no upgrades to apply", });
            }

            var ver = new Version(version);
            var upgrade = upgrades.FirstOrDefault(u => u.Version.Equals(ver));
            if (upgrade == null || !upgrade.IsValid || upgrade.IsOutdated)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = $"There is no valid upgrade for version {version}", });
            }

            await this.localUpgradeService.StartLocalUpgrade(upgrade, cancellationToken);

            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }

        /// <summary>Starts a local upgrade.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An empty success response.</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> StartFirstUpgrade(CancellationToken cancellationToken)
        {
            var upgrades = await this.localUpgradeService.GetLocalUpgrades(cancellationToken);
            if (!upgrades.Any(u => u.IsValid && !u.IsOutdated))
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = "There are no upgrades to apply", });
            }

            await this.localUpgradeService.StartLocalUpgrade(upgrades, cancellationToken);

            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }
    }
}
