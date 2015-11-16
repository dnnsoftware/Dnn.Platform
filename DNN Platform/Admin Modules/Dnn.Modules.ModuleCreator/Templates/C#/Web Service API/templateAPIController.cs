#region Copyright

// 
// Copyright (c) _YEAR_
// by _OWNER_
// 

#endregion

#region Using Statements

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using DotNetNuke.Web.Api;

#endregion

namespace _OWNER_._MODULE_
{
    [AllowAnonymous]
    public class _MODULE_APIController : DnnApiController
    {
        [HttpGet]
        public HttpResponseMessage MyResponse()
        {
            return Request.CreateResponse(HttpStatusCode.OK, "Hello World!");
        }
    }
} 
