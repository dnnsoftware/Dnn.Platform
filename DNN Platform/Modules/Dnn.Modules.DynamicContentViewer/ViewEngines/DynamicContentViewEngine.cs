// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Web.Mvc;
using Dnn.Modules.DynamicContentViewer.Controllers;
using DotNetNuke.Common;

namespace Dnn.Modules.DynamicContentViewer.ViewEngines
{
	/// <summary>
    /// The templated view engine is used to dynamically determine the View to use.
    /// </summary>
    public class DynamicContentViewEngine : IViewEngine
    {
        /// <summary>
        /// Finds the specified partial view by using the specified controller context.
        /// </summary>
        /// <returns>
        /// The partial view.
        /// </returns>
        /// <param name="controllerContext">The controller context.</param><param name="partialViewName">The name of the partial view.</param><param name="useCache">true to specify that the view engine returns the cached view, if a cached view exists; otherwise, false.</param>
        public ViewEngineResult FindPartialView(ControllerContext controllerContext, string partialViewName, bool useCache)
	    {
	        throw new System.NotImplementedException();
	    }

	    /// <summary>
	    /// Finds the specified view by using the specified controller context.
	    /// </summary>
	    /// 
	    /// <returns>
	    /// The page view.
	    /// </returns>
	    /// <param name="controllerContext">The controller context.</param><param name="viewName">The name of the view.</param><param name="masterName">The name of the master.</param><param name="useCache">true to specify that the view engine returns the cached view, if a cached view exists; otherwise, false.</param>
	    public ViewEngineResult FindView(ControllerContext controllerContext, string viewName, string masterName, bool useCache)
	    {
            Requires.NotNull(controllerContext);
            Requires.NotNullOrEmpty("viewName", viewName);

            var controller = controllerContext.Controller as ViewerController;

            if (controller != null && viewName != "GettingStarted")
            {
                return new ViewEngineResult(new DynamicContentView(controllerContext, viewName), this);
	        }
	        else
	        {
	            return new ViewEngineResult(new string[]
	                {
                        "No Suitable view found for 'DynamicContentViewEngine', please ensure View Name contains 'Dynamic'"
                    });
	        }
	    }

	    /// <summary>
        /// Releases the specified view by using the specified controller context.
        /// </summary>
        /// <param name="controllerContext">The controller context.</param><param name="view">The view.</param>
        public void ReleaseView(ControllerContext controllerContext, IView view)
	    {
        }
    }
}
