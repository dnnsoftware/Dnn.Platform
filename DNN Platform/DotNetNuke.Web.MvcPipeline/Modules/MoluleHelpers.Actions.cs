// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.Modules
{
    using System;
    using System.Web;
    using System.Web.Mvc;

    using DotNetNuke.Entities.Modules;
    using DotNetNuke.UI.Modules;
    using DotNetNuke.Web.MvcPipeline.ModuleControl;
    using DotNetNuke.Web.MvcPipeline.ModuleControl.Page;
    using DotNetNuke.Web.MvcPipeline.Utils;

    /// <summary>
    /// HTML helpers for rendering module action menus.
    /// </summary>
    public static partial class ModuleHelpers
    {
        /// <summary>
        /// Renders the module actions UI for the specified MVC module control.
        /// </summary>
        /// <param name="htmlHelper">The HTML helper.</param>
        /// <param name="moduleControl">The MVC module control.</param>
        /// <returns>The rendered module actions HTML.</returns>
        public static IHtmlString ModuleActions(this HtmlHelper htmlHelper, IMvcModuleControl moduleControl)
        {
            var actionsControl = new ModuleActionsControl();
            actionsControl.ConfigurePage(new PageConfigurationContext(Common.Globals.GetCurrentServiceProvider()));
            actionsControl.ModuleContext.Configuration = moduleControl.ModuleContext.Configuration;

            try
            {
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
