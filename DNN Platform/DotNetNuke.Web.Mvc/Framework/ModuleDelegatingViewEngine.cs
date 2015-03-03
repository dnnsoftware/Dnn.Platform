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
using System.Web.Mvc;
using DotNetNuke.Web.Mvc.Framework.Modules;
using DotNetNuke.Web.Mvc.Routing;

namespace DotNetNuke.Web.Mvc.Framework
{
    public class ModuleDelegatingViewEngine : IViewEngine
    {
        private readonly Dictionary<IView, IViewEngine> _viewEngineMappings = new Dictionary<IView, IViewEngine>();

        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            return RunAgainstModuleViewEngines(controllerContext, e => e.FindPartialView(controllerContext, partialViewName));
        }

        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return RunAgainstModuleViewEngines(controllerContext, e => e.FindView(controllerContext, viewName, masterName));
        }

        public void ReleaseView(ControllerContext controllerContext, IView view)
        {
            if (_viewEngineMappings.ContainsKey(view))
            {
                _viewEngineMappings[view].ReleaseView(controllerContext, view);
            }
        }

        private ViewEngineResult RunAgainstModuleViewEngines(ControllerContext controllerContext, Func<ViewEngineCollection, ViewEngineResult> engineRequest)
        {
            // Get the current module request
            ModuleRequestResult moduleRequestResult = GetCurrentModuleRequestResult(controllerContext);

            // No current request => Skip this view engine
            if (moduleRequestResult == null)
            {
                return new ViewEngineResult(new string[0]);
            }

            // Delegate to the module's view engine collection
            ViewEngineResult result = engineRequest(moduleRequestResult.Application.ViewEngines);

            // If there is a view, store the view<->viewengine mapping so release works correctly
            if (result.View != null)
            {
                _viewEngineMappings[result.View] = result.ViewEngine;
            }

            return result;
        }

        private static ModuleRequestResult GetCurrentModuleRequestResult(ControllerContext controllerContext)
        {
            if (controllerContext.HttpContext.HasModuleRequestResult())
            {
                return controllerContext.HttpContext.GetModuleRequestResult();
            }
            return null;
        }
    }
}