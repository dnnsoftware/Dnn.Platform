#region Copyright

// 
// Copyright (c) [YEAR]
// by [OWNER]
// 

#endregion

#region Using Statements

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;

#endregion

namespace [OWNER].[MODULE]
{
    [AllowAnonymous]
    public class [MODULE]APIController : DnnApiController
    {
        [HttpGet]
        public HttpResponseMessage MyResponse()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Hello World!");
        }
    }
} 
