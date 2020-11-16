// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Net.Http.Headers;
    using System.Text;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Host;

    public class AuthFilterContext
    {
        public AuthFilterContext(AuthorizationContext filterContext, string authFailureMessage)
        {
            this.ActionContext = filterContext;
            this.AuthFailureMessage = authFailureMessage;
        }

        public AuthorizationContext ActionContext { get; private set; }

        public string AuthFailureMessage { get; set; }

        /// <summary>
        /// Processes requests that fail authorization. This default implementation creates a new response with the
        /// Unauthorized status code. Override this method to provide your own handling for unauthorized requests.
        /// </summary>
        public virtual void HandleUnauthorizedRequest()
        {
            this.ActionContext.Result = new HttpUnauthorizedResult(this.AuthFailureMessage);
            if (!Host.DebugMode)
            {
                this.ActionContext.RequestContext.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;
            }
        }
    }
}
