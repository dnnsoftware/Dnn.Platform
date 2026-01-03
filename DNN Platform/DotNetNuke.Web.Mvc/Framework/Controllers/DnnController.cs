// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.Controllers
{
    using System;
    using System.Diagnostics.CodeAnalysis;
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

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic", Justification = "Breaking change")]
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

            if (requestContext.RouteData != null && requestContext.RouteData.Values.ContainsKey("mvcpage"))
            {
                var values = requestContext.RouteData.Values;
                var moduleContext = new ModuleInstanceContext();
                var moduleInfo = ModuleController.Instance.GetModule((int)values["ModuleId"], (int)values["TabId"], false);

                if (moduleInfo.ModuleControlId != (int)values["ModuleControlId"])
                {
                    moduleInfo = moduleInfo.Clone();
                    moduleInfo.ContainerPath = (string)values["ContainerPath"];
                    moduleInfo.ContainerSrc = (string)values["ContainerSrc"];
                    moduleInfo.ModuleControlId = (int)values["ModuleControlId"];
                    moduleInfo.PaneName = (string)values["PanaName"];
                    moduleInfo.IconFile = (string)values["IconFile"];
                }

                moduleContext.Configuration = moduleInfo;

                this.ModuleContext = new ModuleInstanceContext() { Configuration = moduleInfo };
                this.LocalResourceFile = string.Format(
                        "~/DesktopModules/MVC/{0}/{1}/{2}.resx",
                        moduleInfo.DesktopModule.FolderName,
                        Localization.LocalResourceDirectory,
                        this.RouteData.Values["controller"]);

                var moduleApplication = new ModuleApplication(requestContext, true)
                {
                    ModuleName = moduleInfo.DesktopModule.ModuleName,
                    FolderPath = moduleInfo.DesktopModule.FolderName,
                };
                moduleApplication.Init();

                this.ViewEngineCollectionEx = moduleApplication.ViewEngines;
            }

            this.Url = new DnnUrlHelper(requestContext, this);
        }
    }
}
