// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Routing
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.SessionState;

    using DotNetNuke.Entities.Portals;
    using DotNetNuke.HttpModules.Membership;
    using DotNetNuke.Services.Localization;

    /// <summary>
    /// MVC handler used by the DNN MVC pipeline to process page requests with proper culture and authentication.
    /// </summary>
    public class DnnMvcPageHandler : MvcHandler, IHttpHandler, IRequiresSessionState
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DnnMvcPageHandler"/> class.
        /// </summary>
        /// <param name="requestContext">The current request context.</param>
        public DnnMvcPageHandler(RequestContext requestContext)
            : base(requestContext)
            {
        }

        /// <inheritdoc/>
        protected override void ProcessRequest(HttpContext httpContext)
        {
            this.SetThreadCulture();
            MembershipModule.AuthenticateRequest(this.RequestContext.HttpContext, allowUnknownExtensions: true);
            base.ProcessRequest(httpContext);
        }

        /// <inheritdoc/>
        protected override IAsyncResult BeginProcessRequest(HttpContext httpContext, AsyncCallback callback, object state)
        {
            this.SetThreadCulture();
            MembershipModule.AuthenticateRequest(this.RequestContext.HttpContext, allowUnknownExtensions: true);
            return base.BeginProcessRequest(httpContext, callback, state);
        }

        /// <summary>
        /// Sets the current thread culture based on portal and page locale settings.
        /// </summary>
        private void SetThreadCulture()
        {
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            if (portalSettings is null)
            {
                return;
            }

            var pageLocale = Localization.GetPageLocale(portalSettings);
            if (pageLocale is null)
            {
                return;
            }

            Localization.SetThreadCultures(pageLocale, portalSettings);
        }
    }
}
