using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc.Html;
using System.Web.UI;
using System.Web.UI.WebControls;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Web.MvcPipeline.ModuleControl;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Resources;
using DotNetNuke.Web.MvcPipeline.UI.Utilities;
using DotNetNuke.Web.MvcPipeline.Utils;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Demo { 

    public class DemoModule : PortalModuleBase, IActionable
    {
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
            var mc = MvcUtils.CreateModuleControl(this.ModuleConfiguration);
            html = ViewRenderer.RenderHtmlHelperToString(helper => mc.Html(helper));
            /*
            if (mc is RazorModuleControlBase)
            {
                var moduleControl = (RazorModuleControlBase)mc;
                // moduleControl.Control = this;
                // moduleControl.ModuleContext = this.ModuleContext;
                moduleControl.ViewContext.HttpContext = new HttpContextWrapper(this.Context);
                var res = moduleControl.Invoke();
                var renderer = new ViewRenderer();
                html = renderer.RenderViewToString(res.ViewName, res.Model);
            }
            */
            if (mc is IActionable){
                var moduleControl = (IActionable)mc;
                this.ModuleActions = moduleControl.ModuleActions;
            }
            if (mc is IResourcable)
            {
                var moduleControl = (IResourcable)mc;
                moduleControl.RegisterResources(this.Page);
            }

            EnsureChildControls();
        }
    }
}
