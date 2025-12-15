// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc
{
    using System;
    using System.Globalization;
    using System.IO;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.UI;

    using DotNetNuke.Abstractions.Modules;
    using DotNetNuke.Collections;
    using DotNetNuke.Common.Extensions;
    using DotNetNuke.Common.Utilities;
    using DotNetNuke.ComponentModel;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Services.Exceptions;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Common;
    using DotNetNuke.Web.Mvc.Framework.Modules;
    using DotNetNuke.Web.Mvc.Routing;

    using Microsoft.Extensions.DependencyInjection;

    /// <summary>WebForms control for hosting an MVC module control.</summary>
    public class MvcHostControl : ModuleControlBase, IActionable
    {
        private ModuleRequestResult result;
        private string controlKey;

        /// <summary>Initializes a new instance of the <see cref="MvcHostControl"/> class.</summary>
        public MvcHostControl()
        {
            this.controlKey = string.Empty;
        }

        /// <summary>Initializes a new instance of the <see cref="MvcHostControl"/> class.</summary>
        /// <param name="controlKey">The module control key.</param>
        public MvcHostControl(string controlKey)
        {
            this.controlKey = controlKey;
        }

        /// <inheritdoc/>
        public ModuleActionCollection ModuleActions { get; private set; }

        /// <summary>Gets or sets a value indicating whether the module controller should execute immediately (i.e. during <see cref="Control.OnInit"/> rather than <see cref="ISettingsControl.LoadSettings"/>).</summary>
        protected bool ExecuteModuleImmediately { get; set; } = true;

        /// <summary>Runs and renders the MVC action.</summary>
        protected void ExecuteModule()
        {
            try
            {
                HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);

                var moduleExecutionEngine = GetModuleExecutionEngine();

                this.result = moduleExecutionEngine.ExecuteModule(this.GetModuleRequestContext(httpContext));

                this.ModuleActions = this.LoadActions(this.result);

                httpContext.SetModuleRequestResult(this.result);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <inheritdoc/>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (this.ExecuteModuleImmediately)
            {
                this.ExecuteModule();
            }
        }

        /// <inheritdoc/>
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            try
            {
                if (this.result == null)
                {
                    return;
                }

                var mvcString = RenderModule(this.result);
                if (!string.IsNullOrEmpty(Convert.ToString(mvcString)))
                {
                    this.Controls.Add(new LiteralControl(Convert.ToString(mvcString)));
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private static ModuleApplication GetModuleApplication(
            IBusinessControllerProvider businessControllerProvider,
            DesktopModuleInfo desktopModule,
            RouteData defaultRouteData)
        {
            // Check if the MVC Module overrides the base ModuleApplication class.
            var moduleApplication = businessControllerProvider.GetInstance<ModuleApplication>(desktopModule);
            if (moduleApplication != null)
            {
                defaultRouteData.Values["controller"] = moduleApplication.DefaultControllerName;
                defaultRouteData.Values["action"] = moduleApplication.DefaultActionName;
                defaultRouteData.DataTokens["namespaces"] = moduleApplication.DefaultNamespaces;
                return moduleApplication;
            }

            var defaultControllerName = (string)defaultRouteData.Values["controller"];
            var defaultActionName = (string)defaultRouteData.Values["action"];
            var defaultNamespaces = (string[])defaultRouteData.DataTokens["namespaces"];

            return new ModuleApplication
            {
                DefaultActionName = defaultControllerName,
                DefaultControllerName = defaultActionName,
                DefaultNamespaces = defaultNamespaces,
                ModuleName = desktopModule.ModuleName,
                FolderPath = desktopModule.FolderName,
            };
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

        private static MvcHtmlString RenderModule(ModuleRequestResult moduleResult)
        {
            using var writer = new StringWriter(CultureInfo.CurrentCulture);
            var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();

            moduleExecutionEngine.ExecuteModuleResult(moduleResult, writer);

            return MvcHtmlString.Create(writer.ToString());
        }

        private ModuleRequestContext GetModuleRequestContext(HttpContextBase httpContext)
        {
            var module = this.ModuleContext.Configuration;

            // TODO DesktopModuleControllerAdapter usage is temporary in order to make method testable
            var desktopModule = DesktopModuleControllerAdapter.Instance.GetDesktopModule(module.DesktopModuleID, module.PortalID);
            var defaultControl = ModuleControlControllerAdapter.Instance.GetModuleControlByControlKey(string.Empty, module.ModuleDefID);

            var defaultRouteData = ModuleRoutingProvider.Instance().GetRouteData(null, defaultControl);

            var moduleApplication = GetModuleApplication(
                httpContext.GetScope().ServiceProvider.GetRequiredService<IBusinessControllerProvider>(),
                desktopModule,
                defaultRouteData);

            RouteData routeData;

            var queryString = httpContext.Request.QueryString;

            if (string.IsNullOrEmpty(this.controlKey))
            {
                this.controlKey = queryString.GetValueOrDefault("ctl", string.Empty);
            }

            var moduleId = Null.NullInteger;
            if (queryString["moduleid"] != null)
            {
                if (!int.TryParse(queryString["moduleid"], out moduleId))
                {
                    moduleId = Null.NullInteger;
                }
            }

            if (moduleId != this.ModuleContext.ModuleId && string.IsNullOrEmpty(this.controlKey))
            {
                // Set default routeData for module that is not the "selected" module
                routeData = defaultRouteData;
            }
            else
            {
                var control = ModuleControlControllerAdapter.Instance.GetModuleControlByControlKey(this.controlKey, module.ModuleDefID);
                routeData = ModuleRoutingProvider.Instance().GetRouteData(httpContext, control);
            }

            var moduleRequestContext = new ModuleRequestContext
            {
                DnnPage = this.Page,
                HttpContext = httpContext,
                ModuleContext = this.ModuleContext,
                ModuleApplication = moduleApplication,
                RouteData = routeData,
            };

            return moduleRequestContext;
        }

        private ModuleActionCollection LoadActions(ModuleRequestResult requestResult)
        {
            var actions = new ModuleActionCollection();

            if (requestResult.ModuleActions != null)
            {
                foreach (ModuleAction action in requestResult.ModuleActions)
                {
                    action.ID = this.ModuleContext.GetNextActionID();
                    actions.Add(action);
                }
            }

            return actions;
        }
    }
}
