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

    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.HttpModules.Membership;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.MvcPipeline.Routing;

    public class DnnMvcPageHandler : MvcHandler, IHttpHandler, IRequiresSessionState
    {
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

        protected override IAsyncResult BeginProcessRequest(HttpContext httpContext, AsyncCallback callback, object state)
        {
            this.SetThreadCulture();
            MembershipModule.AuthenticateRequest(this.RequestContext.HttpContext, allowUnknownExtensions: true);
            return base.BeginProcessRequest(httpContext, callback, state);
        }

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
