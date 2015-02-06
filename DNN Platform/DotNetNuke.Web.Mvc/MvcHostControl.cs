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

using System;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.UI;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Security;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Common;
using DotNetNuke.Web.Mvc.Framework.Modules;
using DotNetNuke.Web.Mvc.Routing;

namespace DotNetNuke.Web.Mvc
{
    public class MvcHostControl : ModuleControlBase, IActionable
    {
        private ModuleRequestResult _result;

        private ModuleRequestContext GetModuleRequestContext(HttpContextBase httpContext, ModuleInfo module)
        {
            //TODO DesktopModuleControllerAdapter usage is temporary in order to make method testable
            DesktopModuleInfo desktopModule = DesktopModuleControllerAdapter.Instance.GetDesktopModule(module.DesktopModuleID, module.PortalID);
            var defaultControl = ModuleControlController.GetModuleControlByControlKey("", module.ModuleDefID);
            var defaultSegments = defaultControl.ControlSrc.Replace(".mvc", "").Split('/');

            var moduleApplication = new ModuleApplication
                                            {
                                                DefaultActionName = defaultSegments[1],
                                                DefaultControllerName = defaultSegments[0],
                                                ModuleName = desktopModule.ModuleName,
                                                FolderPath = desktopModule.FolderName
                                            };

            var segments = module.ModuleControl.ControlSrc.Replace(".mvc", "").Split('/');

            var moduleRequestContext = new ModuleRequestContext
                                            {
                                                ActionName = segments[1],
                                                ControllerName = segments[0],
                                                HttpContext = httpContext,
                                                Module = module, 
                                                ModuleApplication = moduleApplication
                                            };

            return moduleRequestContext;
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

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            try
            {
                if (String.IsNullOrEmpty(ModuleContext.Configuration.ModuleControl.ControlKey))
                {
                    LoadActions(ModuleContext.Configuration);
                }

                HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);

                var moduleExecutionEngine = GetModuleExecutionEngine();

                LoadActions(ModuleContext.Configuration);

                _result = moduleExecutionEngine.ExecuteModule(GetModuleRequestContext(httpContext, ModuleContext.Configuration));

                httpContext.SetModuleRequestResult(_result);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            try
            {
                HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);

                if (_result != null)
                {
                    Controls.Add(new LiteralControl(RenderModule(_result, httpContext).ToString()));
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

        private void LoadActions(ModuleInfo module)
        {
            ModuleActions = new ModuleActionCollection();

            if (String.IsNullOrEmpty(module.ModuleControl.ControlKey))
            {
                var moduleControls = ModuleControlController.GetModuleControlsByModuleDefinitionID(module.ModuleDefID);

                foreach (var moduleControl in moduleControls.Values)
                {
                    if (!String.IsNullOrEmpty(moduleControl.ControlKey))
                    {
                        ModuleActions.Add(ModuleContext.GetNextActionID(),
                            moduleControl.ControlKey,
                            moduleControl.ControlKey + ".Action",
                            "",
                            "edit.gif",
                            ModuleContext.EditUrl("Edit"),
                            false,
                            SecurityAccessLevel.Edit,
                            true,
                            false);
                    }
                }
            }
        }

        private MvcHtmlString RenderModule(ModuleRequestResult moduleResult, HttpContextBase httpContext)
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

        public ModuleActionCollection ModuleActions { get; private set; }
    }
}
