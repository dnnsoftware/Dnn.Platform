// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2018
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

using System;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using DotNetNuke.Collections;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Framework;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Common;
using DotNetNuke.Web.Mvc.Framework.Modules;
using DotNetNuke.Web.Mvc.Routing;

namespace DotNetNuke.Web.Mvc
{
    public class MvcHostControl : ModuleControlBase, IActionable
    {
        #region Fields

        private ModuleRequestResult _result;
        private string _controlKey;

        #endregion

        #region Constructors

        public MvcHostControl()
        {
            _controlKey = String.Empty;
        }

        public MvcHostControl(string controlKey)
        {
            _controlKey = controlKey;
        }

        #endregion

        #region Private Methods

        private ModuleApplication GetModuleApplication(DesktopModuleInfo desktopModule, RouteData defaultRouteData)
        {

            ModuleApplication moduleApplication = null;

            //Check if the MVC Module overrides the base ModuleApplication class.
            var businessControllerClass = desktopModule.BusinessControllerClass;
            if (!String.IsNullOrEmpty(businessControllerClass))
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
                                                FolderPath = desktopModule.FolderName
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
            var module = ModuleContext.Configuration;

            //TODO DesktopModuleControllerAdapter usage is temporary in order to make method testable
            var desktopModule = DesktopModuleControllerAdapter.Instance.GetDesktopModule(module.DesktopModuleID, module.PortalID);
            var defaultControl = ModuleControlControllerAdapter.Instance.GetModuleControlByControlKey("", module.ModuleDefID);

            var defaultRouteData = ModuleRoutingProvider.Instance().GetRouteData(null, defaultControl);

            var moduleApplication = GetModuleApplication(desktopModule, defaultRouteData);

            RouteData routeData;

            if (String.IsNullOrEmpty(_controlKey))
            {
                _controlKey = httpContext.Request.QueryString.GetValueOrDefault("ctl", String.Empty);
            }

            var moduleId = httpContext.Request.QueryString.GetValueOrDefault("moduleId", -1);
            if (moduleId != ModuleContext.ModuleId && String.IsNullOrEmpty(_controlKey))
            {
                //Set default routeData for module that is not the "selected" module
                routeData = defaultRouteData;
            }
            else
            {
                var control = ModuleControlControllerAdapter.Instance.GetModuleControlByControlKey(_controlKey, module.ModuleDefID);
                routeData = ModuleRoutingProvider.Instance().GetRouteData(httpContext, control);
            }

            var moduleRequestContext = new ModuleRequestContext
                                            {
                                                DnnPage = Page,
                                                HttpContext = httpContext,
                                                ModuleContext = ModuleContext,
                                                ModuleApplication = moduleApplication,
                                                RouteData = routeData
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
                    action.ID = ModuleContext.GetNextActionID();
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

        protected void ExecuteModule()
        {
            try
            {
                HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);

                var moduleExecutionEngine = GetModuleExecutionEngine();

                _result = moduleExecutionEngine.ExecuteModule(GetModuleRequestContext(httpContext));

                ModuleActions = LoadActions(_result);

                httpContext.SetModuleRequestResult(_result);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion

        #region Properties

        public ModuleActionCollection ModuleActions { get; private set; }

        protected bool ExecuteModuleImmediately { get; set; } = true;

        #endregion

        #region Event Handlers

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            if (ExecuteModuleImmediately)
            {
                ExecuteModule();
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
            try
            {
                if (_result == null) return;
                var mvcString = RenderModule(_result);
                if (!string.IsNullOrEmpty(Convert.ToString(mvcString)))
                {
                    Controls.Add(new LiteralControl(Convert.ToString(mvcString)));
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        #endregion
    }
}
