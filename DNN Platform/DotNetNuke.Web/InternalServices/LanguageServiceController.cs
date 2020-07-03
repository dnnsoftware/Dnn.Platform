// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.InternalServices
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Web;
    using System.Web.Http;

    using DotNetNuke.Abstractions;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Entities.Tabs;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Web.Api;

    [DnnAuthorize]
    public class LanguageServiceController : DnnApiController
    {
        public LanguageServiceController(INavigationManager navigationManager)
        {
            this.NavigationManager = navigationManager;
        }

        protected INavigationManager NavigationManager { get; }

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

        public class PageDto
        {
            public string Name { get; set; }

            public string ViewUrl { get; set; }

            public string EditUrl { get; set; }
        }
    }
}
