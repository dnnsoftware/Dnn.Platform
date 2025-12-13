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

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.WebForms
{

    public class WrapperModule : PortalModuleBase, IActionable
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(WrapperModule));

        private string html = string.Empty;

        public ModuleActionCollection ModuleActions { get; private set; } = new ModuleActionCollection();

        // Make sure child controls are created when needed
        protected override void CreateChildControls()
        {
            Controls.Clear();
            Controls.Add(new LiteralControl(html));
            // important so ASP.NET tracks the created controls across postbacks
            ChildControlsCreated = true;
            base.CreateChildControls();
        }

        // ensure child controls exist early in page lifecycle
        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            try
            {
                var mc = ModuleControlFactory.CreateModuleControl(this.ModuleConfiguration);
                html = MvcViewEngine.RenderHtmlHelperToString(helper => mc.Html(helper));
                if (mc is IActionable)
                {
                    var moduleControl = (IActionable)mc;
                    this.ModuleActions = moduleControl.ModuleActions;
                }
                if (mc is IPageContributor)
                {
                    var moduleControl = (IPageContributor)mc;
                    moduleControl.ConfigurePage(new PageConfigurationContext(DependencyProvider));
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Skin.AddModuleMessage(this, "An error occurred while loading the module. Please contact the site administrator.", ModuleMessage.ModuleMessageType.RedError);
                html = "<div class=\"dnnFormMessage dnnFormError\">" + ex.Message + "</div>";
            }
            EnsureChildControls();
        }
    }
}
