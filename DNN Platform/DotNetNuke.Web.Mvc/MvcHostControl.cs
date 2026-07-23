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
        /// <summary>Initializes a new instance of the <see cref="MvcHostControl"/> class.</summary>
        public MvcHostControl()
        {
            this.ControlKey = string.Empty;
        }

        /// <summary>Initializes a new instance of the <see cref="MvcHostControl"/> class.</summary>
        /// <param name="controlKey">The module control key.</param>
        public MvcHostControl(string controlKey)
        {
            this.ControlKey = controlKey;
        }

        /// <inheritdoc />
        public ModuleActionCollection ModuleActions { get; protected set; }

        protected ModuleRequestResult Result { get; set; }

        protected string ControlKey { get; set; }

        /// <summary>Gets or sets a value indicating whether the module controller should execute immediately (i.e. during <see cref="Control.OnInit"/> rather than <see cref="ISettingsControl.LoadSettings"/>).</summary>
        protected bool ExecuteModuleImmediately { get; set; } = true;

        protected static IModuleExecutionEngine GetModuleExecutionEngine()
        {
            var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();

            if (moduleExecutionEngine == null)
            {
                moduleExecutionEngine = new ModuleExecutionEngine();
                ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(moduleExecutionEngine);
            }

            return moduleExecutionEngine;
        }

        protected static MvcHtmlString RenderModule(ModuleRequestResult moduleResult)
        {
            using var writer = new StringWriter(CultureInfo.CurrentCulture);
            var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();

            moduleExecutionEngine.ExecuteModuleResult(moduleResult, writer);

            return MvcHtmlString.Create(writer.ToString());
        }

        /// <summary>Runs and renders the MVC action.</summary>
        protected void ExecuteModule()
        {
            try
            {
                HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);

                var moduleExecutionEngine = GetModuleExecutionEngine();

                this.Result = moduleExecutionEngine.ExecuteModule(this.GetModuleRequestContext(httpContext));

                this.ModuleActions = this.LoadActions(this.Result);

                httpContext.SetModuleRequestResult(this.Result);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        /// <inheritdoc />
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            this.OnInitInternal(e);
        }

        protected virtual void OnInitInternal(EventArgs e)
        {
            if (this.ExecuteModuleImmediately)
            {
                this.ExecuteModule();
            }
        }

        /// <inheritdoc />
        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            this.OnPreRenderInternal(e);
        }

        protected virtual void OnPreRenderInternal(EventArgs e)
        {
            try
            {
                if (this.Result == null)
                {
                    return;
                }

                var mvcString = RenderModule(this.Result);
                if (!string.IsNullOrEmpty(Convert.ToString(mvcString, CultureInfo.InvariantCulture)))
                {
                    this.Controls.Add(new LiteralControl(Convert.ToString(mvcString, CultureInfo.InvariantCulture)));
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        protected ModuleRequestContext GetModuleRequestContext(HttpContextBase httpContext)
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

            if (string.IsNullOrEmpty(this.ControlKey))
            {
                this.ControlKey = queryString.GetValueOrDefault("ctl", string.Empty);
            }

            var moduleId = Null.NullInteger;
            if (queryString["moduleid"] != null)
            {
                if (!int.TryParse(queryString["moduleid"], out moduleId))
                {
                    moduleId = Null.NullInteger;
                }
            }

            if (moduleId != this.ModuleContext.ModuleId && string.IsNullOrEmpty(this.ControlKey))
            {
                // Set default routeData for module that is not the "selected" module
                routeData = defaultRouteData;
            }
            else
            {
                var control = ModuleControlControllerAdapter.Instance.GetModuleControlByControlKey(this.ControlKey, module.ModuleDefID);
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

        protected ModuleActionCollection LoadActions(ModuleRequestResult requestResult)
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
    }
}
