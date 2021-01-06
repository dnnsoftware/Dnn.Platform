// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using DotNetNuke.Common;
using DotNetNuke.Services.Installer.Log;
using DotNetNuke.Web.Api.Internal;

namespace DNNConnect.CKEditorProvider.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;

    using DotNetNuke.Web.Api;

    public class EasyImageController : DnnApiController
    {
        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage GetToken()
        {

            return Request.CreateResponse(HttpStatusCode.OK, "dummytoken");
        }

        [DnnAuthorize]
        [HttpPost]
        //[IFrameSupportedValidateAntiForgeryToken]
        public HttpResponseMessage UploadFile(int portalId, int tabId, int mid, string ckid)
        {
            //var statuses = new List<FilesStatus>();
            //try
            //{
            //    //todo can we eliminate the HttpContext here
            //    UploadWholeFile(HttpContextSource.Current, statuses);
            //}
            //catch (Exception exc)
            //{
            //    Logger.Error(exc);
            //}
            //return new HttpResponseMessage
            //{
            //    Content = new StringContent(JsonConvert.SerializeObject(new ImageStatus
            //    {
            //        Default = statuses[0].url
            //    }))
            //};
            return Request.CreateResponse(HttpStatusCode.OK);
        }

    }
}
