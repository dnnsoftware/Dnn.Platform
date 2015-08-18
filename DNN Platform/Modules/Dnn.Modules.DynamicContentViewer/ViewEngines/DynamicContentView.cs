// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Dynamic;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.WebPages;
using DotNetNuke.Common;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Razor;

namespace Dnn.Modules.DynamicContentViewer.ViewEngines
{
    /// <summary>
    /// 
    /// </summary>
    public class DynamicContentView : IView
    {
        private string _viewName;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="controllerContext"></param>
        /// <param name="viewName"></param>
        public DynamicContentView(ControllerContext controllerContext, string viewName)
        {
            Requires.NotNull(controllerContext);
            Requires.NotNullOrEmpty("viewName", viewName);

            _viewName = viewName;

            var controller = controllerContext.Controller as IDnnController;

            if (controller != null)
            {
                ModuleContext = controller.ModuleContext;
                LocalResourceFile = controller.LocalResourceFile;
            }

        }

        /// <summary>
        /// 
        /// </summary>
        public string LocalResourceFile { get; set; }

        /// <summary>
        /// 
        /// </summary>
        public ModuleInstanceContext ModuleContext { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="viewContext"></param>
        /// <param name="writer"></param>
        public void Render(ViewContext viewContext, TextWriter writer)
        {
            var engine = new RazorEngine(_viewName, ModuleContext, LocalResourceFile);

            if (viewContext.ViewData != null && viewContext.ViewData.Model != null && !(string.IsNullOrEmpty(_viewName)))
            {
                var context = new WebPageContext(new HttpContextWrapper(HttpContext.Current), engine.Webpage, viewContext.ViewData.Model);
                engine.Render(writer, context);

            }
        }
    }
}
