#region Copyright

// 
// DotNetNuke® - http://www.dotnetnuke.com
// Copyright (c) 2002-2017
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.

#endregion

using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Collections.Generic;

using DotNetNuke.Application;
using DotNetNuke.Common;
using DotNetNuke.Entities.Controllers;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Api;
using DotNetNuke.Entities.Tabs;

namespace DotNetNuke.Web.InternalServices
{
    [DnnAuthorize]
    public class LanguageServiceController : DnnApiController
    {
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
                        ViewUrl = DotNetNuke.Common.Globals.NavigateURL(page.TabID),
                        EditUrl = DotNetNuke.Common.Globals.NavigateURL(page.TabID, "Tab", "action=edit", "returntabid=" + PortalSettings.ActiveTab.TabID)
                    });
                }
            }
            return Request.CreateResponse(HttpStatusCode.OK, pages);
        }
    }
}
