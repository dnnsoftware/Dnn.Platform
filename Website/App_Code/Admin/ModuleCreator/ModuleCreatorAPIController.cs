#region Copyright

// 
// Copyright (c) 2013
// by DotNetNuke
// 

#endregion

#region Using Statements

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;

#endregion

namespace DotNetNuke.ModuleCreator
{
    [AllowAnonymous]
    public class ModuleCreatorAPIController : DnnApiController
    {
        [HttpPost]
        public HttpResponseMessage MyResponse()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Hello World!");
        }
    }
} 

