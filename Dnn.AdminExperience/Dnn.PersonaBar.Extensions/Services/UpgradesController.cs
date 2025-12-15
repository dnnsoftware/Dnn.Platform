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

        /// <summary>Initializes a new instance of the <see cref="UpgradesController"/> class.</summary>
        /// <param name="applicationStatusInfo">The application status.</param>
        /// <param name="localUpgradeService">The local upgrade service.</param>
        public UpgradesController(IApplicationStatusInfo applicationStatusInfo, ILocalUpgradeService localUpgradeService)
        {
            this.applicationStatusInfo = applicationStatusInfo;
            this.localUpgradeService = localUpgradeService;
        }

        /// <summary>Retrieves upgrade settings.</summary>
        /// <returns>A <see cref="HttpResponseMessage"/> containing the upgrade settings with a status code
        /// of <see cref="HttpStatusCode.OK"/> if successful, or an error message with a status code of
        /// <see cref="HttpStatusCode.InternalServerError"/> if an exception occurs.</returns>
        [HttpGet]
        public HttpResponseMessage GetSettings()
        {
            try
            {
                if (!bool.TryParse(DotNetNuke.Common.Utilities.Config.GetSetting("AllowDnnUpgradeUpload"), out var allowDnnUpgradeUpload))
                {
                    allowDnnUpgradeUpload = false;
                }

                return this.Request.CreateResponse(HttpStatusCode.OK, new
                {
                    AllowDnnUpgradeUpload = allowDnnUpgradeUpload,
                });
            }
            catch (Exception ex)
            {
                return this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { ex.Message, });
            }
        }

        /// <summary>Retrieves a list of local upgrades.</summary>
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
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = LocalizeString("Upgrade_NoUpgrade"), });
            }

            var upgrade = upgrades.FirstOrDefault(u => u.PackageName.Equals(data.PackageName, StringComparison.InvariantCultureIgnoreCase));
            if (upgrade == null || !upgrade.CanInstall)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = LocalizeString($"Upgrade_NoValidUpgrade", data.PackageName), });
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
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = LocalizeString("Upgrade_NoUpgrade"), });
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

        /// <summary>Uploads a DNN package for local upgrade.</summary>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Either a package info or an error message.</returns>
        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        public Task<HttpResponseMessage> Upload(CancellationToken cancellationToken)
        {
            try
            {
                if (!bool.TryParse(DotNetNuke.Common.Utilities.Config.GetSetting("AllowDnnUpgradeUpload"), out var allowDnnUpgradeUpload))
                {
                    allowDnnUpgradeUpload = false;
                }

                if (!allowDnnUpgradeUpload)
                {
                    return Task.FromResult(this.Request.CreateResponse(HttpStatusCode.Forbidden, new { message = LocalizeString("Upgrade_UploadNotAllowed"), }));
                }

                return this.UploadFileAction(cancellationToken);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Task.FromResult(this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { message = ex.Message, }));
            }
        }

        /// <summary>
        /// Completes the uploads of a DNN package for local upgrade.
        /// </summary>
        /// <param name="data">The upload complete data.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>Either a package info or an error message.</returns>
        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        public async Task<HttpResponseMessage> UploadComplete(UpgradeUploadCompleteDto data, CancellationToken cancellationToken)
        {
            var fileName = Path.GetFileName(data.FileName);
            var fileId = Path.GetFileName(data.FileId);

            if (!string.IsNullOrEmpty(fileName) && !string.IsNullOrEmpty(fileId))
            {
                var writtenFile = Path.Combine(this.applicationStatusInfo.ApplicationMapPath, "App_Data", "Upgrade", fileId + ".resources");
                if (!File.Exists(writtenFile))
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = LocalizeString($"Upgrade_InvalidPackage", fileName), });
                }

                var info = await this.localUpgradeService.GetLocalUpgradeInfo(writtenFile, cancellationToken);
                if (!info.IsValid)
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = LocalizeString($"Upgrade_InvalidPackage", fileName), });
                }
                else if (info.IsOutdated)
                {
                    return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = LocalizeString($"Upgrade_OutdatedPackage", fileName), });
                }
                else
                {
                    var upgradePath = Path.Combine(this.applicationStatusInfo.ApplicationMapPath, "App_Data", "Upgrade", Path.GetFileNameWithoutExtension(fileName) + ".zip");
                    File.Move(writtenFile, upgradePath);
                    return this.Request.CreateResponse(HttpStatusCode.OK, fileName);
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = LocalizeString($"Upgrade_InvalidPackage", fileName), });
        }

        private static string LocalizeString(string key, params object[] args)
        {
            return string.Format(Localization.GetString(key, ResourceFile), args);
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

            long startPosition = 0;
            long totalSize = 0;
            string fileId = string.Empty;
            Stream stream = null;

            foreach (var item in provider.Contents)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var name = item.Headers.ContentDisposition.Name;
                switch (name.ToUpperInvariant())
                {
                    case "\"CHUNK\"":
                        stream = await item.ReadAsStreamAsync();
                        break;
                    case "\"START\"":
                        if (!long.TryParse(await item.ReadAsStringAsync(), out startPosition))
                        {
                            startPosition = 0;
                        }

                        break;
                    case "\"TOTALSIZE\"":
                        if (!long.TryParse(await item.ReadAsStringAsync(), out totalSize))
                        {
                            totalSize = 0;
                        }

                        break;
                    case "\"FILEID\"":
                        fileId = await item.ReadAsStringAsync();
                        break;
                }
            }

            if (stream != null && totalSize != 0 && !string.IsNullOrEmpty(fileId))
            {
                var fileToWriteTo = Path.Combine(this.applicationStatusInfo.ApplicationMapPath, "App_Data", "Upgrade", fileId + ".resources");

                // use the file stream and start position to write the chunk to the file
                using var fileStream = new FileStream(fileToWriteTo, FileMode.OpenOrCreate, FileAccess.Write);
                fileStream.Position = startPosition;
                await stream.CopyToAsync(fileStream).ConfigureAwait(false);
            }

            return request.CreateResponse(HttpStatusCode.OK);
        }
    }
}
