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
using System.Collections.Generic;
using System.IO;
using System.Web;

using DotNetNuke.Common;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Web.Mvc.Common;
using DotNetNuke.Web.Mvc.Framework.ActionResults;

namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    public class ModuleExecutionEngine : IModuleExecutionEngine
    {
        public virtual ModuleRequestResult ExecuteModule(HttpContextBase httpContext, ModuleInfo module, string moduleRoute)
        {
            Requires.NotNull("httpContext", httpContext);
            Requires.NotNull("module", module);
            Requires.NotNull("moduleRoute", moduleRoute); // Empty route is OK!

            //Get the module application for this module
            var app = GetModuleApplication(module);

            return ExecuteModule(httpContext, module, app, moduleRoute);
        }

        private ModuleRequestResult ExecuteModule(HttpContextBase httpContext, ModuleInfo module, ModuleApplication moduleApplication, string moduleRoute)
        {
            Requires.NotNull("httpContext", httpContext);
            Requires.NotNull("module", module);
            Requires.NotNull("moduleRoute", moduleRoute);

            if (moduleApplication != null)
            {
                // Setup the module's context
                var moduleRequestContext = new ModuleRequestContext
                                                {
                                                    Application = moduleApplication,
                                                    Module = module,
                                                    ModuleRoutingUrl = moduleRoute,
                                                    HttpContext = httpContext,
                                                };


                // Run the module
                ModuleRequestResult result = moduleApplication.ExecuteRequest(moduleRequestContext);
                return result;
            }
            return null;
        }

        public virtual void ExecuteModuleResult(SiteContext siteContext, ModuleRequestResult moduleResult, TextWriter writer)
        {
            RunInModuleResultContext(siteContext, moduleResult, () => 
                    { 
                        var result = moduleResult.ActionResult as IDnnViewResult;
                        if (result != null)
                        {
                            result.ExecuteResult(moduleResult.ControllerContext, writer);
                        }
                        else
                        {
                            moduleResult.ActionResult.ExecuteResult(moduleResult.ControllerContext);
                        }
                    });
        }

        public virtual ModuleApplication GetModuleApplication(ModuleInfo module)
        {
            //TODO DesktopModuleControllerAdapter usage is temporary in order to make method testable
            DesktopModuleInfo desktopModule = DesktopModuleControllerAdapter.Instance.GetDesktopModule(module.DesktopModuleID, module.PortalID);

            Type moduleType = Reflection.CreateType(desktopModule.BusinessControllerClass);

            return Reflection.CreateInstance(moduleType) as ModuleApplication;
        }

        protected internal void RunInModuleResultContext(SiteContext siteContext, ModuleRequestResult moduleResult, Action action)
        {
            // Set the active module
            ModuleRequestResult oldRequest = siteContext.ActiveModuleRequest;
            siteContext.ActiveModuleRequest = moduleResult;

            // Run the action
            action();

            // Restore the previous active module
            siteContext.ActiveModuleRequest = oldRequest;
        }
    }
}