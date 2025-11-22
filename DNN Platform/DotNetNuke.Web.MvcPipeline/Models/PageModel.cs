// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.MvcPipeline.Models
{
    using System.Collections.Generic;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Abstractions.ClientResources;

    public class PageModel
    {
        public int? TabId { get; set; }

        public string Language { get; set; }

        public int? PortalId { get; set; }

        public SkinModel Skin { get; set; }

        public string AntiForgery { get; set; }

        public Dictionary<string, string> ClientVariables { get; set; }

        public string PageHeadText { get; set; }

        public string PortalHeadText { get; set; }

        public string Title { get; set; }

        public string BackgroundUrl { get; set; }

        public string MetaRefresh { get; set; }

        public string Description { get; set; }

        public string KeyWords { get; set; }

        public string Copyright { get; set; }

        public string Generator { get; set; }

        public string MetaRobots { get; set; }

        public Dictionary<string, string> StartupScripts { get; set; }

        public bool IsEditMode { get; set; }

        public string FavIconLink { get; set; }

        public string CanonicalLinkUrl { get; set; }

        //TODO: CSP - enable when CSP implementation is ready
        //public IContentSecurityPolicy ContentSecurityPolicy { get; set; }

        public INavigationManager NavigationManager { get; set; }

        public IClientResourceController ClientResourceController { get; set; }
    }
}
