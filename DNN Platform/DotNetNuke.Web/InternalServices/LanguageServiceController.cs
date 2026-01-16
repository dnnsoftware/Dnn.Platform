// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;

    /// <summary>A web API for getting the translation status of a site.</summary>
    [DnnAuthorize]
    public class LanguageServiceController : DnnApiController
    {
        /// <summary>Initializes a new instance of the <see cref="LanguageServiceController"/> class.</summary>
        /// <param name="navigationManager">The navigation manager.</param>
        public LanguageServiceController(INavigationManager navigationManager)
        {
            this.NavigationManager = navigationManager;
        }

        /// <summary>Gets the navigation manager.</summary>
        protected INavigationManager NavigationManager { get; }

        /// <summary>Gets the pages which aren't translated for the <paramref name="languageCode"/>.</summary>
        /// <param name="languageCode">The language code.</param>
        /// <returns>A response with a list of <see cref="PageDto"/> instances.</returns>
        [HttpGet]
        public HttpResponseMessage GetNonTranslatedPages(string languageCode)
        {
            var request = HttpContext.Current.Request;
            var locale = new LocaleController().GetLocale(languageCode);

            List<PageDto> pages = new List<PageDto>();
            if (!this.IsDefaultLanguage(locale.Code))
            {
                TabController ctl = new TabController();
                var nonTranslated = from t in ctl.GetTabsByPortal(this.PortalSettings.PortalId).WithCulture(locale.Code, false).Values where !t.IsTranslated && !t.IsDeleted select t;
                foreach (TabInfo page in nonTranslated)
                {
                    pages.Add(new PageDto()
                    {
                        Name = page.TabName,
                        ViewUrl = this.NavigationManager.NavigateURL(page.TabID),
                        EditUrl = this.NavigationManager.NavigateURL(page.TabID, "Tab", "action=edit", "returntabid=" + this.PortalSettings.ActiveTab.TabID),
                    });
                }
            }

            return this.Request.CreateResponse(HttpStatusCode.OK, pages);
        }

        private bool IsDefaultLanguage(string code)
        {
            return code == this.PortalSettings.DefaultLanguage;
        }

        /// <summary>A data transfer object with information about a page.</summary>
        public class PageDto
        {
            /// <summary>Gets or sets the page's name.</summary>
            public string Name { get; set; }

            /// <summary>Gets or sets the page's view URL.</summary>
            public string ViewUrl { get; set; }

            /// <summary>Gets or sets the page's edit URL.</summary>
            public string EditUrl { get; set; }
        }
    }
}
