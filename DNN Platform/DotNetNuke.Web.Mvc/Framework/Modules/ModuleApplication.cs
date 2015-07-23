﻿#region Copyright
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
using System.Web.Mvc;
using System.Web.Routing;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Mvc.Framework.Controllers;

namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    public class ModuleApplication
    {
        private const string ControllerMasterFormat = "~/DesktopModules/MVC/{0}/Views/{{1}}/{{0}}.cshtml";
        private const string SharedMasterFormat = "~/DesktopModules/MVC/{0}/Views/Shared/{{0}}.cshtml";
        private const string ControllerViewFormat = "~/DesktopModules/MVC/{0}/Views/{{1}}/{{0}}.cshtml";
        private const string SharedViewFormat = "~/DesktopModules/MVC/{0}/Views/Shared/{{0}}.cshtml";
        private const string ControllerPartialFormat = "~/DesktopModules/MVC/{0}/Views/{{1}}/{{0}}.cshtml";
        private const string SharedPartialFormat = "~/DesktopModules/MVC/{0}/Views/Shared/{{0}}.cshtml";
        
        private bool _initialized;
        private readonly object _lock = new object();

        public ModuleApplication()
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            ControllerFactory = ControllerBuilder.Current.GetControllerFactory();
            ViewEngines = new ViewEngineCollection();
        }

        public virtual IControllerFactory ControllerFactory { get; set; }

        public virtual string DefaultActionName { get; set; }

        public virtual string DefaultControllerName { get; set; }

        public virtual string FolderPath { get; set; }

        public virtual string ModuleName { get; set; }

        public ViewEngineCollection ViewEngines { get; set; }

        private void EnsureInitialized()
        {
            // Double-check lock to wait for initialization
            // TODO: Is there a better (preferably using events and waits) way to do this?
            if (!_initialized)
            {
                lock (_lock)
                {
                    if (!_initialized)
                    {
                        Init();
                        _initialized = true;
                    }
                }
            }
        }

        public virtual ModuleRequestResult ExecuteRequest(ModuleRequestContext context)
        {
            EnsureInitialized();

            var requestContext = new RequestContext(context.HttpContext, context.RouteData);

            var controllerName = (string)context.RouteData.Values["controller"];

            //Construct the controller using the ControllerFactory
            IController controller = ControllerFactory.CreateController(requestContext, controllerName);
            try
            {
                // Check if the controller supports IDnnController
                var moduleController = controller as IDnnController;

                // If we couldn't adapt it, we fail.  We can't support IController implementations directly :(
                // Because we need to retrieve the ActionResult without executing it, IController won't cut it
                if (moduleController == null)
                {
                    throw new InvalidOperationException("Could Not Construct Controller");
                }
                moduleController.ModuleContext = context.ModuleContext;

                moduleController.LocalResourceFile = String.Format("~/DesktopModules/MVC/{0}/{1}/{2}.resx",
                                                    context.ModuleContext.Configuration.DesktopModule.FolderName,
                                                    Localization.LocalResourceDirectory,
                                                    controllerName);

                // Execute the controller and capture the result
                moduleController.Execute(requestContext);
                ActionResult result = moduleController.ResultOfLastExecute;

                // Return the final result
                return new ModuleRequestResult
                                {
                                    ActionResult = result,
                                    ControllerContext = moduleController.ControllerContext,
                                    ModuleActions = moduleController.ModuleActions,
                                    ModuleContext = context.ModuleContext,
                                    ModuleApplication = this
                                };
            }
            finally
            {
                ControllerFactory.ReleaseController(controller);
            }
        }

        protected internal virtual void Init()
        {
            string prefix = NormalizeFolderPath(FolderPath);
            string[] masterFormats =
            { 
                String.Format(CultureInfo.InvariantCulture, ControllerMasterFormat, prefix),
                String.Format(CultureInfo.InvariantCulture, SharedMasterFormat, prefix)
            };
            string[] viewFormats =
            { 
                String.Format(CultureInfo.InvariantCulture, ControllerViewFormat, prefix),
                String.Format(CultureInfo.InvariantCulture, SharedViewFormat, prefix),
                String.Format(CultureInfo.InvariantCulture, ControllerPartialFormat, prefix),
                String.Format(CultureInfo.InvariantCulture, SharedPartialFormat, prefix)
            };

            ViewEngines.Add(new RazorViewEngine
                                    {
                                        MasterLocationFormats = masterFormats,
                                        ViewLocationFormats = viewFormats,
                                        PartialViewLocationFormats = viewFormats
                                    });
        }

        private static string NormalizeFolderPath(string path)
        {
            // Remove leading and trailing slashes
            if (!String.IsNullOrEmpty(path))
            {
                return path.Trim('/');
            }
            return path;
        }
    }
}