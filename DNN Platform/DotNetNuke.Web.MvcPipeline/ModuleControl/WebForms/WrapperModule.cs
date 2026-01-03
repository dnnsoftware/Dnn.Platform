// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.WebForms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc.Html;
    using System.Web.UI;
    using System.Web.UI.WebControls;

    using DotNetNuke.Abstractions.ClientResources;
    using DotNetNuke.Abstractions.Pages;
    using DotNetNuke.Entities.Modules;
    using DotNetNuke.Entities.Modules.Actions;
    using DotNetNuke.Framework;
    using DotNetNuke.Instrumentation;
    using DotNetNuke.Services.Installer.Log;
    using DotNetNuke.UI.Skins;
    using DotNetNuke.UI.Skins.Controls;
    using DotNetNuke.Web.MvcPipeline.ModuleControl.Page;
    using DotNetNuke.Web.MvcPipeline.UI.Utilities;
    using DotNetNuke.Web.MvcPipeline.Utils;
    using Microsoft.Extensions.DependencyInjection;

    /// <summary>
    /// WebForms wrapper module that hosts an MVC module control inside a classic DNN module.
    /// </summary>
    public class WrapperModule : PortalModuleBase, IActionable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WrapperModule));

        private string html = string.Empty;

        /// <inheritdoc/>
        public ModuleActionCollection ModuleActions { get; private set; } = new ModuleActionCollection();

        /// <summary>
        /// Ensures child controls are created and wired up for the wrapped MVC output.
        /// </summary>
        protected override void CreateChildControls()
        {
            this.Controls.Clear();
            this.Controls.Add(new LiteralControl(this.html));

            // important so ASP.NET tracks the created controls across postbacks
            this.ChildControlsCreated = true;
            base.CreateChildControls();
        }

        /// <summary>
        /// Ensures the wrapped MVC module control is created and rendered early in the page lifecycle.
        /// </summary>
        /// <param name="e">The event arguments.</param>
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            try
            {
                var mc = ModuleControlFactory.CreateModuleControl(this.ModuleConfiguration);
                this.html = MvcViewEngine.RenderHtmlHelperToString(helper => mc.Html(helper));
                if (mc is IActionable actionable)
                {
                    this.ModuleActions = actionable.ModuleActions;
                }

                if (mc is IPageContributor pageContributor)
                {
                    pageContributor.ConfigurePage(new PageConfigurationContext(this.DependencyProvider));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Skin.AddModuleMessage(this, "An error occurred while loading the module. Please contact the site administrator.", ModuleMessage.ModuleMessageType.RedError);
                this.html = "<div class=\"dnnFormMessage dnnFormError\">" + ex.Message + "</div>";
            }

            this.EnsureChildControls();
        }
    }
}
