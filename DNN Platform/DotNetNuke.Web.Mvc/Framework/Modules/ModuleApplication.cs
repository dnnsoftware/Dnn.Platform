// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

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
        protected const string ControllerMasterFormat = "~/DesktopModules/MVC/{0}/Views/{{1}}/{{0}}.cshtml";
        protected const string SharedMasterFormat = "~/DesktopModules/MVC/{0}/Views/Shared/{{0}}.cshtml";
        protected const string ControllerViewFormat = "~/DesktopModules/MVC/{0}/Views/{{1}}/{{0}}.cshtml";
        protected const string SharedViewFormat = "~/DesktopModules/MVC/{0}/Views/Shared/{{0}}.cshtml";
        protected const string ControllerPartialFormat = "~/DesktopModules/MVC/{0}/Views/{{1}}/{{0}}.cshtml";
        protected const string SharedPartialFormat = "~/DesktopModules/MVC/{0}/Views/Shared/{{0}}.cshtml";
        
        private bool _initialized;
        private readonly object _lock = new object();

        public ModuleApplication()
        {
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            ControllerFactory = ControllerBuilder.Current.GetControllerFactory();
            ViewEngines = new ViewEngineCollection();
        }

        public virtual IControllerFactory ControllerFactory { get; set; }

        public string DefaultActionName { get; set; }

        public string DefaultControllerName { get; set; }

        public string[] DefaultNamespaces { get; set; }

        public string FolderPath { get; set; }

        public string ModuleName { get; set; }

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

                moduleController.ValidateRequest = false;

                moduleController.DnnPage = context.DnnPage;

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

        protected static string NormalizeFolderPath(string path)
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