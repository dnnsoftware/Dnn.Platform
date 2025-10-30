// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace Dnn.PersonaBar.Extensions.Services
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web.Http;

    using Dnn.PersonaBar.Extensions.Services.Dto;
    using Dnn.PersonaBar.Library;
    using Dnn.PersonaBar.Library.Attributes;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api.Internal;

    [MenuPermission(Scope = ServiceScope.Host)]
    public class UpgradesController : PersonaBarApiController
    {
        private const string ResourceFile = "~/DesktopModules/Admin/Dnn.PersonaBar/Modules/Dnn.Servers/App_LocalResources/Servers.resx";
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(UpgradesController));
        private readonly IApplicationStatusInfo applicationStatusInfo;
        private readonly ILocalUpgradeService localUpgradeService;

        public UpgradesController(IApplicationStatusInfo applicationStatusInfo, ILocalUpgradeService localUpgradeService)
        {
            this.applicationStatusInfo = applicationStatusInfo;
            this.localUpgradeService = localUpgradeService;
        }

        /// <summary>
        /// Retrieves upgrade settings.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="HttpResponseMessage"/> containing the upgrade settings with a status code
        /// of <see cref="HttpStatusCode.OK"/> if successful, or an error message with a status code of
        /// <see cref="HttpStatusCode.InternalServerError"/> if an exception occurs.</returns>
        [HttpGet]
        public HttpResponseMessage GetSettings(CancellationToken cancellationToken)
        {
            try
            {
                bool.TryParse(DotNetNuke.Common.Utilities.Config.GetSetting("AllowDnnUpgradeUpload"), out bool allowDnnUpgradeUpload);

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    AllowDnnUpgradeUpload = allowDnnUpgradeUpload,
                });
            }
            catch (Exception ex)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a list of local upgrades.
        /// </summary>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns>A <see cref="HttpResponseMessage"/> containing the list of local upgrades with a status code
        /// of <see cref="HttpStatusCode.OK"/> if successful, or an error message with a status code of
        /// <see cref="HttpStatusCode.InternalServerError"/> if an exception occurs.</returns>
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
        /// <param name="data">The DTO containing the packageName.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An empty success response.</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> StartUpgrade(UpgradePackageRequestDto data, CancellationToken cancellationToken)
        {
            var upgrades = await this.localUpgradeService.GetLocalUpgrades(cancellationToken);
            if (!upgrades.Any(u => u.IsValid && !u.IsOutdated))
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = this.LocalizeString("Upgrade_NoUpgrade"), });
            }

            var upgrade = upgrades.FirstOrDefault(u => u.PackageName.Equals(data.PackageName, StringComparison.InvariantCultureIgnoreCase));
            if (upgrade == null || !upgrade.IsValid || upgrade.IsOutdated)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = this.LocalizeString($"Upgrade_NoValidUpgrade", data.PackageName), });
            }

            await this.localUpgradeService.StartLocalUpgrade(upgrade, cancellationToken);

            await this.localUpgradeService.DeleteLocalUpgrade(data.PackageName, cancellationToken);

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
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = this.LocalizeString("Upgrade_NoUpgrade"), });
            }

            await this.localUpgradeService.StartLocalUpgrade(upgrades, cancellationToken);

            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }

        /// <summary>Deletes a local upgrade package.</summary>
        /// <param name="data">The DTO containing the packageName.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An empty success response.</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> Delete(UpgradePackageRequestDto data, CancellationToken cancellationToken)
        {
            try
            {
                await this.localUpgradeService.DeleteLocalUpgrade(data.PackageName, cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { message = ex.Message, });
            }

            return this.Request.CreateResponse(HttpStatusCode.NoContent);
        }

        /// <summary>
        /// Uploads a DNN package for local upgrade.
        /// </summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Either a package info or an error message.</returns>
        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        public Task<HttpResponseMessage> Upload(CancellationToken cancellationToken)
        {
            try
            {
                bool.TryParse(DotNetNuke.Common.Utilities.Config.GetSetting("AllowDnnUpgradeUpload"), out bool allowDnnUpgradeUpload);
                if (!allowDnnUpgradeUpload)
                {
                    return Task.FromResult(this.Request.CreateResponse(HttpStatusCode.Forbidden, new { message = this.LocalizeString("Upgrade_UploadNotAllowed"), }));
                }

                return this.UploadFileAction(cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Task.FromResult(this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { message = ex.Message, }));
            }
        }

        private async Task<HttpResponseMessage> UploadFileAction(CancellationToken cancellationToken)
        {
            var request = this.Request;
            if (!request.Content.IsMimeMultipartContent())
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            // Ensure content is buffered (helps if something read headers but not the body).
            // Note: if a filter already consumed the body, buffering here won't restore it —
            // the filter must use LoadIntoBufferAsync or otherwise avoid consuming the stream.
            await request.Content.LoadIntoBufferAsync().ConfigureAwait(false);

            var provider = new MultipartMemoryStreamProvider();

            // read multipart parts
            await request.Content.ReadAsMultipartAsync(provider, cancellationToken).ConfigureAwait(false);

            object result = null;
            var fileName = string.Empty;
            Stream stream = null;

            foreach (var item in provider.Contents)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var name = item.Headers.ContentDisposition.Name;
                switch (name.ToUpperInvariant())
                {
                    case "\"POSTFILE\"":
                        fileName = item.Headers.ContentDisposition.FileName.Replace("\"", string.Empty);
                        if (fileName.IndexOf("\\", StringComparison.Ordinal) != -1)
                        {
                            fileName = Path.GetFileName(fileName);
                        }

                        if (Globals.FileEscapingRegex.Match(fileName).Success == false && Path.GetExtension(fileName).ToLowerInvariant() == ".zip")
                        {
                            stream = item.ReadAsStreamAsync().Result;
                        }

                        break;
                }
            }

            if (!string.IsNullOrEmpty(fileName) && stream != null)
            {
                var info = await this.localUpgradeService.GetLocalUpgradeInfo(Path.GetFileNameWithoutExtension(fileName), stream, cancellationToken);
                if (!info.IsValid)
                {
                    return request.CreateResponse(HttpStatusCode.BadRequest, new { message = this.LocalizeString($"Upgrade_InvalidPackage", fileName), });
                }
                else if (info.IsOutdated)
                {
                    return request.CreateResponse(HttpStatusCode.BadRequest, new { message = this.LocalizeString($"Upgrade_OutdatedPackage", fileName), });
                }
                else
                {
                    var upgradePath = Path.Combine(this.applicationStatusInfo.ApplicationMapPath, "App_Data", "Upgrade", info.PackageName + ".zip");
                    using (var fileStream = File.Create(upgradePath))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.CopyTo(fileStream);
                    }

                    return this.Request.CreateResponse(HttpStatusCode.OK, result);
                }
            }

            return request.CreateResponse(HttpStatusCode.BadRequest, new { message = this.LocalizeString($"Upgrade_InvalidPackage", fileName), });
        }

        private string LocalizeString(string key, params object[] args)
        {
            return string.Format(Localization.GetString(key, ResourceFile), args);
        }
    }
}
