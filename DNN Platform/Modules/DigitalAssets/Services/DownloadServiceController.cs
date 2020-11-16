// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Modules.DigitalAssets.Services
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Web.Http;

    using DotNetNuke.Instrumentation;
    using DotNetNuke.Modules.DigitalAssets.Components.Controllers;
    using DotNetNuke.Security;
    using DotNetNuke.Web.Api;

    [SupportedModules("DotNetNuke.Modules.DigitalAssets")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    [DamExceptionFilter]
    public class DownloadServiceController : DnnApiController
    {
        protected static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ContentServiceController));

        public DownloadServiceController()
        {
            var f = new Factory();
            this.DigitalAssetsController = f.DigitalAssetsController;
        }

        protected IDigitalAssetsController DigitalAssetsController { get; private set; }

        [HttpGet]
        public HttpResponseMessage Download(int fileId, bool forceDownload)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            string fileName;
            string contentType;
            var streamContent = this.DigitalAssetsController.GetFileContent(fileId, out fileName, out contentType);
            result.Content = new StreamContent(streamContent);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(forceDownload ? "attachment" : "inline");
            result.Content.Headers.ContentDisposition.FileName = fileName;
            return result;
        }
    }
}
