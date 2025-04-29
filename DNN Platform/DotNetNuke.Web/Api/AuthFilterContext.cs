// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Api;

using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;

public class AuthFilterContext
{
    /// <summary>Initializes a new instance of the <see cref="AuthFilterContext"/> class.</summary>
    /// <param name="actionContext">The HTTP action context.</param>
    /// <param name="authFailureMessage">The message to include in the response if there's an auth failure.</param>
    public AuthFilterContext(HttpActionContext actionContext, string authFailureMessage)
    {
        this.ActionContext = actionContext;
        this.AuthFailureMessage = authFailureMessage;
    }

    public HttpActionContext ActionContext { get; private set; }

    public string AuthFailureMessage { get; set; }

    /// <summary>
    /// Processes requests that fail authorization. This default implementation creates a new response with the
    /// Unauthorized status code. Override this method to provide your own handling for unauthorized requests.
    /// </summary>
    public virtual void HandleUnauthorizedRequest()
    {
        this.ActionContext.Response = this.ActionContext.ControllerContext.Request.CreateErrorResponse(HttpStatusCode.Unauthorized, this.AuthFailureMessage);
    }
}
