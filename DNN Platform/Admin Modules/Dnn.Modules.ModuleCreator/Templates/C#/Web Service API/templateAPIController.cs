﻿// 
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See LICENSE file in the project root for full license information.
// 
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
