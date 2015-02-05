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
using System.Web.Mvc;
using System.Web.Routing;
using DotNetNuke.Web.Mvc.Framework.Controllers;

namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    public class ModuleApplication
    {
        private const string ControllerMasterFormat = "~/Modules/{0}/Views/{{1}}/{{0}}.cshtml";
        private const string SharedMasterFormat = "~/Modules/{0}/Views/Shared/{{0}}.cshtml";
        private const string ControllerViewFormat = "~/Modules/{0}/Views/{{1}}/{{0}}.cshtml";
        private const string SharedViewFormat = "~/Modules/{0}/Views/Shared/{{0}}.cshtml";
        private const string ControllerPartialFormat = "~/Modules/{0}/Views/{{1}}/{{0}}.cshtml";
        private const string SharedPartialFormat = "~/Modules/{0}/Views/Shared/{{0}}.cshtml";
        
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

        protected internal virtual IDnnController AdaptController(IController controller)
        {
            var mvcController = controller as Controller;
            if (mvcController != null && mvcController.ActionInvoker is ControllerActionInvoker)
            {
                return new DnnControllerAdapter(mvcController);
            }
            return null;
        }

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
            var routeData = new RouteData();
            routeData.Values.Add("controller", context.ControllerName);
            routeData.Values.Add("action", context.ActionName);

            var requestContext = new RequestContext(context.HttpContext, routeData);

            //Construct the controller using the ControllerFactory
            IController controller = ControllerFactory.CreateController(requestContext, context.ControllerName);
            try
            {
                // Check if the controller supports IDnnController and if not, try to adapt it
                var moduleController = controller as IDnnController ?? AdaptController(controller);

                // If we couldn't adapt it, we fail.  We can't support IController implementations without some kind of adaptor :(
                // Because we need to retrieve the ActionResult without executing it, IController won't cut it
                if (moduleController == null)
                {
                    throw new InvalidOperationException("Could Not Construct Controller");
                }
                moduleController.ActiveModule = context.Module;

                // Execute the controller and capture the result
                moduleController.Execute(requestContext);
                ActionResult result = moduleController.ResultOfLastExecute;

                // Return the final result
                return new ModuleRequestResult
                                {
                                    Application = this,
                                    ActionResult = result,
                                    ControllerContext = moduleController.ControllerContext,
                                    Module = context.Module
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