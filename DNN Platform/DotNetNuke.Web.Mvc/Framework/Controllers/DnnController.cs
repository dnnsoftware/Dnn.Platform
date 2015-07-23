﻿#region Copyright
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

using System;
using System.Web.Mvc;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Entities.Portals;
using DotNetNuke.Entities.Tabs;
using DotNetNuke.Entities.Users;
using DotNetNuke.Services.Localization;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Framework.ActionResults;
using DotNetNuke.Web.Mvc.Framework.Modules;

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
    }
}