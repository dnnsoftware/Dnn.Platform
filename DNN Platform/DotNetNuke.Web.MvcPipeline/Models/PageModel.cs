// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Models
{
    using System.Collections.Generic;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Pages;

    /// <summary>
    /// Represents the data required to render a DNN page through the MVC pipeline.
    /// </summary>
    public class PageModel
    {
        /// <summary>
        /// Gets or sets the identifier of the current tab.
        /// </summary>
        public int? TabId { get; set; }

        /// <summary>
        /// Gets or sets the language code for the current request.
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// Gets or sets the identifier of the current portal.
        /// </summary>
        public int? PortalId { get; set; }

        /// <summary>
        /// Gets or sets the skin model used to render the page.
        /// </summary>
        public SkinModel Skin { get; set; }

        /// <summary>
        /// Gets or sets the anti-forgery token markup to include in the page.
        /// </summary>
        public string AntiForgery { get; set; }

        /// <summary>
        /// Gets or sets the client-side variables available to scripts.
        /// </summary>
        public Dictionary<string, string> ClientVariables { get; set; }

        /// <summary>
        /// Gets or sets the page-level head content.
        /// </summary>
        public string PageHeadText { get; set; }

        /// <summary>
        /// Gets or sets the portal-level head content.
        /// </summary>
        public string PortalHeadText { get; set; }

        /// <summary>
        /// Gets or sets the page title.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the background image URL for the page.
        /// </summary>
        public string BackgroundUrl { get; set; }

        /// <summary>
        /// Gets or sets the meta refresh directive, if any.
        /// </summary>
        public string MetaRefresh { get; set; }

        /// <summary>
        /// Gets or sets the meta description for the page.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the meta keywords for the page.
        /// </summary>
        public string KeyWords { get; set; }

        /// <summary>
        /// Gets or sets the copyright meta information for the page.
        /// </summary>
        public string Copyright { get; set; }

        /// <summary>
        /// Gets or sets the generator meta information for the page.
        /// </summary>
        public string Generator { get; set; }

        /// <summary>
        /// Gets or sets the robots meta directive for the page.
        /// </summary>
        public string MetaRobots { get; set; }

        /// <summary>
        /// Gets or sets the startup scripts registered for the page.
        /// </summary>
        public Dictionary<string, string> StartupScripts { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the page is in edit mode.
        /// </summary>
        public bool IsEditMode { get; set; }

        /// <summary>
        /// Gets or sets the favicon link element for the page.
        /// </summary>
        public string FavIconLink { get; set; }

        /// <summary>
        /// Gets or sets the canonical link URL for the page.
        /// </summary>
        public string CanonicalLinkUrl { get; set; }

        // TODO: CSP - enable when CSP implementation is ready
        // public IContentSecurityPolicy ContentSecurityPolicy { get; set; }

        /// <summary>
        /// Gets or sets the navigation manager for generating navigation URLs.
        /// </summary>
        public INavigationManager NavigationManager { get; set; }

        /// <summary>
        /// Gets or sets the client resource controller used to register scripts and styles.
        /// </summary>
        public IClientResourceController ClientResourceController { get; set; }

        /// <summary>
        /// Gets or sets the page service used to interact with page metadata.
        /// </summary>
        public IPageService PageService { get; set; }
    }
}
