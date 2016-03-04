// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using System.Web.Mvc;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Framework.Modules;
using DotNetNuke.Web.Mvc.Routing;

namespace DotNetNuke.Web.Mvc.Framework
{
    /// <summary>
    /// A View Engine that will delegate to whatever ViewEngine(s) the module application defines.
    /// </summary>
    public class ModuleDelegatingViewEngine : IViewEngine
    {
        private readonly Dictionary<IView, IViewEngine> _viewEngineMappings = new Dictionary<IView, IViewEngine>();

        /// <summary>
        /// Finds the specified partial view by using the specified controller context.
        /// </summary>
        /// <returns>
        /// The partial view.
        /// </returns>
        /// <param name="controllerContext">The controller context.</param><param name="partialViewName">The name of the partial view.</param><param name="useCache">true to specify that the view engine returns the cached view, if a cached view exists; otherwise, false.</param>
        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
        {
            return RunAgainstModuleViewEngines(controllerContext, e => e.FindPartialView(controllerContext, partialViewName, useCache));
        }

        /// <summary>
        /// Finds the specified view by using the specified controller context.
        /// </summary>
        /// <returns>
        /// The page view.
        /// </returns>
        /// <param name="controllerContext">The controller context.</param><param name="viewName">The name of the view.</param><param name="masterName">The name of the master.</param><param name="useCache">true to specify that the view engine returns the cached view, if a cached view exists; otherwise, false.</param>
        public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
        {
            return RunAgainstModuleViewEngines(controllerContext, e => e.FindView(controllerContext, viewName, masterName, useCache));
        }

        /// <summary>
        /// Releases the specified view by using the specified controller context.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param><param name="view">The view.</param>
        public void ReleaseView(ControllerContext controllerContext, IView view)
        {
            if (_viewEngineMappings.ContainsKey(view))
            {
                _viewEngineMappings[view].ReleaseView(controllerContext, view);
            }
        }

        private ViewEngineResult RunAgainstModuleViewEngines(ControllerContext controllerContext, Func<ViewEngineCollection, ViewEngineResult> engineRequest)
        {
            var controller = controllerContext.Controller as IDnnController;
            if (controller == null || controller.ViewEngineCollectionEx == null)
                return new ViewEngineResult(new string[0]);

            var result = engineRequest(controller.ViewEngineCollectionEx);

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