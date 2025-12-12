using System;
using System.Web.UI;
using DotNetNuke.Entities.Modules;
using DotNetNuke.UI.Modules;
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
