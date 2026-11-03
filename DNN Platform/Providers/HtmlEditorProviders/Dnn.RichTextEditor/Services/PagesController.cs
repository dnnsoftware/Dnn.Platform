// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace Dnn.RichTextEditorProvider.Services
{
    using System.Net;
    using System.Net.Http;
    using System.Web.Http;

    using Dnn.RichTextEditorProvider.Components.PageLinks;
    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Web.Api;

    /// <summary>Lists the pages the editor's Insert Page Link button can link to.</summary>
    [DnnAuthorize]
    public class PagesController : DnnApiController
    {
        private readonly PageLinkService pageLinks;

        /// <summary>Initializes a new instance of the <see cref="PagesController"/> class.</summary>
        /// <param name="hostSettings">The host settings.</param>
        /// <param name="appStatus">The application status.</param>
        /// <param name="portalController">The portal controller.</param>
        public PagesController(IHostSettings hostSettings, IApplicationStatusInfo appStatus, IPortalController portalController)
        {
            this.pageLinks = new PageLinkService(new PageSource(hostSettings, appStatus, portalController));
        }

        /// <summary>Gets the pages of the current site the user can view, in page tree order.</summary>
        /// <returns>A list of <see cref="PageLink"/>.</returns>
        [HttpGet]
        public HttpResponseMessage List()
        {
            return this.Request.CreateResponse(HttpStatusCode.OK, this.pageLinks.GetPageLinks(this.PortalSettings.PortalId));
        }
    }
}
