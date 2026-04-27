// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.Mvc.Framework.Modules
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Controllers;
    using DotNetNuke.Entities.Host;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Portals;
    using DotNetNuke.Services.Localization;
    using DotNetNuke.Services.Log.EventLog;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.Mvc.Framework.Controllers;

    public class ResultCapturingActionInvoker : ControllerActionInvoker
    {
        public ActionResult ResultOfLastInvoke { get; set; }

        /// <inheritdoc />
        protected override ActionExecutedContext InvokeActionMethodWithFilters(ControllerContext controllerContext, IList<IActionFilter> filters, ActionDescriptor actionDescriptor, IDictionary<string, object> parameters)
        {
            if (controllerContext.RouteData.Values.ContainsKey("mvcpage"))
            {
                var values = controllerContext.RouteData.Values;
                var moduleContext = new ModuleInstanceContext();
                var moduleInfo = ModuleController.Instance.GetModule((int)values["ModuleId"], (int)values["TabId"], false);

                if (moduleInfo.ModuleControlId != (int)values["ModuleControlId"])
                {
                    moduleInfo = moduleInfo.Clone();
                    moduleInfo.ContainerPath = (string)values["ContainerPath"];
                    moduleInfo.ContainerSrc = (string)values["ContainerSrc"];
                    moduleInfo.ModuleControlId = (int)values["ModuleControlId"];
                    moduleInfo.PaneName = (string)values["PanaName"];
                    moduleInfo.IconFile = (string)values["IconFile"];
                }

                moduleContext.Configuration = moduleInfo;

                if (controllerContext.Controller is DnnController)
                {
                    var dnnController = controllerContext.Controller as DnnController;
                    dnnController.ModuleContext = new ModuleInstanceContext() { Configuration = moduleInfo };
                    dnnController.LocalResourceFile = $"~/DesktopModules/MVC/{moduleInfo.DesktopModule.FolderName}/{Localization.LocalResourceDirectory}/{actionDescriptor.ControllerDescriptor.ControllerName}.resx";

                    var moduleApplication = new ModuleApplication(controllerContext.RequestContext, true)
                    {
                        ModuleName = moduleInfo.DesktopModule.ModuleName,
                        FolderPath = moduleInfo.DesktopModule.FolderName,
                    };

                    moduleApplication.ViewEngines.Add(
                        new ModuleDelegatingViewEngine(
                            new HostSettings(
                                new HostController(
#pragma warning disable CS0618 // Type or member is obsolete
                                    new EventLogController(),
#pragma warning restore CS0618 // Type or member is obsolete
                                    new Lazy<IPortalController>(() => PortalController.Instance)))));

                    moduleApplication.Init();

                    dnnController.ViewEngineCollectionEx = moduleApplication.ViewEngines;
                }
            }

            var context = base.InvokeActionMethodWithFilters(controllerContext, filters, actionDescriptor, parameters);
            this.ResultOfLastInvoke = context.Result;
            return context;
        }

        /// <inheritdoc />
        protected override ExceptionContext InvokeExceptionFilters(ControllerContext controllerContext, IList<IExceptionFilter> filters, Exception exception)
        {
            var context = base.InvokeExceptionFilters(controllerContext, filters, exception);
            this.ResultOfLastInvoke = context.Result;
            return context;
        }

        /// <inheritdoc />
        protected override void InvokeActionResult(ControllerContext controllerContext, ActionResult actionResult)
        {
            // Do not invoke the action.  Instead, store it for later retrieval
            if (this.ResultOfLastInvoke == null)
            {
                this.ResultOfLastInvoke = actionResult;
            }

            if (controllerContext.RouteData.Values.ContainsKey("mvcpage"))
            {
                base.InvokeActionResult(controllerContext, actionResult);
            }
        }
    }
}
