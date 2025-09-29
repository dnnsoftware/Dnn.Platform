using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.UI;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Framework;
using DotNetNuke.Framework.JavaScriptLibraries;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Client.ClientResourceManagement;
using DotNetNuke.Web.MvcPipeline.Framework.JavascriptLibraries;
using DotNetNuke.Web.MvcPipeline.ModuleControl.Resources;
using DotNetNuke.Web.MvcPipeline.Utils;

namespace DotNetNuke.Web.MvcPipeline.ModuleControl.Utils
{
    public class MvcModuleControlRenderer<T> where T : RazorModuleControlBase, new()
    {
        private readonly T moduleControl;

        public MvcModuleControlRenderer(Control control, ModuleInstanceContext moduleContext)
        {
            this.moduleControl = new T();
            moduleControl.ModuleContext.Configuration = moduleContext.Configuration;
            moduleControl.ViewContext.HttpContext = new System.Web.HttpContextWrapper(System.Web.HttpContext.Current);
            //moduleControl.Control = control;
        }

        public MvcModuleControlRenderer(PortalModuleBase control)
            : this(control, control.ModuleContext)
        {
        }

        public string RenderToString()
        {
            return MvcViewEngine.RenderHtmlHelperToString(helper => moduleControl.Html(helper));
        }

        public T ModuleControl => this.moduleControl;
       
    }
}
