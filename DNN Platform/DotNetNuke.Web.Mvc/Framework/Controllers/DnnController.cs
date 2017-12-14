// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Text;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Framework.ActionResults;
using DotNetNuke.Web.Mvc.Framework.Modules;
using DotNetNuke.Web.Mvc.Helpers;

namespace DotNetNuke.Web.Mvc.Framework.Controllers
{
    public abstract class DnnController : Controller, IDnnController
    {
        protected DnnController()
        {
            ActionInvoker = new ResultCapturingActionInvoker();
        }

        public ModuleInfo ActiveModule
        {
            get { return (ModuleContext == null) ? null : ModuleContext.Configuration; }
        }

        public TabInfo ActivePage
        {
            get { return (PortalSettings == null) ? null : PortalSettings.ActiveTab; }
        }

        public Page DnnPage { get; set; }

        public new DnnUrlHelper Url { get; set; }

        public string LocalResourceFile { get; set; }

        public string LocalizeString(string key)
        {
            return Localization.GetString(key, LocalResourceFile);
        }

        public ModuleActionCollection ModuleActions { get; set; }

        public ModuleInstanceContext ModuleContext { get; set; }

        public PortalSettings PortalSettings
        {
            get { return (ModuleContext == null) ? null : ModuleContext.PortalSettings; }
        }

        protected internal RedirectToRouteResult RedirectToDefaultRoute()
        {
            return new DnnRedirecttoRouteResult(String.Empty, String.Empty, String.Empty, null, false);
        }

        public ActionResult ResultOfLastExecute
        {
            get
            {
                var actionInvoker = ActionInvoker as ResultCapturingActionInvoker;
                return (actionInvoker != null) ?  actionInvoker.ResultOfLastInvoke : null;
            }
        }

        public new UserInfo User
        {
            get { return (PortalSettings == null) ? null : PortalSettings.UserInfo; }
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

        protected override PartialViewResult PartialView(string viewName, object model)
        {
            if (model != null)
            {
                ViewData.Model = model;
            }

            return new DnnPartialViewResult
            {
                ViewName = viewName,
                ViewData = ViewData,
                TempData = TempData,
                ViewEngineCollection = ViewEngineCollection
            };
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            Url = new DnnUrlHelper(requestContext, this);
        }

        public ViewEngineCollection ViewEngineCollectionEx { get; set; }
    }
}