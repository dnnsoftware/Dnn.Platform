// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
#region Using Statements

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;

#endregion

namespace _OWNER_._MODULE_
{
    public class _MODULE_APIController : DnnApiController
    {
        [HttpGet]
        [AllowAnonymous]
        public HttpResponseMessage MyResponse()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Hello World!");
        }
    }
} 
