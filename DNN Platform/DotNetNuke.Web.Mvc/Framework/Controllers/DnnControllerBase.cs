#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2014
// by DNN Corporation
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

using System.Web.Mvc;
using DotNetNuke.Common;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Web.Mvc.Framework.ActionResults;
using DotNetNuke.Web.Mvc.Helpers;

namespace DotNetNuke.Web.Mvc.Framework.Controllers
{
    public abstract class DnnControllerBase : Controller
    {
        public ModuleInfo ActiveModule { get; set; }

        public TabInfo ActivePage
        {
            get { return (SiteContext == null) ? null : SiteContext.ActivePage; }
        }

        public PortalInfo ActiveSite
        {
            get { return (SiteContext == null) ? null : SiteContext.ActiveSite; }
        }

        public PortalAliasInfo ActiveSiteAlias
        {
            get { return (SiteContext == null) ? null : SiteContext.ActiveSiteAlias; }
        }

        protected internal virtual ResourceNotFoundResult ResourceNotFound()
        {
            return new ResourceNotFoundResult();
        }

        protected internal virtual ResourceNotFoundResult ResourceNotFound(string viewName)
        {
            Requires.NotNullOrEmpty("viewName", viewName);
            return ResourceNotFound(View(viewName));
        }

        protected internal virtual ResourceNotFoundResult ResourceNotFound(ActionResult innerResult)
        {
            Requires.NotNull("innerResult", innerResult);
            return new ResourceNotFoundResult { InnerResult = innerResult };
        }

        public SiteContext SiteContext
        {
            get { return HttpContext.GetSiteContext(); }
        }

        public new UserInfo User
        {
            get { return (SiteContext == null) ? null : SiteContext.User; }
        }
        
        protected override ViewResult View(IView view, object model)
        {
            if (model != null)
            {
                ViewData.Model = model;
            }

            return new DnnViewResult
                        {
                            View = view,
                            ViewData = ViewData,
                            TempData = TempData
                        };
        }

        protected override ViewResult View(string viewName, string masterName, object model)
        {
            if (model != null)
            {
                ViewData.Model = model;
            }

            return new DnnViewResult
                            {
                                ViewName = viewName,
                                MasterName = masterName,
                                ViewData = ViewData,
                                TempData = TempData,
                                ViewEngineCollection = ViewEngineCollection
                            };
        }
    }
}



