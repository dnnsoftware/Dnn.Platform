// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Web.Http;

using DotNetNuke.Instrumentation;
using DotNetNuke.Modules.DigitalAssets.Components.Controllers;
using DotNetNuke.Security;
using DotNetNuke.Web.Api;

namespace DotNetNuke.Modules.DigitalAssets.Services
{
    [SupportedModules("DotNetNuke.Modules.DigitalAssets")]
    [DnnModuleAuthorize(AccessLevel = SecurityAccessLevel.View)]
    [DamExceptionFilter]
    public class DownloadServiceController : DnnApiController
    {
        protected static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(ContentServiceController));

        public DownloadServiceController()
        {
            var f = new Factory();
            DigitalAssetsController = f.DigitalAssetsController;
        }

        protected IDigitalAssetsController DigitalAssetsController { get; private set; }

        [HttpGet]
        public HttpResponseMessage Download(int fileId, bool forceDownload)
        {
            var result = new HttpResponseMessage(HttpStatusCode.OK);
            string fileName;
            string contentType;
            var streamContent = DigitalAssetsController.GetFileContent(fileId, out fileName, out contentType);
            result.Content = new StreamContent(streamContent);
            result.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);
            result.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue(forceDownload ? "attachment" : "inline");
            result.Content.Headers.ContentDisposition.FileName = fileName;
            return result;
        }             
    }
}
