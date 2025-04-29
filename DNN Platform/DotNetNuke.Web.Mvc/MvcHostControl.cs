// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information
namespace DotNetNuke.Web.Mvc;

using System;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;

using DotNetNuke.Collections;
using DotNetNuke.Common.Utilities;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Common;
using DotNetNuke.Web.Mvc.Framework.Modules;
using DotNetNuke.Web.Mvc.Routing;

public class MvcHostControl : ModuleControlBase, IActionable
{
    private ModuleRequestResult result;
    private string controlKey;

    public MvcHostControl()
    {
        this.controlKey = string.Empty;
    }

    public MvcHostControl(string controlKey)
    {
        this.controlKey = controlKey;
    }

    /// <inheritdoc/>
    public ModuleActionCollection ModuleActions { get; private set; }

    protected bool ExecuteModuleImmediately { get; set; } = true;

    protected void ExecuteModule()
    {
        try
        {
            HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);

            var moduleExecutionEngine = this.GetModuleExecutionEngine();

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

            var mvcString = this.RenderModule(this.result);
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

    private ModuleApplication GetModuleApplication(DesktopModuleInfo desktopModule, RouteData defaultRouteData)
    {
        ModuleApplication moduleApplication = null;

        // Check if the MVC Module overrides the base ModuleApplication class.
        var businessControllerClass = desktopModule.BusinessControllerClass;
        if (!string.IsNullOrEmpty(businessControllerClass))
        {
            var moduleApplicationType = Reflection.CreateType(businessControllerClass);
            if (moduleApplicationType != null)
            {
                moduleApplication = Reflection.CreateInstance(moduleApplicationType) as ModuleApplication;
                if (moduleApplication != null)
                {
                    defaultRouteData.Values["controller"] = moduleApplication.DefaultControllerName;
                    defaultRouteData.Values["action"] = moduleApplication.DefaultActionName;
                    defaultRouteData.DataTokens["namespaces"] = moduleApplication.DefaultNamespaces;
                }
            }
        }

        if (moduleApplication == null)
        {
            var defaultControllerName = (string)defaultRouteData.Values["controller"];
            var defaultActionName = (string)defaultRouteData.Values["action"];
            var defaultNamespaces = (string[])defaultRouteData.DataTokens["namespaces"];

            moduleApplication = new ModuleApplication
            {
                DefaultActionName = defaultControllerName,
                DefaultControllerName = defaultActionName,
                DefaultNamespaces = defaultNamespaces,
                ModuleName = desktopModule.ModuleName,
                FolderPath = desktopModule.FolderName,
            };
        }

        return moduleApplication;
    }

    private IModuleExecutionEngine GetModuleExecutionEngine()
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
        var module = this.ModuleContext.Configuration;

        // TODO DesktopModuleControllerAdapter usage is temporary in order to make method testable
        var desktopModule = DesktopModuleControllerAdapter.Instance.GetDesktopModule(module.DesktopModuleID, module.PortalID);
        var defaultControl = ModuleControlControllerAdapter.Instance.GetModuleControlByControlKey(string.Empty, module.ModuleDefID);

        var defaultRouteData = ModuleRoutingProvider.Instance().GetRouteData(null, defaultControl);

        var moduleApplication = this.GetModuleApplication(desktopModule, defaultRouteData);

        RouteData routeData;

        var queryString = httpContext.Request.QueryString;

        if (string.IsNullOrEmpty(this.controlKey))
        {
            this.controlKey = queryString.GetValueOrDefault("ctl", string.Empty);
        }

        var moduleId = Null.NullInteger;
        if (queryString["moduleid"] != null)
        {
            int.TryParse(queryString["moduleid"], out moduleId);
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

    private ModuleActionCollection LoadActions(ModuleRequestResult result)
    {
        var actions = new ModuleActionCollection();

        if (result.ModuleActions != null)
        {
            foreach (ModuleAction action in result.ModuleActions)
            {
                action.ID = this.ModuleContext.GetNextActionID();
                actions.Add(action);
            }
        }

        return actions;
    }

    private MvcHtmlString RenderModule(ModuleRequestResult moduleResult)
    {
        MvcHtmlString moduleOutput;

        using (var writer = new StringWriter(CultureInfo.CurrentCulture))
        {
            var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();

            moduleExecutionEngine.ExecuteModuleResult(moduleResult, writer);
            moduleOutput = MvcHtmlString.Create(writer.ToString());
        }

        return moduleOutput;
    }
}
