// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Modules
{
    using System;
    using System.Web;
    using System.Web.Mvc;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Web.MvcPipeline.ModuleControl;
    using DotNetNuke.Web.MvcPipeline.Utils;

    public static partial class HtmlHelpers
    {
        public static IHtmlString ModuleActions(this HtmlHelper htmlHelper, ModuleInfo module)
        {
            var actionsControl = new ModuleActionsControl();
            actionsControl.RegisterResources(htmlHelper.ViewContext.Controller.ControllerContext);
            actionsControl.ModuleContext.Configuration = module;

            try
            {
                var moduleControl = MvcUtils.GetModuleControl(module, module.ModuleControl.ControlSrc);
                actionsControl.ModuleControl = moduleControl;
                return actionsControl.Html(htmlHelper);
            }
            catch (Exception)
            {
                return new MvcHtmlString(string.Empty);
            }
        }
    }
}
