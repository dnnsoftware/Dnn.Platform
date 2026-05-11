// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.SessionState;

    using DotNetNuke.Abstractions.Application;
    using DotNetNuke.Abstractions.Logging;
    using DotNetNuke.Common;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.HttpModules.Membership;
    using DotNetNuke.Security.Roles;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.UserRequest;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Common;
    using DotNetNuke.Web.Mvc.Framework.Modules;
    using DotNetNuke.Web.Mvc.Routing;

    using Microsoft.Extensions.DependencyInjection;

    public class DnnMvcHandler : IHttpHandler, IRequiresSessionState
    {
        public static readonly string MvcVersionHeaderName = "X-AspNetMvc-Version";

        private ControllerBuilder controllerBuilder;

        public DnnMvcHandler(RequestContext requestContext)
        {
            this.RequestContext = requestContext ?? throw new ArgumentNullException(nameof(requestContext));
        }

        public static bool DisableMvcResponseHeader { get; set; }

        public RequestContext RequestContext { get; private set; }

        /// <inheritdoc />
        bool IHttpHandler.IsReusable => this.IsReusable;

        internal ControllerBuilder ControllerBuilder
        {
            get => this.controllerBuilder ??= ControllerBuilder.Current;
            set => this.controllerBuilder = value;
        }

        protected virtual bool IsReusable => false;

        /// <inheritdoc />
        void IHttpHandler.ProcessRequest(HttpContext httpContext)
        {
            SetThreadCulture();
            MembershipModule.AuthenticateRequest(
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettingsService>(),
                PortalController.Instance,
                UserRequestIPAddressController.Instance,
                RoleController.Instance,
                Globals.GetCurrentServiceProvider().GetRequiredService<IEventLogger>(),
                Globals.GetCurrentServiceProvider().GetRequiredService<IHostSettings>(),
                this.RequestContext.HttpContext,
                allowUnknownExtensions: true);
            this.ProcessRequest(httpContext);
        }

        protected internal virtual void ProcessRequest(HttpContextBase httpContext)
        {
            try
            {
                var moduleExecutionEngine = GetModuleExecutionEngine();

                // Check if the controller supports IDnnController
                var moduleResult =
                    moduleExecutionEngine.ExecuteModule(this.GetModuleRequestContext(httpContext));
                httpContext.SetModuleRequestResult(moduleResult);
                this.RenderModule(moduleResult);
            }
            finally
            {
            }
        }

        protected virtual void ProcessRequest(HttpContext httpContext)
        {
            HttpContextBase httpContextBase = new HttpContextWrapper(httpContext);
            this.ProcessRequest(httpContextBase);
        }

        private static void SetThreadCulture()
        {
            var portalSettings = PortalController.Instance.GetCurrentSettings();
            if (portalSettings is null)
            {
                return;
            }

            var pageLocale = Localization.GetPageLocale(portalSettings);
            if (pageLocale is null)
            {
                return;
            }

            Localization.SetThreadCultures(pageLocale, portalSettings);
        }

        private static IModuleExecutionEngine GetModuleExecutionEngine()
        {
            var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();

            if (moduleExecutionEngine == null)
            {
                moduleExecutionEngine = new ModuleExecutionEngine();
                ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(moduleExecutionEngine);
            }

            return moduleExecutionEngine;
        }

        private ModuleRequestContext GetModuleRequestContext(HttpContextBase httpContext)
        {
            var moduleInfo = httpContext.Request.FindModuleInfo();
            var moduleContext = new ModuleInstanceContext() { Configuration = moduleInfo };
            var desktopModule = DesktopModuleControllerAdapter.Instance.GetDesktopModule(moduleInfo.DesktopModuleID, moduleInfo.PortalID);
            var moduleRequestContext = new ModuleRequestContext
            {
                HttpContext = httpContext,
                ModuleContext = moduleContext,
                ModuleApplication = new ModuleApplication(this.RequestContext)
                {
                    ModuleName = desktopModule.ModuleName,
                    FolderPath = desktopModule.FolderName,
                },
            };

            return moduleRequestContext;
        }

        private void RenderModule(ModuleRequestResult moduleResult)
        {
            var writer = this.RequestContext.HttpContext.Response.Output;

            var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();

            moduleExecutionEngine.ExecuteModuleResult(moduleResult, writer);
        }
    }
}
