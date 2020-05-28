// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

using System;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;

namespace DotNetNuke.Web.Api
{
    public class AuthFilterContext
    {
        public AuthFilterContext(HttpActionContext actionContext, string authFailureMessage)
        {
            ActionContext = actionContext;
            AuthFailureMessage = authFailureMessage;
        }

        public HttpActionContext ActionContext { get; private set; }
        public string AuthFailureMessage { get; set; }

        /// <summary>
        /// Processes requests that fail authorization. This default implementation creates a new response with the
        /// Unauthorized status code. Override this method to provide your own handling for unauthorized requests.
        /// </summary>
        public virtual void HandleUnauthorizedRequest()
        {
            ActionContext.Response = ActionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, AuthFailureMessage);
        }
    }
}
