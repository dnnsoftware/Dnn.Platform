// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.ActionFilters
{
    using System;
    using System.Web.Mvc;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Common;

    using Microsoft.Extensions.DependencyInjection;

    public class AuthFilterContext
    {
        private readonly IHostSettings hostSettings;

        [Obsolete("Deprecated in DotNetNuke 10.2.2. Please use overload with IHostSettings. Scheduled removal in v12.0.0.")]
        public AuthFilterContext(AuthorizationContext filterContext, string authFailureMessage)
            : this(null, filterContext, authFailureMessage)
        {
        }

        public AuthFilterContext(IHostSettings hostSettings, AuthorizationContext filterContext, string authFailureMessage)
        {
            this.hostSettings = hostSettings ?? Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>();
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
            if (!this.hostSettings.DebugMode)
            {
                this.ActionContext.RequestContext.HttpContext.Response.SuppressFormsAuthenticationRedirect = true;
            }
        }
    }
}
