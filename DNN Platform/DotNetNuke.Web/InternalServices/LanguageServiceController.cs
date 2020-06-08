// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using System.Collections.Generic;

using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Abstractions;

namespace DotNetNuke.Web.InternalServices
{
    [DnnAuthorize]
    public class LanguageServiceController : DnnApiController
    {
        protected INavigationManager NavigationManager { get; }
        public LanguageServiceController(INavigationManager navigationManager)
        {
            NavigationManager = navigationManager;
        }

        public class PageDto
        {
            public string Name { get; set; }
            public string ViewUrl { get; set; }
            public string EditUrl { get; set; }
        }
        private bool IsDefaultLanguage(string code)
        {
            return code == PortalSettings.DefaultLanguage;
        }

        [HttpGet]
        public HttpResponseMessage GetNonTranslatedPages(string languageCode)
        {
            var request = HttpContext.Current.Request;
           var locale = new LocaleController().GetLocale(languageCode);

            List<PageDto> pages = new List<PageDto>();
            if (!IsDefaultLanguage(locale.Code))
            {
                TabController ctl = new TabController();
                var nonTranslated = (from t in ctl.GetTabsByPortal(PortalSettings.PortalId).WithCulture(locale.Code, false).Values where !t.IsTranslated && !t.IsDeleted select t);
                foreach (TabInfo page in nonTranslated)
                {
                    pages.Add(new PageDto()
                    {
                        Name = page.TabName,
                        ViewUrl = NavigationManager.NavigateURL(page.TabID),
                        EditUrl = NavigationManager.NavigateURL(page.TabID, "Tab", "action=edit", "returntabid=" + PortalSettings.ActiveTab.TabID)
                    });
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, pages);
        }
    }
}
