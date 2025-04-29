// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DNNConnect.CKEditorProvider.Services;

using System.Net;
using System.Net.Http;
using System.Web.Http;

using DotNetNuke.Web.Api;

/// <summary>WebAPI controller to enable the CKE cloudservices plugin.</summary>
public class CloudServicesController : DnnApiController
{
    /// <summary>Provides a fake token the cloudservices plugin needs.</summary>
    /// <returns>A fake token.</returns>
    [AllowAnonymous]
    [HttpGet]
    public HttpResponseMessage GetToken()
    {
        return this.Request.CreateResponse(HttpStatusCode.OK, "faketoken");
    }
}
