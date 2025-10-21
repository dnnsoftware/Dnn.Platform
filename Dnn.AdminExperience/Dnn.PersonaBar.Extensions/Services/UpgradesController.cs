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
    using System.Net.Http.Formatting;
    using System.Net.Http.Headers;
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
    using DotNetNuke.Web.Api.Internal;

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
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = "There are no upgrades to apply", });
            }

            var upgrade = upgrades.FirstOrDefault(u => u.PackageName.Equals(data.PackageName, StringComparison.InvariantCultureIgnoreCase));
            if (upgrade == null || !upgrade.IsValid || upgrade.IsOutdated)
            {
                return this.Request.CreateResponse(HttpStatusCode.BadRequest, new { message = $"There is no valid upgrade for package {data.PackageName}", });
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

        /// <summary>Deletes a local upgrade package.</summary>
        /// <param name="data">The DTO containing the packageName.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>An empty success response.</returns>
        [HttpPost]
        public async Task<HttpResponseMessage> Delete(UpgradePackageRequestDto data, CancellationToken cancellationToken)
        {
            var upgrades = await this.localUpgradeService.GetLocalUpgrades(cancellationToken);
            var upgrade = upgrades.FirstOrDefault(u => u.PackageName.Equals(data.PackageName, StringComparison.InvariantCultureIgnoreCase));
            try
            {
                var packagePath = Path.Combine(this.applicationStatusInfo.ApplicationMapPath, "App_Data", "Upgrade", upgrade.PackageName + ".zip");
                if (File.Exists(packagePath))
                {
                    File.Delete(packagePath);
                }
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
        /// <returns>Either a package info or an error message.</returns>
        [HttpPost]
        [IFrameSupportedValidateAntiForgeryToken]
        public Task<HttpResponseMessage> Upload()
        {
            try
            {
                return this.UploadFileAction();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return Task.FromResult(this.Request.CreateResponse(HttpStatusCode.InternalServerError, new { message = ex.Message, }));
            }
        }

        private async Task<HttpResponseMessage> UploadFileAction()
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
            await request.Content.ReadAsMultipartAsync(provider).ConfigureAwait(false);

            object result = null;
            var fileName = string.Empty;
            Stream stream = null;

            foreach (var item in provider.Contents)
            {
                var name = item.Headers.ContentDisposition.Name;
                switch (name.ToUpper())
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

            // Response Content Type cannot be application/json
            // because IE9 with iframe-transport manages the response
            // as a file download 1
            var mediaTypeFormatter = new JsonMediaTypeFormatter();
            mediaTypeFormatter.SupportedMediaTypes.Add(new MediaTypeHeaderValue("text/plain"));

            if (!string.IsNullOrEmpty(fileName) && stream != null)
            {
                var info = this.localUpgradeService.GetLocalUpgradeInfo(Path.GetFileNameWithoutExtension(fileName), stream, CancellationToken.None).Result;
                if (!info.IsValid)
                {
                    return request.CreateResponse(HttpStatusCode.BadRequest, new { message = $"The uploaded file {fileName} is not a valid DNN package.", });
                }
                else if (info.IsOutdated)
                {
                    return request.CreateResponse(HttpStatusCode.BadRequest, new { message = $"The uploaded file {fileName} is an outdated DNN package.", });
                }
                else
                {
                    var upgradePath = Path.Combine(this.applicationStatusInfo.ApplicationMapPath, "App_Data", "Upgrade", info.PackageName + ".zip");
                    using (var fileStream = File.Create(upgradePath))
                    {
                        stream.Seek(0, SeekOrigin.Begin);
                        stream.CopyTo(fileStream);
                    }

                    return this.Request.CreateResponse(
                        HttpStatusCode.OK,
                        result,
                        mediaTypeFormatter,
                        "text/plain");
                }
            }

            return request.CreateResponse(HttpStatusCode.BadRequest, new { message = $"The uploaded file {fileName} was not a DNN package.", });
        }
    }
}
