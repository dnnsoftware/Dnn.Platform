// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;

    using DotNetNuke.Common;
    using DotNetNuke.Services.Installer.Log;
    using DotNetNuke.Web.Api;
    using DotNetNuke.Web.Api.Internal;

    /// <summary>
    /// WebAPI controller to enable the CKE cloudservices plugin.
    /// </summary>
    public class CloudServicesController : DnnApiController
    {
        /// <summary>
        /// Provides a dummy token the cloudservices plugin needs.
        /// </summary>
        /// <returns>A dummy token.</returns>
        [AllowAnonymous]
        [HttpGet]
        public HttpResponseMessage GetToken()
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, "dummytoken");
        }
    }
}
