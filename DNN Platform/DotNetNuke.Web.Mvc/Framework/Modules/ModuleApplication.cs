// Copyright (c) DNN Software. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Globalization;
using System.Web.Mvc;
using System.Web.Routing;
using DotNetNuke.Services.Localization;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using System.Web;
using System.Reflection;
using DotNetNuke.Web.Mvc.Common;
using Microsoft.Web.Infrastructure.DynamicValidationHelper;

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

        public RequestContext RequestContext { get; private set; }
        internal static readonly string MvcVersion = GetMvcVersionString();
        private const string MvcVersionHeaderName = "X-AspNetMvc-Version";
        private static bool DisableMvcResponseHeader { get; set; }

        private bool _initialized;
        private readonly object _lock = new object();

        public ModuleApplication():this(null, false)
        {
        }
        public ModuleApplication(bool disableMvcResponseHeader) : this(null, disableMvcResponseHeader)
        {
        }
        public ModuleApplication(RequestContext requestContext, bool disableMvcResponseHeader)
        {
            RequestContext = requestContext;
            // ReSharper disable once DoNotCallOverridableMethodsInConstructor
            DisableMvcResponseHeader = disableMvcResponseHeader;
            ControllerFactory = ControllerBuilder.Current.GetControllerFactory();
            ViewEngines = new ViewEngineCollection();
            //ViewEngines.Add(new ModuleDelegatingViewEngine());
        }

        public virtual IControllerFactory ControllerFactory { get; set; }

        public string DefaultActionName { get; set; }

        public string DefaultControllerName { get; set; }

        public string[] DefaultNamespaces { get; set; }

        public string FolderPath { get; set; }

        public string ModuleName { get; set; }

        public ViewEngineCollection ViewEngines { get; set; }

        protected void EnsureInitialized()
        {
            // Double-check lock to wait for initialization
            // TODO: Is there a better (preferably using events and waits) way to do this?
            if (_initialized) return;
            lock (_lock)
            {
                if (_initialized) return;
                Init();
                _initialized = true;
            }
        }

        public virtual ModuleRequestResult ExecuteRequest(ModuleRequestContext context)
        {
            EnsureInitialized();
            RequestContext = RequestContext ?? new RequestContext(context.HttpContext, context.RouteData);
            var currentContext = HttpContext.Current;
            if (currentContext != null)
            {
                var isRequestValidationEnabled = ValidationUtility.IsValidationEnabled(currentContext);
                if (isRequestValidationEnabled == true)
                {
                    ValidationUtility.EnableDynamicValidation(currentContext);
                }
            }
            AddVersionHeader(RequestContext.HttpContext);
            RemoveOptionalRoutingParameters();

            var controllerName = RequestContext.RouteData.GetRequiredString("controller");

            //Construct the controller using the ControllerFactory
            var controller = ControllerFactory.CreateController(RequestContext, controllerName);
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

                moduleController.ViewEngineCollectionEx = ViewEngines;
                // Execute the controller and capture the result
                // if our ActionFilter is executed after the ActionResult has triggered an Exception the filter
                // MUST explicitly flip the ExceptionHandled bit otherwise the view will not render
                moduleController.Execute(RequestContext);
                var result = moduleController.ResultOfLastExecute;

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
            var prefix = NormalizeFolderPath(FolderPath);
            string[] masterFormats =
            { 
                string.Format(CultureInfo.InvariantCulture, ControllerMasterFormat, prefix),
                string.Format(CultureInfo.InvariantCulture, SharedMasterFormat, prefix)
            };
            string[] viewFormats =
            { 
                string.Format(CultureInfo.InvariantCulture, ControllerViewFormat, prefix),
                string.Format(CultureInfo.InvariantCulture, SharedViewFormat, prefix),
                string.Format(CultureInfo.InvariantCulture, ControllerPartialFormat, prefix),
                string.Format(CultureInfo.InvariantCulture, SharedPartialFormat, prefix)
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
            return !string.IsNullOrEmpty(path) ? path.Trim('/') : path;
        }

        protected internal virtual void AddVersionHeader(HttpContextBase httpContext)
        {
            if (!DisableMvcResponseHeader)
            {
                httpContext.Response.AppendHeader(MvcVersionHeaderName, MvcVersion);
            }
        }

        private void RemoveOptionalRoutingParameters()
        {
            var rvd = RequestContext.RouteData.Values;

            // Ensure delegate is stateless
            rvd.RemoveFromDictionary((entry) => entry.Value == UrlParameter.Optional);
        }

        private static string GetMvcVersionString()
        {
            // DevDiv 216459:
            // This code originally used Assembly.GetName(), but that requires FileIOPermission, which isn't granted in
            // medium trust. However, Assembly.FullName *is* accessible in medium trust.
            return new AssemblyName(typeof(MvcHandler).Assembly.FullName).Version.ToString(2);
        }
    }
}