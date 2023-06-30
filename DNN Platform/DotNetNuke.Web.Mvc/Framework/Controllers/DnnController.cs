// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.Controllers
{
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

    public abstract class DnnController : Controller, IDnnController
    {
        /// <summary>Initializes a new instance of the <see cref="DnnController"/> class.</summary>
        protected DnnController()
        {
            this.ActionInvoker = new ResultCapturingActionInvoker();
        }

        public ModuleInfo ActiveModule
        {
            get { return (this.ModuleContext == null) ? null : this.ModuleContext.Configuration; }
        }

        public TabInfo ActivePage
        {
            get { return (this.PortalSettings == null) ? null : this.PortalSettings.ActiveTab; }
        }

        public PortalSettings PortalSettings
        {
            get { return (this.ModuleContext == null) ? null : this.ModuleContext.PortalSettings; }
        }

        /// <inheritdoc/>
        public ActionResult ResultOfLastExecute
        {
            get
            {
                var actionInvoker = this.ActionInvoker as ResultCapturingActionInvoker;
                return (actionInvoker != null) ? actionInvoker.ResultOfLastInvoke : null;
            }
        }

        public new UserInfo User
        {
            get { return (this.PortalSettings == null) ? null : this.PortalSettings.UserInfo; }
        }

        /// <inheritdoc/>
        public Page DnnPage { get; set; }

        /// <inheritdoc/>
        public new DnnUrlHelper Url { get; set; }

        /// <inheritdoc/>
        public string LocalResourceFile { get; set; }

        /// <inheritdoc/>
        public ModuleActionCollection ModuleActions { get; set; }

        /// <inheritdoc/>
        public ModuleInstanceContext ModuleContext { get; set; }

        /// <inheritdoc/>
        public ViewEngineCollection ViewEngineCollectionEx { get; set; }

        /// <inheritdoc/>
        public string LocalizeString(string key)
        {
            return Localization.GetString(key, this.LocalResourceFile);
        }

        protected internal RedirectToRouteResult RedirectToDefaultRoute()
        {
            return new DnnRedirecttoRouteResult(string.Empty, string.Empty, string.Empty, null, false);
        }

        /// <inheritdoc/>
        protected override RedirectToRouteResult RedirectToAction(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            return new DnnRedirecttoRouteResult(actionName, controllerName, string.Empty, routeValues, false, this.Url);
        }

        /// <inheritdoc/>
        protected override ViewResult View(IView view, object model)
        {
            if (model != null)
            {
                this.ViewData.Model = model;
            }

            return new DnnViewResult
            {
                View = view,
                ViewData = this.ViewData,
                TempData = this.TempData,
            };
        }

        /// <inheritdoc/>
        protected override ViewResult View(string viewName, string masterName, object model)
        {
            if (model != null)
            {
                this.ViewData.Model = model;
            }

            return new DnnViewResult
            {
                ViewName = viewName,
                MasterName = masterName,
                ViewData = this.ViewData,
                TempData = this.TempData,
                ViewEngineCollection = this.ViewEngineCollection,
            };
        }

        /// <inheritdoc/>
        protected override PartialViewResult PartialView(string viewName, object model)
        {
            if (model != null)
            {
                this.ViewData.Model = model;
            }

            return new DnnPartialViewResult
            {
                ViewName = viewName,
                ViewData = this.ViewData,
                TempData = this.TempData,
                ViewEngineCollection = this.ViewEngineCollection,
            };
        }

        /// <inheritdoc/>
        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);
            this.Url = new DnnUrlHelper(requestContext, this);
        }
    }
}
